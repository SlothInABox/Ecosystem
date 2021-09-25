using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    [SerializeField] private Transform tilePrefab;
    [SerializeField] private Vector2 mapSize;

    private void Start()
    {
        GenerateMap();
    }

    public void GenerateMap()
    {
        string holderName = "Generated Map";
        if (transform.Find(holderName))
        {
            DestroyImmediate(transform.Find(holderName).gameObject);
        }

        Transform mapHolder = new GameObject(holderName).transform;
        mapHolder.parent = transform;

        for (int x = 0; x < mapSize.x; x++)
        {
            for (int z = 0; z < mapSize.y; z++)
            {
                Vector3 tilePos = new Vector3(-mapSize.x / 2 + 0.5f + x, 0f, -mapSize.y / 2 + 0.5f + z);
                Transform newTile = Instantiate(tilePrefab, tilePos, Quaternion.Euler(Vector3.zero), mapHolder);
            }
        }
    }
}
