using UnityEngine;
using UnityEngine.EventSystems;

public class GridTileSquare : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler
{
    public uint y;
    public uint x;

    public Material clickedMaterial;

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log($"Click: {x}, {y}");

        // Send message to server using clientmanager
        FindObjectOfType<Client>().SendPlayerMove(x, y);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        Debug.Log("Highlight");
    }

    public void MarkAsClicked()
    {
        GetComponent<MeshRenderer>().material = clickedMaterial;
        Invoke(nameof(Disappear), 1.0f);
    }

    public void Disappear()
    {
        // Move down
        gameObject.SetActive(false);
    }
}

