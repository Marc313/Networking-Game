using Unity.Networking.Transport; 
using UnityEngine;

public class Client : MonoBehaviour
{
    public NetworkDriver networkDriver;
    public NetworkConnection connection;

    private bool isDoneConnecting;
    private uint playerID;
    private static bool hasTurn;

    void Start()
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
            uint playerID = stream.ReadUInt();
            uint playerX = stream.ReadUInt();
            uint playerY = stream.ReadUInt();
            Debug.LogError($"Client: Player {playerID} moved to {playerX}, {playerY}");
            MarkTile(playerX, playerY);
            hasTurn = true;
            UIManager.Instance.SetTurnText(hasTurn);
        }
        else if (messageType == (uint)NetworkMessageType.SEND_PLAYER_ID)
        {
            playerID = stream.ReadUInt();
            hasTurn = stream.ReadUInt() == 0 ? false : true;
            Debug.Log($"Client: Received ID of " + playerID);
            isDoneConnecting = true;
            UIManager.Instance.SetTurnText(hasTurn);
            UIManager.Instance.SetPlayerIDText(playerID);
        }
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
        writer.WriteUInt(3);
        networkDriver.EndSend(writer);
        connection = default(NetworkConnection);
    }

    private void MarkTile(uint x, uint y)
    {
        FindObjectOfType<GridManager>().MarkTile(x, y);
    }
}