using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    // Tile types.
    [SerializeField] private Transform groundTilePrefab;
    [SerializeField] private Transform waterTilePrefab;
    [SerializeField] private Transform treePrefab;
    [SerializeField] private Transform plantPrefab;

    [SerializeField] private Vector2 mapSize;

    [Range(0, 1)]
    [SerializeField] private float waterDensity;
    [Range(0, 1)]
    [SerializeField] private float treeDensity;
    [Range(0, 1)]
    [SerializeField] private float plantDensity;

    [SerializeField] private int seed;

    // Store the centre of the map.
    private Coord mapCentre;

    // Map tile coordinates.
    private List<Coord> allTileCoords;
    private Queue<Coord> shuffledTileCoords;

    // Tile map.
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

        AddWaterTiles();
        AddEnvironmentTiles();

        // Create a store for the map tiles
        string holderName = "Generated Map";
        if (transform.Find(holderName))
        {
            DestroyImmediate(transform.Find(holderName).gameObject);
        }
        Transform mapHolder = new GameObject(holderName).transform;
        mapHolder.parent = transform;

        // Instantiate tiles.
        foreach (Coord tileCoord in allTileCoords)
        {
            Vector3 tilePos = CoordToPosition(tileCoord.x, tileCoord.y);

            switch (tileMap[tileCoord.x, tileCoord.y])
            {
                case TileType.Ground:
                    Instantiate(groundTilePrefab, tilePos, Quaternion.identity, mapHolder);
                    break;
                case TileType.Water:
                    Instantiate(waterTilePrefab, tilePos, Quaternion.identity, mapHolder);
                    break;
                case TileType.Tree:
                    Instantiate(groundTilePrefab, tilePos, Quaternion.identity, mapHolder);
                    Instantiate(treePrefab, tilePos + Vector3.up * 0.5f, Quaternion.identity, mapHolder);
                    break;
                case TileType.Plant:
                    Instantiate(groundTilePrefab, tilePos, Quaternion.identity, mapHolder);
                    Instantiate(plantPrefab, tilePos + Vector3.up * 0.5f, Quaternion.identity, mapHolder);
                    break;
                default:
                    break;
            }
        }
    }

    private void AddWaterTiles()
    {
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
    }

    private void AddEnvironmentTiles()
    {
        // Number of valid tiles for plants and trees
        int validTiles = 0;
        foreach (TileType tile in tileMap)
        {
            if (tile == TileType.Ground)
            {
                validTiles++;
            }
        }

        // Calculate the number of trees and plants to generate
        float normalisedTreeDensity = treeDensity;
        float normalisedPlantDensity = plantDensity;
        if (treeDensity + plantDensity > 1)
        {
            normalisedTreeDensity = treeDensity / (treeDensity + plantDensity);
            normalisedPlantDensity = plantDensity / (treeDensity + plantDensity);
        }
        int numberOfTrees = (int)(normalisedTreeDensity * validTiles);
        int numberOfPlants = (int)(normalisedPlantDensity * validTiles);

        int currentNumberOfTrees = 0;
        int currentNumberOfPlants = 0;
        while (currentNumberOfTrees < numberOfTrees || currentNumberOfPlants < numberOfPlants)
        {
            Coord randomCoord = GetRandomCoord();
            if (tileMap[randomCoord.x, randomCoord.y] == TileType.Ground)
            {
                if (currentNumberOfTrees < numberOfTrees)
                {
                    currentNumberOfTrees++;
                    tileMap[randomCoord.x, randomCoord.y] = TileType.Tree;
                }
                else if (currentNumberOfPlants < numberOfPlants)
                {
                    currentNumberOfPlants++;
                    tileMap[randomCoord.x, randomCoord.y] = TileType.Plant;
                }
            }
        }
    }

    // Flood fill algorithm for populating with water tiles.
    private bool MapIsFullyAccessible(int currentObstacleCount)
    {
        // Visited tiles.
        bool[,] mapFlags = new bool[tileMap.GetLength(0), tileMap.GetLength(1)];
        // Tiles to visit.
        Queue<Coord> queue = new Queue<Coord>();

        queue.Enqueue(mapCentre);
        mapFlags[mapCentre.x, mapCentre.y] = true;

        // Number of accessible tiles.
        int accessibleTileCount = 1;

        while (queue.Count > 0)
        {
            // Remove current tile.
            Coord tile = queue.Dequeue();

            // Consider neighbouring tiles.
            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    int neighbourX = tile.x + x;
                    int neighbourY = tile.y + y;
                    if (x == 0 || y == 0)
                    {
                        // Ensure tile exists.
                        if (neighbourX >= 0 && neighbourX < tileMap.GetLength(0) && neighbourY >= 0 && neighbourY < tileMap.GetLength(1))
                        {
                            // If tile hasn't previously been visited and it is a ground tile then it is an accessible tile.
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
        // Calculate the expected number of accessible tiles.
        int targetAccessibleTileCount = (int)(mapSize.x * mapSize.y - currentObstacleCount);
        return targetAccessibleTileCount == accessibleTileCount;
    }

    // Member function for converting a tile coordinate to an actual position.
    private Vector3 CoordToPosition(int x, int y)
    {
        return new Vector3(-mapSize.x / 2 + 0.5f + x, -0.5f, -mapSize.y / 2 + 0.5f + y);
    }

    // Member function for selecting a random coordinate.
    public Coord GetRandomCoord()
    {
        // Move the next random coordinate to the end of the queue.
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

    // Tile types.
    public enum TileType
    {
        Ground,
        Water,
        Tree,
        Plant
    }
}
