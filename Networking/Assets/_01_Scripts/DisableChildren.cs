using UnityEngine;
using UnityEngine.UI;

public class DisableChildren : MonoBehaviour
{
    public void EnableAllChildButtons(bool enabled)
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            Button button = transform.GetChild(i).GetComponent<Button>();
            if (button != null)
            {
                button.enabled = enabled;
            }
        }
    }
}
