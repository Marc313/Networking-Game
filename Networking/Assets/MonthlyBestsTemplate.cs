using TMPro;
using UnityEngine;

public class MonthlyBestsTemplate : ScoreTemplates
{
    [SerializeField] protected TMP_Text winnerNameText;
    [SerializeField] protected TMP_Text winsText;

    public void DisplayData(string winnerName, int wins)
    {
        base.DisplayData(winnerName);
        winsText.text = "Wins: " + wins;
    }
}
