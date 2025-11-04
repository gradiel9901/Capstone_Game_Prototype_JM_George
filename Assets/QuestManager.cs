using UnityEngine;

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance;

    [Header("Active Quest Info")]
    public NPCInteraction CurrentQuestGiver;
    public QuestType CurrentQuestType = QuestType.None;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void StartQuest(NPCInteraction giver, QuestType questType)
    {
        CurrentQuestGiver = giver;
        CurrentQuestType = questType;
        Debug.Log($"Quest started: {questType} from {giver.name}");
    }

    public void CompleteQuest()
    {
        if (CurrentQuestGiver != null)
        {
            Debug.Log($"Quest completed: {CurrentQuestType} from {CurrentQuestGiver.name}");
            CurrentQuestGiver = null;
            CurrentQuestType = QuestType.None;
        }
    }

    public void RegisterEnemyKill()
    {
        if (CurrentQuestGiver != null && CurrentQuestType == QuestType.EnemyExtermination)
        {
            CurrentQuestGiver.RegisterEnemyKill();
        }
    }

    public void RegisterDummyDamage(int amount)
    {
        if (CurrentQuestGiver != null && CurrentQuestType == QuestType.RequirementQuest)
        {
            CurrentQuestGiver.RegisterDamage(amount);
        }
    }

    public void CheckTalkToNPC(string npcName)
    {
        if (CurrentQuestGiver != null && CurrentQuestType == QuestType.TalkToNPC)
        {
            if (CurrentQuestGiver != null && CurrentQuestGiver.name == npcName)
            {
                CurrentQuestGiver.RegisterEnemyKill();
            }
        }
    }
}
