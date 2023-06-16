using UnityEngine;
using UnityEngine.UI;

public class ItemButton : MonoBehaviour
{
    [SerializeField] private Image itemIcon;

    public void UseItem()
    {
        PlayerManager.Instance.GetLocalPlayer().UseItem();
    }

    public void DisableSelf()
    {
        if (itemIcon != null)
        {
            itemIcon.sprite = null;
            itemIcon.gameObject.SetActive(false);
        }
    }
}
