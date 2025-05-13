using System.Collections.Generic;
using UnityEngine;

public class TerrainGenerator : MonoBehaviour
{
    [Header("References")]
    public GameObject hexPrefab;
    public GameConfig config;
    public Material[] terrainMaterials; // [0]=base, [1]=slow, [2]=fast, [3-6]=visual variation

    private Dictionary<Vector2Int, GameObject> tileMap = new();
    private float perlinScale = 0.1f;
    private float seedOffsetX;
    private float seedOffsetY;

    private void Start()
    {
        seedOffsetX = Random.Range(0f, 10000f);
        seedOffsetY = Random.Range(0f, 10000f);
        GenerateInitialMap();
    }

    private void GenerateInitialMap()
    {
        int radius = config.initialRadius;

        for (int x = -radius; x <= radius; x++)
        {
            for (int y = -radius; y <= radius; y++)
            {
                Vector2Int coord = new Vector2Int(x, y);
                if (!tileMap.ContainsKey(coord))
                    CreateTile(coord);
            }
        }

        AssignAllNeighbors();
        ApplyRandomGrowthModifiers(); // ✅ Llamamos esta función una vez después de crear todo
    }

    private void CreateTile(Vector2Int coord)
    {
        float hexWidth = HexRenderer.SharedOuterRadius * 2f;
        float hexHeight = HexRenderer.SharedOuterRadius * Mathf.Sqrt(3f);
        float xOffset = coord.x * hexWidth * 0.75f;
        float yOffset = coord.y * hexHeight + (coord.x % 2 != 0 ? hexHeight / 2f : 0);
        Vector3 worldPos = new Vector3(xOffset, yOffset, 0f);

        GameObject hexObj = Instantiate(hexPrefab, worldPos, Quaternion.identity, transform);
        hexObj.name = $"Hex_{coord.x}_{coord.y}";
        tileMap.Add(coord, hexObj);

        HexBehavior behavior = hexObj.GetComponent<HexBehavior>();
        if (behavior != null)
        {
            behavior.gridX = coord.x;
            behavior.gridY = coord.y;
            behavior.growthMultiplier = 1f; // ✅ Valor por defecto

            // Perlin Noise solo para visuales
            float noiseX = (coord.x + seedOffsetX) * perlinScale;
            float noiseY = (coord.y + seedOffsetY) * perlinScale;
            float noise = Mathf.PerlinNoise(noiseX, noiseY);

            int matIndex = 0;
            if (terrainMaterials.Length > 3)
                matIndex = Mathf.FloorToInt(noise * (terrainMaterials.Length - 3)) + 3;

            MeshRenderer renderer = hexObj.GetComponent<MeshRenderer>();
            if (renderer != null && matIndex < terrainMaterials.Length)
                renderer.material = terrainMaterials[matIndex];
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

        AssignAllNeighbors(); // Asegura vecinos correctos tras expansión
    }

    private void ApplyRandomGrowthModifiers()
    {
        int totalTiles = tileMap.Count;
        int slowCount = Mathf.RoundToInt(totalTiles * config.slowTilePercent / 100f);
        int fastCount = Mathf.RoundToInt(totalTiles * config.fastTilePercent / 100f);

        AssignRandomModifiers(slowCount, 0.5f, 1); // Material index 1 = slow
        AssignRandomModifiers(fastCount, 2f, 2);    // Material index 2 = fast
    }

    private void AssignRandomModifiers(int count, float multiplier, int matIndex)
    {
        List<Vector2Int> coords = new List<Vector2Int>(tileMap.Keys);
        int placed = 0;

        while (placed < count && coords.Count > 0)
        {
            int index = Random.Range(0, coords.Count);
            Vector2Int coord = coords[index];
            coords.RemoveAt(index);

            GameObject hex = tileMap[coord];
            if (hex == null) continue;

            HexBehavior behavior = hex.GetComponent<HexBehavior>();
            if (behavior != null && behavior.growthMultiplier == 1f)
            {
                behavior.growthMultiplier = multiplier;

                MeshRenderer renderer = hex.GetComponent<MeshRenderer>();
                if (renderer != null && terrainMaterials.Length > matIndex)
                    renderer.material = terrainMaterials[matIndex];

                if (config.enableDebugLabels)
                    CreateLabel(hex.transform.position, $"x{multiplier}");

                placed++;
            }
        }
    }

    private void AssignNeighbors(HexBehavior newHex, Vector2Int coord)
    {
        Vector2Int[] directionsEven = new Vector2Int[]
        {
            new(0, 1), new(1, 0), new(1, -1),
            new(0, -1), new(-1, -1), new(-1, 0)
        };

        Vector2Int[] directionsOdd = new Vector2Int[]
        {
            new(0, 1), new(1, 1), new(1, 0),
            new(0, -1), new(-1, 0), new(-1, 1)
        };

        Vector2Int[] directions = coord.x % 2 == 0 ? directionsEven : directionsOdd;

        foreach (Vector2Int dir in directions)
        {
            Vector2Int neighborCoord = coord + dir;
            if (tileMap.TryGetValue(neighborCoord, out GameObject neighborGO))
            {
                HexBehavior neighbor = neighborGO.GetComponent<HexBehavior>();
                if (neighbor != null)
                {
                    if (!newHex.neighbors.Contains(neighbor)) newHex.neighbors.Add(neighbor);
                    if (!neighbor.neighbors.Contains(newHex)) neighbor.neighbors.Add(newHex);
                }
            }
        }
    }

    private void AssignAllNeighbors()
    {
        foreach (var kvp in tileMap)
        {
            Vector2Int coord = kvp.Key;
            HexBehavior hex = kvp.Value.GetComponent<HexBehavior>();
            if (hex != null)
                AssignNeighbors(hex, coord);
        }
    }

    private void CreateLabel(Vector3 position, string text)
    {
        GameObject labelObj = new GameObject("TileLabel");
        labelObj.transform.position = position + new Vector3(0, 0.25f, 0);
        TextMesh textMesh = labelObj.AddComponent<TextMesh>();
        textMesh.text = text;
        textMesh.fontSize = 50;
        textMesh.characterSize = 0.1f;
        textMesh.anchor = TextAnchor.MiddleCenter;
        textMesh.color = Color.black;
        labelObj.transform.SetParent(this.transform);
    }
}
