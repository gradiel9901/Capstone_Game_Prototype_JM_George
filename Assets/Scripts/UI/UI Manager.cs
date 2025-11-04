using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour
{
    public GameObject dialogueBox;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI dialogueText;
    public Image characterArt;

    private string[] lines;
    private int index;
    private bool isActive = false;

    void Start()
    {
        dialogueBox.SetActive(false);
    }

    public void StartDialogue(string npcName, string[] dialogueLines, Sprite portrait)
    {
        nameText.text = npcName;
        lines = dialogueLines;
        index = 0;
        characterArt.sprite = portrait;
        characterArt.enabled = portrait != null;

        dialogueBox.SetActive(true);
        isActive = true;
        ShowLine();
    }

    void Update()
    {
        if (isActive && Input.GetKeyDown(KeyCode.E))
        {
            NextLine();
        }
    }

    void ShowLine()
    {
        dialogueText.text = lines[index];
    }

    void NextLine()
    {
        index++;
        if (index < lines.Length)
        {
            ShowLine();
        }
        else
        {
            EndDialogue();
        }
    }

    void EndDialogue()
    {
        dialogueBox.SetActive(false);
        isActive = false;
    }
}
