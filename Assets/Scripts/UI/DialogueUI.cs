using UnityEngine;
using UnityEngine.UI;
using TMPro; // if using TextMeshPro

public class DialogueUI : MonoBehaviour
{
    public GameObject dialogueBox;
    public Image characterArt;
    public TextMeshProUGUI dialogueText;
    public TextMeshProUGUI characterNameText;

    private void Start()
    {
        dialogueBox.SetActive(false);
        characterArt.enabled = false;
    }

    public void ShowDialogue(string characterName, string dialogue, Sprite art)
    {
        dialogueBox.SetActive(true);
        characterArt.enabled = true;

        if (art != null)
            characterArt.sprite = art;

        if (characterNameText != null)
            characterNameText.text = characterName;

        dialogueText.text = dialogue;
    }

    public void HideDialogue()
    {
        dialogueBox.SetActive(false);
        characterArt.enabled = false;
    }
}
