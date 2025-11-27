using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance;

    [Header("Global UI References")]
    public GameObject dialogueBox;
    public TMP_Text dialogueText;
    public TMP_Text characterNameText;
    public Image characterArtImage;
    public TMP_Text questTitleText;
    public TMP_Text questDescriptionText;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }
}