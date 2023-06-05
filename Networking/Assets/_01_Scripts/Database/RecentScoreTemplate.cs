using TMPro;
using UnityEngine;

public class RecentScoreTemplate : ScoreTemplates
{
    [SerializeField] protected TMP_Text scoreText;
    [SerializeField] protected TMP_Text dateTimeText;

    public void DisplayData(string userName, string score, string dateTime)
    {
        base.DisplayData(userName);
        scoreText.text = score;
        dateTimeText.text = dateTime;
    }
}
