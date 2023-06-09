using TMPro;
using UnityEngine;

public class RecentScoreTemplate : ScoreTemplates
{
    [SerializeField] protected TMP_Text scoreText;
    [SerializeField] protected TMP_Text dateTimeText;
    [SerializeField] protected TMP_Text opponentText;

    public void DisplayData(string userName, string score, string dateTime, string opponentName)
    {
        base.DisplayData(userName);
        scoreText.text = score;
        dateTimeText.text = dateTime;
        opponentText.text = "Opponents: " + opponentName;
    }
}
