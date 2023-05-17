using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [SerializeField] private TMP_Text playerIDText;
    [SerializeField] private TMP_Text turnText;

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
}
