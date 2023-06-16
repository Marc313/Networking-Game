using MarcoHelpers;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This refers to the player controlled by the client, 
/// which contains more information that the representations of other players..
/// </summary>
public class LocalPlayer : APlayer
{
    [HideInInspector] public List<GridTileSquare> possibleTiles = new List<GridTileSquare>();
    private int points;

    public Item currentItem;

    private void Update()
    {
        // Check for use item input if player has turn.
        if (Input.GetKeyDown(KeyCode.I))
        {
            UseItem();
        }
    }

    public void UseItem()
    {
        Debug.Log("Used item!");
        //currentItem.Use(this);  // Illegal, check with server

        // Send item use to server
        FindObjectOfType<Client>().OnUseItem(currentItem);
    }

    public void AfterItemUse()
    {
        currentItem = null;
        FindObjectOfType<ItemButton>()?.DisableSelf();
    }

    public override void SetPosition(Vector3 position, bool hasTurn)
    {
        EventSystem.RaiseEvent(EventName.LOCAL_MOVE_SENT);
        base.SetPosition(position, hasTurn);
        if (hasTurn) ShowPossibleMoves();
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

    public void ObtainItem(Item item)
    {
        currentItem = item;
        UIManager.Instance.SetItemInfo("Item: " + item.itemEventType, item.icon);
        Debug.LogError("Picked up Item!");
    }
}