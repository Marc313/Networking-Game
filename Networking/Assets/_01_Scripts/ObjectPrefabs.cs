using System;
using UnityEngine;

[CreateAssetMenu(menuName = "ObjectPrefabs")]
public class ObjectPrefabs : ScriptableObject
{
    public ObjectPrefabEntry[] prefabs;

    public NetworkedObject Get(string key)
    {
        foreach (ObjectPrefabEntry prefabEntry in prefabs)
        {
            if (prefabEntry.key.ToLower() == key.ToLower())
            {
                return prefabEntry.networkedObject;
            }
        }

        throw new System.Exception($"Networked Prefablist did not contain the key {key}");
    }

    public bool Contains(string key)
    {
        foreach (ObjectPrefabEntry prefabEntry in prefabs)
        {
            if (prefabEntry.key.ToLower() == key.ToLower())
            {
                return true;
            }
        }
        return false;
    }
}

[Serializable]
public class ObjectPrefabEntry
{
    public string key;
    public NetworkedObject networkedObject;
}