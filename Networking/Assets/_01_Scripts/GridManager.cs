using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GridManager : NetworkedObject
{
    public static int sGridSize = 10;
    [SerializeField] private int gridSize = 10;
    [SerializeField] private GridTileSquare tilePrefab;
    [SerializeField] private GameObject itemPrefab;

    private static Dictionary<Vector3Int, GridTileSquare> generatedSquares = new Dictionary<Vector3Int, GridTileSquare>();
    private GridTileSquare[] squares;
    private static Vector3Int[] playerStartPosList;

    public Server server;

    /*    private void Start()
        {
            sGridSize = gridSize;
            OnGameStart();
        }*/

/*    private void Start()
    {
        // Convert to trigger with the server button
        //server = FindObjectOfType<Server>();
        if (server != null)
        {
            OnGameStart();
        }
    }*/

    public void OnGameStart()
    {
        sGridSize = gridSize;

        if (generatedSquares.Count > 0)
        {
            Debug.Log("Grid already generated");
        }
        else
        {
            GenerateAllTiles();
        }

        //squares = FindObjectsOfType<GridTileSquare>();

        //GenerateItem();
        playerStartPosList = new[]{
            new Vector3Int(0, 0, 0),
            new Vector3Int(gridSize - 1, 0, gridSize - 1),
            new Vector3Int(gridSize - 1, 0, 0),
            new Vector3Int(0, 0, gridSize - 1),
        };
    }

    /// <summary>
    /// Function only used by the Server, that generates the entire grid
    /// </summary>
    private void GenerateAllTiles()
    {
        for (int x = 0; x < gridSize; x++)
        {
            for (int z = 0; z < gridSize; z++)
            {
                GenerateTileOnPosition(x, z);

/*                uint id = NetworkManager.GetNextID;
                NetworkedObject o = GenerateTileOnPosition(id, x, z);*/
                // Broadcast message to all clients to spawn a tile
                // Server.BroadcastSpawn(server, o.networkedID, "tile", new Vector3(x, 0, z), Quaternion.identity);
            }
        }
    }

    private void GenerateTileOnPosition(int x, int z)
    {
        Vector3Int position = new Vector3Int(x, 0, z);
        GridTileSquare tile = Instantiate(tilePrefab, position, Quaternion.identity, transform);
        tile.z = (uint)position.z;
        tile.x = (uint)position.x;
        generatedSquares.Add(position, tile);
    }

    private NetworkedObject GenerateTileOnPosition(uint networkedID, int x, int z)
    {
        return NetworkManager.Instance.Create(0, "tile", true, new Vector3(x, 0, z), Quaternion.identity);

        Vector3Int position = new Vector3Int(x, 0, z);
        GridTileSquare tile = Instantiate(tilePrefab, position, Quaternion.identity, transform);
        tile.z = (uint)position.z;
        tile.x = (uint)position.x;
        tile.networkedID = networkedID;
        generatedSquares.Add(position, tile);
    }

    public override void OnCreate()
    {
        base.OnCreate();

        OnGameStart();
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
                    GridTileSquare neighbour = generatedSquares[newPos];
                    if (!neighbour.isDisappeared)
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

    public static Vector3Int GetRandomExistingTilePosition(params Vector3Int[] exceptions)
    {
        Vector3Int[] keys = generatedSquares.Keys.ToArray();
        Vector3Int result;
        do
        {
            result = keys.GetRandomEntry();
        } while (exceptions.Contains(result));

        return result;
    }

    public static void DestroyTilesInLine(Vector3Int position, Vector3 direction)
    {
        for (Vector3Int v = position + direction.ToVector3Int(); v.x < sGridSize && v.z < sGridSize & v.x >= 0 && v.z >= 0; v += direction.ToVector3Int())
        {
            if (generatedSquares.ContainsKey(v))
            {
                GridTileSquare tile = generatedSquares[v];
                generatedSquares.Remove(v);
                tile.Disappear();
                Debug.Log($"Removed tile at {v}");
            }
        }
    }

    public static void TryAddTile(GridTileSquare tile)
    {
        if (!generatedSquares.ContainsKey(new Vector3Int((int)tile.x, 0, (int)tile.z)))
            generatedSquares.Add(new Vector3Int((int)tile.x, 0, (int)tile.z), tile);
    }
}
