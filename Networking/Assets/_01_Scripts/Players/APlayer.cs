using System;
using UnityEngine;

// Subclasses: LocalPlayer, RemotePlayer
public abstract class APlayer : MovingObject
{
    [HideInInspector] public Vector3Int currentPosition;
    [HideInInspector] public int playerID;
    public float walkDuration = 1;

    private void Start()
    {
        SetPosition(transform.position, false);
    }

    public virtual void MoveToTile(Vector3Int position)
    {
        GridManager.GetTile(currentPosition).Disappear();

        Vector3 moveDirection = (position - transform.position);
        moveDirection.y = 0;
        transform.forward = moveDirection.normalized;
        MoveToInSeconds(currentPosition, position, walkDuration);
        currentPosition = position;
    }

    public virtual void SetPosition(Vector3 position, bool highlightMoves)
    {
        currentPosition = position.ToVector3Int() ;
        transform.position = currentPosition + Vector3.up;
    }

    public void TryObtainItem(Item item)
    {
        if (this is LocalPlayer)
        {
            (this as LocalPlayer).ObtainItem(item);
        }

        // TODO: Send to server that item is in possession
    }
}