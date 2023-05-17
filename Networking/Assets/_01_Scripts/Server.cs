using System.Collections.Generic;
using Unity.Collections;
using Unity.Networking.Transport;
using UnityEngine;

public delegate void NetworkMessageHandler(object handler, NetworkConnection con, DataStreamReader stream);

public enum NetworkMessageType
{
    PLAYER_JOINED = 0,          // Acts as handshake
    REMOTE_PLAYER_JOINED = 7,
    HANDSHAKE_RESPONSE = 1,
    PLAYER_MOVED = 2,
    PLAYER_QUIT = 3,
    MOVE_CONFIRM = 4,
    SEND_OPPONENT_CHOICE = 5,
    SEND_PLAYER_ID = 6,          // Acts as handshake response
}

public class Server : MonoBehaviour
{
    static Dictionary<NetworkMessageType, NetworkMessageHandler> networkMessageHandlers = new Dictionary<NetworkMessageType, NetworkMessageHandler> {
            { NetworkMessageType.PLAYER_JOINED, HandleClientJoined },
            { NetworkMessageType.PLAYER_MOVED, HandlePlayerMoved },
            { NetworkMessageType.PLAYER_QUIT, HandleClientExit },
        };

    public NetworkDriver m_Driver;
    private NativeList<NetworkConnection> m_Connections;
    private Dictionary<NetworkConnection, uint> nameList = new Dictionary<NetworkConnection, uint>();

    private void Start()
    {
        StartServer();
        m_Connections = new NativeList<NetworkConnection>(16, Allocator.Persistent);
    }

    private void StartServer()
    {
        m_Driver = NetworkDriver.Create();
        var endpoint = NetworkEndPoint.AnyIpv4;
        endpoint.Port = 9000;

        if (m_Driver.Bind(endpoint) != 0)
            Debug.Log("Failed to bind to port 9000");
        else
            m_Driver.Listen();
    }

    private void Update()
    {
        m_Driver.ScheduleUpdate().Complete();

        UpdateConnections();
        HandleMessages();

        // If all players are ready or other game start condition
        
    }

    private void HandleMessages()
    {
        DataStreamReader stream;
        for (int i = 0; i < m_Connections.Length; i++)
        {
            if (!m_Connections[i].IsCreated)
                continue;

            // Loop through available events
            NetworkEvent.Type cmd;
            while ((cmd = m_Driver.PopEventForConnection(m_Connections[i], out stream)) != NetworkEvent.Type.Empty)
            {
                if (cmd == NetworkEvent.Type.Data)
                {
                    // First UInt is always message type (this is our own first design choice)
                    NetworkMessageType msgType = (NetworkMessageType)stream.ReadUInt();
                    Debug.Log("Type:" + msgType);

                    if (networkMessageHandlers.ContainsKey(msgType))
                    {
                        try
                        {
                            networkMessageHandlers[msgType].Invoke(this, m_Connections[i], stream);
                        }
                        catch
                        {
                            Debug.LogError("Badly formatted message received...");
                        }
                    }
                    else
                    {
                        Debug.LogWarning($"Unsupported message type received: {msgType}", this);
                    }
                }
            }
        }
    }

    static void HandleClientJoined(object handler, NetworkConnection connection, DataStreamReader stream)
    {
        Server serv = handler as Server;
        uint playerID = (uint) serv.nameList.Count;

        // Add to list
        serv.nameList.Add(connection, playerID);
        Debug.Log("New Client Joined with ID: " + playerID);

        // Send player id message back to connection
        int result = serv.m_Driver.BeginSend(NetworkPipeline.Null, connection, out var writer);

        // non-0 is an error code
        if (result == 0)
        {
            uint isPlayersTurn = (uint) (playerID == 0 ? 1 : 0);

            writer.WriteUInt((uint) NetworkMessageType.SEND_PLAYER_ID);
            writer.WriteUInt(playerID);
            writer.WriteUInt(isPlayersTurn);

            serv.m_Driver.EndSend(writer);
        }
        else
        {
            Debug.LogError($"Could not write message to driver: {result}", serv);
        }

        // Send player joined signal to other connections
        foreach(NetworkConnection otherConnection in serv.nameList.Keys)
        {
            if (otherConnection != connection)
            {
                int result2 = serv.m_Driver.BeginSend(NetworkPipeline.Null, otherConnection, out var writer2);

                // non-0 is an error code
                if (result2 == 0)
                {
                    writer.WriteUInt((uint) NetworkMessageType.REMOTE_PLAYER_JOINED);
                    writer.WriteUInt(playerID);

                    serv.m_Driver.EndSend(writer);
                }
                else
                {
                    Debug.LogError($"Could not write message to driver: {result2}", serv);
                }
            }
        }
    }

    static void HandlePlayerMoved(object handler, NetworkConnection connection, DataStreamReader stream)
    {
        // Pop message
        uint playerX = stream.ReadUInt();
        uint playerY = stream.ReadUInt();
        Debug.Log($"Received Message: ({playerX}, {playerY})");

        Server serv = handler as Server;

        if (serv.nameList.ContainsKey(connection))
        {
            // Send confirm message back
            DataStreamWriter writer;
            int result = serv.m_Driver.BeginSend(NetworkPipeline.Null, connection, out writer);

            // non-0 is an error code
            if (result == 0)
            {
                writer.WriteUInt((uint)NetworkMessageType.MOVE_CONFIRM);
                serv.m_Driver.EndSend(writer);

                // For all other connections:
                // Send opponent move
                foreach (NetworkConnection opponent in serv.nameList.Keys)
                {
                    if (opponent == connection) continue;

                    serv.m_Driver.BeginSend(NetworkPipeline.Null, opponent, out writer);
                    writer.WriteUInt((uint) NetworkMessageType.SEND_OPPONENT_CHOICE);
                    writer.WriteUInt(serv.nameList[connection]);
                    writer.WriteUInt(playerX);
                    writer.WriteUInt(playerY);
                    serv.m_Driver.EndSend(writer);
                }
            }
            else
            {
                Debug.LogError($"Could not write message to driver: {result}", serv);
            }
        }
        else
        {
            Debug.LogError($"Received message from unlisted connection");
        }
    }

    static void HandleClientExit(object handler, NetworkConnection connection, DataStreamReader stream)
    {
        Server serv = handler as Server;

        if (serv.nameList.ContainsKey(connection))
        {
            // Inform all players that a player has left the game. Auto-win ;)
            Debug.Log("Disconnect");
            connection.Disconnect(serv.m_Driver);
        }
        else
        {
            Debug.LogError("Received exit from unlisted connection");
        }
    }

    private void UpdateConnections()
    {
        // Clean up connections
        for (int i = 0; i < m_Connections.Length; i++)
        {
            if (!m_Connections[i].IsCreated)
            {
                m_Connections.RemoveAtSwapBack(i);
                --i;
            }
        }

        // Accept new connections
        NetworkConnection c;
        while ((c = m_Driver.Accept()) != default(NetworkConnection))
        {
            m_Connections.Add(c);
            Debug.Log("Accepted a connection");
        }
    }

    private void OnDestroy()
    {
        // Clean up network driver
        if (m_Driver.IsCreated)
        {
            m_Driver.Dispose();
            m_Connections.Dispose();
        }
    }
}