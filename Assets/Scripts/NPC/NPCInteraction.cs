using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Collections;

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
    [Tooltip("How long (in seconds) each line of the 'Complete Dialogue' stays on screen.")]
    [SerializeField] private float completionTextDuration = 4f;
    [SerializeField, TextArea(2, 5)] private string[] dialogueLines;

    [Header("Conditional NPC Settings")]
    [SerializeField] private bool requiresOtherNPC = false;
    [SerializeField] private string requiredNPCName;
    [SerializeField, TextArea(2, 5)] private string[] conditionalDialogueLines;

    [Header("Interaction Settings")]
    [SerializeField] private float interactionRange = 2f;

    [Header("Quest Chain Settings")]
    [SerializeField] private bool isQuestGiver = false;
    [SerializeField] private QuestData[] quests;

    private int currentQuestIndex = 0;
    private int currentKills = 0;
    private int currentDamage = 0;
    private bool questActive = false;
    private bool questCompleted = false;
    private Transform player;
    private PlayerController playerController;
    private int dialogueIndex = 0;
    private string[] activeDialogue;
    private bool isPlayerInRange = false;

    // Choice system variables
    private bool waitingForChoice = false;
    private QuestChoice[] currentChoices;
    private bool isReadingResult = false;
    private int pendingQuestStart = -1;

    private PlayerControls controls;
    private InputAction interactAction;

    public bool IsDialogueActive => dialogueBox != null && dialogueBox.activeSelf;
    public bool IsQuestCompleted => questCompleted;
    public bool HasActiveQuest => questActive;
    public string NPCName => npcName;

    public QuestType CurrentQuestType => (quests != null && quests.Length > 0 && currentQuestIndex < quests.Length)
        ? quests[currentQuestIndex].questType
        : QuestType.None;

    private void Awake()
    {
        if (controls == null) controls = new PlayerControls();
    }

    private void OnEnable()
    {
        if (controls == null) controls = new PlayerControls();
        controls.Enable();
        if (interactAction == null) interactAction = controls.Interaction.NPCInteractionButton;
        interactAction.performed -= OnInteract;
        interactAction.performed += OnInteract;
    }

    private void OnDisable()
    {
        if (interactAction != null) interactAction.performed -= OnInteract;
        if (controls != null) controls.Disable();
    }

    // --- SAFETY CLEANUP ---
    private void OnDestroy()
    {
        if (interactAction != null) interactAction.performed -= OnInteract;
        if (GameManager.Instance != null) GameManager.Instance.UnregisterQuestGiver(this);

        // Force time back to normal on destroy
        Time.timeScale = 1f;

        if (playerController != null) playerController.EnableMovement();
        if (questTitleText != null) questTitleText.text = "";
        if (questDescriptionText != null) questDescriptionText.text = "";
        if (dialogueBox != null) dialogueBox.SetActive(false);
    }

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (player != null) playerController = player.GetComponent<PlayerController>();

        // 1. Try QuestManager first
        if (QuestManager.Instance != null && QuestManager.Instance.dialogueBox != null)
        {
            dialogueBox = QuestManager.Instance.dialogueBox;
            dialogueText = QuestManager.Instance.dialogueText;
            characterNameText = QuestManager.Instance.characterNameText;
            characterArtImage = QuestManager.Instance.characterArtImage;
            questTitleText = QuestManager.Instance.questTitleText;
            questDescriptionText = QuestManager.Instance.questDescriptionText;
        }
        else
        {
            // 2. Failsafe: Nuclear Search
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

        // Initial Reset
        if (dialogueBox != null) dialogueBox.SetActive(false);
        if (characterArtImage != null) characterArtImage.gameObject.SetActive(false);
        if (questTitleText != null) questTitleText.text = "";
        if (questDescriptionText != null) questDescriptionText.text = "";

        if (GameManager.Instance != null) GameManager.Instance.RegisterQuestGiver(this);
    }

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

        if (waitingForChoice)
        {
            HandleChoiceInput();
            return;
        }

        if (questActive)
            UpdateQuestUI();
    }

    private void HandleChoiceInput()
    {
        int selected = -1;
        if (Keyboard.current != null)
        {
            if (Keyboard.current.zKey.wasPressedThisFrame) selected = 0;
            if (Keyboard.current.xKey.wasPressedThisFrame) selected = 1;
            if (Keyboard.current.cKey.wasPressedThisFrame) selected = 2;
            if (Keyboard.current.vKey.wasPressedThisFrame) selected = 3;
            if (Keyboard.current.bKey.wasPressedThisFrame) selected = 4;
            if (Keyboard.current.nKey.wasPressedThisFrame) selected = 5;
            if (Keyboard.current.mKey.wasPressedThisFrame) selected = 6;
        }

        if (selected >= 0 && currentChoices != null && selected < currentChoices.Length)
        {
            ApplyChoice(selected);
        }
    }

    private void OnInteract(InputAction.CallbackContext context)
    {
        if (this == null || gameObject == null) return;
        if (!isPlayerInRange || dialogueBox == null) return;

        if (dialogueBox.activeSelf)
            NextLine();
        else
            StartDialogue();
    }

    private void StartDialogue()
    {
        if (playerController != null) playerController.DisableMovement();

        // ✅ FIX: Use a tiny number instead of 0 to prevent Unity Input System bug
        Time.timeScale = 0.00001f;

        bool requiredTalked = GameManager.Instance.HasTalkedTo(requiredNPCName);
        bool requiredQuestCompleted = GameManager.Instance.IsQuestCompletedFrom(requiredNPCName);

        if (requiresOtherNPC && (!requiredTalked || !requiredQuestCompleted))
            activeDialogue = conditionalDialogueLines;
        else
            activeDialogue = dialogueLines;

        // Check Score Quest logic
        if (questActive && currentQuestIndex < quests.Length)
        {
            QuestData q = quests[currentQuestIndex];
            if (q.questType == QuestType.ScoreChecker && EconomyManager.Instance != null)
            {
                if (EconomyManager.Instance.currentGold < q.requiredGold)
                {
                    if (q.notMetDialogue != null && q.notMetDialogue.Length > 0)
                    {
                        activeDialogue = q.notMetDialogue;
                    }
                }
            }
        }

        isReadingResult = false;
        pendingQuestStart = -1;

        if (activeDialogue == null || activeDialogue.Length == 0)
        {
            if (dialogueText != null) dialogueText.text = "...";
            return;
        }

        dialogueIndex = 0;
        SetupDialogueUI();
        if (dialogueText != null) dialogueText.text = activeDialogue[dialogueIndex];

        if (!requiresOtherNPC || GameManager.Instance.HasTalkedTo(requiredNPCName))
            GameManager.Instance.MarkNPCAsTalked(npcName);
    }

    private void SetupDialogueUI()
    {
        if (dialogueBox != null) dialogueBox.SetActive(true);
        if (characterNameText != null) characterNameText.text = npcName;
        if (characterArtImage != null)
        {
            characterArtImage.sprite = npcSprite;
            characterArtImage.gameObject.SetActive(true);
        }
    }

    private void NextLine()
    {
        if (waitingForChoice) return;

        dialogueIndex++;

        if (activeDialogue == null || dialogueIndex >= activeDialogue.Length)
        {
            if (isReadingResult)
            {
                if (pendingQuestStart != -1)
                {
                    currentQuestIndex = pendingQuestStart;
                    StartQuest();
                    pendingQuestStart = -1;
                    isReadingResult = false;
                }
                else
                    HideDialogue();
                return;
            }

            if (isQuestGiver && !questActive && !questCompleted && currentQuestIndex < quests.Length)
            {
                QuestData q = quests[currentQuestIndex];
                if (q.offersChoices)
                    MaybeShowQuestChoices();
                else
                    StartQuest();
                return;
            }

            HideDialogue();
        }
        else
        {
            if (dialogueText != null) dialogueText.text = activeDialogue[dialogueIndex];
        }
    }

    private void MaybeShowQuestChoices()
    {
        if (isQuestGiver && currentQuestIndex < quests.Length)
        {
            QuestData q = quests[currentQuestIndex];
            if (q.offersChoices && q.choices != null && q.choices.Length > 0)
            {
                ShowChoicesInDialogue(q.choices);
                return;
            }
        }
        HideDialogue();
    }

    private void ShowChoicesInDialogue(QuestChoice[] choices)
    {
        waitingForChoice = true;
        currentChoices = choices;
        string output = "Choose:\n\n";
        string[] keys = { "[Z]", "[X]", "[C]", "[V]", "[B]", "[N]", "[M]" };

        int count = Mathf.Min(choices.Length, keys.Length);
        for (int i = 0; i < count; i++)
            output += $"{keys[i]} {choices[i].buttonLabel}\n";

        if (choices.Length > keys.Length)
            for (int i = keys.Length; i < choices.Length; i++)
                output += $"[?] {choices[i].buttonLabel}\n";

        if (dialogueText != null) dialogueText.text = output;
        if (playerController != null) playerController.DisableMovement();
    }

    private void ApplyChoice(int index)
    {
        waitingForChoice = false;

        if (currentChoices == null || index < 0 || index >= currentChoices.Length)
            return;

        var chosen = currentChoices[index];
        activeDialogue = chosen.resultDialogue ?? new string[] { "..." };

        if (chosen.triggersQuest)
            pendingQuestStart = chosen.questIndexToTrigger;
        else
            pendingQuestStart = -1;

        isReadingResult = true;
        dialogueIndex = 0;

        if (dialogueText != null)
            dialogueText.text = activeDialogue[dialogueIndex];
    }

    private void HideDialogue()
    {
        if (dialogueBox != null) dialogueBox.SetActive(false);
        if (characterArtImage != null) characterArtImage.gameObject.SetActive(false);
        waitingForChoice = false;
        currentChoices = null;
        isReadingResult = false;
        pendingQuestStart = -1;
        if (playerController != null) playerController.EnableMovement();

        Time.timeScale = 1f; // Resume Game
    }

    private void StartQuest()
    {
        if (!isQuestGiver || quests == null || quests.Length == 0) return;
        if (currentQuestIndex >= quests.Length) return;

        QuestData currentQuest = quests[currentQuestIndex];
        questActive = true;
        questCompleted = false;
        currentKills = 0;
        currentDamage = 0;

        if (questTitleText != null)
            questTitleText.text = $"Quest: {currentQuest.questTitle}";
        if (questDescriptionText != null)
            questDescriptionText.text = currentQuest.questDescription;

        UpdateQuestUI();

        if (currentQuest.startDialogue != null && currentQuest.startDialogue.Length > 0)
        {
            activeDialogue = currentQuest.startDialogue;
            dialogueIndex = 0;
            if (dialogueBox != null) dialogueBox.SetActive(true);
            if (dialogueText != null) dialogueText.text = activeDialogue[dialogueIndex];
            if (characterNameText != null) characterNameText.text = npcName;
            if (characterArtImage != null)
            {
                characterArtImage.sprite = npcSprite;
                characterArtImage.gameObject.SetActive(true);
            }
        }
        else
        {
            HideDialogue();
        }

        Debug.Log($"{npcName} started quest: {currentQuest.questTitle}");
    }

    private void UpdateQuestUI()
    {
        if (!questActive || questCompleted || currentQuestIndex >= quests.Length) return;

        QuestData currentQuest = quests[currentQuestIndex];
        string baseDesc = currentQuest.questDescription;

        switch (currentQuest.questType)
        {
            case QuestType.EnemyExtermination:
                if (questDescriptionText != null)
                    questDescriptionText.text = $"{baseDesc}\n\nDefeat {currentQuest.requiredKills} enemies. ({currentKills}/{currentQuest.requiredKills})";
                if (currentKills >= currentQuest.requiredKills)
                    CompleteQuest();
                break;

            case QuestType.TalkToNPC:
                if (questDescriptionText != null)
                    questDescriptionText.text = $"{baseDesc}\n\nTalk to {currentQuest.targetNPCName}.";
                if (GameManager.Instance.HasTalkedTo(currentQuest.targetNPCName))
                    CompleteQuest();
                break;

            case QuestType.RequirementQuest:
                if (questDescriptionText != null)
                    questDescriptionText.text = $"{baseDesc}\n\nDeal {currentQuest.requiredDamage} total damage. ({currentDamage}/{currentQuest.requiredDamage})";
                if (currentDamage >= currentQuest.requiredDamage)
                    CompleteQuest();
                break;

            case QuestType.ScoreChecker:
                if (EconomyManager.Instance != null)
                {
                    int currentGold = EconomyManager.Instance.currentGold;

                    // Don't show "0/10", just show description (Hidden mechanic)
                    if (questDescriptionText != null)
                        questDescriptionText.text = baseDesc;

                    if (currentGold >= currentQuest.requiredGold)
                        CompleteQuest();
                }
                break;
        }
    }

    public void RegisterEnemyKill()
    {
        if (questActive && currentQuestIndex < quests.Length && quests[currentQuestIndex].questType == QuestType.EnemyExtermination)
        {
            currentKills++;
            UpdateQuestUI();
        }
    }

    public void RegisterDamage(int damage)
    {
        if (questActive && currentQuestIndex < quests.Length && quests[currentQuestIndex].questType == QuestType.RequirementQuest)
        {
            currentDamage += damage;
            UpdateQuestUI();
        }
    }

    private void CompleteQuest()
    {
        if (currentQuestIndex >= quests.Length) return;

        QuestData currentQuest = quests[currentQuestIndex];
        questCompleted = true;
        questActive = false;

        if (questDescriptionText != null)
            questDescriptionText.text = $"{currentQuest.questDescription}\n\n✅ Quest Completed!";

        Debug.Log($"{npcName}: Quest completed!");
        if (GameManager.Instance != null) GameManager.Instance.MarkQuestCompleted(npcName);

        if (currentQuest.objectsToDeactivate != null)
        {
            foreach (var obj in currentQuest.objectsToDeactivate)
                if (obj != null) obj.SetActive(false);
        }

        if (currentQuest.objectsToActivate != null)
        {
            foreach (var obj in currentQuest.objectsToActivate)
                if (obj != null) obj.SetActive(true);
        }

        if (playerController != null) playerController.DisableMovement();

        // ✅ FIX: Use almost-zero to prevent Unity errors
        Time.timeScale = 0.00001f;

        StartCoroutine(PlayCompletionSequence(currentQuest));
    }

    private IEnumerator PlayCompletionSequence(QuestData quest)
    {
        if (quest.completeDialogue != null && quest.completeDialogue.Length > 0)
        {
            if (dialogueBox != null) dialogueBox.SetActive(true);
            if (characterNameText != null) characterNameText.text = npcName;
            if (characterArtImage != null)
            {
                characterArtImage.sprite = npcSprite;
                characterArtImage.gameObject.SetActive(true);
            }

            foreach (string line in quest.completeDialogue)
            {
                if (dialogueText != null) dialogueText.text = line;
                yield return new WaitForSecondsRealtime(completionTextDuration);
            }
        }
        else
        {
            yield return new WaitForSecondsRealtime(2f);
        }

        if (questTitleText != null) questTitleText.text = "";
        if (questDescriptionText != null) questDescriptionText.text = "";

        if (dialogueBox != null) dialogueBox.SetActive(false);
        if (characterArtImage != null) characterArtImage.gameObject.SetActive(false);

        if (playerController != null) playerController.EnableMovement();

        Time.timeScale = 1f;

        if (quest.hasNextQuest)
        {
            currentQuestIndex = quest.nextQuestIndex;
        }
        else
        {
            currentQuestIndex = quests.Length + 99;
        }
    }

    public void StartNextQuestInChain()
    {
        if (isQuestGiver && currentQuestIndex < quests.Length && !questActive)
            StartQuest();
    }

    public void ForceConditionalDialogue(System.Action onComplete = null)
    {
        if (requiresOtherNPC && !GameManager.Instance.HasTalkedTo(requiredNPCName))
        {
            activeDialogue = conditionalDialogueLines;
            dialogueIndex = 0;
            if (dialogueBox != null) dialogueBox.SetActive(true);
            if (characterArtImage != null)
            {
                characterArtImage.sprite = npcSprite;
                characterArtImage.gameObject.SetActive(true);
            }
            if (characterNameText != null) characterNameText.text = npcName;
            if (dialogueText != null) dialogueText.text = activeDialogue[dialogueIndex];
            onComplete?.Invoke();
        }
    }
}