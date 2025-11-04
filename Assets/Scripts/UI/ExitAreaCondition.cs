using UnityEngine;

public class ExitAreaCondition : MonoBehaviour
{
    [Header("Required NPCs to Proceed")]
    [SerializeField] private string requiredNPC1 = "Warrior";
    [SerializeField] private string requiredNPC2 = "Elf";

    [Header("Optional Reference to Conditional NPC")]
    [SerializeField] private NPCInteraction conditionalNPC; // NPC that warns the player

    private PlayerController playerInput; // Reference to player movement script

    private void Start()
    {
        // Automatically find the player controller in the scene
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            playerInput = player.GetComponent<PlayerController>(); // Make sure this matches your movement script
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        bool talkedToNPC1 = GameManager.Instance.HasTalkedTo(requiredNPC1);
        bool talkedToNPC2 = GameManager.Instance.HasTalkedTo(requiredNPC2);

        if (talkedToNPC1 && talkedToNPC2)
        {
            Debug.Log("✅ Player has talked to all required NPCs. Gate deactivated.");
            gameObject.SetActive(false); // Allow passage
        }
        else
        {
            Debug.Log("❌ Player hasn't talked to all required NPCs yet!");

            // Stop player movement
            if (playerInput != null)
                playerInput.DisableMovement();

            // Trigger conditional dialogue
            if (conditionalNPC != null)
            {
                conditionalNPC.ForceConditionalDialogue(() =>
                {
                    // Re-enable player movement once dialogue is finished
                    if (playerInput != null)
                        playerInput.EnableMovement();
                });
            }
        }
    }
}
