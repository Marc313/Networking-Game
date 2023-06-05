using UnityEngine;

public class NetworkedObject : MonoBehaviour
{
    public uint networkedID;
    public bool isServer;

    public virtual void OnCreate()
    {

    }
}
