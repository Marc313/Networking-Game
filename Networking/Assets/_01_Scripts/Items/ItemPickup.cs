using UnityEngine;

public class ItemPickup : NetworkedObject
{
    public Item item;

    private void OnTriggerEnter(Collider other)
    {
        // LocalPlayer player = other.GetComponent<LocalPlayer>();
        // player.GiveItem();

        APlayer player = other.GetComponent<APlayer>();

        if (player != null && isServer)
        {
            // Send message to player
            Server server = FindObjectOfType<Server>();
            server.BroadcastItemReceive(server, player.playerID, item);

            // Send message about item
            RPCExecute(nameof(OnCollected));
        }
        // Destroy
    }

    public void OnCollected()
    {
        gameObject.SetActive(false);
    }
}