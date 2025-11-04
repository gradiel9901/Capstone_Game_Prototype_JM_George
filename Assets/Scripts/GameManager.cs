using UnityEngine;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    private HashSet<string> talkedNPCs = new HashSet<string>();
    private Dictionary<string, bool> npcQuestCompletion = new Dictionary<string, bool>();

    // ✅ Track all active quests globally
    private List<NPCInteraction> activeQuestGivers = new List<NPCInteraction>();

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    // ----------------------------
    // ✅ Dialogue Tracking
    // ----------------------------
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

        // Notify all active quests that depend on this NPC
        foreach (var questGiver in activeQuestGivers)
        {
            if (questGiver.HasActiveQuest && questGiver.QuestType == QuestType.TalkToNPC)
            {
                questGiver.RegisterEnemyKill(); // to refresh UI (harmless if not applicable)
            }
        }
    }

    // ----------------------------
    // ✅ Quest Progress Registration
    // ----------------------------
    public void RegisterDummyDamage(int damage)
    {
        foreach (var questGiver in activeQuestGivers)
        {
            if (questGiver.HasActiveQuest && questGiver.QuestType == QuestType.RequirementQuest)
            {
                questGiver.RegisterDamage(damage);
            }
        }
    }

    public void RegisterEnemyKill()
    {
        foreach (var questGiver in activeQuestGivers)
        {
            if (questGiver.HasActiveQuest && questGiver.QuestType == QuestType.EnemyExtermination)
            {
                questGiver.RegisterEnemyKill();
            }
        }
    }

    // ----------------------------
    // ✅ Quest Completion Tracking
    // ----------------------------
    public void MarkQuestCompleted(string npcName)
    {
        if (!npcQuestCompletion.ContainsKey(npcName))
            npcQuestCompletion.Add(npcName, true);
        else
            npcQuestCompletion[npcName] = true;

        Debug.Log($"{npcName}'s quest completed!");

        // Trigger next chain quest if applicable
        TryStartNextChainQuest(npcName);
    }

    public bool IsQuestCompletedFrom(string npcName)
    {
        return npcQuestCompletion.ContainsKey(npcName) && npcQuestCompletion[npcName];
    }

    // ----------------------------
    // ✅ Chain Quest System
    // ----------------------------
    public void RegisterQuestGiver(NPCInteraction questGiver)
    {
        if (!activeQuestGivers.Contains(questGiver))
        {
            activeQuestGivers.Add(questGiver);
        }
    }

    public void UnregisterQuestGiver(NPCInteraction questGiver)
    {
        if (activeQuestGivers.Contains(questGiver))
        {
            activeQuestGivers.Remove(questGiver);
        }
    }

    private void TryStartNextChainQuest(string npcName)
    {
        foreach (var questGiver in activeQuestGivers)
        {
            if (questGiver.NPCName == npcName)
            {
                questGiver.StartNextQuestInChain();
                break;
            }
        }
    }
}
