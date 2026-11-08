using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;

public class NPCInteraction : MonoBehaviour
{
    [Header("UI References (Auto-Filled)")]
    [SerializeField] private GameObject dialogueBox;
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private TMP_Text characterNameText;
    [SerializeField] private Image characterArtImage;

    [Header("Quest UI References")]
    [SerializeField] private TMP_Text questTitleText;
    [SerializeField] private TMP_Text questDescriptionText;

    [Header("NPC Info")]
    [SerializeField] private string npcName;
    [SerializeField] private Sprite npcSprite;

    [Header("Dialogue Settings")]
    [SerializeField] private float completionTextDuration = 4f;
    [SerializeField, TextArea(2, 5)] private string[] dialogueLines;

    [Header("Conditional NPC Settings")]
    [SerializeField] private bool requiresOtherNPC = false;
    [SerializeField] private string requiredNPCName;
    [SerializeField, TextArea(2, 5)] private string[] conditionalDialogueLines;

    [Header("Interaction Settings")]
    [SerializeField] private float interactionRange = 2f;

    [Header("Quest Chain Settings (Legacy / Inspector)")]
    [SerializeField] private bool isQuestGiver = false;
    [SerializeField] private QuestData[] quests;

    // Runtime state
    private int currentQuestIndex = 0;
    private int currentKills = 0;
    private int currentDamage = 0;
    private bool questActive = false;
    private bool questCompleted = false;
    private bool isCompletingQuest = false;

    private Transform player;
    private PlayerController playerController;
    private bool isPlayerInRange = false;

    // Screenplay dialogue state
    private List<QuestDialogueLine> activeScreenplayLines = new List<QuestDialogueLine>();
    private int dialogueIndex = 0;
    private System.Action onDialogueFinishCallback = null;

    // Overhead Floating Indicator (Dragon Nest / BDO Style)
    private GameObject indicatorObj;
    private TextMeshPro indicatorTMP;
    private Vector3 indicatorBaseOffset = new Vector3(0, 1.6f, 0);

    // Input
    private PlayerControls controls;
    private InputAction interactAction;

    public bool IsDialogueActive => dialogueBox != null && dialogueBox.activeSelf;
    public bool IsQuestCompleted => questCompleted || (QuestManager.Instance != null && QuestManager.Instance.IsAnyQuestCompletedForNPC(EffectiveNPCName));
    public bool HasActiveQuest => questActive;
    public string NPCName => EffectiveNPCName;
    public string EffectiveNPCName => string.IsNullOrEmpty(npcName) ? gameObject.name : npcName;

    public QuestType CurrentQuestType
    {
        get
        {
            var mmoQuest = QuestManager.Instance != null ? QuestManager.Instance.GetActiveOrAvailableQuestForNPC(EffectiveNPCName) : null;
            if (mmoQuest != null && mmoQuest.state == QuestState.InProgress) return mmoQuest.questType;

            if (quests != null && quests.Length > 0 && currentQuestIndex < quests.Length)
                return quests[currentQuestIndex].questType;

            return QuestType.None;
        }
    }

    private void Awake()
    {
        if (string.IsNullOrEmpty(npcName)) npcName = gameObject.name;
        if (controls == null) controls = new PlayerControls();
    }

    private void OnEnable()
    {
        if (controls == null) controls = new PlayerControls();
        try { controls.Enable(); } catch { }
        if (interactAction == null) interactAction = controls.Interaction.NPCInteractionButton;
        if (interactAction != null)
        {
            interactAction.performed -= OnInteract;
            interactAction.performed += OnInteract;
        }

        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.OnQuestStateChanged -= HandleQuestStateChanged;
            QuestManager.Instance.OnQuestStateChanged += HandleQuestStateChanged;
        }
    }

    private void OnDisable()
    {
        if (interactAction != null) interactAction.performed -= OnInteract;
        if (controls != null)
        {
            try { controls.Disable(); } catch { }
        }

        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.OnQuestStateChanged -= HandleQuestStateChanged;
        }
    }

    private void OnDestroy()
    {
        if (interactAction != null) interactAction.performed -= OnInteract;
        if (GameManager.Instance != null) GameManager.Instance.UnregisterQuestGiver(this);

        Time.timeScale = 1f;
        if (playerController != null) playerController.EnableMovement();
    }

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (player != null) playerController = player.GetComponent<PlayerController>();

        BindUIReferences();
        CreateOverheadIndicator();

        if (GameManager.Instance != null) GameManager.Instance.RegisterQuestGiver(this);
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.OnQuestStateChanged -= HandleQuestStateChanged;
            QuestManager.Instance.OnQuestStateChanged += HandleQuestStateChanged;
        }

        UpdateIndicator();
    }

    private void BindUIReferences()
    {
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.BindUI();
            dialogueBox = QuestManager.Instance.dialogueBox;
            dialogueText = QuestManager.Instance.dialogueText;
            characterNameText = QuestManager.Instance.characterNameText;
            characterArtImage = QuestManager.Instance.characterArtImage;
            questTitleText = QuestManager.Instance.questTitleText;
            questDescriptionText = QuestManager.Instance.questDescriptionText;
        }

        if (dialogueBox == null)
        {
            GameObject canvas = GameObject.Find("UICanvas");
            if (canvas != null)
            {
                Transform[] allChildren = canvas.GetComponentsInChildren<Transform>(true);
                foreach (Transform t in allChildren)
                {
                    if (t.name == "DialogueBox") dialogueBox = t.gameObject;
                    if (t.name == "DialogueText") dialogueText = t.GetComponent<TMP_Text>();
                    if (t.name == "CharacterNameText") characterNameText = t.GetComponent<TMP_Text>();
                    if (t.name == "CharacterArtImage") characterArtImage = t.GetComponent<Image>();
                    if (t.name == "QuestTitleText") questTitleText = t.GetComponent<TMP_Text>();
                    if (t.name == "QuestDescriptionText") questDescriptionText = t.GetComponent<TMP_Text>();
                }
            }
        }

        if (dialogueBox != null) dialogueBox.SetActive(false);
        if (characterArtImage != null) characterArtImage.gameObject.SetActive(false);
    }

    #region Dragon Nest / BDO Overhead Indicator
    private void CreateOverheadIndicator()
    {
        if (indicatorObj != null) return;

        indicatorObj = new GameObject("OverheadQuestIndicator");
        indicatorObj.transform.SetParent(transform);
        indicatorObj.transform.localPosition = indicatorBaseOffset;

        indicatorTMP = indicatorObj.AddComponent<TextMeshPro>();
        indicatorTMP.alignment = TextAlignmentOptions.Center;
        indicatorTMP.fontSize = 7;
        indicatorTMP.fontStyle = FontStyles.Bold;
        indicatorTMP.sortingOrder = 100;
        indicatorTMP.text = "";
    }

    private void LateUpdate()
    {
        if (indicatorObj != null && indicatorObj.activeSelf)
        {
            // Gentle MMORPG floating bob animation
            float bob = Mathf.Sin(Time.time * 4f) * 0.08f;
            indicatorObj.transform.localPosition = indicatorBaseOffset + new Vector3(0, bob, 0);
        }
    }

    private void HandleQuestStateChanged(string questID, QuestState state)
    {
        UpdateIndicator();
    }

    public void UpdateIndicator()
    {
        if (indicatorTMP == null) return;

        var mmoQuest = QuestManager.Instance != null ? QuestManager.Instance.GetActiveOrAvailableQuestForNPC(EffectiveNPCName) : null;
        if (mmoQuest == null)
        {
            indicatorTMP.text = "";
            indicatorObj.SetActive(false);
            return;
        }

        // 1. ReadyToTurnIn -> Golden '?'
        if (mmoQuest.state == QuestState.ReadyToTurnIn && string.Equals(mmoQuest.turnInNpcID, EffectiveNPCName, System.StringComparison.OrdinalIgnoreCase))
        {
            indicatorObj.SetActive(true);
            indicatorTMP.text = "?";
            indicatorTMP.color = new Color(1f, 0.843f, 0f, 1f); // #FFD700 Gold
            indicatorTMP.fontSize = 8;
        }
        // 2. InProgress -> Gray '?'
        else if (mmoQuest.state == QuestState.InProgress && string.Equals(mmoQuest.turnInNpcID, EffectiveNPCName, System.StringComparison.OrdinalIgnoreCase))
        {
            indicatorObj.SetActive(true);
            indicatorTMP.text = "?";
            indicatorTMP.color = new Color(0.6f, 0.6f, 0.6f, 1f); // Gray
            indicatorTMP.fontSize = 7;
        }
        // 3. Available -> Golden '!'
        else if (mmoQuest.state == QuestState.Available && string.Equals(mmoQuest.startNpcID, EffectiveNPCName, System.StringComparison.OrdinalIgnoreCase))
        {
            indicatorObj.SetActive(true);
            indicatorTMP.text = "!";
            indicatorTMP.color = new Color(1f, 0.843f, 0f, 1f); // #FFD700 Gold
            indicatorTMP.fontSize = 8;
        }
        else
        {
            indicatorTMP.text = "";
            indicatorObj.SetActive(false);
        }
    }
    #endregion

    private void Update()
    {
        if (player == null) return;

        float distance = Vector2.Distance(transform.position, player.position);
        bool inRangeNow = distance <= interactionRange;

        if (inRangeNow && !isPlayerInRange)
            isPlayerInRange = true;
        else if (!inRangeNow && isPlayerInRange)
        {
            isPlayerInRange = false;
            HideDialogue();
        }

        if (questActive)
            UpdateQuestUI();
    }

    private void OnInteract(InputAction.CallbackContext context)
    {
        if (this == null || gameObject == null) return;
        if (!isPlayerInRange) return;

        if (IsDialogueActive)
        {
            NextScreenplayLine();
        }
        else
        {
            StartMMOInteraction();
        }
    }

    private void StartMMOInteraction()
    {
        var mmoQuest = QuestManager.Instance != null ? QuestManager.Instance.GetActiveOrAvailableQuestForNPC(EffectiveNPCName) : null;

        if (mmoQuest != null)
        {
            // Case 1: Ready to turn in!
            if (mmoQuest.state == QuestState.ReadyToTurnIn && string.Equals(mmoQuest.turnInNpcID, EffectiveNPCName, System.StringComparison.OrdinalIgnoreCase))
            {
                var lines = (mmoQuest.turnInDialogue != null && mmoQuest.turnInDialogue.Count > 0)
                    ? mmoQuest.turnInDialogue
                    : new List<QuestDialogueLine> { new QuestDialogueLine(EffectiveNPCName, "Thank you! The quest is complete.") };

                StartScreenplayDialogue(lines, () =>
                {
                    QuestManager.Instance.CompleteQuest(mmoQuest.questID);
                    questCompleted = true;
                    questActive = false;
                    UpdateIndicator();
                });
                return;
            }

            // Case 2: In Progress
            if (mmoQuest.state == QuestState.InProgress && string.Equals(mmoQuest.turnInNpcID, EffectiveNPCName, System.StringComparison.OrdinalIgnoreCase))
            {
                var lines = (mmoQuest.inProgressDialogue != null && mmoQuest.inProgressDialogue.Count > 0)
                    ? mmoQuest.inProgressDialogue
                    : new List<QuestDialogueLine> { new QuestDialogueLine(EffectiveNPCName, "Please finish the task and return to me.") };

                StartScreenplayDialogue(lines, null);
                return;
            }

            // Case 3: Available to accept
            if (mmoQuest.state == QuestState.Available && string.Equals(mmoQuest.startNpcID, EffectiveNPCName, System.StringComparison.OrdinalIgnoreCase))
            {
                var lines = (mmoQuest.offerDialogue != null && mmoQuest.offerDialogue.Count > 0)
                    ? mmoQuest.offerDialogue
                    : new List<QuestDialogueLine> { new QuestDialogueLine(EffectiveNPCName, $"I have a mission for you: {mmoQuest.questTitle}.") };

                StartScreenplayDialogue(lines, () =>
                {
                    QuestManager.Instance.AcceptQuest(mmoQuest.questID);
                    questActive = true;
                    UpdateIndicator();
                });
                return;
            }
        }

        // Case 4: Ambient / Lore / Completed dialogue
        List<QuestDialogueLine> fallbackLines = new List<QuestDialogueLine>();
        if (requiresOtherNPC && GameManager.Instance != null && !GameManager.Instance.HasTalkedTo(requiredNPCName))
        {
            if (conditionalDialogueLines != null && conditionalDialogueLines.Length > 0)
            {
                foreach (var line in conditionalDialogueLines)
                    fallbackLines.Add(new QuestDialogueLine(EffectiveNPCName, line));
            }
        }
        else if (mmoQuest != null && mmoQuest.state == QuestState.Completed && mmoQuest.completedDialogue != null && mmoQuest.completedDialogue.Count > 0)
        {
            fallbackLines = mmoQuest.completedDialogue;
        }
        else if (dialogueLines != null && dialogueLines.Length > 0)
        {
            foreach (var line in dialogueLines)
                fallbackLines.Add(new QuestDialogueLine(EffectiveNPCName, line));
        }
        else
        {
            fallbackLines.Add(new QuestDialogueLine(EffectiveNPCName, "Greetings, traveler."));
        }

        StartScreenplayDialogue(fallbackLines, () =>
        {
            if (GameManager.Instance != null)
                GameManager.Instance.MarkNPCAsTalked(EffectiveNPCName);
            UpdateIndicator();
        });
    }

    private void StartScreenplayDialogue(List<QuestDialogueLine> lines, System.Action onComplete)
    {
        if (lines == null || lines.Count == 0)
        {
            onComplete?.Invoke();
            return;
        }

        activeScreenplayLines = new List<QuestDialogueLine>(lines);
        dialogueIndex = 0;
        onDialogueFinishCallback = onComplete;

        if (playerController != null) playerController.DisableMovement();
        Time.timeScale = 0.00001f;

        BindUIReferences();
        if (dialogueBox != null) dialogueBox.SetActive(true);

        DisplayCurrentScreenplayLine();
    }

    private void DisplayCurrentScreenplayLine()
    {
        if (dialogueIndex < 0 || dialogueIndex >= activeScreenplayLines.Count)
        {
            EndScreenplayDialogue();
            return;
        }

        var line = activeScreenplayLines[dialogueIndex];

        if (characterNameText != null)
            characterNameText.text = line.speaker;

        if (dialogueText != null)
            dialogueText.text = line.text;

        if (characterArtImage != null)
        {
            if (string.Equals(line.speaker, "Player", System.StringComparison.OrdinalIgnoreCase))
            {
                characterArtImage.gameObject.SetActive(false);
            }
            else
            {
                if (npcSprite != null)
                {
                    characterArtImage.sprite = npcSprite;
                    characterArtImage.gameObject.SetActive(true);
                }
                else
                {
                    characterArtImage.gameObject.SetActive(false);
                }
            }
        }
    }

    private void NextScreenplayLine()
    {
        dialogueIndex++;
        if (dialogueIndex < activeScreenplayLines.Count)
        {
            DisplayCurrentScreenplayLine();
        }
        else
        {
            EndScreenplayDialogue();
        }
    }

    private void EndScreenplayDialogue()
    {
        HideDialogue();
        var cb = onDialogueFinishCallback;
        onDialogueFinishCallback = null;
        cb?.Invoke();
    }

    public void HideDialogue()
    {
        if (dialogueBox != null) dialogueBox.SetActive(false);
        if (characterArtImage != null) characterArtImage.gameObject.SetActive(false);

        if (playerController != null) playerController.EnableMovement();
        Time.timeScale = 1f;
    }

    #region Legacy & Inspector Compatibility
    private void UpdateQuestUI()
    {
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.UpdateHUD();
        }
    }

    public void RegisterEnemyKill()
    {
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.AdvanceObjective(QuestType.EnemyExtermination, "", 1);
        }
    }

    public void RegisterDamage(int damage)
    {
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.AdvanceObjective(QuestType.RequirementQuest, "Dummy", damage);
        }
    }

    public void StartNextQuestInChain()
    {
        UpdateIndicator();
    }

    public void ForceConditionalDialogue(System.Action onComplete = null)
    {
        StartMMOInteraction();
        onComplete?.Invoke();
    }
    #endregion
}
