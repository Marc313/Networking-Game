using UnityEngine;

public class GridManager : MonoBehaviour
{
    private GridTileSquare[] squares;

    private void Start()
    {
        squares = FindObjectsOfType<GridTileSquare>();
    }

    public void MarkTile(uint x, uint y)
    {
        foreach(GridTileSquare tile in squares)
        {
            if (tile.x == x && tile.y == y)
            {
                tile.MarkAsClicked();
            }
        }
    }
}
