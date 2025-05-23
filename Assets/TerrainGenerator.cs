using System.Collections.Generic;
using UnityEngine;

public class TerrainGenerator : MonoBehaviour
{
    public GameObject hexPrefab;
    public GameConfig config;
    public Material[] terrainMaterials;

    private Dictionary<Vector2Int, GameObject> tileMap = new();
    public static TerrainGenerator Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        if (config == null || hexPrefab == null)
        {
            Debug.LogError("Missing GameConfig or HexPrefab in TerrainGenerator.");
            return;
        }

        GenerateMap();
    }

    private void GenerateMap()
    {
        tileMap.Clear();

        
        switch (config.mapShape)
        {
            case MapShape.Square:
                GenerateSquareMap(config.initialRadius);
                break;
            case MapShape.Hexagonal:
                GenerateHexagonalMap(config.initialRadius);
                break;
            case MapShape.Random:
                GenerateRandomMap(config.initialRadius);
                break;
        }


        AssignAllNeighbors();
    }

    private void GenerateSquareMap(int radius)
    {
        for (int x = -radius; x <= radius; x++)
        {
            for (int y = -radius; y <= radius; y++)
            {
                CreateTile(new Vector2Int(x, y));
            }
        }
    }

    private void GenerateHexagonalMap(int radius)
    {
        for (int q = -radius; q <= radius; q++)
        {
            int r1 = Mathf.Max(-radius, -q - radius);
            int r2 = Mathf.Min(radius, -q + radius);
            for (int r = r1; r <= r2; r++)
            {
                CreateTile(new Vector2Int(q, r));
            }
        }
    }

    private void GenerateRandomMap(int radius)
    {
        for (int x = -radius; x <= radius; x++)
        {
            for (int y = -radius; y <= radius; y++)
            {
                float noise = Mathf.PerlinNoise(
                    (x + 1000f) * config.terrainDiversity,
                    (y + 1000f) * config.terrainDiversity
                );
                if (noise > 0.4f) // Ajusta el umbral para controlar densidad
                {
                    CreateTile(new Vector2Int(x, y));
                }
            }
        }
    }

    private void CreateTile(Vector2Int coord)
    {
        if (tileMap.ContainsKey(coord)) return;

        Vector3 position = HexToWorld(coord);
        GameObject hex = Instantiate(hexPrefab, position, Quaternion.identity, transform);
        hex.name = $"Hex_{coord.x}_{coord.y}";

        var behavior = hex.GetComponent<HexBehavior>();
        if (behavior != null)
        {
        behavior.coordinates = new HexCoordinates(coord.x, coord.y);

        }

        // Visual diversity via material
        if (config.terrainMaterials != null && config.terrainMaterials.Length > 0)
        {
            float noise = Mathf.PerlinNoise(
                (coord.x + 500f) * config.terrainDiversity,
                (coord.y + 500f) * config.terrainDiversity
            );
            int matIndex = Mathf.FloorToInt(noise * config.terrainMaterials.Length);
            matIndex = Mathf.Clamp(matIndex, 0, config.terrainMaterials.Length - 1);

            var renderer = hex.GetComponent<MeshRenderer>();
            if (renderer) renderer.material = config.terrainMaterials[matIndex];
        }


        tileMap[coord] = hex;
    }

    private Vector3 HexToWorld(Vector2Int coord)
{
    return HexCoordinates.ToWorldPosition(new HexCoordinates(coord.x, coord.y), HexRenderer.SharedOuterRadius);
}


    private void AssignAllNeighbors()
    {
        foreach (var kvp in tileMap)
        {
            Vector2Int coord = kvp.Key;
            HexBehavior hex = kvp.Value.GetComponent<HexBehavior>();
            if (hex != null)
            {
                AssignNeighbors(hex, coord);
            }
        }
    }

    private void AssignNeighbors(HexBehavior hex, Vector2Int coord)
    {
        Vector2Int[] offsetsEven = {
            new(1, 0), new(1, -1), new(0, -1),
            new(-1, -1), new(-1, 0), new(0, 1)
        };

        Vector2Int[] offsetsOdd = {
            new(1, 1), new(1, 0), new(0, -1),
            new(-1, 0), new(-1, 1), new(0, 1)
        };

        var offsets = (coord.x % 2 == 0) ? offsetsEven : offsetsOdd;

        foreach (var offset in offsets)
        {
            Vector2Int neighborCoord = coord + offset;
            if (tileMap.TryGetValue(neighborCoord, out GameObject neighborObj))
            {
                HexBehavior neighbor = neighborObj.GetComponent<HexBehavior>();
                if (neighbor != null && !hex.neighbors.Contains(neighbor))
                {
                    hex.neighbors.Add(neighbor);
                }
            }
        }
    }
    public void TryExpandFrom(Vector2Int center)
{
    int expansionRadius = 1;

    for (int dx = -expansionRadius; dx <= expansionRadius; dx++)
    {
        for (int dy = -expansionRadius; dy <= expansionRadius; dy++)
        {
            Vector2Int neighbor = new Vector2Int(center.x + dx, center.y + dy);
            if (!tileMap.ContainsKey(neighbor) &&
                Mathf.Abs(neighbor.x) <= config.maxX &&
                Mathf.Abs(neighbor.y) <= config.maxY &&
                tileMap.Count < config.maxTiles)
            {
                CreateTile(neighbor);
            }
        }
    }
}

}
