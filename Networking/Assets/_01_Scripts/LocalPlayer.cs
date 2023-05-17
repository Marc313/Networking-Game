using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This refers to the player controlled by the client, 
/// which contains more information that the representations of other players..
/// </summary>
public class LocalPlayer : APlayer
{
    [HideInInspector] public List<GridTileSquare> possibleTiles = new List<GridTileSquare>();
    private object currentItem;
    private int points;

    private void Start()
    {
        currentPosition = transform.position.ToVector3Int();
    }

    private void Update()
    {
        // Check for use item input if player has turn.
        if (Input.GetKeyDown(KeyCode.Mouse1))
        {
            Debug.Log("Used item!");
            currentItem = null;
        }
    }

    public void OnReceiveTurn()
    {
        ShowPossibleMoves();
    }

    private void ShowPossibleMoves()
    {
        possibleTiles = GridManager.GetNeighboursOfPosition(currentPosition);
        foreach (GridTileSquare tile in possibleTiles)
        {
            tile.MarkAsPossibleMove();
        }
    }
}