using UnityEngine;

public class NetworkedObject : MonoBehaviour
{
    public uint networkedID;
    public bool isServer;

    public virtual void OnCreate()
    {

    }

    public void RPCExecute(string methodName, params object[] data)
    {
        NetworkManager.Instance.SendRPCMessage(this, methodName, data);
    }
}
