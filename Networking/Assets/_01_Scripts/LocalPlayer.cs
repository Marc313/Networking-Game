using UnityEngine;
/// <summary>
/// This refers to the player controlled by the client, 
/// which contains more information that the representations of other players..
/// </summary>
public class LocalPlayer : APlayer
{
    private object currentItem;
    private int points;

    private void Update()
    {
        // Check for use item input if player has turn.
        if (Input.GetKeyDown(KeyCode.Mouse1))
        {
            Debug.Log("Used item!");
            currentItem = null;
        }
    }

    private void CalculatePossibleMoves()
    {
        currentPosition.GetNeighbourCords();
    }

    private void ShowPossibleMoves()
    {

    }
}