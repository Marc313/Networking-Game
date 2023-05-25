using UnityEngine;

public class ItemPickup : NetworkedObject
{
    public Item item;

    private void OnTriggerEnter(Collider other)
    {
        LocalPlayer player = other.GetComponent<LocalPlayer>();
        // player.GiveItem();

        gameObject.SetActive(false);
        // Destroy
    }
}