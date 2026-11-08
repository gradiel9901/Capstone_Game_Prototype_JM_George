using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class ExitAreaCondition : MonoBehaviour
{
    [Header("Required NPCs to Proceed")]
    [Tooltip("List of NPCs that the player must talk to or complete quests for before the barrier is removed.")]
    [SerializeField] private List<NPCInteraction> requiredNPCs = new List<NPCInteraction>();

    [Header("Required Quest IDs (MMO Quest System)")]
    [SerializeField] private List<string> requiredQuestIDs = new List<string>();

    [Header("Barrier Collider")]
    [SerializeField] private Collider2D barrierCollider;

    private bool conditionMet = false;

    private void Awake()
    {
        if (barrierCollider == null)
            barrierCollider = GetComponent<Collider2D>();

        if (barrierCollider != null)
            barrierCollider.enabled = true; // Strictly block by default
    }

    private void OnEnable()
    {
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.OnQuestStateChanged -= HandleQuestStateChanged;
            QuestManager.Instance.OnQuestStateChanged += HandleQuestStateChanged;
        }
    }

    private void OnDisable()
    {
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.OnQuestStateChanged -= HandleQuestStateChanged;
        }
    }

    private void Start()
    {
        if (barrierCollider == null)
            barrierCollider = GetComponent<Collider2D>();

        if (barrierCollider != null)
            barrierCollider.enabled = true; // Strictly block by default

        AutoConfigureRequirements();

        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.OnQuestStateChanged -= HandleQuestStateChanged;
            QuestManager.Instance.OnQuestStateChanged += HandleQuestStateChanged;
        }

        EvaluateBarrierState();
    }

    private void AutoConfigureRequirements()
    {
        // Clean out any null serialized references
        if (requiredNPCs != null)
        {
            requiredNPCs.RemoveAll(npc => npc == null);
        }

        string sceneName = SceneManager.GetActiveScene().name;

        // Auto-configure default quests for standard scenes if not explicitly set
        if ((requiredQuestIDs == null || requiredQuestIDs.Count == 0) && (requiredNPCs == null || requiredNPCs.Count == 0))
        {
            if (sceneName.Equals("Tutorial", System.StringComparison.OrdinalIgnoreCase))
            {
                requiredQuestIDs = new List<string> { "TUT_01", "TUT_02" };
            }
            else if (sceneName.Equals("Scene1", System.StringComparison.OrdinalIgnoreCase))
            {
                requiredQuestIDs = new List<string> { "SC1_01" };
            }
            else if (sceneName.Equals("Scene2", System.StringComparison.OrdinalIgnoreCase))
            {
                requiredQuestIDs = new List<string> { "SC2_01", "SC2_02" };
            }
            else if (sceneName.Equals("Scene3", System.StringComparison.OrdinalIgnoreCase))
            {
                requiredQuestIDs = new List<string> { "SC3_01", "SC3_02" };
            }
        }
    }

    private void HandleQuestStateChanged(string questID, QuestState state)
    {
        EvaluateBarrierState();
    }

    private void Update()
    {
        EvaluateBarrierState();
    }

    public void EvaluateBarrierState()
    {
        if (conditionMet)
            return;

        if (barrierCollider == null)
            barrierCollider = GetComponent<Collider2D>();

        if (CheckIfConditionsMet())
        {
            Debug.Log("[ExitAreaCondition] All required quests/NPC conditions met! Lowering barrier collider.");
            if (barrierCollider != null)
                barrierCollider.enabled = false;
            conditionMet = true;
        }
        else
        {
            // Keep collider strictly active while conditions are unmet
            if (barrierCollider != null && !barrierCollider.enabled)
            {
                barrierCollider.enabled = true;
            }
        }
    }

    public bool CheckIfConditionsMet()
    {
        string sceneName = SceneManager.GetActiveScene().name;

        // 1. Check explicit or auto-populated MMO Quest IDs
        if (requiredQuestIDs != null && requiredQuestIDs.Count > 0)
        {
            foreach (var qID in requiredQuestIDs)
            {
                if (string.IsNullOrEmpty(qID)) continue;
                if (QuestManager.Instance == null || !QuestManager.Instance.IsQuestCompleted(qID))
                    return false;
            }
            return true;
        }

        // 2. Check Tutorial scene specifically
        if (sceneName.Equals("Tutorial", System.StringComparison.OrdinalIgnoreCase))
        {
            if (QuestManager.Instance != null)
            {
                return QuestManager.Instance.IsQuestCompleted("TUT_01") && QuestManager.Instance.IsQuestCompleted("TUT_02");
            }
            if (GameManager.Instance != null)
            {
                return GameManager.Instance.IsQuestCompletedFrom("Warrior") && GameManager.Instance.HasTalkedTo("Elf");
            }
            return false;
        }

        // 3. Check serialized NPCs
        if (requiredNPCs != null && requiredNPCs.Count > 0)
        {
            foreach (var npc in requiredNPCs)
            {
                if (npc == null) continue;

                bool talkedToNPC = GameManager.Instance != null && (GameManager.Instance.HasTalkedTo(npc.name) || GameManager.Instance.HasTalkedTo(npc.NPCName));
                bool questCompleted = npc.IsQuestCompleted || (QuestManager.Instance != null && (QuestManager.Instance.IsAnyQuestCompletedForNPC(npc.name) || QuestManager.Instance.IsAnyQuestCompletedForNPC(npc.NPCName)));

                if (!talkedToNPC || !questCompleted)
                    return false;
            }
            return true;
        }

        return false;
    }
}
