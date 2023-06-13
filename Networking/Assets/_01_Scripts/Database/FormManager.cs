using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Form and menu manager in one ;)
public class FormManager : MonoBehaviour
{
    public bool executeStart = true;
    public bool loginIntoServerOnStart = false;

    [Header("Login screen")]
    public GameObject login_menu;
    public TMP_InputField login_mailField;
    public TMP_InputField login_passwordField;
    public Button login_submitButton;

    [Header("Register screen")]
    public GameObject register_menu;
    public TMP_InputField register_mailField;
    public TMP_InputField register_usernameField;
    public TMP_InputField register_passwordField;
    public Button register_submitButton;

    [Header("Home Screen")]
    public GameObject home_menu;

    [Header("Scores")]
    public TMP_Text scoreTextField;
    public GameObject recentScoresParent;
    public GameObject recentScorePrefab;
    public GameObject leaderboardParent;
    public GameObject leaderboardScoreTemplate;
    public TMP_Text gamesPlayedText;

    private static string sessionId;

    private void Start()
    {
        if (!executeStart) return;

        login_submitButton.onClick.AddListener(SubmitLogin);
        register_submitButton.onClick.AddListener(SubmitRegistration);

        if (AccountManager.isSet) GoToHomeScreen();
        if (loginIntoServerOnStart) ServerLogin();
    }

    public async void ServerLogin()
    {
        string phpUrl = $"https://studenthome.hku.nl/~marc.neeleman/serverlogin.php?server_id=4&password=ducks69";
        string response = await PostURL(phpUrl);

        if (CheckErrors(response))
        {
            sessionId = response;
        }
    }

    public async void SubmitLogin()
    {
        string phpUrl = $"https://studenthome.hku.nl/~marc.neeleman/userlogin.php?usermail={login_mailField.text}&password={login_passwordField.text}&session_id={sessionId}";
        string response = await PostURL(phpUrl);

        if (CheckErrors(response))
        {
            SaveAccountData(response);
        }
        else
        {
            Debug.Log("Login failed!");
        }
    }

    private void SaveAccountData(string response)
    {
        JObject user = JObject.Parse(response);
        AccountManager.playerID = (uint)user["id"];
        AccountManager.userName = (string)user["name"];
        AccountManager.userMail = (string)user["email"];
        AccountManager.isSet = true;

        GoToHomeScreen();
    }

    public async void SubmitRegistration()
    {
        string phpUrl = $"https://studenthome.hku.nl/~marc.neeleman/registerUser.php?email={register_mailField.text}&username={register_usernameField.text}&password={register_passwordField.text}&session_id=lbem30ojq32uda24e0g2b9uebl&session_id={sessionId}";
        string response = await PostURL(phpUrl);

        if (CheckErrors(response))
        {
            SaveAccountData(response);
        }
        else
        {
            Debug.Log("Registration failed!");
        }
    }

    public async void LoadPlayerScores()
    {
        string phpUrl = $"https://studenthome.hku.nl/~marc.neeleman/recentplayersscores.php?player_id={AccountManager.playerID}&session_id={sessionId}";
        string response = await PostURL(phpUrl);

        if (CheckErrors(response))
        {
            ParsePlayerScores(response);
        }
        else
        {
            scoreTextField.text = "Something went wrong, try again later";
        }
    }

    public async void LoadLeaderboard()
    {
        string phpURL = $"https://studenthome.hku.nl/~marc.neeleman/monthlyLeaderboard.php?session_id={sessionId}";
        string response = await PostURL(phpURL);

        if (CheckErrors(response))
        {
            ParseLeaderboardScores(response);
        }
    }

    public async void LoadGamePlayedAmount()
    {
        string phpURL = $"https://studenthome.hku.nl/~marc.neeleman/gamePlayedAmount.php?session_id={sessionId}";
        string response = await PostURL(phpURL);

        JObject jObject = JObject.Parse(response);
        gamesPlayedText.text = $"Played this month: {jObject["games"]} games";
    }

    public async void InsertScore(int player1ID, int player2ID, int winnerID)
    {
        string phpUrl = $"https://studenthome.hku.nl/~marc.neeleman/scoreinsert.php?player1_id={player1ID}&player2_id={player2ID}&winner_id={winnerID}&session_id={sessionId}";
        string response = await PostURL(phpUrl);
        Debug.LogError("Response");
    }

    private async Task<string> PostURL(string phpUrl)
    {
        using (UnityWebRequest www = UnityWebRequest.Get(phpUrl))
        {
            var operation = www.SendWebRequest();
            while (!operation.isDone) await Task.Yield();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(www.error);
                return "0";
            }
            else
            {
                Debug.Log(www.downloadHandler.text);
                return www.downloadHandler.text;
            }
        }
    }

    private void LoadNextScene()
    {
        SceneManager.LoadScene(1);
    }

    /// <summary>
    /// Returns false when errors are detected, true when no errors are detected.
    /// </summary>
    /// <param name="response"></param>
    /// <returns></returns>
    public bool CheckErrors(string response)
    {
        if (response[0] != '0') return true;
        else
        {
            Debug.LogError("Received PHP Error code 0");
            return false;
        }
    }

    public async void ParsePlayerScores(string text)
    {
        await Task.Delay(10);
        string newText = (text.Remove(0, 10) + "]").Replace("}{", "},{");
        Debug.Log(newText);

        try
        {
            JArray json = JArray.Parse(newText);

            foreach (JObject score in json)
            {
                int player1_id = (int) score["player1_id"];
                int player2_id = (int) score["player2_id"];
                int winner_id = (int) score["winner_id"];
                string player1_name = (string) score["p1_name"];
                string player2_name = (string) score["p2_name"];
                string date = (string) score["date_time"];

                int win = winner_id == AccountManager.playerID ? 1 : 0;
                int opponentID = player1_id == AccountManager.playerID ? player2_id : player1_id;
                string opponentName = opponentID == player1_id ? player1_name : player2_name;

                Instantiate(recentScorePrefab, recentScoresParent.transform)
                    .GetComponent<RecentScoreTemplate>()
                    .DisplayData(AccountManager.userName, win.ToString(), date, opponentName);
            }
        }
        catch(Exception e)
        {
            ShowNoScoreText();
        }
    }

    public async void ParseLeaderboardScores(string text)
    {
        await Task.Delay(10);

        // Whoopsie
        string newText = (text.Remove(0, 10) + "]").Replace("}{", "},{");
        try
        {
            JArray json = JArray.Parse(newText);

            foreach (JObject score in json)
            {
                int winCount = (int)score["wins"];
                int winner_id = (int)score["winner_id"];
                string winner_name = (string)score["winner_name"];

                Instantiate(leaderboardScoreTemplate, leaderboardParent.transform)
                    .GetComponent<MonthlyBestsTemplate>()
                    .DisplayData(winner_name, winCount);
            }
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
            Debug.Log(e.StackTrace);
            ShowNoScoreText();
        }
    }

    private void ShowNoScoreText()
    {
        scoreTextField.GetComponent<TMP_Text>().enabled = true;
        scoreTextField.text = "No recent games";
    }

    private void GoToHomeScreen()
    {
        login_menu.SetActive(false);
        register_menu.SetActive(false);
        home_menu.SetActive(true);
    }
}
