using TMPro;
using UnityEngine;

namespace ChatClientExample
{
    public class ChatCanvas : MonoBehaviour
    {
        public TMP_InputField field;
        public TMP_Text messageText;

        public void NewMessage(string _message)
        {
            messageText.text += "\n" + _message;
        }

        private void Start()
        {
            messageText.text = "";
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Return))
            {
                SendMessage();
            }
        }

        private void SendMessage()
        {
            string message = field.text;

            FindObjectOfType<ClientManager>().SendChatMessage(message);
            field.text = string.Empty;
        }

    }
}
