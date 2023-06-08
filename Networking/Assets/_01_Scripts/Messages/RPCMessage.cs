using System.Reflection;
using Unity.Networking.Transport;
using UnityEngine;


// Client / Server implementation of this class
/*
static void HandleRPC(Client client, MessageHeader header) {
    RPCMessage msg = header as RPCMessage;

    // try to call the function
    try {
        msg.mInfo.Invoke(msg.target, msg.data);
    }
    catch (System.Exception e) {
        Debug.Log(e.Message);
        Debug.Log(e.StackTrace);
    }
}
*/

namespace ChatClientExample
{
    public class RPCMessage
    {
        public NetworkedObject target;
        public string methodName;
        public object[] data;

        public uint targetId;
        public MethodInfo mInfo;
        public ParameterInfo[] parameters;

        public RPCMessage()
        {

        }

        public RPCMessage(NetworkedObject target, string methodName, object[] data)
        {
            this.target = target;
            this.methodName = methodName;
            this.data = data;
        }

        public void SerializeObject(ref DataStreamWriter writer)
        {
            writer.WriteUInt((uint)target.networkedID);
            writer.WriteFixedString128(methodName);

            // Get method information
            mInfo = target.GetType().GetMethod(methodName);
            if (mInfo == null)
            {
                throw new System.ArgumentException($"Object of type {target.GetType()} does not contain method called {methodName}");
            }

            // Get Function Parameters, and write them one in order
            parameters = mInfo.GetParameters();
            for (int i = 0; i < parameters.Length; ++i)
            {
                if (parameters[i].ParameterType == typeof(string))
                {
                    writer.WriteFixedString128((string)data[i]);
                }
                else if (parameters[i].ParameterType == typeof(float))
                {
                    writer.WriteFloat((float)data[i]);
                }
                else if (parameters[i].ParameterType == typeof(int))
                {
                    writer.WriteInt((int)data[i]);
                }
                else if (parameters[i].ParameterType == typeof(uint))
                {
                    writer.WriteUInt((uint)data[i]);
                }
                else if (parameters[i].ParameterType == typeof(Vector3))
                {
                    Vector3 vec = (Vector3)data[i];
                    writer.WriteFloat(vec.x);
                    writer.WriteFloat(vec.y);
                    writer.WriteFloat(vec.z);
                }
                else
                {
                    throw new System.ArgumentException($"Unhandled RPC Parameter: {parameters[i].ParameterType.ToString()}");
                }
            }
        }

        public void DeserializeObject(ref DataStreamReader reader)
        {
            targetId = reader.ReadUInt();
            methodName = reader.ReadFixedString128().ToString();

            // Get Object reference, and extract the MethodInfo from it's NetworkedBehaviour
            //	-> this works because the object itself knows its type (e.g. PlayerBehaviour), even if we treat it as its base (NetworkedBehaviour)
            NetworkedObject obj;
            if (NetworkManager.Instance.GetObject(targetId, out obj))
            {
                target = obj.GetComponent<NetworkedObject>();
                mInfo = target.GetType().GetMethod(methodName);
                if (mInfo == null)
                {
                    throw new System.ArgumentException($"Object of type {target.GetType()} does not contain method called {methodName}");
                }
            }
            else
            {
                Debug.LogError($"Could not find object with id {targetId}");
            }

            // Extract Parameter info
            parameters = mInfo.GetParameters();

            // Prepare Data to store parameters into
            data = new object[parameters.Length];

            // Read each parameter in order
            for (int i = 0; i < parameters.Length; ++i)
            {
                if (parameters[i].ParameterType == typeof(string))
                {
                    data[i] = reader.ReadFixedString128().ToString();
                }
                else if (parameters[i].ParameterType == typeof(float))
                {
                    data[i] = reader.ReadFloat();
                }
                else if (parameters[i].ParameterType == typeof(int))
                {
                    data[i] = reader.ReadInt();
                }
                else if (parameters[i].ParameterType == typeof(uint))
                {
                    data[i] = reader.ReadUInt();
                }
                else if (parameters[i].ParameterType == typeof(Vector3))
                {
                    data[i] = new Vector3(reader.ReadFloat(), reader.ReadFloat(), reader.ReadFloat());
                }
                else
                {
                    throw new System.ArgumentException($"Unhandled RPC Type: {parameters[i].ParameterType.ToString()}");
                }
            }
        }
    }
}