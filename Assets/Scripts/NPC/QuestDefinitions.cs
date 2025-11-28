using UnityEngine;

public enum QuestType
{
    None,
    EnemyExtermination,
    TalkToNPC,
    RequirementQuest,
    ScoreChecker
}

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

    [Tooltip("Amount of Gold required for ScoreChecker quests")]
    public int requiredGold;

    [Header("Quest Dialogue")]
    public bool offersChoices = false;
    public QuestChoice[] choices;

    [TextArea(2, 5)] public string[] startDialogue;
    [TextArea(2, 5)] public string[] completeDialogue;

    // ✅ NEW FIELD: Dialogue when requirements aren't met
    [Header("Score Checker Settings")]
    [Tooltip("Dialogue shown if player interacts while Score Checker requirement is NOT met.")]
    [TextArea(2, 5)] public string[] notMetDialogue;

    [Header("Quest Triggers")]
    public GameObject[] objectsToActivate;
    public GameObject[] objectsToDeactivate;

    [Header("Chain Logic")]
    public bool hasNextQuest;
    public int nextQuestIndex;
}