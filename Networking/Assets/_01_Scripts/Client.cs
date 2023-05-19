using MarcoHelpers;
using System;
using Unity.Networking.Transport;
using UnityEngine;

public class Client : MonoBehaviour
{
    public NetworkDriver networkDriver;
    public NetworkConnection connection;

    // To character manager?
    public LocalPlayer localPlayerPrefab;
    public RemotePlayer remotePlayerPrefab;

    private LocalPlayer localPlayer;
    private RemotePlayer[] remotePlayers = new RemotePlayer[4];

    private bool isDoneConnecting;
    private uint playerID;
    private static bool hasTurn;

    private void Start()
    {
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
        writer.WriteUInt(x);
        writer.WriteUInt(y);
        networkDriver.EndSend(writer);
        hasTurn = false;
        UIManager.Instance.SetTurnText(hasTurn);

        MarkTile(x, y);
        localPlayer.MoveToTile(new Vector3Int((int) x, 0, (int) y));
        EventSystem.RaiseEvent(EventName.LOCAL_MOVE_SENT);
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

    private void ReceiveMessages(DataStreamReader stream)
    {
        uint messageType = stream.ReadUInt();

        if (messageType == (uint)NetworkMessageType.SEND_OPPONENT_CHOICE)
        {
            HandleOpponentTurn(stream);
        }
        else if (messageType == (uint)NetworkMessageType.SEND_PLAYER_ID)
        {
            HandleReceivePlayerID(stream);
        }
        else if (messageType == (uint) NetworkMessageType.REMOTE_PLAYER_JOINED)
        {
            HandleRemotePlayerJoined(stream);
        }
        else if (messageType == (uint) NetworkMessageType.GAME_START)
        {
            HandleGameStart(stream);
        }
    }

    private void HandleGameStart(DataStreamReader stream)
    {
        uint startingPlayerID = stream.ReadUInt();

        if (playerID == startingPlayerID)
        {
            ReceiveTurn();
        }
    }

    private void ReceiveTurn()
    {
        hasTurn = true;
        localPlayer.OnReceiveTurn();
        UIManager.Instance.SetTurnText(hasTurn);
    }

    private void HandleOpponentTurn(DataStreamReader stream)
    {
        uint opponentPlayerID = stream.ReadUInt();

        if (remotePlayers[opponentPlayerID] == null)
        {
            CreateRemotePlayer((int)opponentPlayerID);
        }

        uint playerX = stream.ReadUInt();
        uint playerY = stream.ReadUInt();
        uint nextPlayerID = stream.ReadUInt();
        Debug.LogError($"Client: Player {opponentPlayerID} moved to {playerX}, {playerY}");
        remotePlayers[opponentPlayerID].MoveToTile(new Vector3Int((int)playerX, 0, (int)playerY));

        if (playerID == nextPlayerID)
        {
            ReceiveTurn();
        }
    }

    private void HandleReceivePlayerID(DataStreamReader stream)
    {
        playerID = stream.ReadUInt();
        Debug.Log($"Client: Received ID of " + playerID);

        CreateLocalPlayer();

        isDoneConnecting = true;
        UIManager.Instance.SetTurnText(hasTurn);
        UIManager.Instance.SetPlayerIDText(playerID);

        if (playerID > 0)
        {
            for (int previousID = (int)playerID - 1; previousID >= 0; previousID--)
            {
                CreateRemotePlayer(previousID);
            }
        }
    }

    private void HandleRemotePlayerJoined(DataStreamReader stream)
    {
        uint remotePlayerID = stream.ReadUInt();
        CreateRemotePlayer((int)remotePlayerID);
    }

    private void CreateLocalPlayer()
    {
        Vector3Int playerStartPos = GridManager.GetPlayerStartPos((int)playerID);
        localPlayer = Instantiate(localPlayerPrefab, playerStartPos, Quaternion.identity);
        localPlayer.playerID = (int) playerID;
    }

    private void CreateRemotePlayer(int remotePlayerID)
    {
        Vector3Int playerStartPos = GridManager.GetPlayerStartPos(remotePlayerID);
        remotePlayers[remotePlayerID] = Instantiate(remotePlayerPrefab, playerStartPos, Quaternion.identity);
        remotePlayers[remotePlayerID].playerID = remotePlayerID;

        Debug.LogError($"Created player {remotePlayerID} at {playerStartPos}");
    }

    private void SendHandshake()
    {
        Debug.Log("We are now connected to the server");
        uint messageType = (uint) NetworkMessageType.PLAYER_JOINED;

        networkDriver.BeginSend(connection, out var writer);
        writer.WriteUInt(messageType);
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
}