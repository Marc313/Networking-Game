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
        newObject.OnCreate();

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

    public bool GetObject(uint id, out NetworkedObject networkedObject)
    {
        if (objects.ContainsKey(id))
        {
            networkedObject = objects[id];
            return true;
        }

        networkedObject = null;
        return false;
    }

    public void SendRPCMessage(NetworkedObject target, string methodName, params object[] data)
    {
        FindObjectOfType<Client>().SendRPCMessage(target, methodName, data);
    }
}
