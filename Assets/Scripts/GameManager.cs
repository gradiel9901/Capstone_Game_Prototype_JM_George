using UnityEngine;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    private HashSet<string> talkedNPCs = new HashSet<string>();
    private Dictionary<string, bool> npcQuestCompletion = new Dictionary<string, bool>();
    private List<NPCInteraction> activeQuestGivers = new List<NPCInteraction>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public bool HasTalkedTo(string npcName)
    {
        return talkedNPCs.Contains(npcName);
    }

    public void MarkNPCAsTalked(string npcName)
    {
        if (!talkedNPCs.Contains(npcName))
        {
            talkedNPCs.Add(npcName);
            Debug.Log($"Talked to {npcName}");
        }

        foreach (var questGiver in activeQuestGivers)
        {
            // Now "QuestType" is visible here because it's in QuestDefinitions.cs
            if (questGiver != null && questGiver.HasActiveQuest && questGiver.CurrentQuestType == QuestType.TalkToNPC)
            {
                questGiver.RegisterEnemyKill();
            }
        }
    }

    public void RegisterDummyDamage(int damage)
    {
        for (int i = activeQuestGivers.Count - 1; i >= 0; i--)
        {
            var questGiver = activeQuestGivers[i];
            if (questGiver != null && questGiver.HasActiveQuest && questGiver.CurrentQuestType == QuestType.RequirementQuest)
            {
                questGiver.RegisterDamage(damage);
            }
        }
    }

    public void RegisterEnemyKill()
    {
        for (int i = activeQuestGivers.Count - 1; i >= 0; i--)
        {
            var questGiver = activeQuestGivers[i];
            if (questGiver != null && questGiver.HasActiveQuest && questGiver.CurrentQuestType == QuestType.EnemyExtermination)
            {
                questGiver.RegisterEnemyKill();
            }
        }
    }

    public void MarkQuestCompleted(string npcName)
    {
        if (!npcQuestCompletion.ContainsKey(npcName))
            npcQuestCompletion.Add(npcName, true);
        else
            npcQuestCompletion[npcName] = true;

        Debug.Log($"{npcName}'s quest completed!");
        TryStartNextChainQuest(npcName);
    }

    public bool IsQuestCompletedFrom(string npcName)
    {
        return npcQuestCompletion.ContainsKey(npcName) && npcQuestCompletion[npcName];
    }

    public void RegisterQuestGiver(NPCInteraction questGiver)
    {
        if (!activeQuestGivers.Contains(questGiver))
            activeQuestGivers.Add(questGiver);
    }

    public void UnregisterQuestGiver(NPCInteraction questGiver)
    {
        if (activeQuestGivers.Contains(questGiver))
            activeQuestGivers.Remove(questGiver);
    }

    private void TryStartNextChainQuest(string npcName)
    {
        foreach (var questGiver in activeQuestGivers)
        {
            if (questGiver != null && questGiver.NPCName == npcName)
            {
                questGiver.StartNextQuestInChain();
                break;
            }
        }
    }
}