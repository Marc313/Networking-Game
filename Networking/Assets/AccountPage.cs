using TMPro;
using UnityEngine;

public class AccountPage : MonoBehaviour
{
    public TMP_Text mailText;
    public TMP_Text IDText;
    public TMP_Text usernameText;

    public void DisplayData()
    {
        mailText.text = "Email address: " + AccountManager.userMail;
        IDText.text = "Player ID: " + AccountManager.playerID;
        usernameText.text = "Username: " + AccountManager.userName;
    }
}
