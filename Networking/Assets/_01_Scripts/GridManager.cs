using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    public int gridSize = 10;
    [SerializeField] private GridTileSquare tilePrefab;
    [SerializeField] private GameObject itemPrefab;

    private static Dictionary<Vector3Int, GridTileSquare> generatedSquares = new Dictionary<Vector3Int, GridTileSquare>();
    private GridTileSquare[] squares;
    private static Vector3Int[] playerStartPosList;

    private void Start()
    {
        //squares = FindObjectsOfType<GridTileSquare>();
        GenerateTiles();
        GenerateItem();
        playerStartPosList = new[]{
            new Vector3Int(0, 0, 0),
            new Vector3Int(gridSize - 1, 0, gridSize - 1),
            new Vector3Int(gridSize - 1, 0, 0),
            new Vector3Int(0, 0, gridSize - 1),
        };
    }

    private void GenerateTiles()
    {
        for (int x = 0; x < gridSize; x++)
        {
            for (int z = 0; z < gridSize; z++)
            {
                Vector3Int position = new Vector3Int(x, 0, z);
                GridTileSquare tile = Instantiate(tilePrefab, position, Quaternion.identity, transform);
                tile.x = (uint) position.x;
                tile.z = (uint) position.z;
                generatedSquares.Add(position, tile);
            }
        }
    }

    private void GenerateItem()
    {
        for (int i = 0; i <= 5; i++)
        {
            int xPos = Random.Range(0, gridSize);
            int zPos = Random.Range(0, gridSize);

            if (!((xPos == 0 && zPos == 0)
                || Mathf.Abs(xPos - zPos) == gridSize - 1
                || (xPos == gridSize - 1 && zPos == gridSize - 1)))
            {
                Instantiate(itemPrefab, new Vector3(xPos, 1f, zPos), Quaternion.identity);
            }
        }
    }

    public void MarkTile(uint x, uint y)
    {
        GridTileSquare tile = generatedSquares[new Vector3Int((int)x, 0, (int)y)];
        tile.MarkAsClicked();
    }

    public static List<GridTileSquare> GetNeighbours(GridTileSquare tile)
    {
        List<GridTileSquare> neighbours = new List<GridTileSquare>();
        Vector3Int position = new Vector3Int((int) tile.x, 0, (int) tile.z);

        for (int x = -1; x <= 1; x++)
        {
            for (int z = -1; z <= 1; z++)
            {
                if (x == 0 && z == 0) continue;

                Vector3Int newPos = new Vector3Int(position.x + x, 0, position.z + z);
                if (generatedSquares.ContainsKey(newPos))
                {
                    neighbours.Add(generatedSquares[newPos]);
                }
            }
        }

        return neighbours;
    }

    public static List<GridTileSquare> GetNeighboursOfPosition(Vector3Int currentPosition)
    {
        GridTileSquare tile = generatedSquares[currentPosition];
        return GetNeighbours(tile);
    }

    public static GridTileSquare GetTile(Vector3Int position)
    {
        if (generatedSquares.ContainsKey(position))
        {
            return generatedSquares[position];
        }

        return null;
    }

    public static Vector3Int GetPlayerStartPos(int playerID)
    {
        if (playerStartPosList == null 
            || playerID >= playerStartPosList.Length) 
            return default;

        return playerStartPosList[playerID];
    }
}
