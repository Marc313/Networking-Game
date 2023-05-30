using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class FormManager : MonoBehaviour
{
    public TMP_InputField nameField;
    public TMP_InputField passwordField;
    private Button submitButton;

    private void Start()
    {
        submitButton = GetComponent<Button>();
        submitButton.onClick.AddListener(Submit);
    }

    public void Submit()
    {
        Debug.Log($"Name: {nameField.text} and Password: {passwordField.text}");
        StartCoroutine(PostUserLogin(nameField.text, passwordField.text));
    }

    IEnumerator PostUserLogin(string userName, string password)
    {
/*        // TODO: Server Authentication...

        // yield op die return...

        // TODO: Dit via een UI

        //Use WWWForm with Post request

        // WWWForm form = new WWWForm();
        // form.AddField("username", "test");
        // form.AddField("password", "test");
        // // TODO: valid id from Server Auth
        // form.AddField("sessionid", "cmj8fqtdh50fborr4tl63b46tdhord057uu19nhlvkrgqmfikdd1");*/

        using (UnityWebRequest www = UnityWebRequest.Get($"https://studenthome.hku.nl/~marc.neeleman/userlogin.php?username={userName}&password={password}"))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(www.error);
            }
            else
            {
                Debug.Log(www.downloadHandler.text);

                // We're probably expecting something in return:
                // JObject json = JObject.Parse(www.downloadHandler.text);
                // int sessionID = (int)json["sessionid"];
                // Debug.Log("Login Complete!");
            }
        }

        yield return null;
    }
}
