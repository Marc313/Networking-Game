using UnityEngine;
using Unity.Networking.Transport;

namespace ChatClientExample
{
    public class ClientManager : MonoBehaviour
    {
        public NetworkDriver m_Driver;
        public NetworkConnection m_Connection;
        public bool m_Done;

        public string clientName;

        void Start()
        {
            m_Driver = NetworkDriver.Create();
            m_Connection = default(NetworkConnection);

            var endpoint = NetworkEndPoint.LoopbackIpv4;
            endpoint.Port = 9000;
            m_Connection = m_Driver.Connect(endpoint);
        }

        public void OnDestroy()
        {
            m_Connection.Disconnect(m_Driver);
            m_Connection = default(NetworkConnection);
            m_Driver.Dispose();
        }

        void Update()
        {
            m_Driver.ScheduleUpdate().Complete();

            if (!m_Connection.IsCreated)
            {
                if (!m_Done)
                    Debug.Log("Something went wrong during connect");
                return;
            }

            DataStreamReader stream;
            NetworkEvent.Type cmd;

            while ((cmd = m_Connection.PopEvent(m_Driver, out stream)) != NetworkEvent.Type.Empty)
            {
                if (cmd == NetworkEvent.Type.Connect)
                {
                    Debug.Log("We are now connected to the server");
                    uint messageType = 0;
                    string message = clientName;

                    if (message != null && message != string.Empty)
                    {
                        Debug.Log("Sending message: " + message);
                        m_Driver.BeginSend(m_Connection, out var writer);
                        writer.WriteUInt(messageType);
                        writer.WriteFixedString512(message);
                        m_Driver.EndSend(writer);
                    }
                }
                else if (cmd == NetworkEvent.Type.Data)
                {
                    uint value = stream.ReadUInt();
                    Debug.Log("Got the value = " + value + " back from the server");
                    m_Done = true;
                }
                else if (cmd == NetworkEvent.Type.Disconnect)
                {
                    Debug.Log("Client got disconnected from server");
                    m_Driver.BeginSend(m_Connection, out var writer);
                    writer.WriteUInt(3);
                    m_Driver.EndSend(writer);
                    m_Connection = default(NetworkConnection);
                }
            }
        }

        public void SendChatMessage(string _message)
        {
            uint messageType = 2;

            if (_message != null && _message != string.Empty)
            {
                Debug.Log("Sending message: " + _message);
                m_Driver.BeginSend(m_Connection, out var writer);
                writer.WriteUInt(messageType);
                writer.WriteFixedString512(_message);
                m_Driver.EndSend(writer);
            }
        }

        public void SetName(string _name)
        {
            clientName = _name;
        }

        private void MarkTile(uint x, uint y)
        {
            FindObjectOfType<GridManager>().MarkTile(x, y);
        }
    }
}