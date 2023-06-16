using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("Join Screen menu")]
    [SerializeField] private GameObject joinScreen;
    [SerializeField] private TMP_Text playersJoinedCount;

    [Header("Win/Lose Screen")]
    [SerializeField] private GameObject resultScreen;
    [SerializeField] private TMP_Text winScreenText;
    [SerializeField] private TMP_Text loseScreenText;

    [Header("Debug Info")]
    [SerializeField] private GameObject debugInfo;
    [SerializeField] private TMP_Text playerIDText;
    [SerializeField] private TMP_Text turnText;
    [SerializeField] private TMP_Text itemText;
    [SerializeField] private Image itemIcon;


    private void Awake()
    {
        Instance = this;
    }

    public void SetPlayerIDText(uint playerID)
    {
        if (playerIDText != null)
        playerIDText.text = "Player ID: " + playerID;
    }

    public void SetTurnText(bool turn)
    {
        turnText.text = "Turn: " + turn;
    }

    public void SetItemInfo(string itemName, Sprite itemSprite)
    {
        //itemText.text = "Item: " + itemName;
        itemIcon.gameObject.SetActive(true);
        itemIcon.sprite = itemSprite;
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
