using System.Collections.Generic;
using UnityEngine;

public class NetworkManager : Singleton<NetworkManager>
{
    public ObjectPrefabs prefabs;

    public static uint GetNextID => nextID++;
    private static uint nextID = 1;

    public Dictionary<uint, NetworkedObject> objects = new Dictionary<uint, NetworkedObject>();

    private void Awake()
    {
        Instance = this;
    }

    public NetworkedObject Create(uint id, string prefabKey, bool isServer, Vector3 position, Quaternion rotation)
    {
        if (!prefabs.Contains(prefabKey))
        {
            Debug.LogError("Failed to create networked object");
            return null;
        }

        NetworkedObject prefab = prefabs.Get(prefabKey);
        NetworkedObject newObject = Instantiate(prefab, position, rotation);
        newObject.networkedID = id == 0 ? GetNextID : id;
        newObject.isServer = isServer;
        objects.Add(newObject.networkedID, newObject);
        return newObject;
    }

    public bool Destroy(uint id)
    {
        if (objects.ContainsKey(id))
        {
            NetworkedObject currentObject = objects[id];
            objects.Remove(id);
            Destroy(currentObject.gameObject);
            return true;
        }
        return false;
    }
}
