using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    private HashSet<string> talkedToNPCs = new HashSet<string>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void MarkNPCAsTalked(string npcName)
    {
        talkedToNPCs.Add(npcName);
    }

    public bool HasTalkedTo(string npcName)
    {
        return talkedToNPCs.Contains(npcName);
    }
}
