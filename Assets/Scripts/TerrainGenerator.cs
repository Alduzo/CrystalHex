using System.Collections.Generic;
using UnityEngine;

public class TerrainGenerator : MonoBehaviour
{
    [Header("References")]
    public GameObject hexPrefab;
    public GameConfig config;
    public Material[] terrainMaterials; // [0] = base, [1] = slow (/2), [2] = fast (x2)

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
                {
                    CreateTile(coord);
                }
            }
        }
        AssignAllNeighbors(); // ✅ Agrega esta línea aquí
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

            float noiseX = (coord.x + seedOffsetX) * perlinScale;
            float noiseY = (coord.y + seedOffsetY) * perlinScale;
            float noise = Mathf.PerlinNoise(noiseX, noiseY);
            float multiplier = 1f;
            int matIndex = 0;

            // Ajustar probabilidad de aparición y reducir clústers grandes
            if (noise > 0.82f && Random.value < 0.4f)
            {
                multiplier = 2f;
                matIndex = 2; // crecimiento rápido
            }
            else if (noise < 0.18f && Random.value < 0.4f)
            {
                multiplier = 0.5f;
                matIndex = 1; // crecimiento lento
            }


            behavior.growthMultiplier = multiplier;

            MeshRenderer renderer = hexObj.GetComponent<MeshRenderer>();
            if (renderer != null && terrainMaterials != null && matIndex < terrainMaterials.Length)
            {
                renderer.material = terrainMaterials[matIndex];
            }

            if (config != null && config.enableDebugLabels && multiplier != 1f)
            {
                CreateLabel(worldPos, $"x{multiplier}");
            }

            // 👇 Asignar vecinos correctamente
            AssignNeighbors(behavior, coord);
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
                    if (!newHex.neighbors.Contains(neighbor))
                        newHex.neighbors.Add(neighbor);

                    if (!neighbor.neighbors.Contains(newHex))
                        neighbor.neighbors.Add(newHex);
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
            {
                AssignNeighbors(hex, coord);
            }
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
