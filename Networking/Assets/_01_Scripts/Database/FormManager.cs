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

    private void Start()
    {
        login_submitButton.onClick.AddListener(SubmitLogin);
        register_submitButton.onClick.AddListener(SubmitRegistration);
    }

    public async void SubmitLogin()
    {
        string phpUrl = $"https://studenthome.hku.nl/~marc.neeleman/userlogin.php?usermail={login_mailField.text}&password={login_passwordField.text}";
        string response = await PostURL(phpUrl);

        if (CheckErrors(response))
        {
            JObject user = JObject.Parse(response);
            AccountManager.playerID = (uint) user["id"];
            AccountManager.userName = (string) user["name"];
            AccountManager.userMail = (string) user["email"];

            login_menu.SetActive(false);
            home_menu.SetActive(true);
        }
        else
        {
            Debug.Log("Login failed!");
        }
    }

    public async void SubmitRegistration()
    {
        string phpUrl = $"https://studenthome.hku.nl/~marc.neeleman/registerUser.php?email={register_mailField.text}&username={register_usernameField.text}&password={register_passwordField.text}&session_id=lbem30ojq32uda24e0g2b9uebl";
        string response = await PostURL(phpUrl);

        if (CheckErrors(response))
        {
            register_menu.SetActive(false);
            home_menu.SetActive(true);

            // userName = register_usernameField.text;
        }
        else
        {
            Debug.Log("Registration failed!");
        }
    }

    public async void LoadDefaultScores()
    {
        string phpUrl = $"https://studenthome.hku.nl/~marc.neeleman/recentplayersscores.php?username={AccountManager.userName}";
        string response = await PostURL(phpUrl);

        if (CheckErrors(response))
        {
            ParseScores(response);
        }
        else
        {
            scoreTextField.text = "Something went wrong, try again later";
        }
    }

    public async void InsertScore(int player1ID, int player2ID, int winnerID)
    {
        string phpUrl = $"https://studenthome.hku.nl/~marc.neeleman/scoreinsert.php?player1_id={player1ID}&player2_id={player2ID}&winner_id={winnerID}";
        string response = await PostURL(phpUrl);
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

    IEnumerator PostUserLogin(string userName, string password)
    {
        /*      // TODO: Server Authentication...

                // yield op die return...

                // TODO: Dit via een UI

                //Use WWWForm with Post request

                // WWWForm form = new WWWForm();
                // form.AddField("username", "test");
                // form.AddField("password", "test");
                // // TODO: valid id from Server Auth
                // form.AddField("sessionid", "cmj8fqtdh50fborr4tl63b46tdhord057uu19nhlvkrgqmfikdd1");*/

        string phpUrl = $"https://studenthome.hku.nl/~marc.neeleman/userlogin.php?usermail={userName}&password={password}";
        using (UnityWebRequest www = UnityWebRequest.Get(phpUrl))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(www.error);
            }
            else
            {
                Debug.Log(www.downloadHandler.text);

                if (!www.downloadHandler.text.Contains("0"))
                {
                    LoadNextScene();
                }
                else
                {
                    Debug.Log("Login failed!");
                }
            }
        }

        yield return null;
    }

    private void LoadNextScene()
    {
        SceneManager.LoadScene(1);
    }

    public bool CheckErrors(string response)
    {
        if (response[0] != '0') return true;
        else
        {
            Debug.LogError("Received PHP Error code 0");
            return false;
        }
    }

    public async void ParseScores(string text)
    {
        await Task.Delay(10);
        string newText = (text.Remove(0, 10) + "]").Replace("}{", "},{");
        Debug.Log(newText);

        try
        {
            JArray json = JArray.Parse(newText);

            foreach (JObject score in json)
            {
                string date = (string)score["date_time"];
                string wins = (string)score["score"];

                Instantiate(recentScorePrefab, recentScoresParent.transform)
                    .GetComponent<RecentScoreTemplate>()
                    .DisplayData(AccountManager.userName, wins, date);
            }
        }
        catch(Exception e)
        {
            ShowNoScoreText();
        }
    }

    private void ShowNoScoreText()
    {
        scoreTextField.GetComponent<TMP_Text>().enabled = true;
        scoreTextField.text = "No recent games";
    }
}
