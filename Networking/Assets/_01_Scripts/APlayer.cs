using UnityEngine;

// Subclasses: LocalPlayer, RemotePlayer
public abstract class APlayer : MonoBehaviour
{
    [HideInInspector] public GridCord currentPosition;

    public virtual void MoveToTile()
    {

    }
}