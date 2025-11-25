using UnityEngine;
using System.Collections.Generic;

public class ExitAreaCondition : MonoBehaviour
{
    [Header("Required NPCs to Proceed")]
    [Tooltip("List of NPCs that the player must talk to or complete quests for before the barrier is removed.")]
    [SerializeField] private List<NPCInteraction> requiredNPCs = new List<NPCInteraction>();

    [Header("Barrier Collider")]
    [SerializeField] private Collider2D barrierCollider;

    private bool conditionMet = false;

    private void Start()
    {
        if (barrierCollider == null)
            barrierCollider = GetComponent<Collider2D>();

        if (barrierCollider != null)
            barrierCollider.enabled = true; // Default: block player
    }

    private void Update()
    {
        if (conditionMet || barrierCollider == null)
            return;

        // ✅ Skip if GameManager is missing
        if (GameManager.Instance == null)
            return;

        // ✅ If there are no required NPCs, automatically disable the barrier
        if (requiredNPCs == null || requiredNPCs.Count == 0)
        {
            Debug.LogWarning("[ExitAreaCondition] No required NPCs set. Disabling barrier automatically.");
            barrierCollider.enabled = false;
            conditionMet = true;
            return;
        }

        bool allConditionsMet = true;

        // ✅ Loop through each required NPC
        foreach (var npc in requiredNPCs)
        {
            if (npc == null)
                continue; // Skip null references (NPC not present in the scene)

            bool talkedToNPC = GameManager.Instance.HasTalkedTo(npc.name);
            bool questCompleted = npc.IsQuestCompleted;

            // If either condition is false, we can stop checking further
            if (!talkedToNPC || !questCompleted)
            {
                allConditionsMet = false;
                break;
            }
        }

        // ✅ All NPCs talked to and quests done → disable barrier
        if (allConditionsMet)
        {
            Debug.Log("[ExitAreaCondition] All conditions met. Disabling barrier collider.");
            barrierCollider.enabled = false;
            conditionMet = true;
        }
    }
}
