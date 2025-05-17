--- /Users/alexandrodupuiszorrilla/CrystalHex/Assets/Scripts/PlayerController.cs ---
using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;

    private Vector2Int currentChunkCoord;

IEnumerator Start()
{
    yield return new WaitUntil(() => ChunkManager.Instance != null);

    HexCoordinates playerHex = HexCoordinates.FromWorldPosition(transform.position, HexRenderer.SharedOuterRadius);
    Vector2Int chunkCoord = ChunkManager.WorldToChunkCoord(playerHex);
    currentChunkCoord = chunkCoord;
    ChunkManager.Instance.UpdateChunks(chunkCoord);
}



    void Update()
    {
        float h = 0f;
        float v = 0f;

        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) h = -1f;
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) h = 1f;
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) v = 1f;
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) v = -1f;


        Vector3 move = new Vector3(h, v, 0f) * moveSpeed * Time.deltaTime;
        transform.position += move;

        UpdateChunkLoading();
    }

    void UpdateChunkLoading(bool force = false)
        {
            HexCoordinates playerHex = HexCoordinates.FromWorldPosition(transform.position, HexRenderer.SharedOuterRadius);
            Vector2Int chunkCoord = ChunkManager.WorldToChunkCoord(playerHex);

            if (force || chunkCoord != currentChunkCoord)
            {
                Debug.Log("Player moved to chunk " + chunkCoord);
                currentChunkCoord = chunkCoord;
                ChunkManager.Instance.UpdateChunks(chunkCoord);
            }
        }

    
}
--- /Users/alexandrodupuiszorrilla/CrystalHex/Assets/Scripts/CrystalSelectorUI.cs ---
using UnityEngine;

public class CrystalSelectorUI : MonoBehaviour
{
    public static CrystalSelectorUI Instance;

    public CrystalType selectedCrystal = CrystalType.Red;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void SelectRedCrystal() => selectedCrystal = CrystalType.Red;
    public void SelectBlueCrystal() => selectedCrystal = CrystalType.Blue;
    public void SelectGreenCrystal() => selectedCrystal = CrystalType.Green;
}
--- /Users/alexandrodupuiszorrilla/CrystalHex/Assets/Scripts/GameSpeedDropdownToggle.cs ---
using UnityEngine;

public class GameSpeedDropdownToggle : MonoBehaviour
{
    public GameObject dropdownPanel;

    public void ToggleDropdown()
    {
        dropdownPanel.SetActive(!dropdownPanel.activeSelf);
    }
}
--- /Users/alexandrodupuiszorrilla/CrystalHex/Assets/Scripts/CrystalGenerator.cs ---
/*using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]

public class CrystalGenerator : MonoBehaviour

{
    public Material crystalBaseMaterial;
    [HideInInspector] public Color crystalColor = Color.white;

    [Header("Shape Settings")]
    public float radius = 0.3f;
    public float height = 0.1f;
    public int segments = 6;  // more segments = rounder

    void Start()
    {
        GenerateCrystalMesh();
    }

    void GenerateCrystalMesh()
    {
        MeshFilter meshFilter = GetComponent<MeshFilter>();
        MeshRenderer renderer = GetComponent<MeshRenderer>();

        Mesh mesh = new Mesh();
        Vector3[] vertices = new Vector3[segments + 2];
        int[] triangles = new int[segments * 3];

        vertices[0] = Vector3.zero; // center bottom
        for (int i = 0; i < segments; i++)
        {
            float angle = (2 * Mathf.PI / segments) * i;
            float x = Mathf.Cos(angle) * radius;
            float z = Mathf.Sin(angle) * radius;
            vertices[i + 1] = new Vector3(x, 0, z);
        }
        vertices[segments + 1] = new Vector3(0, height, 0); // tip

        for (int i = 0; i < segments; i++)
        {
            triangles[i * 3 + 0] = 0;
            triangles[i * 3 + 1] = i + 1;
            triangles[i * 3 + 2] = (i + 2 > segments) ? 1 : i + 2;
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        meshFilter.mesh = mesh;

        if (crystalBaseMaterial != null)
        {
            Material instance = new Material(crystalBaseMaterial);
            instance.color = crystalColor;
            renderer.material = instance;
        }
    }
}*/
--- /Users/alexandrodupuiszorrilla/CrystalHex/Assets/Scripts/HexCoordinates.cs ---
using UnityEngine;

[System.Serializable]
public struct HexCoordinates
{
    public readonly int Q; // eje x axial
    public readonly int R; // eje z axial (se llama R en muchos sistemas)

    public int S => -Q - R; // coordenada cúbica implícita

    public HexCoordinates(int q, int r)
    {
        this.Q = q;
        this.R = r;
    }

    // Conversión desde coordenadas de mundo a coordenadas hexagonales
    public static HexCoordinates FromWorldPosition(Vector3 position, float hexOuterRadius)
    {
        float width = hexOuterRadius * 2f;
        float height = Mathf.Sqrt(3f) * hexOuterRadius;

        float q = (position.x * 2f/3f) / hexOuterRadius;
        float r = (-position.x / 3f + Mathf.Sqrt(3f)/3f * position.y) / hexOuterRadius;

        return FromFractional(q, r);
    }

    // Ajuste de coordenadas flotantes a coordenadas hexagonales enteras (redondeo cúbico)
    public static HexCoordinates FromFractional(float q, float r)
    {
        float s = -q - r;

        int rq = Mathf.RoundToInt(q);
        int rr = Mathf.RoundToInt(r);
        int rs = Mathf.RoundToInt(s);

        float q_diff = Mathf.Abs(rq - q);
        float r_diff = Mathf.Abs(rr - r);
        float s_diff = Mathf.Abs(rs - s);

        if (q_diff > r_diff && q_diff > s_diff)
            rq = -rr - rs;
        else if (r_diff > s_diff)
            rr = -rq - rs;

        return new HexCoordinates(rq, rr);
    }

    public override string ToString()
    {
        return $"({Q}, {R}, {S})";
    }

    public Vector2Int ToVector2Int()
    {
        return new Vector2Int(Q, R);
    }

public static Vector3 ToWorldPosition(HexCoordinates coord, float outerRadius)
{
    float width = outerRadius * 2f;
    float height = outerRadius * Mathf.Sqrt(3f);

    float x = coord.Q * width * 0.75f;
    float y = coord.R * height;

    if (coord.Q % 2 != 0)
    {
        y += height / 2f;
    }

    return new Vector3(x, y, 0f);
}


    public static int Distance(HexCoordinates a, HexCoordinates b)
    {
        return (Mathf.Abs(a.Q - b.Q) + Mathf.Abs(a.R - b.R) + Mathf.Abs(a.S - b.S)) / 2;
    }

    public static HexCoordinates Zero => new HexCoordinates(0, 0);
}
--- /Users/alexandrodupuiszorrilla/CrystalHex/Assets/Scripts/WorldLogic/WorldMapManager.cs ---
using System.Collections.Generic;
using UnityEngine;

public class WorldMapManager : MonoBehaviour
{
    public static WorldMapManager Instance;

    public int seed = 42;
    public int chunkSize = 6;
    public PerlinSettings perlinSettings;

    private Dictionary<HexCoordinates, HexData> worldMap = new();

    private void Awake()
    {
        Instance = this;
    }

    public HexData GetOrGenerateHex(HexCoordinates coord)
    {
        if (worldMap.TryGetValue(coord, out var existing))
            return existing;

        HexData hex = new HexData();
        hex.coordinates = coord;

        // Capas Perlin
        hex.elevation = PerlinUtility.Perlin(coord, perlinSettings.elevationFreq, perlinSettings.elevationSeedOffset + seed);
        hex.moisture  = PerlinUtility.Perlin(coord, perlinSettings.moistureFreq, perlinSettings.moistureSeedOffset + seed);
        hex.temperature = PerlinUtility.Perlin(coord, perlinSettings.tempFreq, perlinSettings.tempSeedOffset + seed);

        // Bioma simple provisional
        hex.terrainType = hex.elevation < 0.3f ? TerrainType.Water : (hex.moisture > 0.6f ? TerrainType.Forest : TerrainType.Plains);

        worldMap[coord] = hex;
        return hex;
    }

    public List<HexData> GetChunkHexes(Vector2Int chunkCoord)
    {
        List<HexData> chunkHexes = new();

        for (int dx = 0; dx < chunkSize; dx++)
        {
            for (int dy = 0; dy < chunkSize; dy++)
            {
                int q = chunkCoord.x * chunkSize + dx;
                int r = chunkCoord.y * chunkSize + dy;
                chunkHexes.Add(GetOrGenerateHex(new HexCoordinates(q, r)));
            }
        }

        return chunkHexes;
    }
}
--- /Users/alexandrodupuiszorrilla/CrystalHex/Assets/Scripts/WorldLogic/HexData.cs ---
public enum TerrainType { Plains, Mountains, Forest, Water }
public enum HexType { Natural, Rural, Urban }
public enum ResourceType { Minerals, Wood, Food, Water, Energy }

public class HexData
{
    public HexCoordinates coordinates;

    // Capa estática
    public float elevation;
    public float moisture;
    public float temperature;
    public TerrainType terrainType;

    // Capa dinámica
    public HexType hexType = HexType.Natural;
    public bool isExplored = false;
    public Dictionary<ResourceType, float> extractedResources = new();

    public List<HexCoordinates> neighbors = new();
}
--- /Users/alexandrodupuiszorrilla/CrystalHex/Assets/Scripts/ChunkGizmoDrawer.cs ---
using UnityEngine;

[ExecuteAlways]
public class ChunkGizmoDrawer : MonoBehaviour
{
    public Color gizmoColor = Color.cyan;

    void OnDrawGizmos()
    {
        Gizmos.color = gizmoColor;

        foreach (Transform child in transform)
        {
            if (child.name.StartsWith("Chunk_"))
            {
                Bounds bounds = new Bounds(child.position, new Vector3(10f, 10f, 0.1f)); // ajusta según chunkSize
                Gizmos.DrawWireCube(bounds.center, bounds.size);
            }
        }
    }
}
--- /Users/alexandrodupuiszorrilla/CrystalHex/Assets/Scripts/CrystalMesh.cs ---
using UnityEngine;

public class CrystalMesh : MonoBehaviour
{
    [SerializeField] private MeshRenderer meshRenderer;
    [SerializeField] private GameObject seededVisual;
    [SerializeField] private GameObject growingVisual;
    [SerializeField] private GameObject fullVisual;

    private Material crystalBaseMaterial;
    private Color crystalColor;
    public void SetCrystalColor(Color color)
    {
        if (meshRenderer != null)
        {
            meshRenderer.material.color = color;
        }
    }

    public void SetColor(Material baseMaterial, Color color)
    {
        crystalBaseMaterial = baseMaterial;
        crystalColor = color;
    }

    public void SetState(HexState state, Color color)
    {
        if (seededVisual != null) seededVisual.SetActive(state == HexState.Seeded);
        if (growingVisual != null) growingVisual.SetActive(state == HexState.Growing);
        if (fullVisual != null) fullVisual.SetActive(state == HexState.Full);

        if (meshRenderer != null)
        {
            meshRenderer.material.color = color;
        }
    }


    public void ShowState(HexState state)
    {
        // Ocultar visual principal
        if (meshRenderer != null)
            meshRenderer.enabled = false;

        // Ocultar todos
        if (seededVisual != null) seededVisual.SetActive(false);
        if (growingVisual != null) growingVisual.SetActive(false);
        if (fullVisual != null) fullVisual.SetActive(false);

        // Elegir el objeto a mostrar
        GameObject visualToShow = null;
        switch (state)
        {
            case HexState.Seeded: visualToShow = seededVisual; break;
            case HexState.Growing: visualToShow = growingVisual; break;
            case HexState.Full: visualToShow = fullVisual; break;
        }

        if (visualToShow != null)
        {
            visualToShow.SetActive(true);

            // Aplicar material personalizado con color
            var renderer = visualToShow.GetComponent<MeshRenderer>();
            if (renderer != null && crystalBaseMaterial != null)
            {
                Material coloredMaterial = new Material(crystalBaseMaterial);
                coloredMaterial.color = crystalColor;
                renderer.material = coloredMaterial;
            }
        }
    }


    public void Clear()
    {
        if (seededVisual != null) seededVisual.SetActive(false);
        if (growingVisual != null) growingVisual.SetActive(false);
        if (fullVisual != null) fullVisual.SetActive(false);
    }

    

}
--- /Users/alexandrodupuiszorrilla/CrystalHex/Assets/Scripts/TickManager.cs ---
using System.Collections.Generic;
using UnityEngine;

public class TickManager : MonoBehaviour
{
    private float tickTimer;
    private float tickInterval;

    public List<HexBehavior> hexesToTick = new();
    public GameSpeed speed = GameSpeed.Fast;

    private void Start()
    {
        // Ensure tickInterval is set when the game starts
        SetSpeed(speed);  // Default speed set in Inspector or here
    }

    void Update()
    {
        tickTimer += Time.deltaTime;
        if (tickTimer >= tickInterval)
        {
            tickTimer = 0f;
            Tick();
            Debug.Log($"Tick occurred at time {Time.time}, current interval: {tickInterval}");
        }
    }

    public void SetSpeed(GameSpeed newSpeed)
    {
        speed = newSpeed;

        switch (speed)
        {
            case GameSpeed.Slow:
                tickInterval = 5f;  // One tick every 100.5 seconds
                break;
            case GameSpeed.Normal:
                tickInterval = 1.5f;     // One tick every 50 seconds
                break;
            case GameSpeed.Fast:
                tickInterval = 0.5f;   // One tick every 60.5 seconds
                break;
        }

        Debug.Log($"Game speed set to {speed} with interval {tickInterval}");
    }

    private void Tick()
    {
        foreach (var hex in hexesToTick)
        {
            hex.OnTick();
        }
    }

    public void Register(HexBehavior hex)
    {
        if (!hexesToTick.Contains(hex))
            hexesToTick.Add(hex);
    }
}
--- /Users/alexandrodupuiszorrilla/CrystalHex/Assets/Scripts/ChunkManager.cs ---
// ChunkManager.cs
using System.Collections.Generic;
using UnityEngine;

public class ChunkManager : MonoBehaviour
{
    public static ChunkManager Instance;

    public int chunkSize = 6; // e.g. 10x10 hexes per chunk
    public int loadRadius = 0; // how many chunks around the player to load
    public int unloadBufferRadius = 2; // ← aquí
    public GameObject hexPrefab;

    private Dictionary<Vector2Int, GameObject> loadedChunks = new();

    private void Awake()
    {
        Instance = this;
    }

    public void UpdateChunks(Vector2Int playerChunkCoord)
    {
        HashSet<Vector2Int> chunksToKeep = new();

        for (int dx = -loadRadius; dx <= loadRadius; dx++)
        {
            for (int dy = -loadRadius; dy <= loadRadius; dy++)
            {
                Vector2Int coord = new Vector2Int(playerChunkCoord.x + dx, playerChunkCoord.y + dy);
                chunksToKeep.Add(coord);

                if (!loadedChunks.ContainsKey(coord))
                {
                    GameObject chunk = ChunkGenerator.GenerateChunk(coord, chunkSize, hexPrefab);
                    loadedChunks[coord] = chunk;
                }
            }
        }

        // Unload chunks that are no longer in range
        List<Vector2Int> toUnload = new();
        foreach (var kvp in loadedChunks)
        {
            Vector2Int diff = kvp.Key - playerChunkCoord;
            if (Mathf.Abs(diff.x) > unloadBufferRadius || Mathf.Abs(diff.y) > unloadBufferRadius)
            {
                Destroy(kvp.Value);
                toUnload.Add(kvp.Key);
            }

        }
        foreach (var coord in toUnload)
        {
            loadedChunks.Remove(coord);
        }
    }

   public static Vector2Int WorldToChunkCoord(HexCoordinates hex)
    {
        int qChunk = Mathf.FloorToInt(hex.Q / (float)Instance.chunkSize);
        int rChunk = Mathf.FloorToInt(hex.R / (float)Instance.chunkSize);
        return new Vector2Int(qChunk, rChunk);
    }

}
--- /Users/alexandrodupuiszorrilla/CrystalHex/Assets/Scripts/GameSpeedUI.cs ---
using UnityEngine;

public class GameSpeedUI : MonoBehaviour
{
    public GameObject dropdownPanel; // assign this in the Inspector

    public void SetSpeedSlow()
    {
        Object.FindFirstObjectByType<TickManager>().SetSpeed(GameSpeed.Slow);
        dropdownPanel.SetActive(false);
    }

    public void SetSpeedNormal()
    {
        Object.FindFirstObjectByType<TickManager>().SetSpeed(GameSpeed.Normal);
        dropdownPanel.SetActive(false);
    }

    public void SetSpeedFast()
    {
        Object.FindFirstObjectByType<TickManager>().SetSpeed(GameSpeed.Fast);
        dropdownPanel.SetActive(false);
    }
}
--- /Users/alexandrodupuiszorrilla/CrystalHex/Assets/Scripts/ChunkGenerator.cs ---
using UnityEngine;

public static class ChunkGenerator
{
    public static GameObject GenerateChunk(Vector2Int chunkCoord, int chunkSize, GameObject hexPrefab)
    {
        GameObject parent = new GameObject($"Chunk_{chunkCoord.x}_{chunkCoord.y}");

        for (int dx = 0; dx < chunkSize; dx++)
        {
            for (int dy = 0; dy < chunkSize; dy++)
            {
                int globalQ = chunkCoord.x * chunkSize + dx;
                int globalR = chunkCoord.y * chunkSize + dy;

                HexCoordinates hexCoord = new HexCoordinates(globalQ, globalR);
                Vector3 worldPos = HexCoordinates.ToWorldPosition(hexCoord, HexRenderer.SharedOuterRadius);

                GameObject hex = Object.Instantiate(hexPrefab, worldPos, Quaternion.identity, parent.transform);
                hex.name = $"Hex_{globalQ}_{globalR}";

                HexBehavior behavior = hex.GetComponent<HexBehavior>();
                if (behavior != null)
                {
                    behavior.coordinates = hexCoord;
                }
            }
            AssignNeighbors(parent);
        }
        return parent;
        
    }
    public static void AssignNeighbors(GameObject chunkRoot)
    {
        HexBehavior[] hexes = chunkRoot.GetComponentsInChildren<HexBehavior>();

        foreach (HexBehavior hex in hexes)
        {
            hex.neighbors.Clear();
            foreach (HexBehavior other in hexes)
            {
                if (hex == other) continue;

                int dq = Mathf.Abs(hex.coordinates.Q - other.coordinates.Q);
                int dr = Mathf.Abs(hex.coordinates.R - other.coordinates.R);

                if ((dq == 1 && dr == 0) || (dq == 0 && dr == 1) || (dq == 1 && dr == 1))
                {
                    hex.neighbors.Add(other);
                }
            }
        }
    }

   

}
--- /Users/alexandrodupuiszorrilla/CrystalHex/Assets/Scripts/HexRenderer.cs ---
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class HexRenderer : MonoBehaviour
{
    private Mesh _mesh;
    private MeshFilter _meshFilter;
    private MeshRenderer _meshRenderer;

    [Header("Hex Settings")]
    public float innerRadius = 0.5f;
    public static float SharedOuterRadius = 1f;
    public float height = 0f;

    [Header("Visual Settings")]
    public Material material;
    public Color baseColor = Color.green;
    private Color _baseColor; // active color used for drawing
    public Color edgeColor = Color.black;

    private List<Face> _faces;
    private List<Color> _colors;

    private void Awake()
    {
        SetupComponents();
        DrawMesh();
        _baseColor = baseColor;

    }

    private void OnEnable()
    {
        SetupComponents();
        DrawMesh();
    }

    public void OnValidate()
    {
#if UNITY_EDITOR
        if (!Application.isPlaying) return;
#endif
        SetupComponents();
        DrawMesh();
    }

    private void SetupComponents()
    {
        if (_meshFilter == null) _meshFilter = GetComponent<MeshFilter>();
        if (_meshRenderer == null) _meshRenderer = GetComponent<MeshRenderer>();
        if (_mesh == null)
        {
            _mesh = new Mesh { name = "HexMesh_Runtime" };
            GetComponent<MeshFilter>().mesh = _mesh;
            GetComponent<MeshCollider>().sharedMesh = _mesh;
        }

        if (_meshRenderer.sharedMaterial == null && material != null)
            _meshRenderer.sharedMaterial = material;
    }

    public void DrawMesh()
    {
        _faces = new List<Face>();
        DrawFaces();
        CombineFaces();
        if (TryGetComponent(out MeshCollider col))
        {
            col.sharedMesh = null; // reset first
            col.sharedMesh = _mesh; // reassign updated mesh
        }

    }

    private void DrawFaces()
    {
        for (int point = 0; point < 6; point++)
        {
            _faces.Add(CreateFace(innerRadius, SharedOuterRadius, height, height, point));
        }
    }

    private Face CreateFace(float innerRad, float outerRad, float heightA, float heightB, int point, bool reverse = false)
    {
        Vector3 pointA = GetPoint(innerRad, heightB, point);
        Vector3 pointB = GetPoint(innerRad, heightB, (point < 5) ? point + 1 : 0);
        Vector3 pointC = GetPoint(outerRad, heightA, (point < 5) ? point + 1 : 0);
        Vector3 pointD = GetPoint(outerRad, heightA, point);

        List<Vector3> vertices = new() { pointA, pointB, pointC, pointD };
        List<int> triangles = new() { 0, 1, 2, 2, 3, 0 };
        List<Vector2> uvs = new() {
            new Vector2(0, 0),
            new Vector2(1, 0),
            new Vector2(1, 1),
            new Vector2(0, 1)
        };

        if (reverse) vertices.Reverse();

        return new Face(vertices, triangles, uvs);
    }

    protected Vector3 GetPoint(float radius, float height, int index)
    {
        float angle_deg = 60 * index;
        float angle_rad = Mathf.Deg2Rad * angle_deg;
        return new Vector3(radius * Mathf.Cos(angle_rad), radius * Mathf.Sin(angle_rad), height);
    }

    private void CombineFaces()
    {
        List<Vector3> vertices = new();
        List<int> triangles = new();
        List<Vector2> uvs = new();
        _colors = new List<Color>();

        for (int i = 0; i < _faces.Count; i++)
        {
            int offset = vertices.Count;
            vertices.AddRange(_faces[i].vertices);
            uvs.AddRange(_faces[i].uvs);

            // Add color: center vertices are lighter, outer ones are darker
            _colors.Add(_baseColor);    // A
            _colors.Add(_baseColor);    // B
            _colors.Add(edgeColor);     // C
            _colors.Add(edgeColor);     // D


            foreach (int t in _faces[i].triangles)
                triangles.Add(t + offset);
        }

        _mesh.Clear();
        _mesh.SetVertices(vertices);
        _mesh.SetTriangles(triangles, 0);
        _mesh.SetUVs(0, uvs);
        _mesh.SetColors(_colors);
        _mesh.RecalculateNormals();

        Debug.Log($"HexRenderer: Mesh generated with {_mesh.vertexCount} vertices and {_mesh.triangles.Length} triangles");
    }

    public struct Face
    {
        public List<Vector3> vertices { get; private set; }
        public List<int> triangles { get; private set; }
        public List<Vector2> uvs { get; private set; }

        public Face(List<Vector3> vertices, List<int> triangles, List<Vector2> uvs)
        {
            this.vertices = vertices;
            this.triangles = triangles;
            this.uvs = uvs;
        }
    }
    public void SetColor(Color newColor)
{
    _baseColor = newColor;
    DrawMesh();
}

public void ResetColor()
{
    _baseColor = baseColor;
    DrawMesh();
}

        #if UNITY_EDITOR
private void Update()
{
    if (Application.isPlaying && Input.GetKeyDown(KeyCode.R))
    {
        DrawMesh();
    }
}
#endif
// Following lines commented where for activating hover and clicked states for Tiles.
// private void OnMouseEnter()
//{
//    _baseColor = edgeColor; // Use a highlight or hover color
 //   DrawMesh();
//}

//private void OnMouseExit()
//{
//    _baseColor = baseColor; // Reset to default
//    DrawMesh();
//}

//private void OnMouseDown()
//{
//    Debug.Log($"Hex clicked: {gameObject.name}");
    
    // Placeholder: simulate crystal planting
//    _baseColor = Color.cyan;
//    DrawMesh();
//}



}
--- /Users/alexandrodupuiszorrilla/CrystalHex/Assets/Scripts/CameraController.cs ---
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float moveSpeed = 10f;
    public float zoomSpeed = 5f;
    public float minZoom = 3f;
    public float maxZoom = 15f;

    private Camera cam;

    void Start()
    {
        cam = Camera.main;
    }

    void Update()
    {
        // Keyboard movement (WASD / Arrow Keys)
        float moveX = Input.GetAxis("Horizontal");
        float moveY = Input.GetAxis("Vertical");
        transform.position += new Vector3(moveX, moveY, 0) * moveSpeed * Time.deltaTime;

        // Zoom with scroll wheel
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0f)
        {
            cam.orthographicSize -= scroll * zoomSpeed;
            cam.orthographicSize = Mathf.Clamp(cam.orthographicSize, minZoom, maxZoom);
        }
    }
}
--- /Users/alexandrodupuiszorrilla/CrystalHex/Assets/Scripts/GameEnums.cs ---
using UnityEngine;

public enum GameSpeed
{
    Slow,
    Normal,
    Fast
}

--- /Users/alexandrodupuiszorrilla/CrystalHex/Assets/Scripts/TerrainGenerator.cs ---
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
--- /Users/alexandrodupuiszorrilla/CrystalHex/Assets/Scripts/HexGridLayout.cs ---
/*using UnityEngine;

public class HexGridLayout : MonoBehaviour
{
    [Header("Grid Settings")]
    public int gridWidth = 5;
    public int gridHeight = 5;
    public float outerRadius = HexRenderer.SharedOuterRadius; // Reference to HexRenderer's outerRadius

    [Header("Hex Prefab")]
    public GameObject hexPrefab;

    private HexBehavior[,] hexGrid;

    //private void Start()
   // {
     //   GenerateGrid();
    //}

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
*/--- /Users/alexandrodupuiszorrilla/CrystalHex/Assets/Scripts/HexBehavior.cs ---
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum HexState { Empty, Influenced, Seeded, Growing, Full }
public enum PlayerType { Red, Blue, Green, Yellow, Purple, Orange }
public enum CrystalType { Red, Blue, Green, Yellow, Purple, Orange }
public enum CrystalSubtype {
    Red1, Red2, Red3, 
    Blue1, Blue2, Blue3,
    Green1, Green2, Green3,
    Yellow1, Yellow2, Yellow3,
    Purple1, Purple2, Purple3,
    Orange1, Orange2, Orange3
}

public class HexBehavior : MonoBehaviour
{
    [SerializeField] private Material crystalBaseMaterial;

    public HexState state = HexState.Empty;
    public CrystalType? crystalType = null;
    public CrystalSubtype? crystalSubtype = null;
    public CrystalType? influencedByType = null;

    public HexCoordinates coordinates;

    public List<HexBehavior> neighbors = new();

    private HexRenderer hexRenderer;
    private CrystalMesh crystalMesh;

    private int ticksRemaining = -1;
    private bool isProgressing = false;

    public Dictionary<CrystalType, int> influenceMap = new();
    public int influenceAmount = 0;
    public int influenceThreshold = 2;

    [Header("Crystal Growth")]
    public float growthMultiplier = 1f;

    public static Dictionary<CrystalType, PlayerType> crystalToPlayer = new()
    {
        { CrystalType.Red, PlayerType.Red },
        { CrystalType.Blue, PlayerType.Blue },
        { CrystalType.Green, PlayerType.Green },
        { CrystalType.Yellow, PlayerType.Yellow },
        { CrystalType.Purple, PlayerType.Purple },
        { CrystalType.Orange, PlayerType.Orange }
    };

    public static Color GetColorForCrystal(CrystalType type)
    {
        return type switch
        {
            CrystalType.Red => Color.red,
            CrystalType.Blue => Color.cyan,
            CrystalType.Green => Color.green,
            CrystalType.Yellow => new Color(1f, 1f, 0.2f),
            CrystalType.Purple => new Color(0.6f, 0f, 0.6f),
            CrystalType.Orange => new Color(1f, 0.5f, 0f),
            _ => Color.white
        };
    }

    private void Awake()
    {
        hexRenderer = GetComponent<HexRenderer>();
        crystalMesh = GetComponentInChildren<CrystalMesh>();
        crystalMesh?.Clear();
    }

    private void Start()
    {
        var tickManager = Object.FindFirstObjectByType<TickManager>();
        tickManager?.Register(this);
    }

    private void OnMouseDown()
    {
        if (state == HexState.Empty || state == HexState.Influenced)
        {
            crystalType = CrystalSelectorUI.Instance?.selectedCrystal ?? CrystalType.Red;
            state = HexState.Seeded;

            if (crystalType.HasValue)
            {
                crystalMesh?.SetColor(crystalBaseMaterial, GetColorForCrystal(crystalType.Value));
                crystalMesh?.ShowState(state);
            }

            var terrainGen = GameObject.FindFirstObjectByType<TerrainGenerator>();
            terrainGen?.TryExpandFrom(coordinates.ToVector2Int()
);
            Debug.Log($"{name} → Seeded");
        }
    }

    public void OnTick()
    {
        EvaluateInfluence();

        if (isProgressing)
        {
            ticksRemaining--;
            if (ticksRemaining <= 0)
                AdvanceState();
            return;
        }

        if ((state == HexState.Empty || state == HexState.Influenced) && influencedByType != null)
        {
            if (influenceAmount >= influenceThreshold)
            {
                if (state == HexState.Empty)
                {
                    state = HexState.Influenced;
                    hexRenderer.SetColor(Color.Lerp(GetColorForCrystal(influencedByType.Value), Color.white, 0.7f));
                    isProgressing = true;
                    ticksRemaining = Mathf.Clamp(Mathf.RoundToInt((10f - influenceAmount * 2f) / growthMultiplier), 1, 6);
                    Debug.Log($"{name} → Influenced");
                    return;
                }

                if (state == HexState.Influenced && influenceAmount >= influenceThreshold + 1)
                {
                    state = HexState.Seeded;
                    crystalType = influencedByType;
                    crystalMesh?.SetColor(crystalBaseMaterial, GetColorForCrystal(crystalType.Value));
                    crystalMesh?.ShowState(state);
                    Debug.Log($"{name} → Auto-Seeded from strong influence");
                    isProgressing = true;
                    ticksRemaining = Mathf.Clamp(Mathf.RoundToInt((8f - influenceAmount * 1f) / growthMultiplier), 2, 6);
                    return;
                }

                if (state == HexState.Influenced)
                {
                    state = HexState.Growing;
                    crystalType = influencedByType;
                    if (crystalType.HasValue)
                    {
                        crystalMesh?.SetColor(crystalBaseMaterial, GetColorForCrystal(crystalType.Value));
                        crystalMesh?.ShowState(state);
                    }
                    isProgressing = true;
                    ticksRemaining = Mathf.Clamp(Mathf.RoundToInt((10f - influenceAmount * 2f) / growthMultiplier), 1, 5);
                    Debug.Log($"{name} → Growing from influence");
                    return;
                }
            }
        }

        if (state == HexState.Seeded)
        {
            isProgressing = true;
            ticksRemaining = Mathf.Clamp(Mathf.RoundToInt((8f - influenceAmount * 1f) / growthMultiplier), 2, 6);
        }
        else if (state == HexState.Growing)
        {
            isProgressing = true;
            ticksRemaining = Mathf.RoundToInt(3f / growthMultiplier);
        }

        if (state == HexState.Full && influencedByType != null && influencedByType != crystalType)
        {
            if (influenceMap[influencedByType.Value] >= influenceThreshold + 1)
            {
                state = HexState.Growing;
                crystalType = influencedByType;
                if (crystalType.HasValue)
                {
                    crystalMesh?.SetColor(crystalBaseMaterial, GetColorForCrystal(crystalType.Value));
                    crystalMesh?.ShowState(state);
                }
                isProgressing = true;
                ticksRemaining = Mathf.Clamp(Mathf.RoundToInt((8f - influenceAmount * 1f) / growthMultiplier), 1, 5);
                Debug.Log($"{name} → Reclaimed by {influencedByType}");
            }
        }

        if (state == HexState.Full)
        {
            var terrainGen = GameObject.FindFirstObjectByType<TerrainGenerator>();
            terrainGen?.TryExpandFrom(coordinates.ToVector2Int());
        }
    }

    public void AdvanceState()
    {
        isProgressing = false;
        switch (state)
        {
            case HexState.Seeded:
                state = HexState.Growing;
                crystalMesh?.ShowState(state);
                Debug.Log($"{name} → Advancing to Growing");
                break;
            case HexState.Growing:
                state = HexState.Full;
                crystalMesh?.ShowState(state);
                Debug.Log($"{name} → Advancing to Full");
                break;
        }
    }

    public void EvaluateInfluence()
    {
        influenceMap.Clear();

        foreach (var neighbor in neighbors)
        {
            if (neighbor.crystalType != null && neighbor.state == HexState.Full)
            {
                var type = neighbor.crystalType.Value;
                if (!influenceMap.ContainsKey(type))
                    influenceMap[type] = 0;
                influenceMap[type]++;
            }
        }

        if (influenceMap.Count == 0)
        {
            influencedByType = null;
            influenceAmount = 0;
            return;
        }

        var sorted = influenceMap.OrderByDescending(kvp => kvp.Value).ToList();
        var top = sorted[0];

        if (sorted.Count > 1 && top.Value == sorted[1].Value)
        {
            influencedByType = null;
            influenceAmount = 0;
        }
        else
        {
            influencedByType = top.Key;
            influenceAmount = top.Value;

            if (state == HexState.Full && influencedByType != null)
            {
                var baseColor = GetColorForCrystal(influencedByType.Value);
                hexRenderer.SetColor(Color.Lerp(baseColor, Color.white, 0.1f));
            }
            else if (state == HexState.Empty && influenceAmount > 0 && influenceAmount < influenceThreshold)
            {
                var baseColor = GetColorForCrystal(influencedByType.Value);
                hexRenderer.SetColor(Color.Lerp(baseColor, Color.white, 0.7f));
            }
        }
    }
}
--- /Users/alexandrodupuiszorrilla/CrystalHex/Assets/Scripts/GameConfig.cs ---
using UnityEngine;

public enum MapShape { Square, Hexagonal, Random }  // ✅ Ahora correctamente declarado FUERA de la clase

[CreateAssetMenu(fileName = "GameConfig", menuName = "Configs/GameConfig")]
public class GameConfig : ScriptableObject
{
    [Header("Terrain Generation")]
    public int initialRadius = 5;
    public int maxExpansionRadius = 10;
    public int maxTiles = 500;

    [Header("Map Bounds (Used for Expansion)")]
    public int minX = -10;
    public int maxX = 10;
    public int minY = -10;
    public int maxY = 10;

    [Header("Visual Diversity")]
    public MapShape mapShape = MapShape.Hexagonal; // ✅ Usa el enum declarado fuera
    [Range(0.01f, 0.2f)]
    public float terrainDiversity = 0.05f;

    [Header("Multipliers")]
    [Range(0f, 1f)] public float slowTilePercent = 0.05f;
    [Range(0f, 1f)] public float fastTilePercent = 0.05f;

    [Header("Debug Options")]
    public bool enableDebugLabels = true;

    [Header("Terrain Materials")]
    [SerializeField]
    public Material[] terrainMaterials;

}
