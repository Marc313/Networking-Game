using MarcoHelpers;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : Singleton<PlayerManager>
{
    [SerializeField] private LocalPlayer localPlayerPrefab;
    [SerializeField] private RemotePlayer remotePlayerPrefab;

    private LocalPlayer localPlayer;
    private uint localPlayerID;
    private Dictionary<uint, RemotePlayer> remotePlayers = new Dictionary<uint, RemotePlayer>();
    private List<uint> playerOrder = new List<uint>();

    private void Awake()
    {
        Instance = this;
    }

    private void OnEnable()
    {
        EventSystem.Subscribe(EventName.PLAYERS_SWAP, SwapPlayerPositions);
    }

    private void OnDisable()
    {
        EventSystem.Unsubscribe(EventName.PLAYERS_SWAP, SwapPlayerPositions);
    }

    public void CreateLocalPlayer(uint playerID, Vector3Int position)
    {
        playerOrder.Add(playerID);

        localPlayer = Instantiate(localPlayerPrefab, position, Quaternion.identity);
        localPlayer.playerID = (int)playerID;
        localPlayerID = playerID;
    }

    public void CreateRemotePlayer(uint remotePlayerID, Vector3Int position)
    {
        playerOrder.Add(remotePlayerID);

        remotePlayers.Add(remotePlayerID, Instantiate(remotePlayerPrefab, position, Quaternion.identity));
        remotePlayers[remotePlayerID].playerID = (int)remotePlayerID;

        Debug.LogError($"Created player {remotePlayerID} at {position}");
    }

    public LocalPlayer GetLocalPlayer()
    {
        return localPlayer;
    }

    public RemotePlayer GetRemotePlayer(uint playerID)
    {
        if (remotePlayers.ContainsKey(playerID)) return remotePlayers[playerID];
        else return null;
    }

    public APlayer GetPlayer(uint playerID)
    {
        return playerID == localPlayerID ? localPlayer : GetRemotePlayer(playerID);
    }

    private void SwapPlayerPositions(object value = null)
    {
        RemotePlayer randomRemote = remotePlayers.Values.GetRandomEntry();
        Vector3 remotePos = randomRemote.transform.position;
        randomRemote.SetPosition(localPlayer.transform.position, false);
        localPlayer.SetPosition(remotePos, true);
    }

}
