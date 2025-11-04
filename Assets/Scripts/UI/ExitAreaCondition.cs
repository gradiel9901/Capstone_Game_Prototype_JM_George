using UnityEngine;

public class ExitAreaCondition : MonoBehaviour
{
    [Header("Required NPCs to Proceed")]
    [SerializeField] private NPCInteraction elfNPC;
    [SerializeField] private NPCInteraction warriorNPC;

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

        // Make sure both NPC references are valid
        if (elfNPC == null || warriorNPC == null) return;

        bool talkedToElf = GameManager.Instance.HasTalkedTo(elfNPC.name);
        bool talkedToWarrior = GameManager.Instance.HasTalkedTo(warriorNPC.name);
        bool warriorQuestDone = warriorNPC.IsQuestCompleted;

        // ✅ All conditions must be true
        if (talkedToElf && talkedToWarrior && warriorQuestDone)
        {
            Debug.Log("[ExitAreaCondition] All conditions met. Disabling barrier collider.");
            barrierCollider.enabled = false;
            conditionMet = true;
        }
    }
}
