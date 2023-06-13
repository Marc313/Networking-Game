using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [SerializeField] private GameObject debugInfo;
    [SerializeField] private TMP_Text playerIDText;
    [SerializeField] private TMP_Text turnText;
    [SerializeField] private TMP_Text itemText;

    [SerializeField] private GameObject resultScreen;
    [SerializeField] private TMP_Text winScreenText;
    [SerializeField] private TMP_Text loseScreenText;

    [SerializeField] private GameObject joinScreen;
    [SerializeField] private TMP_Text playersJoinedCount;

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
        resultScreen.SetActive(true);
        winScreenText.gameObject.SetActive(true);
        loseScreenText.gameObject.SetActive(false);
    }

    public void ShowLoseScreen(uint winnerID)
    {
        resultScreen.SetActive(true);
        winScreenText.gameObject.SetActive(false);
        loseScreenText.gameObject.SetActive(true);
        loseScreenText.text = $"Oh no, you fell! Player {winnerID} won the game!";
    }

    public void DisableJoinScreen()
    {
        joinScreen.SetActive(false);
        debugInfo.SetActive(true);
    }

    public void UpdatePlayerJoinedCount(int players)
    {
        playersJoinedCount.text = $"Players: {players}/2";
    }
}
