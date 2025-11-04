using System;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class NPCInteraction : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject dialogueBox;
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private TMP_Text characterNameText;
    [SerializeField] private Image characterArtImage;

    [Header("NPC Info")]
    [SerializeField] private string npcName;
    [SerializeField] private Sprite npcSprite;

    [Header("Dialogue Settings")]
    [SerializeField, TextArea(2, 5)] private string[] dialogueLines;

    [Header("Conditional NPC Settings")]
    [SerializeField] private bool requiresOtherNPC = false;
    [SerializeField] private string requiredNPCName;
    [SerializeField, TextArea(2, 5)] private string[] conditionalDialogueLines;

    [Header("Interaction Settings")]
    [SerializeField] private float interactionRange = 2f;
    [SerializeField] private KeyCode interactKey = KeyCode.E;

    private Transform player;
    private int dialogueIndex = 0;
    private string[] activeDialogue;
    private bool isPlayerInRange = false;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;

        if (dialogueBox != null)
            dialogueBox.SetActive(false);

        if (characterArtImage != null)
            characterArtImage.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (player == null) return;

        float distance = Vector2.Distance(transform.position, player.position);
        bool inRangeNow = distance <= interactionRange;

        if (inRangeNow && !isPlayerInRange)
        {
            isPlayerInRange = true;
        }
        else if (!inRangeNow && isPlayerInRange)
        {
            isPlayerInRange = false;
            HideDialogue();
        }

        if (isPlayerInRange && Input.GetKeyDown(interactKey))
        {
            if (dialogueBox.activeSelf)
                NextLine();
            else
                StartDialogue();
        }
    }

    private void StartDialogue()
    {
        if (requiresOtherNPC && !GameManager.Instance.HasTalkedTo(requiredNPCName))
        {
            activeDialogue = conditionalDialogueLines;
            Debug.Log($"{npcName} showing conditional dialogue (needs {requiredNPCName})");
        }
        else
        {
            activeDialogue = dialogueLines;
            Debug.Log($"{npcName} showing main dialogue");
        }

        if (activeDialogue == null || activeDialogue.Length == 0)
        {
            Debug.LogWarning($"{npcName} has no dialogue lines to display!");
            dialogueText.text = "...";
            return;
        }

        dialogueIndex = 0;
        dialogueBox.SetActive(true);
        characterNameText.text = npcName;
        dialogueText.text = activeDialogue[dialogueIndex];

        if (characterArtImage != null)
        {
            characterArtImage.sprite = npcSprite;
            characterArtImage.gameObject.SetActive(true);
        }

        if (!requiresOtherNPC || GameManager.Instance.HasTalkedTo(requiredNPCName))
            GameManager.Instance.MarkNPCAsTalked(npcName);
    }

    private void NextLine()
    {
        dialogueIndex++;

        if (activeDialogue == null || dialogueIndex >= activeDialogue.Length)
        {
            HideDialogue();
        }
        else
        {
            dialogueText.text = activeDialogue[dialogueIndex];
        }
    }

    private void HideDialogue()
    {
        dialogueBox.SetActive(false);
        if (characterArtImage != null)
            characterArtImage.gameObject.SetActive(false);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }

    // ✅ Updated version to match ExitAreaCondition
    public void ForceConditionalDialogue(Action onDialogueEnd = null)
    {
        if (conditionalDialogueLines == null || conditionalDialogueLines.Length == 0)
        {
            Debug.LogWarning($"{npcName} has no conditional dialogue lines set!");
            onDialogueEnd?.Invoke();
            return;
        }

        StartCoroutine(ForceDialogueRoutine(onDialogueEnd));
    }

    private IEnumerator ForceDialogueRoutine(Action onDialogueEnd)
    {
        activeDialogue = conditionalDialogueLines;
        dialogueIndex = 0;

        dialogueBox.SetActive(true);
        characterNameText.text = npcName;
        dialogueText.text = activeDialogue[dialogueIndex];

        if (characterArtImage != null)
        {
            characterArtImage.sprite = npcSprite;
            characterArtImage.gameObject.SetActive(true);
        }

        Debug.Log($"{npcName} forced to show conditional dialogue.");

        while (dialogueIndex < activeDialogue.Length)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                dialogueIndex++;
                if (dialogueIndex < activeDialogue.Length)
                {
                    dialogueText.text = activeDialogue[dialogueIndex];
                }
                else
                {
                    HideDialogue();
                    break;
                }
            }
            yield return null;
        }

        onDialogueEnd?.Invoke(); // ✅ Re-enable movement after dialogue ends
    }
}
