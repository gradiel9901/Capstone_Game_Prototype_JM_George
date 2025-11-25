using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public enum QuestType { None, EnemyExtermination, TalkToNPC, RequirementQuest }

[System.Serializable]
public class QuestData
{
    [Header("Quest Info")]
    public string questTitle;
    [TextArea(2, 4)] public string questDescription;
    public QuestType questType = QuestType.None;
    public string targetNPCName;
    public int requiredKills;
    public int requiredDamage;

    [Header("Quest Dialogue")]
    [TextArea(2, 5)] public string[] startDialogue;
    [TextArea(2, 5)] public string[] completeDialogue;

    [Header("Quest Triggers")]
    public GameObject[] objectsToActivate;
    public GameObject[] objectsToDeactivate;
}

public class NPCInteraction : MonoBehaviour
{
    [Header("UI References")]
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

    // ✅ Input System
    private PlayerControls controls;
    private InputAction interactAction;

    // ✅ Public accessors
    public bool IsDialogueActive => dialogueBox != null && dialogueBox.activeSelf;
    public bool IsQuestCompleted => questCompleted;
    public bool HasActiveQuest => questActive;
    public string NPCName => npcName;
    public QuestType QuestType => (quests != null && quests.Length > 0 && currentQuestIndex < quests.Length)
        ? quests[currentQuestIndex].questType
        : QuestType.None;

    private void Awake()
    {
        if (controls == null)
            controls = new PlayerControls();
    }

    private void OnEnable()
    {
        if (controls == null)
            controls = new PlayerControls();

        controls.Enable();

        // ✅ Safe subscribe
        if (interactAction == null)
            interactAction = controls.Interaction.NPCInteractionButton;

        interactAction.performed -= OnInteract; // avoid duplicates
        interactAction.performed += OnInteract;
    }

    private void OnDisable()
    {
        // ✅ Safe unsubscribe
        if (interactAction != null)
            interactAction.performed -= OnInteract;

        if (controls != null)
            controls.Disable();
    }

    private void OnDestroy()
    {
        // ✅ Double-safe unsubscribe for scene unloads
        if (interactAction != null)
            interactAction.performed -= OnInteract;

        if (GameManager.Instance != null)
            GameManager.Instance.UnregisterQuestGiver(this);
    }

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (player != null)
            playerController = player.GetComponent<PlayerController>();

        if (dialogueBox != null) dialogueBox.SetActive(false);
        if (characterArtImage != null) characterArtImage.gameObject.SetActive(false);
        if (questTitleText != null) questTitleText.text = "";
        if (questDescriptionText != null) questDescriptionText.text = "";

        if (GameManager.Instance != null)
            GameManager.Instance.RegisterQuestGiver(this);
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

        if (questActive)
            UpdateQuestUI();
    }

    private void OnInteract(InputAction.CallbackContext context)
    {
        // ✅ Prevent access to destroyed NPCs or missing objects
        if (this == null || gameObject == null)
            return;

        if (!isPlayerInRange || dialogueBox == null)
            return;

        if (dialogueBox.activeSelf)
            NextLine();
        else
            StartDialogue();
    }

    private void StartDialogue()
    {
        if (playerController != null)
            playerController.DisableMovement();

        bool requiredTalked = GameManager.Instance.HasTalkedTo(requiredNPCName);
        bool requiredQuestCompleted = GameManager.Instance.IsQuestCompletedFrom(requiredNPCName);

        if (requiresOtherNPC && (!requiredTalked || !requiredQuestCompleted))
            activeDialogue = conditionalDialogueLines;
        else
            activeDialogue = dialogueLines;

        if (activeDialogue == null || activeDialogue.Length == 0)
        {
            dialogueText.text = "...";
            return;
        }

        dialogueIndex = 0;
        dialogueBox.SetActive(true);
        characterNameText.text = npcName;
        dialogueText.text = activeDialogue[dialogueIndex];

        if (characterArtImage != null)
        {
            characterArtImage.sprite = npcSprite;
            characterArtImage.gameObject.SetActive(true);
        }

        if (!requiresOtherNPC || GameManager.Instance.HasTalkedTo(requiredNPCName))
            GameManager.Instance.MarkNPCAsTalked(npcName);

        if (isQuestGiver && !questActive && !questCompleted)
            StartQuest();
    }

    private void NextLine()
    {
        dialogueIndex++;

        if (activeDialogue == null || dialogueIndex >= activeDialogue.Length)
            HideDialogue();
        else
            dialogueText.text = activeDialogue[dialogueIndex];
    }

    private void HideDialogue()
    {
        dialogueBox.SetActive(false);
        if (characterArtImage != null) characterArtImage.gameObject.SetActive(false);
        if (playerController != null) playerController.EnableMovement();
    }

    private void StartQuest()
    {
        if (!isQuestGiver || quests == null || quests.Length == 0) return;
        if (currentQuestIndex >= quests.Length) return;

        QuestData currentQuest = quests[currentQuestIndex];
        questActive = true;
        questCompleted = false;

        if (questTitleText != null)
            questTitleText.text = $"Quest: {currentQuest.questTitle}";
        if (questDescriptionText != null)
            questDescriptionText.text = currentQuest.questDescription;

        if (currentQuest.startDialogue != null && currentQuest.startDialogue.Length > 0)
        {
            activeDialogue = currentQuest.startDialogue;
            dialogueIndex = 0;
            dialogueBox.SetActive(true);
            dialogueText.text = activeDialogue[dialogueIndex];
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
        GameManager.Instance.MarkQuestCompleted(npcName);

        if (currentQuest.completeDialogue != null && currentQuest.completeDialogue.Length > 0)
        {
            activeDialogue = currentQuest.completeDialogue;
            dialogueIndex = 0;
            dialogueBox.SetActive(true);
            dialogueText.text = activeDialogue[dialogueIndex];
        }

        foreach (var obj in currentQuest.objectsToDeactivate)
            if (obj != null) obj.SetActive(false);

        foreach (var obj in currentQuest.objectsToActivate)
            if (obj != null) obj.SetActive(true);

        currentQuestIndex++;

        if (currentQuestIndex < quests.Length)
            StartQuest();
        else
            StartCoroutine(ClearQuestUIAfterDelay(2f));
    }

    private System.Collections.IEnumerator ClearQuestUIAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (questTitleText != null) questTitleText.text = "";
        if (questDescriptionText != null) questDescriptionText.text = "";
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
            dialogueBox.SetActive(true);
            characterArtImage.sprite = npcSprite;
            characterArtImage.gameObject.SetActive(true);
            characterNameText.text = npcName;
            dialogueText.text = activeDialogue[dialogueIndex];
            Debug.Log($"{npcName} forced to show conditional dialogue.");
            onComplete?.Invoke();
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }
}
