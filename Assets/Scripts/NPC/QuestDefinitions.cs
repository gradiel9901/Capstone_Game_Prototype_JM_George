using UnityEngine;

public enum QuestType
{
    None,
    EnemyExtermination,
    TalkToNPC,
    RequirementQuest
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

    [Header("Quest Dialogue")]
    public bool offersChoices = false;
    public QuestChoice[] choices;

    [TextArea(2, 5)] public string[] startDialogue;
    [TextArea(2, 5)] public string[] completeDialogue;

    [Header("Quest Triggers")]
    public GameObject[] objectsToActivate;
    public GameObject[] objectsToDeactivate;

    [Header("Chain Logic")]
    public bool hasNextQuest; // ✅ THE CHECKBOX
    public int nextQuestIndex;
}