using ChatClientExample;
using MarcoHelpers;
using System;
using Unity.Networking.Transport;
using UnityEngine;

public class Client : MonoBehaviour
{
    public NetworkDriver networkDriver;
    public NetworkConnection connection;

    private bool isDoneConnecting;
    private uint playerID;
    private static bool hasTurn;

    [SerializeField] private GameObject inGameManagers;

    private void Start()
    {
        // Reset static variable
        hasTurn = false;

        Debug.LogError("This will show up");
        networkDriver = NetworkDriver.Create();
        connection = default(NetworkConnection);

        var endpoint = NetworkEndPoint.LoopbackIpv4;
        endpoint.Port = 9000;
        connection = networkDriver.Connect(endpoint);
    }

    public void OnDestroy()
    {
        connection.Disconnect(networkDriver);
        connection = default(NetworkConnection);
        networkDriver.Dispose();
    }

    void Update()
    {
        networkDriver.ScheduleUpdate().Complete();

        if (!connection.IsCreated)
        {
            if (!isDoneConnecting)
                Debug.Log("Something went wrong during connect");
            return;
        }

        DataStreamReader stream;
        NetworkEvent.Type cmd;

        while ((cmd = connection.PopEvent(networkDriver, out stream)) != NetworkEvent.Type.Empty)
        {
            if (cmd == NetworkEvent.Type.Connect)
            {
                SendHandshake();
            }
            else if (cmd == NetworkEvent.Type.Data)
            {
                ReceiveMessages(stream);
            }
            else if (cmd == NetworkEvent.Type.Disconnect)
            {
                Disconnect();
            }
        }

        if (Input.GetKey(KeyCode.P))
        {
            hasTurn = true;
            UIManager.Instance.SetTurnText(hasTurn);

            EventSystem.RaiseEvent(EventName.LOCAL_MOVE_SENT);
        }
    }

    public void SendPlayerMove(uint x, uint y)
    {
        if (!hasTurn)
        {
            Debug.Log("Client: It is not your turn, player " + playerID);
            return;
        }

        uint messageType = (uint) NetworkMessageType.PLAYER_MOVED;

        Debug.Log($"Sending new position: ({x}, {y})");
        networkDriver.BeginSend(connection, out var writer);
        writer.WriteUInt(messageType);
        writer.WriteUInt(playerID);
        writer.WriteUInt(x);
        writer.WriteUInt(y);
        networkDriver.EndSend(writer);
        hasTurn = false;
        UIManager.Instance.SetTurnText(hasTurn);

        PlayerManager.Instance.GetLocalPlayer().MoveToTile(new Vector3Int((int) x, 0, (int) y));
        EventSystem.RaiseEvent(EventName.LOCAL_MOVE_SENT);
        MarkTile(x, y);
    }

    public void SendPlayerItemUse(object item)
    {
        if (hasTurn)
        {
            Debug.Log("Item use allowed");
        }
        else
        {
            Debug.Log("Not your turn!");
        }
    }

    public void SendRPCMessage(NetworkedObject target, string methodName, object[] data)
    {
        RPCMessage message = new RPCMessage(target, methodName, data);
        uint messageType = (uint)NetworkMessageType.RPC_MESSAGE;

        networkDriver.BeginSend(connection, out var writer);
        writer.WriteUInt(messageType);
        message.SerializeObject(ref writer);

        networkDriver.EndSend(writer);
    }

    private void ReceiveMessages(DataStreamReader stream)
    {
        uint messageType = stream.ReadUInt();

        switch (messageType)
        {
            case (uint)NetworkMessageType.SEND_OPPONENT_CHOICE:
                HandleOpponentTurn(stream);
                break;
            case (uint)NetworkMessageType.MOVE_CONFIRM:
                break;
            case (uint)NetworkMessageType.SEND_PLAYER_ID:
                HandleReceivePlayerID(stream);
                break;
            case (uint)NetworkMessageType.REMOTE_PLAYER_JOINED:
                HandleRemotePlayerJoined(stream);
                break;
            case (uint)NetworkMessageType.GAME_START:
                HandleGameStart(stream);
                break;
            case (uint)NetworkMessageType.SPAWN_OBJECT:
                HandleSpawnObject(stream);
                break;
            case (uint)NetworkMessageType.RECEIVE_ITEM_USE:
                HandleReceiveItemUsage(stream);
                break;
            case (uint)NetworkMessageType.RECEIVE_ITEM:
                HandleObtainItem(stream);
                break;
            case (uint)NetworkMessageType.RPC_MESSAGE:
                HandleRPCMessage(stream);
                break;
            case (uint)NetworkMessageType.SEND_GAME_RESULT:
                HandleGameEnd(stream);
                break;
            default:
                Debug.LogError($"Unrecognized network message: {((NetworkMessageType)messageType).ToString()}");
                break;
        }
    }

    private static void HandleSpawnObject(DataStreamReader reader)
    {
        uint networkedID = reader.ReadUInt();
        string prefabKey = reader.ReadFixedString128().ToString();
        float xPos = reader.ReadFloat();
        float yPos = reader.ReadFloat();
        float zPos = reader.ReadFloat();
        float xRot = reader.ReadFloat();
        float yRot = reader.ReadFloat();
        float zRot = reader.ReadFloat();

        NetworkManager.Instance.Create(networkedID, prefabKey, false, 
                                        new Vector3(xPos, yPos, zPos), 
                                        Quaternion.Euler(xRot, yRot, zRot));
    }

    private void HandleGameStart(DataStreamReader reader)
    {
        uint startingPlayerID = reader.ReadUInt();
        UIManager.Instance.DisableJoinScreen();
        inGameManagers.SetActive(true);

        if (playerID == startingPlayerID)
        {
            ReceiveTurn();
        }
    }

    private void HandleGameEnd(DataStreamReader reader)
    {
        uint winnerID = reader.ReadUInt();

        if (winnerID == playerID)
        {
            UIManager.Instance.ShowWinScreen();
        }
        else
        {
            UIManager.Instance.ShowLoseScreen(winnerID);
        }

        // Load Home scene after 5 seconds or button
    }

    private void ReceiveTurn()
    {
        hasTurn = true;
        PlayerManager.Instance.GetLocalPlayer().OnReceiveTurn();
        UIManager.Instance.SetTurnText(hasTurn);
    }

    private void HandleOpponentTurn(DataStreamReader reader)
    {
        uint opponentPlayerID = reader.ReadUInt();

        if (PlayerManager.Instance.GetRemotePlayer(opponentPlayerID) == null)
        {
            PlayerManager.Instance.CreateRemotePlayer(opponentPlayerID, Vector3Int.zero);
        }

        uint playerX = reader.ReadUInt();
        uint playerY = reader.ReadUInt();
        uint nextPlayerID = reader.ReadUInt();
        Debug.LogError($"Client: Player {opponentPlayerID} moved to {playerX}, {playerY}");
        PlayerManager.Instance.GetRemotePlayer(opponentPlayerID).MoveToTile(new Vector3Int((int)playerX, 0, (int)playerY));

        if (playerID == nextPlayerID)
        {
            ReceiveTurn();
        }
    }

    private void HandleReceivePlayerID(DataStreamReader reader)
    {
        //FindObjectOfType<GridManager>().OnGameStart();
        playerID = reader.ReadUInt();
        Debug.Log($"Client: Received ID of " + playerID);

        PlayerManager.Instance.CreateLocalPlayer(playerID, Vector3Int.zero);

        isDoneConnecting = true;
        UIManager.Instance.SetTurnText(hasTurn);
        UIManager.Instance.SetPlayerIDText(playerID);

        if (playerID > 0)
        {
            for (int previousID = (int)playerID - 1; previousID >= 0; previousID--)
            {
                PlayerManager.Instance.CreateRemotePlayer((uint)previousID, Vector3Int.zero);
            }
        }

    }

    private void HandleRemotePlayerJoined(DataStreamReader reader)
    {
        uint remotePlayerID = reader.ReadUInt();
        PlayerManager.Instance.CreateRemotePlayer(remotePlayerID, Vector3Int.zero);
    }

    private void SendHandshake()
    {
        Debug.Log("We are now connected to the server");
        uint messageType = (uint) NetworkMessageType.PLAYER_JOINED;

        networkDriver.BeginSend(connection, out var writer);
        writer.WriteUInt(messageType);
        writer.WriteUInt(AccountManager.playerID);
        networkDriver.EndSend(writer);
    }

    private void Disconnect()
    {
        Debug.Log("Client got disconnected from server");
        networkDriver.BeginSend(connection, out var writer);
        writer.WriteUInt((uint) NetworkMessageType.PLAYER_QUIT);
        networkDriver.EndSend(writer);
        connection = default(NetworkConnection);
    }

    private void MarkTile(uint x, uint y)
    {
        FindObjectOfType<GridManager>().MarkTile(x, y);
    }

    public void OnUseItem(Item currentItem)
    {
        uint messageType = (uint)NetworkMessageType.SEND_ITEM_USE;

        networkDriver.BeginSend(connection, out var writer);
        writer.WriteUInt(messageType);
        writer.WriteUInt(playerID);
        writer.WriteInt(currentItem.itemID);
        networkDriver.EndSend(writer);
    }

    private void HandleObtainItem(DataStreamReader reader)
    {
        int itemID = reader.ReadInt();

        // Use playermanager and itemmanager.
        Item item = ItemSpawner.Instance.GetItemWithID(itemID);
        PlayerManager.Instance.GetLocalPlayer().ObtainItem(item);
    }

    private void HandleReceiveItemUsage(DataStreamReader reader)
    {
        uint itemUserID = reader.ReadUInt();
        int itemID = reader.ReadInt();

        // Use playermanager and itemmanager.
        APlayer player = PlayerManager.Instance.GetPlayer(itemUserID);
        ItemSpawner.Instance.GetItemWithID(itemID).Use(player);
        if (player is LocalPlayer)
        {
            (player as LocalPlayer).AfterItemUse();
        }
    }

    private void HandleRPCMessage(DataStreamReader stream)
    {
        RPCMessage message = new RPCMessage();
        message.DeserializeObject(ref stream);

        try
        {
            message.mInfo.Invoke(message.target, message.data);
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
            Debug.Log(e.StackTrace);
        }
    }
}