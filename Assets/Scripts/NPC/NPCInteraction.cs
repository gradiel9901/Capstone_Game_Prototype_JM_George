using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class NPCInteraction : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject dialogueBox;     // The dialogue UI panel
    [SerializeField] private TMP_Text dialogueText;      // Dialogue content text
    [SerializeField] private TMP_Text characterNameText; // Character name text
    [SerializeField] private Image characterArt;         // NPC portrait image

    [Header("Dialogue Data")]
    [TextArea(2, 5)]
    [SerializeField] private string[] dialogueLines;     // Lines of dialogue
    [SerializeField] private Sprite npcSprite;           // Character art sprite
    [SerializeField] private string npcName = "NPC";     // Character name

    [Header("Settings")]
    [SerializeField] private KeyCode interactKey = KeyCode.E; // Interaction key
    [SerializeField] private float typingSpeed = 0.02f;        // Typewriter effect speed

    private bool playerInRange = false;
    private bool isTalking = false;
    private int currentLineIndex = 0;
    private Coroutine typingCoroutine;

    private void Start()
    {
        dialogueBox.SetActive(false);
        characterArt.enabled = false;
    }

    private void Update()
    {
        if (playerInRange && Input.GetKeyDown(interactKey))
        {
            if (!isTalking)
            {
                StartDialogue();
            }
            else
            {
                NextLine();
            }
        }
    }

    private void StartDialogue()
    {
        isTalking = true;
        currentLineIndex = 0;

        dialogueBox.SetActive(true);
        characterArt.enabled = true;
        characterArt.sprite = npcSprite;
        characterNameText.text = npcName; // ✅ Set character name in UI

        ShowLine(dialogueLines[currentLineIndex]);
    }

    private void NextLine()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            dialogueText.text = dialogueLines[currentLineIndex];
            typingCoroutine = null;
            return;
        }

        if (currentLineIndex < dialogueLines.Length - 1)
        {
            currentLineIndex++;
            ShowLine(dialogueLines[currentLineIndex]);
        }
        else
        {
            EndDialogue();
        }
    }

    private void ShowLine(string line)
    {
        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        typingCoroutine = StartCoroutine(TypeLine(line));
    }

    private IEnumerator TypeLine(string line)
    {
        dialogueText.text = "";
        foreach (char c in line)
        {
            dialogueText.text += c;
            yield return new WaitForSeconds(typingSpeed);
        }
        typingCoroutine = null;
    }

    private void EndDialogue()
    {
        isTalking = false;
        dialogueBox.SetActive(false);
        characterArt.enabled = false;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerInRange = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerInRange = false;
            EndDialogue();
        }
    }
}
