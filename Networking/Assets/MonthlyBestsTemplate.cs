using TMPro;
using UnityEngine;

public class MonthlyBestsTemplate : ScoreTemplates
{
    [SerializeField] protected TMP_Text winnerNameText;
    [SerializeField] protected TMP_Text winsText;
    [SerializeField] protected TMP_Text winRateText;
    [SerializeField] protected TMP_Text positionText;

    public void DisplayData(string winnerName, int wins, float winRate, int position)
    {
        base.DisplayData(winnerName);
        winsText.text = "Wins: " + wins;
        winRateText.text = "Winrate: " + ((int) (winRate * 100)) + "%";
        positionText.text = $"{position}.";
    }
}
