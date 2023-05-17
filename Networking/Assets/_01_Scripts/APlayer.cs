using UnityEngine;

// Subclasses: LocalPlayer, RemotePlayer
public abstract class APlayer : MovingObject
{
    [HideInInspector] public Vector3Int currentPosition;
    [HideInInspector] public int playerID;
    public float walkDuration = 1;

    private void Start()
    {
        currentPosition = transform.position.ToVector3Int();
    }

    public virtual void MoveToTile(Vector3Int position)
    {
        GridManager.GetTile(currentPosition).Disappear();
        MoveToInSeconds(currentPosition, position, walkDuration);
        currentPosition = position;
    }
}