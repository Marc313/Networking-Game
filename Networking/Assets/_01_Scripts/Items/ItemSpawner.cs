using UnityEngine;

public class ItemSpawner : Singleton<ItemSpawner>
{
    [Tooltip("The probability that an item will spawn after a round")]
    [SerializeField] private float itemSpawnProbability = 0.3f;
    [SerializeField] private Item[] items;

    private void Awake()
    {
        Instance = this;
    }

    public int TryGetItem()
    {
        if (Random.Range(0f, 1f) < itemSpawnProbability)
        {
            // Return the id of one of the items randomly
            return 1;
        }

        // -1 means that no item should be spawned
        return -1;
    }

    public Item GetItemWithID(int id)
    {
        return items[id];
    }   
}
