﻿using MarcoHelpers;
using UnityEngine;
using UnityEngine.EventSystems;

public class GridTileSquare : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler
{
    public uint x, z;
    public Material clickedMaterial;
    public Material defaultMaterial;

    private bool isPossibleMove;

    private void OnEnable() => MarcoHelpers.EventSystem.Subscribe(EventName.LOCAL_MOVE_SENT, ResetMovePossibility);
    private void OnDisable() => MarcoHelpers.EventSystem.Unsubscribe(EventName.LOCAL_MOVE_SENT, ResetMovePossibility);

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!isPossibleMove)
        {
            Debug.Log("Invalid Move!");
            return;
        }

        Debug.Log($"Click: {x}, {z}");

        // Send message to server using clientmanager
        FindObjectOfType<Client>().SendPlayerMove(x, z);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
    }

    public void MarkAsClicked()
    {
        GetComponent<MeshRenderer>().material = defaultMaterial;
        //Invoke(nameof(Disappear), 1.0f);
    }

    public void Disappear()
    {
        // Move down
        gameObject.SetActive(false);
    }

    public void MarkAsPossibleMove()
    {
        isPossibleMove = true;
        ShowHighlightEffect();
    }

    private void ResetMovePossibility(object value = null)
    {
        isPossibleMove = false;
        GetComponent<MeshRenderer>().material = defaultMaterial;
    }

    private void ShowHighlightEffect()
    {
        // Visual effect
        GetComponent<MeshRenderer>().material = clickedMaterial;
    }
}

