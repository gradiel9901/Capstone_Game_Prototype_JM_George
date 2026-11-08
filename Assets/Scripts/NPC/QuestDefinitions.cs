using System;
using System.Collections.Generic;
using UnityEngine;

public enum QuestCategory
{
    MainStory,
    SideQuest
}

public enum QuestState
{
    Locked,
    Available,
    InProgress,
    ReadyToTurnIn,
    Completed
}

public enum QuestType
{
    None,
    EnemyExtermination,
    TalkToNPC,
    RequirementQuest,
    ScoreChecker
}

[System.Serializable]
public class QuestDialogueLine
{
    public string speaker;
    [TextArea(2, 5)] public string text;

    public QuestDialogueLine() { }
    public QuestDialogueLine(string speaker, string text)
    {
        this.speaker = speaker;
        this.text = text;
    }
}

[System.Serializable]
public class DialogueLine : QuestDialogueLine
{
    public DialogueLine() { }
    public DialogueLine(string speaker, string text) : base(speaker, text) { }
}

[System.Serializable]
public class QuestDefinition
{
    [Header("Core Identifiers")]
    public string questID;
    public string questTitle;
    [TextArea(2, 4)] public string questDescription;
    public QuestCategory category = QuestCategory.MainStory;
    public string sceneName;

    [Header("MMO Chain Sequencing")]
    public string prerequisiteQuestID;
    public string nextQuestInChainID;

    [Header("Relay NPC Assignments")]
    public string startNpcID;
    public string turnInNpcID;

    [Header("Objective & Reward")]
    public QuestType questType = QuestType.None;
    public string targetID;
    public int requiredAmount;
    public int goldReward;

    [Header("Screenplay Dialogues")]
    public List<QuestDialogueLine> offerDialogue = new List<QuestDialogueLine>();
    public List<QuestDialogueLine> inProgressDialogue = new List<QuestDialogueLine>();
    public List<QuestDialogueLine> turnInDialogue = new List<QuestDialogueLine>();
    public List<QuestDialogueLine> completedDialogue = new List<QuestDialogueLine>();

    [NonSerialized] public QuestState state = QuestState.Locked;
    [NonSerialized] public int currentProgress = 0;

    public int currentAmount { get => currentProgress; set => currentProgress = value; }
    public string targetIdentifier { get => targetID; set => targetID = value; }
    public bool IsSatisfied => currentProgress >= requiredAmount;

    public string GetFormattedObjective()
    {
        switch (questType)
        {
            case QuestType.RequirementQuest:
                return $"Deal {requiredAmount} damage to {targetID}";
            case QuestType.EnemyExtermination:
                return $"Defeat {requiredAmount} {targetID}";
            case QuestType.TalkToNPC:
                return $"Talk to {targetID}";
            case QuestType.ScoreChecker:
                return $"Collect {requiredAmount} {targetID}";
            default:
                return questDescription;
        }
    }
}

[CreateAssetMenu(fileName = "QuestDatabase", menuName = "Quests/Quest Database")]
public class QuestDatabaseSO : ScriptableObject
{
    public List<QuestDefinition> quests = new List<QuestDefinition>();

    private static QuestDatabaseSO cachedInstance;

    public static QuestDatabaseSO LoadDatabase()
    {
        if (cachedInstance != null && cachedInstance.quests != null && cachedInstance.quests.Count > 0)
            return cachedInstance;

        cachedInstance = Resources.Load<QuestDatabaseSO>("QuestDatabase");
        if (cachedInstance == null || cachedInstance.quests == null || cachedInstance.quests.Count == 0)
        {
            if (cachedInstance == null) cachedInstance = CreateInstance<QuestDatabaseSO>();
            cachedInstance.quests = LoadQuestsFromMarkdown();
        }
        return cachedInstance;
    }

    public static List<QuestDefinition> LoadQuestsFromMarkdown()
    {
        string mdPath = System.IO.Path.Combine(Application.dataPath, "Resources/QuestChains.md");
        if (System.IO.File.Exists(mdPath))
        {
            return ParseMarkdownText(System.IO.File.ReadAllText(mdPath));
        }
        return new List<QuestDefinition>();
    }

    public static List<QuestDefinition> ParseMarkdownText(string text)
    {
        var list = new List<QuestDefinition>();
        if (string.IsNullOrEmpty(text)) return list;

        string[] lines = text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
        QuestDefinition currentQuest = null;
        string currentDialogueSection = null;

        foreach (string rawLine in lines)
        {
            string line = rawLine.Trim();
            if (string.IsNullOrEmpty(line) || line.StartsWith("# ") || line.StartsWith("<!--"))
                continue;

            var headerMatch = System.Text.RegularExpressions.Regex.Match(line, @"^##\s*\[(.*?)\]\s*(.*)$");
            if (headerMatch.Success)
            {
                currentQuest = new QuestDefinition();
                currentQuest.questID = headerMatch.Groups[1].Value.Trim();
                currentQuest.questTitle = headerMatch.Groups[2].Value.Trim();
                list.Add(currentQuest);
                currentDialogueSection = null;
                continue;
            }

            if (currentQuest == null) continue;

            if (line.StartsWith("- Category:", StringComparison.OrdinalIgnoreCase))
            {
                string val = line.Substring("- Category:".Length).Trim();
                Enum.TryParse(val, true, out currentQuest.category);
                currentDialogueSection = null;
            }
            else if (line.StartsWith("- Scene:", StringComparison.OrdinalIgnoreCase))
            {
                currentQuest.sceneName = line.Substring("- Scene:".Length).Trim();
                currentDialogueSection = null;
            }
            else if (line.StartsWith("- Prerequisite:", StringComparison.OrdinalIgnoreCase))
            {
                currentQuest.prerequisiteQuestID = line.Substring("- Prerequisite:".Length).Trim();
                currentDialogueSection = null;
            }
            else if (line.StartsWith("- StartNPC:", StringComparison.OrdinalIgnoreCase))
            {
                string content = line.Substring("- StartNPC:".Length).Trim();
                string[] parts = content.Split('|');
                currentQuest.startNpcID = parts[0].Trim();

                foreach (var p in parts)
                {
                    string trimmed = p.Trim();
                    if (trimmed.StartsWith("TurnInNPC:", StringComparison.OrdinalIgnoreCase))
                        currentQuest.turnInNpcID = trimmed.Substring("TurnInNPC:".Length).Trim();
                    else if (trimmed.StartsWith("NextQuest:", StringComparison.OrdinalIgnoreCase))
                        currentQuest.nextQuestInChainID = trimmed.Substring("NextQuest:".Length).Trim();
                }
                if (string.IsNullOrEmpty(currentQuest.turnInNpcID))
                    currentQuest.turnInNpcID = currentQuest.startNpcID;

                currentDialogueSection = null;
            }
            else if (line.StartsWith("- Objective:", StringComparison.OrdinalIgnoreCase))
            {
                string content = line.Substring("- Objective:".Length).Trim();
                string[] parts = content.Split('|');
                string typeStr = parts[0].Trim();
                Enum.TryParse(typeStr, true, out currentQuest.questType);

                foreach (var p in parts)
                {
                    string trimmed = p.Trim();
                    if (trimmed.StartsWith("Target:", StringComparison.OrdinalIgnoreCase))
                        currentQuest.targetID = trimmed.Substring("Target:".Length).Trim();
                    else if (trimmed.StartsWith("Amount:", StringComparison.OrdinalIgnoreCase))
                        int.TryParse(trimmed.Substring("Amount:".Length).Trim(), out currentQuest.requiredAmount);
                    else if (trimmed.StartsWith("GoldReward:", StringComparison.OrdinalIgnoreCase))
                        int.TryParse(trimmed.Substring("GoldReward:".Length).Trim(), out currentQuest.goldReward);
                }
                currentDialogueSection = null;
            }
            else if (line.StartsWith("- Description:", StringComparison.OrdinalIgnoreCase))
            {
                currentQuest.questDescription = line.Substring("- Description:".Length).Trim();
                currentDialogueSection = null;
            }
            else if (line.StartsWith("- OfferDialogue:", StringComparison.OrdinalIgnoreCase))
            {
                currentDialogueSection = "Offer";
            }
            else if (line.StartsWith("- InProgressDialogue:", StringComparison.OrdinalIgnoreCase))
            {
                currentDialogueSection = "InProgress";
            }
            else if (line.StartsWith("- TurnInDialogue:", StringComparison.OrdinalIgnoreCase))
            {
                currentDialogueSection = "TurnIn";
            }
            else if (line.StartsWith("- CompletedDialogue:", StringComparison.OrdinalIgnoreCase))
            {
                currentDialogueSection = "Completed";
            }
            else if (currentDialogueSection != null && line.Contains(":"))
            {
                int colonIdx = line.IndexOf(':');
                string speaker = line.Substring(0, colonIdx).Trim();
                string textVal = line.Substring(colonIdx + 1).Trim();
                var dLine = new QuestDialogueLine(speaker, textVal);

                switch (currentDialogueSection)
                {
                    case "Offer":
                        currentQuest.offerDialogue.Add(dLine);
                        break;
                    case "InProgress":
                        currentQuest.inProgressDialogue.Add(dLine);
                        break;
                    case "TurnIn":
                        currentQuest.turnInDialogue.Add(dLine);
                        break;
                    case "Completed":
                        currentQuest.completedDialogue.Add(dLine);
                        break;
                }
            }
        }

        return list;
    }

    public QuestDefinition GetQuest(string id)
    {
        if (string.IsNullOrEmpty(id) || quests == null) return null;
        return quests.Find(q => q.questID.Equals(id, StringComparison.OrdinalIgnoreCase));
    }

    public List<QuestDefinition> GetQuestsForStartNpc(string npcId)
    {
        var list = new List<QuestDefinition>();
        if (string.IsNullOrEmpty(npcId) || quests == null) return list;
        foreach (var q in quests)
        {
            if (q.startNpcID.Equals(npcId, StringComparison.OrdinalIgnoreCase))
                list.Add(q);
        }
        return list;
    }

    public List<QuestDefinition> GetQuestsForTurnInNpc(string npcId)
    {
        var list = new List<QuestDefinition>();
        if (string.IsNullOrEmpty(npcId) || quests == null) return list;
        foreach (var q in quests)
        {
            if (q.turnInNpcID.Equals(npcId, StringComparison.OrdinalIgnoreCase))
                list.Add(q);
        }
        return list;
    }
}

// ==========================================
// Backwards Compatibility Definitions
// ==========================================
[System.Serializable]
public class QuestChoice
{
    public string buttonLabel;
    [TextArea(2, 5)] public string[] resultDialogue;

    [Header("Quest Trigger Settings")]
    public bool triggersQuest;
    public int questIndexToTrigger;
}

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
    public int requiredGold;

    [Header("Quest Dialogue")]
    public bool offersChoices = false;
    public QuestChoice[] choices;

    [TextArea(2, 5)] public string[] startDialogue;
    [TextArea(2, 5)] public string[] completeDialogue;

    [Header("Score Checker Settings")]
    [TextArea(2, 5)] public string[] notMetDialogue;

    [Header("Quest Triggers")]
    public GameObject[] objectsToActivate;
    public GameObject[] objectsToDeactivate;

    [Header("Chain Logic")]
    public bool hasNextQuest;
    public int nextQuestIndex;
}
