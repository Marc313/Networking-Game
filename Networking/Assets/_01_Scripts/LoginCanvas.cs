using TMPro;
using UnityEngine;

namespace ChatClientExample
{
    public class LoginCanvas : MonoBehaviour
    {
        public TMP_InputField field;
        public GameObject chatCanvas;
        public ClientManager clientManager;

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Return))
            {
                SubmitName();
            }
        }

        private void SubmitName()
        {
            string name = field.text;

            clientManager.gameObject.SetActive(true);
            clientManager.SetName(name);
            field.text = string.Empty;
            gameObject.SetActive(false);
            chatCanvas.gameObject.SetActive(true);
        }
    }
}
