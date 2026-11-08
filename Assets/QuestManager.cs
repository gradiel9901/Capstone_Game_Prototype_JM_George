using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

public class QuestManager : MonoBehaviour
{
    private static QuestManager instance;
    public static QuestManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<QuestManager>();
                if (instance == null)
                {
                    GameObject go = new GameObject("QuestManager");
                    instance = go.AddComponent<QuestManager>();
                    DontDestroyOnLoad(go);
                }
            }
            return instance;
        }
    }

    [Header("Global UI References (Auto-Bound)")]
    public GameObject dialogueBox;
    public TMP_Text dialogueText;
    public TMP_Text characterNameText;
    public Image characterArtImage;
    public TMP_Text questTitleText;
    public TMP_Text questDescriptionText;

    private QuestDatabaseSO database;
    private Dictionary<string, QuestDefinition> runtimeQuests = new Dictionary<string, QuestDefinition>();
    private HashSet<string> completedQuests = new HashSet<string>();

    public event Action<string, QuestState> OnQuestStateChanged;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        if (transform.parent == null)
        {
            DontDestroyOnLoad(gameObject);
        }

        InitializeDatabase();
    }

    private void Start()
    {
        BindUI();
        UpdateHUD();
    }

    public void BindUI()
    {
        if (dialogueBox == null || dialogueText == null || questTitleText == null || questDescriptionText == null)
        {
            GameObject canvas = GameObject.Find("UICanvas");
            if (canvas != null)
            {
                Transform[] allChildren = canvas.GetComponentsInChildren<Transform>(true);
                foreach (Transform t in allChildren)
                {
                    if (dialogueBox == null && t.name == "DialogueBox") dialogueBox = t.gameObject;
                    if (dialogueText == null && t.name == "DialogueText") dialogueText = t.GetComponent<TMP_Text>();
                    if (characterNameText == null && t.name == "CharacterNameText") characterNameText = t.GetComponent<TMP_Text>();
                    if (characterArtImage == null && t.name == "CharacterArtImage") characterArtImage = t.GetComponent<Image>();
                    if (questTitleText == null && t.name == "QuestTitleText") questTitleText = t.GetComponent<TMP_Text>();
                    if (questDescriptionText == null && t.name == "QuestDescriptionText") questDescriptionText = t.GetComponent<TMP_Text>();
                }
            }
        }
    }

    private void InitializeDatabase()
    {
        database = QuestDatabaseSO.LoadDatabase();
        if (database != null && database.quests != null)
        {
            foreach (var q in database.quests)
            {
                if (q == null || string.IsNullOrEmpty(q.questID)) continue;

                var clone = JsonUtility.FromJson<QuestDefinition>(JsonUtility.ToJson(q));

                if (string.IsNullOrEmpty(clone.prerequisiteQuestID))
                {
                    clone.state = QuestState.Available;
                }
                else
                {
                    clone.state = QuestState.Locked;
                }

                runtimeQuests[clone.questID] = clone;
            }
        }
    }

    public QuestDefinition GetQuest(string questID)
    {
        if (string.IsNullOrEmpty(questID)) return null;
        if (runtimeQuests.TryGetValue(questID, out var q)) return q;
        return null;
    }

    public QuestState GetQuestState(string questID)
    {
        var q = GetQuest(questID);
        return q != null ? q.state : QuestState.Locked;
    }

    public bool IsQuestCompleted(string questID)
    {
        return completedQuests.Contains(questID) || GetQuestState(questID) == QuestState.Completed;
    }

    public bool IsAnyQuestCompletedForNPC(string npcName)
    {
        if (string.IsNullOrEmpty(npcName)) return true;
        foreach (var q in runtimeQuests.Values)
        {
            if (string.Equals(q.startNpcID, npcName, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(q.turnInNpcID, npcName, StringComparison.OrdinalIgnoreCase))
            {
                if (q.state == QuestState.Completed || completedQuests.Contains(q.questID))
                    return true;
            }
        }
        return false;
    }

    public List<QuestDefinition> GetAllQuests()
    {
        return new List<QuestDefinition>(runtimeQuests.Values);
    }

    public QuestDefinition GetActiveOrAvailableQuestForNPC(string npcName)
    {
        if (string.IsNullOrEmpty(npcName)) return null;

        // 1. ReadyToTurnIn
        foreach (var q in runtimeQuests.Values)
        {
            if (q.state == QuestState.ReadyToTurnIn && string.Equals(q.turnInNpcID, npcName, StringComparison.OrdinalIgnoreCase))
                return q;
        }

        // 2. InProgress
        foreach (var q in runtimeQuests.Values)
        {
            if (q.state == QuestState.InProgress && string.Equals(q.turnInNpcID, npcName, StringComparison.OrdinalIgnoreCase))
                return q;
        }

        // 3. Available
        foreach (var q in runtimeQuests.Values)
        {
            if (q.state == QuestState.Available && string.Equals(q.startNpcID, npcName, StringComparison.OrdinalIgnoreCase))
                return q;
        }

        // 4. Standalone fallback (Scene testing in isolation)
        foreach (var q in runtimeQuests.Values)
        {
            if (q.state == QuestState.Locked && string.Equals(q.startNpcID, npcName, StringComparison.OrdinalIgnoreCase))
            {
                q.state = QuestState.Available;
                return q;
            }
        }

        return null;
    }

    public void AcceptQuest(string questID)
    {
        var q = GetQuest(questID);
        if (q == null) return;

        q.state = QuestState.InProgress;
        q.currentAmount = 0;

        if (q.questType == QuestType.ScoreChecker && EconomyManager.Instance != null)
        {
            q.currentAmount = EconomyManager.Instance.currentGold;
            if (q.currentAmount >= q.requiredAmount)
            {
                q.state = QuestState.ReadyToTurnIn;
            }
        }
        else if (q.questType == QuestType.TalkToNPC && GameManager.Instance != null)
        {
            if (GameManager.Instance.HasTalkedTo(q.targetIdentifier))
            {
                q.state = QuestState.ReadyToTurnIn;
            }
        }

        UpdateHUD();
        OnQuestStateChanged?.Invoke(questID, q.state);
        Debug.Log($"[QuestManager] Accepted Quest: {q.questTitle} ({q.questID}) - State: {q.state}");
    }

    public void AdvanceObjective(QuestType type, string targetIdentifier, int amount)
    {
        bool changed = false;

        foreach (var q in runtimeQuests.Values)
        {
            if (q.state != QuestState.InProgress) continue;
            if (q.questType != type) continue;

            if (type == QuestType.ScoreChecker)
            {
                q.currentAmount = amount;
                if (q.currentAmount >= q.requiredAmount)
                {
                    q.state = QuestState.ReadyToTurnIn;
                    changed = true;
                }
            }
            else if (type == QuestType.RequirementQuest)
            {
                q.currentAmount += amount;
                if (q.currentAmount >= q.requiredAmount)
                {
                    q.state = QuestState.ReadyToTurnIn;
                    changed = true;
                }
            }
            else if (type == QuestType.EnemyExtermination)
            {
                q.currentAmount += amount;
                if (q.currentAmount >= q.requiredAmount)
                {
                    q.state = QuestState.ReadyToTurnIn;
                    changed = true;
                }
            }
            else if (type == QuestType.TalkToNPC)
            {
                if (string.IsNullOrEmpty(targetIdentifier) || string.Equals(q.targetIdentifier, targetIdentifier, StringComparison.OrdinalIgnoreCase))
                {
                    q.currentAmount = 1;
                    q.state = QuestState.ReadyToTurnIn;
                    changed = true;
                }
            }

            if (changed)
            {
                OnQuestStateChanged?.Invoke(q.questID, q.state);
            }
        }

        UpdateHUD();
    }

    public void CompleteQuest(string questID)
    {
        var q = GetQuest(questID);
        if (q == null || q.state == QuestState.Completed) return;

        q.state = QuestState.Completed;
        completedQuests.Add(questID);

        if (q.goldReward > 0 && EconomyManager.Instance != null)
        {
            EconomyManager.Instance.AddGold(q.goldReward);
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.MarkQuestCompleted(q.turnInNpcID);
            GameManager.Instance.MarkQuestCompleted(q.startNpcID);
        }

        if (!string.IsNullOrEmpty(q.nextQuestInChainID))
        {
            var nextQ = GetQuest(q.nextQuestInChainID);
            if (nextQ != null && nextQ.state == QuestState.Locked)
            {
                nextQ.state = QuestState.Available;
                OnQuestStateChanged?.Invoke(nextQ.questID, nextQ.state);
                Debug.Log($"[QuestManager] Unlocked Next Chain Quest: {nextQ.questTitle} ({nextQ.questID})");
            }
        }

        UpdateHUD();
        OnQuestStateChanged?.Invoke(questID, QuestState.Completed);
        Debug.Log($"[QuestManager] Completed Quest: {q.questTitle} ({q.questID})!");
    }

    public void UpdateHUD()
    {
        BindUI();
        if (questTitleText == null || questDescriptionText == null) return;

        QuestDefinition activeQuest = null;
        foreach (var q in runtimeQuests.Values)
        {
            if (q.state == QuestState.ReadyToTurnIn)
            {
                activeQuest = q;
                break;
            }
            if (q.state == QuestState.InProgress && activeQuest == null)
            {
                activeQuest = q;
            }
        }

        if (activeQuest != null)
        {
            questTitleText.text = $"Quest: {activeQuest.questTitle}";
            if (activeQuest.state == QuestState.ReadyToTurnIn)
            {
                questDescriptionText.text = $"{activeQuest.GetFormattedObjective()}\n[READY] Return to {activeQuest.turnInNpcID}!";
            }
            else
            {
                questDescriptionText.text = $"{activeQuest.GetFormattedObjective()}\n({activeQuest.currentAmount}/{activeQuest.requiredAmount})";
            }
        }
        else
        {
            questTitleText.text = "";
            questDescriptionText.text = "";
        }
    }
}
