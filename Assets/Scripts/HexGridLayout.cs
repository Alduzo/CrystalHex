using UnityEngine;

public class HexGridLayout : MonoBehaviour
{
    [Header("Grid Settings")]
    public int gridWidth = 5;
    public int gridHeight = 5;
    public float outerRadius = HexRenderer.SharedOuterRadius; // Reference to HexRenderer's outerRadius

    [Header("Hex Prefab")]
    public GameObject hexPrefab;

    private HexBehavior[,] hexGrid;

    private void Start()
    {
        GenerateGrid();
    }

    public void GenerateGrid()
    {
        hexGrid = new HexBehavior[gridWidth, gridHeight];

        float outerRadius = HexRenderer.SharedOuterRadius;
        float width = outerRadius * 2f;
        float height = outerRadius * 1.732f;


        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                float xOffset = x * width * 0.75f;
                float yOffset = y * height;

                if (x % 2 == 1)
                {
                    yOffset += height / 2f;
                }

                Vector3 spawnPos = new Vector3(xOffset, yOffset, 0);
                GameObject hexObj = Instantiate(hexPrefab, spawnPos, Quaternion.identity, this.transform);
                hexObj.name = $"Hex_{x}_{y}";

                HexBehavior hexBehavior = hexObj.GetComponent<HexBehavior>();
                hexBehavior.gridX = x;
                hexBehavior.gridY = y;

                hexGrid[x, y] = hexBehavior;
            }
        }

        AssignNeighbors();
    }

    private void AssignNeighbors()
    {
        Vector2Int[][] offsets = new Vector2Int[][]
        {
            // Even columns
            new Vector2Int[]
            {
                new Vector2Int(0, 1), new Vector2Int(1, 0), new Vector2Int(1, -1),
                new Vector2Int(0, -1), new Vector2Int(-1, -1), new Vector2Int(-1, 0)
            },
            // Odd columns
            new Vector2Int[]
            {
                new Vector2Int(0, 1), new Vector2Int(1, 1), new Vector2Int(1, 0),
                new Vector2Int(0, -1), new Vector2Int(-1, 0), new Vector2Int(-1, 1)
            }
        };

        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                HexBehavior current = hexGrid[x, y];
                int parity = x % 2;

                foreach (Vector2Int offset in offsets[parity])
                {
                    int nx = x + offset.x;
                    int ny = y + offset.y;

                    if (nx >= 0 && nx < gridWidth && ny >= 0 && ny < gridHeight)
                    {
                        current.neighbors.Add(hexGrid[nx, ny]);
                    }
                }
            }
        }
    }
}
