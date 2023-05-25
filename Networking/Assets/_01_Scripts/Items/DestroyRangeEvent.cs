using UnityEngine;

public class DestroyRangeEvent : ItemEvent
{
    protected Vector3Int playerPosition;
    protected Vector3 lookDirection;

    public override void Trigger(APlayer player)
    {
        playerPosition = player.transform.position.ToVector3Int();
        lookDirection = player.transform.forward;
        DestroyTiles(playerPosition, lookDirection);
    }

    protected virtual void DestroyTiles(Vector3Int position, Vector3 direction)
    {
        GridManager.DestroyTilesInLine(position, direction);
    }
}
