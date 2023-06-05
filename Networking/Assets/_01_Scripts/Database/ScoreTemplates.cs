using TMPro;
using UnityEngine;

public abstract class ScoreTemplates : MonoBehaviour
{
    [SerializeField] protected TMP_Text userNameText;

    public virtual void DisplayData(string userName)
    {
        userNameText.text = userName;
    }
}
