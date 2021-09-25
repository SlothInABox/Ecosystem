using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    [SerializeField] private Transform groundTilePrefab;
    [SerializeField] private Transform waterTilePrefab;


    [SerializeField] private Vector2 mapSize;
    [Range(0, 1)]
    [SerializeField] private float waterDensity;

    [SerializeField] private int seed;

    private Coord mapCentre;

    // Map tile coordinates
    private List<Coord> allTileCoords;
    private Queue<Coord> shuffledTileCoords;

    // Tile map
    private TileType[,] tileMap;

    private void Start()
    {
        GenerateMap();
    }

    public void GenerateMap()
    {
        // Make a list of all tile coordinates.
        allTileCoords = new List<Coord>();
        for (int x = 0; x < mapSize.x; x++)
        {
            for (int z = 0; z < mapSize.y; z++)
            {
                allTileCoords.Add(new Coord(x, z));
            }
        }
        // Make a shuffled queue of all tile coordinates.
        shuffledTileCoords = new Queue<Coord>(Utility.ShuffleArray(allTileCoords.ToArray(), seed));
        // Calculate the central tile coordinate.
        mapCentre = new Coord((int)(mapSize.x / 2), (int)(mapSize.y / 2));

        // Create a tile map from which tiles will be created.
        tileMap = new TileType[(int)mapSize.x, (int)mapSize.y];

        // Calculate number of water tiles to try to create.
        int waterTileCount = (int)(mapSize.x * mapSize.y * waterDensity);
        int currentWaterTileCount = 0;
        for (int i = 0; i < waterTileCount; i++)
        {
            // Select a random coordinate.
            Coord randomCoord = GetRandomCoord();

            if (randomCoord != mapCentre)
            {
                // Set the tile type to Water.
                tileMap[randomCoord.x, randomCoord.y] = TileType.Water;
                currentWaterTileCount++;

                if (!MapIsFullyAccessible(currentWaterTileCount))
                {
                    tileMap[randomCoord.x, randomCoord.y] = TileType.Ground;
                    currentWaterTileCount--;
                }
            }
        }

        // Create a store for the map tiles
        string holderName = "Generated Map";
        if (transform.Find(holderName))
        {
            DestroyImmediate(transform.Find(holderName).gameObject);
        }
        Transform mapHolder = new GameObject(holderName).transform;
        mapHolder.parent = transform;

        // Instantiate tiles.
        for (int x = 0; x < mapSize.x; x++)
        {
            for (int y = 0; y < mapSize.y; y++)
            {
                Vector3 tilePos = CoordToPosition(x, y);

                if (tileMap[x, y] == TileType.Ground)
                {
                    Instantiate(groundTilePrefab, tilePos, Quaternion.identity, mapHolder);
                }
                else if (tileMap[x, y] == TileType.Water)
                {
                    Instantiate(waterTilePrefab, tilePos, Quaternion.identity, mapHolder);
                }
            }
        }
    }

    private bool MapIsFullyAccessible(int currentObstacleCount)
    {
        bool[,] mapFlags = new bool[tileMap.GetLength(0), tileMap.GetLength(1)];
        Queue<Coord> queue = new Queue<Coord>();

        queue.Enqueue(mapCentre);
        mapFlags[mapCentre.x, mapCentre.y] = true;

        int accessibleTileCount = 1;

        while (queue.Count > 0)
        {
            Coord tile = queue.Dequeue();

            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    int neighbourX = tile.x + x;
                    int neighbourY = tile.y + y;
                    if (x == 0 || y == 0)
                    {
                        if (neighbourX >= 0 && neighbourX < tileMap.GetLength(0) && neighbourY >= 0 && neighbourY < tileMap.GetLength(1))
                        {
                            if(!mapFlags[neighbourX, neighbourY] && tileMap[neighbourX, neighbourY] == TileType.Ground)
                            {
                                mapFlags[neighbourX, neighbourY] = true;
                                queue.Enqueue(new Coord(neighbourX, neighbourY));
                                accessibleTileCount++;
                            }
                        }
                    }
                }
            }
        }
        int targetAccessibleTileCount = (int)(mapSize.x * mapSize.y - currentObstacleCount);
        return targetAccessibleTileCount == accessibleTileCount;
    }

    private Vector3 CoordToPosition(int x, int y)
    {
        return new Vector3(-mapSize.x / 2 + 0.5f + x, 0f, -mapSize.y / 2 + 0.5f + y);
    }

    public Coord GetRandomCoord()
    {
        Coord randomCoord = shuffledTileCoords.Dequeue();
        shuffledTileCoords.Enqueue(randomCoord);
        return randomCoord;
    }

    // Map tile coordinate struct
    public struct Coord
    {
        public int x;
        public int y;

        public Coord(int _x, int _y)
        {
            x = _x;
            y = _y;
        }

        public static bool operator ==(Coord c1, Coord c2)
        {
            return c1.x == c2.x && c1.y == c2.y;
        }

        public static bool operator !=(Coord c1, Coord c2)
        {
            return !(c1 == c2);
        }
    }

    public enum TileType
    {
        Ground,
        Water
    }
}
