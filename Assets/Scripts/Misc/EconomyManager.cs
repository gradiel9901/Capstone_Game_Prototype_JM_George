using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class EconomyManager : Singleton<EconomyManager>
{
    private TMP_Text goldText;
    public int currentGold = 0;

    const string COIN_AMOUNT_TEXT = "Gold Amount Text";

    public void AddGold(int amount)
    {
        currentGold += amount;

        if (goldText == null) {
            GameObject coinObj = GameObject.Find(COIN_AMOUNT_TEXT);
            if (coinObj != null) goldText = coinObj.GetComponent<TMP_Text>();
        }

        if (goldText != null)
            goldText.text = currentGold.ToString("D3");

        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.AdvanceObjective(QuestType.ScoreChecker, "Gold", currentGold);
        }
    }

    public void UpdateCurrentGold() {
        AddGold(1);
    }
    public void UpdateDeadGold()
    {
        currentGold = 0;

        if (goldText == null)
        {
            GameObject coinObj = GameObject.Find(COIN_AMOUNT_TEXT);
            if (coinObj != null) goldText = coinObj.GetComponent<TMP_Text>();
        }

        if (goldText != null)
            goldText.text = currentGold.ToString("D3");
    }
}
