using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [SerializeField] private TMP_Text playerIDText;
    [SerializeField] private TMP_Text turnText;
    [SerializeField] private TMP_Text itemText;

    [SerializeField] private GameObject winScreen;
    [SerializeField] private GameObject loseScreen;
    [SerializeField] private TMP_Text loseScreenText;

    private void Awake()
    {
        Instance = this;
    }

    public void SetPlayerIDText(uint playerID)
    {
        playerIDText.text = "Player ID: " + playerID;
    }

    public void SetTurnText(bool turn)
    {
        turnText.text = "Turn: " + turn;
    }

    public void SetItemText(string itemName)
    {
        itemText.text = "Item: " + itemName;
    }

    public void ShowWinScreen()
    {
        winScreen.SetActive(true);
    }

    public void ShowLoseScreen(uint winnerID)
    {
        loseScreen.SetActive(true);
        loseScreenText.text = $"Oh no, you fell! {winnerID} won the game!";
    }
}
