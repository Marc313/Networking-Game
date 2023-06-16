using ChatClientExample;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Networking.Transport;
using Unity.Networking.Transport.Error;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public delegate void NetworkMessageHandler(object handler, NetworkConnection con, DataStreamReader stream);

public enum NetworkMessageType
{
    PLAYER_JOINED = 0,          // Acts as handshake
    HANDSHAKE_RESPONSE = 1,     // Unused
    PLAYER_QUIT = 2,
    REMOTE_PLAYER_JOINED = 3,
    SEND_PLAYER_ID = 4,          // Acts as handshake response
    GAME_START = 5,
    SPAWN_OBJECT = 6,
    DESTROY_OBJECT = 7,
    PLAYER_MOVED = 8,
    MOVE_CONFIRM = 9,
    SEND_OPPONENT_CHOICE = 10,
    SEND_ITEM_USE = 11,
    RECEIVE_ITEM_USE = 12,
    RECEIVE_ITEM = 13,
    RPC_MESSAGE = 14,
    SEND_GAME_RESULT = 15
}

public class Server : MonoBehaviour
{
    static Dictionary<NetworkMessageType, NetworkMessageHandler> networkMessageHandlers = new Dictionary<NetworkMessageType, NetworkMessageHandler> {
            { NetworkMessageType.PLAYER_JOINED, HandleClientJoined },
            { NetworkMessageType.PLAYER_MOVED, HandlePlayerMoved },
            { NetworkMessageType.PLAYER_QUIT, HandleClientExit },
            { NetworkMessageType.SEND_ITEM_USE, HandleItemUse },
            { NetworkMessageType.RPC_MESSAGE, HandleRPCMessage }
        };

    public Button startButton;
    public NetworkDriver m_Driver;

    private NativeList<NetworkConnection> m_Connections;
    private Dictionary<NetworkConnection, uint> connectionList = new Dictionary<NetworkConnection, uint>();
    private Dictionary<uint, uint> databasePlayerIDs = new Dictionary<uint, uint>();        // Key: Game player ID, Value: Database player ID

    private static bool isStarted;
    private static int callsThisFrame = 0;
    private static uint currentPlayerWithTurn;
    private static int maxPlayerCount = 2;

    private void Start()
    {
        StartServer();
        m_Connections = new NativeList<NetworkConnection>(16, Allocator.Persistent);
        if (startButton != null)
        {
            startButton.onClick.RemoveAllListeners();
            startButton.onClick.AddListener(StartGame);
        }

        // Reset static variables
        isStarted = false;
        callsThisFrame = 0;
        currentPlayerWithTurn = 0;
        maxPlayerCount = 2;
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

    private void StartGame()
    {
        // Cast unnecessary
        string gridManagerKey = "gridmanager";
        NetworkedObject gridManager = NetworkManager.Instance.Create(0, gridManagerKey, true, Vector3.zero, Quaternion.identity);
        uint objectID = gridManager.networkedID;
        BroadcastSpawn(this, objectID, gridManagerKey, Vector3.zero, Quaternion.identity);
        BroadcastGameStart(this);
    }

    private void Update()
    {
        m_Driver.ScheduleUpdate().Complete();

        UpdateConnections();
        HandleMessages();

        callsThisFrame = 0;

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
                    Debug.LogError("Type:" + msgType);

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

/*    public static void BroadcastRPCMessage(RPCMessage rpcMessage, Server serv)
    {
        DataStreamWriter writer;
        foreach (NetworkConnection player in serv.nameList.Keys)
        {
            serv.m_Driver.BeginSend(NetworkPipeline.Null, player, out writer);
            rpcMessage.SerializeObject(ref writer);
            serv.m_Driver.EndSend(writer);
        }
    }*/

    private static void HandleRPCMessage(object handler, NetworkConnection connection, DataStreamReader stream)
    {
        Server serv = (Server)handler;

        RPCMessage message = new RPCMessage();

        message.DeserializeObject(ref stream);

        try
        {
            message.mInfo.Invoke(message.target, message.parameters);
        }
        catch (System.Exception e)
        {
            Debug.Log(e.Message);
            Debug.Log(e.StackTrace);
        }

        DataStreamWriter writer;
        foreach (NetworkConnection player in serv.connectionList.Keys)
        {
            serv.m_Driver.BeginSend(NetworkPipeline.Null, player, out writer);
            writer.WriteUInt((uint) NetworkMessageType.RPC_MESSAGE);
            message.SerializeObject(ref writer);
            serv.m_Driver.EndSend(writer);
        }
    }

    private static void HandleClientJoined(object handler, NetworkConnection connection, DataStreamReader stream)
    {
        Server serv = (Server) handler;

        if (isStarted) {
            Debug.Log("Room full!");
            return; 
        }

        uint playerID = (uint) serv.connectionList.Count;
        uint dbPlayerID = stream.ReadUInt();
            
        // Add to list
        serv.connectionList.Add(connection, playerID);
        serv.databasePlayerIDs.Add(playerID, dbPlayerID);
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
        foreach(NetworkConnection otherConnection in serv.connectionList.Keys)
        {
            BroadcastPlayerJoined(connection, serv, playerID, otherConnection);
        }

        UIManager.Instance.UpdatePlayerJoinedCount(serv.connectionList.Count);

        // Check if game is full
        if (serv.connectionList.Count == maxPlayerCount)
        {
            isStarted = true;
            serv.startButton.interactable = true;
        }
    }

    private static void HandlePlayerMoved(object handler, NetworkConnection connection, DataStreamReader stream)
    {
        Server serv = handler as Server;

        // Pop message
        uint playerID = stream.ReadUInt();
        uint playerX = stream.ReadUInt();
        uint playerZ = stream.ReadUInt();
        Debug.Log($"Received Message: ({playerX}, {playerZ})");

        // Check Move validity & Imitate Outcome
        // if (Gridmanager.IsTileValid(movingPosition))
        // Send reject message.

        // Send move to 
        if (serv.connectionList.ContainsKey(connection))
        {
            // Send confirm message back
            DataStreamWriter writer;
            int result = serv.m_Driver.BeginSend(NetworkPipeline.Null, connection, out writer);

            // non-0 is an error code
            if (result == 0)
            {
                writer.WriteUInt((uint)NetworkMessageType.MOVE_CONFIRM);
                serv.m_Driver.EndSend(writer);

                uint nextPlayerID = (uint)GetNextPlayerID(playerID, serv.connectionList.Count);
                currentPlayerWithTurn = nextPlayerID;
                SendPlayerMove(connection, serv, nextPlayerID, playerX, playerZ, writer);

                if (nextPlayerID == 0)
                {
                    TrySpawnItem(serv);
                }
            }
            else
            {
                Debug.LogError($"Could not write message to driver: {result}", serv);
            }
        }
        else
        {
            Debug.LogError($"Server: Unrecognized connection!");
        }

        // Check win condition
        CheckWin(serv);
    }

    private static void CheckWin(Server serv)
    {
        foreach (APlayer player in PlayerManager.Instance.GetAllPlayers())
        {
            uint playerID = (uint) player.playerID;
            if (GridManager.IsLosePosition(player.currentPosition))
            {
                // For 2 players a loss also means a win
                Debug.LogError($"PLAYER {playerID} LOST");

                // Broadcast result
                uint winnerID = (uint)GetNextPlayerID(playerID, serv.connectionList.Count);   // Winner always the other player
                BroadcastGameResult(serv, winnerID);

                // Save result in database
                uint winnerDatabaseID = serv.databasePlayerIDs[winnerID];
                Debug.LogError($"WINNING PLAYER ID: {winnerDatabaseID}");
                FindObjectOfType<FormManager>().InsertScore((int)serv.databasePlayerIDs[playerID], (int)winnerDatabaseID, (int)winnerDatabaseID);
            }
        }
    }

    private static void SendPlayerMove(NetworkConnection connection, Server serv, uint nextPlayerID, uint playerX, uint playerY, DataStreamWriter writer)
    {
        // TODO: Only send destroy and move
        // For all other connections:
        // Send opponent move
        foreach (NetworkConnection opponent in serv.connectionList.Keys)
        {
            if (opponent == connection) continue;

            serv.m_Driver.BeginSend(NetworkPipeline.Null, opponent, out writer);
            writer.WriteUInt((uint)NetworkMessageType.SEND_OPPONENT_CHOICE);       // Message Type
            writer.WriteUInt(serv.connectionList[connection]);                            // PlayerID of player that moved
            writer.WriteUInt(playerX);                                              // New x of player
            writer.WriteUInt(playerY);                                              // New y of player
            writer.WriteUInt(nextPlayerID);                                         // PlayerID of next player
            serv.m_Driver.EndSend(writer);

            //GridManager.GetTile(new Vector3Int((int)playerX, 0, (int)playerY)).Disappear();
            //NetworkManager.Instance.Destroy(GridManager.GetTile(new Vector3Int((int)playerX, 0, (int)playerY)).networkedID);
        }
    }

    private static void HandleItemUse(object handler, NetworkConnection connection, DataStreamReader stream)
    {
        Server serv = (Server) handler;

        uint playerID = stream.ReadUInt();
        int itemID = stream.ReadInt();

        if (playerID == currentPlayerWithTurn)
        {
            foreach (NetworkConnection player in serv.connectionList.Keys)
            {
                DataStreamWriter writer;
                serv.m_Driver.BeginSend(NetworkPipeline.Null, player, out writer);
                writer.WriteUInt((uint)NetworkMessageType.RECEIVE_ITEM_USE);
                writer.WriteUInt(playerID);
                writer.WriteInt(itemID);
                serv.m_Driver.EndSend(writer);
            }
        }
        else
        {
            Debug.LogError("Item use blokked, not players turn");
        }

        CheckWin(serv);
    }

    private static void TrySpawnItem(Server serv)
    {
        int itemID = ItemSpawner.Instance.TryGetItem();
        if (itemID != -1)
        {
            string prefabKey = "item" + itemID;
            Vector3Int tilePosition = GridManager.GetRandomExistingTilePosition();
            Vector3 actualPosition = new Vector3(tilePosition.x, 1f, tilePosition.z);

            // Spawn item
            NetworkedObject itemObject = NetworkManager.Instance.Create(0, prefabKey, true, actualPosition, Quaternion.identity);

            // Broadcast item spawn to all connections
            BroadcastSpawn(serv, itemObject.networkedID, prefabKey, actualPosition, Quaternion.identity);
        }
    }

    private static int GetNextPlayerID(uint currentPlayerID, int numOfConnections)
    {
        return ((int)currentPlayerID + 1) % numOfConnections;
    }

    private static void BroadcastPlayerJoined(NetworkConnection connection, Server serv, uint playerID, NetworkConnection otherConnection)
    {
        if (otherConnection != connection)
        {
            int result2 = serv.m_Driver.BeginSend(NetworkPipeline.Null, otherConnection, out var writer2);

            // non-0 is an error code
            if (result2 == 0)
            {
                writer2.WriteUInt((uint)NetworkMessageType.REMOTE_PLAYER_JOINED);
                writer2.WriteUInt(playerID);

                serv.m_Driver.EndSend(writer2);
            }
            else
            {
                Debug.LogError($"Could not write message to driver: {result2}", serv);
            }
        }
    }

    private static void BroadcastGameStart(object handler)
    {
        Server serv = handler as Server;

        foreach (NetworkConnection connection in serv.connectionList.Keys)
        {
            int result = serv.m_Driver.BeginSend(NetworkPipeline.Null, connection, out var writer);

            // non-0 is an error code
            if (result == 0)
            {
                writer.WriteUInt((uint)NetworkMessageType.GAME_START);
                writer.WriteUInt(0);

                serv.m_Driver.EndSend(writer);
                isStarted = true;
            }
            else
            {
                Debug.LogError($"Could not write message to driver: {result}", serv);
            }
        }
    }

    private static void BroadcastGameResult(object handler, uint winnerID)
    {
        Server serv = handler as Server;

        foreach (NetworkConnection connection in serv.connectionList.Keys)
        {
            int result = serv.m_Driver.BeginSend(NetworkPipeline.Null, connection, out var writer);

            // non-0 is an error code
            if (result == 0)
            {
                writer.WriteUInt((uint)NetworkMessageType.SEND_GAME_RESULT);
                writer.WriteUInt(winnerID);

                serv.m_Driver.EndSend(writer);
                isStarted = false;
            }
            else
            {
                Debug.LogError($"Could not write message to driver: {result}", serv);
            }
        }
    }


    private static void HandleClientExit(object handler, NetworkConnection connection, DataStreamReader stream)
    {
        Server serv = handler as Server;

        if (serv.connectionList.ContainsKey(connection))
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

    public static void BroadcastSpawn(object handler, uint networkedID, string prefabKey, Vector3 position, Quaternion rotation)
    {
        Server serv = handler as Server;

        foreach (NetworkConnection connection in serv.connectionList.Keys)
        {
            if (connection == serv.connectionList.Keys.ToArray()[0]) continue;

            int result = serv.m_Driver.BeginSend(NetworkPipeline.Null, connection, out var writer);
            callsThisFrame++;
            

            if (callsThisFrame > 10)
            {
                serv.m_Driver.ScheduleUpdate().Complete();
            }

            // non-0 is an error code
            if (result == 0)
            {
                writer.WriteUInt((uint)NetworkMessageType.SPAWN_OBJECT);
                writer.WriteUInt(networkedID);
                writer.WriteFixedString128(prefabKey);
                writer.WriteFloat(position.x);
                writer.WriteFloat(position.y);
                writer.WriteFloat(position.z);
                writer.WriteFloat(rotation.eulerAngles.x);
                writer.WriteFloat(rotation.eulerAngles.y);
                writer.WriteFloat(rotation.eulerAngles.z);

                serv.m_Driver.EndSend(writer);
            }
            else
            {
                Debug.LogError($"Could not write message to driver: {(StatusCode) result}", serv);
            }
        }
    }

    public void BroadcastItemReceive(object handler, int playerID, Item item)
    {
        Server serv = (Server)handler;

        NetworkConnection targetPlayer;
        if (GetConnection(serv, playerID, out targetPlayer))
        {
            int result = serv.m_Driver.BeginSend(NetworkPipeline.Null, targetPlayer, out var writer);
            if (result == 0)
            {
                writer.WriteUInt((uint)NetworkMessageType.RECEIVE_ITEM);
                writer.WriteInt(item.itemID);
                serv.m_Driver.EndSend(writer);
            }
        }
        else
        {
            Debug.LogError("Could not find connection");
        }

    }

    public bool GetConnection(object handler, int playerID, out NetworkConnection targetConnection)
    {
        Server serv = (Server) handler;

        foreach (NetworkConnection connection in serv.connectionList.Keys)
        {
            if (serv.connectionList.ContainsKey(connection))
            {
                if (serv.connectionList[connection] == playerID)
                {
                    targetConnection = connection;
                    return true;
                }
            }
        }

        targetConnection = default;
        return false;
    }
}