# HexMap – Código fuente consolidado

_Generado el Mon May 19 00:16:51 EST 2025_\n

---

## 📁 CameraController.cs
```csharp
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
    // Movimiento horizontal/vertical en X-Z (WASD / flechas)
    float moveX = Input.GetAxis("Horizontal");
    float moveZ = Input.GetAxis("Vertical");

    // Movimiento vertical en Y (Q/E)
    float moveY = 0f;
    if (Input.GetKey(KeyCode.E)) moveY = 1f;
    if (Input.GetKey(KeyCode.Q)) moveY = -1f;

    Vector3 move = new Vector3(moveX, moveY, moveZ) * moveSpeed * Time.deltaTime;
    transform.position += move;

        // Zoom with scroll wheel
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0f)
        {
            cam.orthographicSize -= scroll * zoomSpeed;
            cam.orthographicSize = Mathf.Clamp(cam.orthographicSize, minZoom, maxZoom);
        }
    }
}
```

---

## 📁 ChunkGenerator.cs
```csharp
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

                    var hexData = WorldMapManager.Instance.GetOrGenerateHex(hexCoord);
                    // Obtener renderer y aplicar visualización de altura y color
                    var renderer = hex.GetComponent<HexRenderer>();
                    if (renderer != null)
                    {
                        var config = Resources.Load<ChunkMapGameConfig>("ChunkMapGameConfig");
                        if (config != null)  // ← buena práctica por si no se encuentra el asset
                        {
                            float elevationHeight = hexData.elevation * config.elevationScale;
                            renderer.SetHeight(elevationHeight);

                            Material mat = config.GetMaterialFor(hexData.terrainType);
                            if (mat != null)
                                renderer.GetComponent<MeshRenderer>().material = mat;
                        }
                        else
                        {
                            Debug.LogWarning("ChunkMapGameConfig not found in Resources.");
                        }
                    }

                    WorldMapManager.Instance.AssignNeighborReferences(hexData);

                    foreach (var neighborData in hexData.neighborRefs)
                    {
                        if (WorldMapManager.Instance.TryGetHex(neighborData.coordinates, out var nData))
                        {
                            Vector2Int neighborChunkCoord = ChunkManager.WorldToChunkCoord(nData.coordinates);
                            if (ChunkManager.Instance.loadedChunks.TryGetValue(neighborChunkCoord, out var neighborChunk))
                            {
                                var behaviorList = neighborChunk.GetComponentsInChildren<HexBehavior>();
                                foreach (var other in behaviorList)
                                {
                                    if (other.coordinates.Equals(nData.coordinates))
                                    {
                                        behavior.neighbors.Add(other);
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        // ✅ Asignar vecinos visuales entre chunks (cuando ya todos existen)
        HexBehavior[] behaviors = parent.GetComponentsInChildren<HexBehavior>();
        foreach (var behavior in behaviors)
        {
            AssignBehaviorNeighborsFromWorldMap(behavior);
            Debug.Log($"Assigning neighbors to {behavior.name}, found {behavior.neighbors.Count}");

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


    public static void AssignBehaviorNeighborsFromWorldMap(HexBehavior behavior)
    {
        var hexData = WorldMapManager.Instance.GetOrGenerateHex(behavior.coordinates);
        WorldMapManager.Instance.AssignNeighborReferences(hexData);

        foreach (var neighborData in hexData.neighborRefs)
        {
            if (WorldMapManager.Instance.TryGetHex(neighborData.coordinates, out var nData))
            {
                Vector2Int neighborChunkCoord = ChunkManager.WorldToChunkCoord(nData.coordinates);
                if (ChunkManager.Instance.loadedChunks.TryGetValue(neighborChunkCoord, out var neighborChunk))
                {
                    var behaviorList = neighborChunk.GetComponentsInChildren<HexBehavior>();
                    foreach (var other in behaviorList)
                    {
                        if (other.coordinates.Equals(nData.coordinates))
                        {
                            behavior.neighbors.Add(other);
                            break;
                        }
                    }
                }
            }
        }
    }



}

```

---

## 📁 ChunkGizmoDrawer.cs
```csharp
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
                Bounds bounds = new Bounds(child.position, new Vector3(10f, 0.1f, 10f)); // ajusta según chunkSize
                Gizmos.DrawWireCube(bounds.center, bounds.size);
            }
        }
    }
}
```

---

## 📁 ChunkManager.cs
```csharp
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChunkManager : MonoBehaviour
{
    public static ChunkManager Instance;

    public GameObject hexPrefab;
    public int chunkSize = 10;
    public int loadRadius = 1; // Aumentado a 1 para tener vecinos

    [Range(0, 10)]
    public int unloadRadius = 2; // Si es 0, no descarga nada


    public Dictionary<Vector2Int, GameObject> loadedChunks = new();

    private void Awake()
    {
        Instance = this;
    }

    public void UpdateChunks(Vector2Int playerChunkCoord)
    {
        HashSet<Vector2Int> chunksToKeep = new();
        List<Vector2Int> toUnload = new();

        bool anyNewChunks = false;

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
                    anyNewChunks = true;
                }
            }
        }

        // ✅ Reasignar vecinos visuales luego de cargar nuevos chunks
        if (anyNewChunks)
        {
            ReassignAllChunkBehaviorNeighbors();
        }

       if (unloadRadius > 0)
        {
            foreach (var coord in loadedChunks.Keys)
            {
                int dist = Mathf.Max(
                    Mathf.Abs(coord.x - playerChunkCoord.x),
                    Mathf.Abs(coord.y - playerChunkCoord.y)
                );

                if (dist > loadRadius + unloadRadius)
                {
                    toUnload.Add(coord);
                }
            }
        }


        foreach (var coord in toUnload)
        {
            if (loadedChunks.TryGetValue(coord, out var chunk))
            {
                Destroy(chunk);
                loadedChunks.Remove(coord);
            }
        }

    }

    public static Vector2Int WorldToChunkCoord(HexCoordinates coordinates)
    {
        int chunkX = Mathf.FloorToInt((float)coordinates.Q / Instance.chunkSize);
        int chunkY = Mathf.FloorToInt((float)coordinates.R / Instance.chunkSize);
        return new Vector2Int(chunkX, chunkY);
    }

    public void ReassignAllChunkBehaviorNeighbors()
    {
        foreach (var chunk in loadedChunks.Values)
        {
            var behaviors = chunk.GetComponentsInChildren<HexBehavior>();
            foreach (var behavior in behaviors)
            {
                ChunkGenerator.AssignBehaviorNeighborsFromWorldMap(behavior);
            }
        }
    }
}
```

---

## 📁 Configs/ChunkMapGameConfig.cs
```csharp
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ChunkMapGameConfig", menuName = "Configs/Chunk Map Game Config")]
public class ChunkMapGameConfig : ScriptableObject
{
    [System.Serializable]
    public struct TerrainMaterialPair
    {
        public TerrainType terrainType;
        public Material material;
    }

    public TerrainMaterialPair[] terrainMaterials;
    public float elevationScale = 1f;

    private Dictionary<TerrainType, Material> _materialMap;

    public Material GetMaterialFor(TerrainType type)
    {
        if (_materialMap == null)
        {
            _materialMap = new Dictionary<TerrainType, Material>();
            foreach (var pair in terrainMaterials)
            {
                _materialMap[pair.terrainType] = pair.material;
            }
        }

        return _materialMap.TryGetValue(type, out var mat) ? mat : null;
    }
}
```

---

## 📁 CrystalGenerator.cs
```csharp
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
```

---

## 📁 CrystalMesh.cs
```csharp
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
```

---

## 📁 CrystalSelectorUI.cs
```csharp
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
```

---

## 📁 GameConfig.cs
```csharp
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
```

---

## 📁 GameEnums.cs
```csharp
using UnityEngine;

public enum GameSpeed
{
    Slow,
    Normal,
    Fast
}

```

---

## 📁 GameSpeedDropdownToggle.cs
```csharp
using UnityEngine;

public class GameSpeedDropdownToggle : MonoBehaviour
{
    public GameObject dropdownPanel;

    public void ToggleDropdown()
    {
        dropdownPanel.SetActive(!dropdownPanel.activeSelf);
    }
}
```

---

## 📁 GameSpeedUI.cs
```csharp
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
```

---

## 📁 HexBehavior.cs
```csharp
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
        Debug.Log($"{name} tiene {neighbors.Count} vecinos.");


        if (neighbors == null || neighbors.Count == 0)
        {
            Debug.LogWarning($"{name} has no neighbors!");
            return;
        }

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
        WorldMapManager.Instance.AssignNeighborReferences(WorldMapManager.Instance.GetOrGenerateHex(coordinates));
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
    private void OnDestroy()
    {
        var tickManager = FindFirstObjectByType<TickManager>();
        tickManager?.Unregister(this);
    }

}
```

---

## 📁 HexCoordinates.cs
```csharp
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct HexCoordinates
{
    public readonly int Q; // Coordenada axial horizontal (x)
    public readonly int R; // Coordenada axial vertical (z)

    // Coordenada cúbica implícita
    public int S => -Q - R;

    public HexCoordinates(int q, int r)
    {
        Q = q;
        R = r;
    }

    // Devuelve la lista de coordenadas vecinas (orientación flat-top)
    public List<HexCoordinates> GetAllNeighbors()
    {
        return new List<HexCoordinates>
        {
            new HexCoordinates(Q + 1, R),
            new HexCoordinates(Q - 1, R),
            new HexCoordinates(Q, R + 1),
            new HexCoordinates(Q, R - 1),
            new HexCoordinates(Q + 1, R - 1),
            new HexCoordinates(Q - 1, R + 1)
        };
    }

    public static HexCoordinates FromWorldPosition(Vector3 position, float hexOuterRadius)
    {
        float width = hexOuterRadius * 2f;
        float height = Mathf.Sqrt(3f) * hexOuterRadius;

        float q = (position.x * 2f / 3f) / hexOuterRadius;
        float r = (-position.x / 3f + Mathf.Sqrt(3f) / 3f * position.z) / hexOuterRadius;

        return FromFractional(q, r);
    }

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
            float z = coord.R * height;

            if (coord.Q % 2 != 0)
            {
                z += height / 2f;
            }

            return new Vector3(x, 0f, z); // ← elevación en Y ahora
        }


    public static int Distance(HexCoordinates a, HexCoordinates b)
    {
        return (Mathf.Abs(a.Q - b.Q) + Mathf.Abs(a.R - b.R) + Mathf.Abs(a.S - b.S)) / 2;
    }

    public static HexCoordinates Zero => new HexCoordinates(0, 0);
}```

---

## 📁 HexGridLayout.cs
```csharp
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

                Vector3 spawnPos = new Vector3(xOffset, 0f, yOffset);
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
*/```

---

## 📁 HexMapCamera.cs
```csharp
﻿using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Component that controls the singleton camera that navigates the hex map.
/// </summary>
public class HexMapCamera : MonoBehaviour
{
	[SerializeField]
	float stickMinZoom, stickMaxZoom;

	[SerializeField]
	float swivelMinZoom, swivelMaxZoom;

	[SerializeField]
	float moveSpeedMinZoom, moveSpeedMaxZoom;

	[SerializeField]
	float rotationSpeed;

	Transform swivel, stick;

	float zoom = 1f;
	float rotationAngle;
	static HexMapCamera instance;

	public static bool Locked
	{
		set => instance.enabled = !value;
	}

	public static void ValidatePosition() => instance.AdjustPosition(0f, 0f);

	void Awake()
	{
		swivel = transform.GetChild(0);
		stick = swivel.GetChild(0);

		// Posición inicial forzada
		swivel.localPosition = new Vector3(0f, 0f, 10f);
		swivel.localRotation = Quaternion.Euler(30f, 0f, 0f);
		stick.localPosition = new Vector3(0f, 0f, -12f);
	}

	void OnEnable()
	{
		instance = this;
		ValidatePosition();
	}

	void Update()
	{
		float zoomDelta = Input.GetAxis("Mouse ScrollWheel");
		if (zoomDelta != 0f)
		{
			AdjustZoom(zoomDelta);
		}

		float rotationDelta = 0f;
		if (Input.GetKey(KeyCode.Q)) rotationDelta = -1f;
		if (Input.GetKey(KeyCode.E)) rotationDelta = 1f;
		if (rotationDelta != 0f)
		{
			AdjustRotation(rotationDelta);
		}

		float xDelta = 0f;
		if (Input.GetKey(KeyCode.A)) xDelta = -1f;
		if (Input.GetKey(KeyCode.D)) xDelta = 1f;

		float zDelta = 0f;
		if (Input.GetKey(KeyCode.W)) zDelta = 1f;
		if (Input.GetKey(KeyCode.S)) zDelta = -1f;

		if (xDelta != 0f || zDelta != 0f)
		{
			AdjustPosition(xDelta, zDelta);
		}
		if (Input.GetKey(KeyCode.C))
		{
			Transform player = GameObject.FindWithTag("Player")?.transform;
			if (player != null)
			{
				Vector3 pos = player.position;
				pos.y = transform.position.y;
				transform.position = pos;
			}
		}
	}

	void AdjustZoom(float delta)
	{
		zoom = Mathf.Clamp01(zoom + delta);

		float distance = Mathf.Lerp(stickMinZoom, stickMaxZoom, zoom);
		stick.localPosition = new Vector3(0f, 0f, distance);

		float angle = Mathf.Lerp(swivelMinZoom, swivelMaxZoom, zoom);
		swivel.localRotation = Quaternion.Euler(angle, 0f, 0f);
	}

	void AdjustRotation (float delta)
	{
		rotationAngle += delta * rotationSpeed * Time.deltaTime;
		if (rotationAngle < 0f)
		{
			rotationAngle += 360f;
		}
		else if (rotationAngle >= 360f)
		{
			rotationAngle -= 360f;
		}
		transform.localRotation = Quaternion.Euler(0f, rotationAngle, 0f);
	}

	void AdjustPosition(float xDelta, float zDelta)
	{
		Vector3 direction = new Vector3(xDelta, 0f, zDelta).normalized;
		float damping = Mathf.Max(Mathf.Abs(xDelta), Mathf.Abs(zDelta));
		float distance = Mathf.Lerp(moveSpeedMinZoom, moveSpeedMaxZoom, zoom) * damping * Time.deltaTime;

		Vector3 position = transform.localPosition;
		position += direction * distance;
		transform.localPosition = position;
	}
}```

---

## 📁 HexRenderer.cs
```csharp
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
public class HexRenderer : MonoBehaviour
{
    public static float SharedOuterRadius = 1f;

    [Header("Hex Settings")]
    public float innerRadius = 0.5f;
    public static float outerRadiusShared = 1f;
    public float columnHeight = 0f;

    [Header("Visual")]
    public Material material;
    public Color topColor = Color.magenta;
    public Color sideColor = Color.black;

    [Header("Scale Settings")]
    [Range(0.01f, 1f)] public float heightScale = 0.25f; // reduce visual height without affecting elevation data

    Mesh _mesh;
    MeshFilter _mf;
    MeshCollider _mc;
    MeshRenderer _mr;

    const int terracesPerSlope = 2;
    const int terraceSteps = terracesPerSlope * 2 + 1;
    const float horizontalTerraceStepSize = 1f / terraceSteps;
    const float verticalTerraceStepSize = 1f / (terracesPerSlope + 1);

    void Awake() => BuildMesh();
    void OnEnable() => BuildMesh();

#if UNITY_EDITOR
    void OnValidate() {
        if (!Application.isPlaying) BuildMesh();
    }
#endif

    public void SetHeight(float h) {
        columnHeight = h;
        BuildMesh();
    }

    public void SetColor(Color color) {
        topColor = color;
        BuildMesh();
    }

    void BuildMesh() {
        if (_mf == null) _mf = GetComponent<MeshFilter>();
        if (_mc == null) _mc = GetComponent<MeshCollider>();
        if (_mr == null) _mr = GetComponent<MeshRenderer>();
        if (_mesh == null) _mesh = new Mesh { name = "HexColumn" };

        _mesh.Clear();

        List<Vector3> v = new();
        List<int> t = new();
        List<Color> c = new();

        float displayHeight = columnHeight * heightScale;

        Vector3 centerTop = new Vector3(0f, displayHeight, 0f);
        Vector3 centerBottom = Vector3.zero;

        // --- Tapa superior ---
        int centerTopIndex = v.Count;
        v.Add(centerTop);
        c.Add(topColor);

        for (int i = 0; i < 6; i++) {
            Vector3 corner = GetFlatPoint(i, displayHeight);
            v.Add(corner);
            c.Add(topColor);
        }

        for (int i = 0; i < 6; i++) {
            int current = centerTopIndex + 1 + i;
            int next = i == 5 ? centerTopIndex + 1 : current + 1;
            t.AddRange(new[] { centerTopIndex, current, next });
        }

        // --- Tapa inferior (para sombreado o debug)
        int centerBottomIndex = v.Count;
        v.Add(centerBottom);
        c.Add(sideColor);

        for (int i = 0; i < 6; i++) {
            Vector3 corner = GetFlatPoint(i, 0f);
            v.Add(corner);
            c.Add(sideColor);
        }

        for (int i = 0; i < 6; i++) {
            int current = centerBottomIndex + 1 + i;
            int next = i == 5 ? centerBottomIndex + 1 : current + 1;
            t.AddRange(new[] { centerBottomIndex, next, current });
        }

        // --- Laterales con slope y terrace (Catlike-inspired)
        for (int i = 0; i < 6; i++) {
            Vector3 bottomA = GetFlatPoint(i, 0f);
            Vector3 topA = GetFlatPoint(i, displayHeight);
            Vector3 bottomB = GetFlatPoint((i + 1) % 6, 0f);
            Vector3 topB = GetFlatPoint((i + 1) % 6, displayHeight);

            TriangulateTerracedEdge(v, t, c, bottomA, topA, bottomB, topB);
        }

        _mesh.SetVertices(v);
        _mesh.SetColors(c);
        _mesh.SetTriangles(t, 0);
        _mesh.RecalculateNormals();

        _mf.sharedMesh = _mesh;
        _mc.sharedMesh = _mesh;
        if (_mr.sharedMaterial == null && material != null)
            _mr.sharedMaterial = material;
    }

    void TriangulateTerracedEdge(List<Vector3> v, List<int> t, List<Color> c, Vector3 beginBottom, Vector3 beginTop, Vector3 endBottom, Vector3 endTop) {
        Vector3 v00 = beginBottom;
        Vector3 v01 = beginTop;
        Vector3 v10, v11;

        for (int i = 1; i <= terraceSteps; i++) {
            float hStep = i * horizontalTerraceStepSize;
            float vStep = i * verticalTerraceStepSize;

            v10 = Vector3.Lerp(beginBottom, endBottom, hStep);
            v11 = Vector3.Lerp(beginTop, endTop, hStep);
            v11.y = Mathf.Lerp(beginTop.y, endTop.y, vStep);

            int baseIndex = v.Count;

            v.Add(v00); c.Add(sideColor);
            v.Add(v01); c.Add(topColor);
            v.Add(v10); c.Add(sideColor);
            v.Add(v11); c.Add(topColor);

            t.AddRange(new[] { baseIndex, baseIndex + 1, baseIndex + 2 });
            t.AddRange(new[] { baseIndex + 2, baseIndex + 1, baseIndex + 3 });

            v00 = v10;
            v01 = v11;
        }
    }

    Vector3 GetFlatPoint(int index, float y) {
        float angle = 60f * index * Mathf.Deg2Rad;
        return new Vector3(
            SharedOuterRadius * Mathf.Cos(angle),
            y,
            SharedOuterRadius * Mathf.Sin(angle)
        );
    }
}```

---

## 📁 PlayerController.cs
```csharp
using UnityEngine;
using System.Collections;


public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;

    private Vector2Int currentChunkCoord;

    IEnumerator Start()
    {
        yield return new WaitUntil(() => ChunkManager.Instance != null);

        // Espera a que WorldMapManager esté inicializado
        yield return new WaitUntil(() => WorldMapManager.Instance != null);

        HexCoordinates spawnCoord = FindNonWaterSpawnTile();
        Vector3 spawnPosition = HexCoordinates.ToWorldPosition(spawnCoord, HexRenderer.SharedOuterRadius);
        transform.position = spawnPosition;

        Vector2Int chunkCoord = ChunkManager.WorldToChunkCoord(spawnCoord);
        currentChunkCoord = chunkCoord;
        ChunkManager.Instance.UpdateChunks(chunkCoord);
    }




    void Update()
    {
        float h = 0f;
        float v = 0f;

        if (Input.GetKey(KeyCode.LeftArrow)) h = -1f;
        if (Input.GetKey(KeyCode.RightArrow)) h = 1f;
        if (Input.GetKey(KeyCode.UpArrow)) v = 1f;
        if (Input.GetKey(KeyCode.DownArrow)) v = -1f;



        Vector3 move = new Vector3(h, 0f, v) * moveSpeed * Time.deltaTime;
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
    HexCoordinates FindNonWaterSpawnTile()
    {
        foreach (var hex in WorldMapManager.Instance.GetAllHexes())
        {
            if (hex.terrainType != TerrainType.OceanDeep && hex.terrainType != TerrainType.OceanShallow)
            {
                return hex.coordinates;
            }
        }

        Debug.LogWarning("No suitable land tile found! Defaulting to (0,0).");
        return new HexCoordinates(0, 0);
    }


}
```

---

## 📁 TerrainGenerator.cs
```csharp
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
```

---

## 📁 TickManager.cs
```csharp
using System.Collections.Generic;
using UnityEngine;

public class TickManager : MonoBehaviour
{
    private float tickTimer;
    private float tickInterval;

    public List<HexBehavior> WorldTickSystem = new();
    public GameSpeed speed = GameSpeed.Slow;

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
        foreach (var hex in WorldTickSystem)
        {
            hex.OnTick();
        }
    }

    public void Register(HexBehavior hex)
    {
        if (!WorldTickSystem.Contains(hex))
            WorldTickSystem.Add(hex);
    }

    public void Unregister(HexBehavior hex)
{
    if (WorldTickSystem.Contains(hex))
        WorldTickSystem.Remove(hex);
}

}
```

---

## 📁 WorldLogic/ChunkMapConfigController.cs
```csharp
using UnityEngine;

public class ChunkMapConfigController : MonoBehaviour
{
    public ChunkMapGameConfig config;

    [Range(0f, 50f)]
    public float elevationScale = 1f;

    void OnValidate()
    {
        if (config != null)
        {
            config.elevationScale = elevationScale;
        }
    }

    void Start()
    {
        if (config != null)
        {
            elevationScale = config.elevationScale;
        }
    }
}
```

---

## 📁 WorldLogic/HexData.cs
```csharp
using System.Collections.Generic;




public enum TerrainType
{
    OceanDeep,
    OceanShallow,
    Beach,
    Plains,
    Hills,
    Valley,
    Mountains,
    Forest
}

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

    // Vecinos (sólo coordenadas, útil para persistencia o reconstrucción rápida)
    public List<HexCoordinates> neighborCoords = new();

    // En runtime, puede poblarse dinámicamente con referencias (si es necesario)
    public List<HexData> neighborRefs = new();
}```

---

## 📁 WorldLogic/PerlinSettings.cs
```csharp
using UnityEngine;

[CreateAssetMenu(menuName = "World/PerlinSettings")]
public class PerlinSettings : ScriptableObject
{
    public float elevationFreq = 0.02f;
    public int elevationSeedOffset = 1000;

[Header("Anomaly Settings")]
    [Range(0f, 1f)] public float anomalyThreshold = 0.15f;
    [Range(0f, 1f)] public float anomalyStrength = 0.25f;
    public float anomalyFrequency = 0.1f;
    public int anomalySeedOffset = 5000;
    public float moistureFreq = 0.03f;
    public int moistureSeedOffset = 2000;

    public float tempFreq = 0.015f;
    public int tempSeedOffset = 3000;


}
```

---

## 📁 WorldLogic/PerlinSettingsController.cs
```csharp
using UnityEngine;

public class PerlinSettingsController : MonoBehaviour
{
    public PerlinSettings perlinSettings;

    [Range(0.001f, 5f)] public float elevationFreq = 0.02f;
    [Range(0.001f, 1f)] public float moistureFreq = 0.03f;
    [Range(0.001f, 1f)] public float tempFreq = 0.015f;

    public int elevationSeedOffset = 1000;
    public int moistureSeedOffset = 2000;
    public int tempSeedOffset = 3000;

    void OnValidate()
    {
        if (perlinSettings != null)
        {
            perlinSettings.elevationFreq = elevationFreq;
            perlinSettings.moistureFreq = moistureFreq;
            perlinSettings.tempFreq = tempFreq;

            perlinSettings.elevationSeedOffset = elevationSeedOffset;
            perlinSettings.moistureSeedOffset = moistureSeedOffset;
            perlinSettings.tempSeedOffset = tempSeedOffset;
        }
    }

    void Start()
    {
        if (perlinSettings != null)
        {
            elevationFreq = perlinSettings.elevationFreq;
            moistureFreq = perlinSettings.moistureFreq;
            tempFreq = perlinSettings.tempFreq;

            elevationSeedOffset = perlinSettings.elevationSeedOffset;
            moistureSeedOffset = perlinSettings.moistureSeedOffset;
            tempSeedOffset = perlinSettings.tempSeedOffset;
        }
    }

    [Header("Anomaly Settings")]
    [Range(0f, 1f)]
    public float anomalyStrength = 0.25f;

    [Range(0f, 1f)]
    public float anomalyThreshold = 0.15f;

    public float anomalyFrequency = 0.1f;
    public int anomalySeedOffset = 5000;

}
```

---

## 📁 WorldLogic/PerlinUtility.cs
```csharp
using UnityEngine;


public static class PerlinUtility
{
    public static float Perlin(HexCoordinates coord, float frequency, int seedOffset)
    {
        float nx = (coord.Q + seedOffset) * frequency;
        float ny = (coord.R + seedOffset) * frequency;
        return Mathf.PerlinNoise(nx, ny);
    }

    public static float FractalPerlin(HexCoordinates coord, float baseFreq, int octaves, float lacunarity, float persistence, int seedOffset)
    {
        float total = 0f;
        float amplitude = 1f;
        float frequency = baseFreq;
        float maxValue = 0f;

        for (int i = 0; i < octaves; i++)
        {
            float nx = (coord.Q + seedOffset) * frequency;
            float ny = (coord.R + seedOffset) * frequency;
            total += Mathf.PerlinNoise(nx, ny) * amplitude;

            maxValue += amplitude;
            amplitude *= persistence;
            frequency *= lacunarity;
        }

        return total / maxValue;
    }

   public static float ApplyElevationAnomaly(
    HexCoordinates coord,
    float baseElevation,
    float anomalyFreq,
    float anomalyThreshold,
    float anomalyStrength,
    int seedOffset)
{
    float noise = Mathf.PerlinNoise(
        (coord.Q + seedOffset) * anomalyFreq,
        (coord.R + seedOffset) * anomalyFreq
    );

    if (noise > 1f - anomalyThreshold)
        return baseElevation + anomalyStrength;

    if (noise < anomalyThreshold)
        return baseElevation - anomalyStrength;

    return baseElevation;
}


}
```

---

## 📁 WorldLogic/WorldMapManager.cs
```csharp
using System.Collections.Generic;
using UnityEngine;

public class WorldMapManager : MonoBehaviour
{
    // Asigna referencias activas a vecinos existentes
    public void AssignNeighborReferences(HexData hex)
    {
        hex.neighborRefs.Clear();
        foreach (var coord in hex.neighborCoords)
        {
            if (worldMap.TryGetValue(coord, out var neighbor))
            {
                hex.neighborRefs.Add(neighbor);
            }
        }
    }

    // Asigna referencias para todos los HexData de un chunk (opcional)
    public void AssignNeighborsForChunk(List<HexData> chunkHexes)
    {
        foreach (var hex in chunkHexes)
        {
            AssignNeighborReferences(hex);
        }
    }

    public static WorldMapManager Instance;

    [Header("World Settings")]
    public int seed = 42;
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
        float baseElevation = PerlinUtility.FractalPerlin(
    coord,
    perlinSettings.elevationFreq,
    4,           // octaves
    2f,          // lacunarity
    0.5f,        // persistence
    perlinSettings.elevationSeedOffset + seed
);

float finalElevation = PerlinUtility.ApplyElevationAnomaly(
    coord,
    baseElevation,
    perlinSettings.anomalyFrequency,
    perlinSettings.anomalyThreshold,
    perlinSettings.anomalyStrength,
    perlinSettings.anomalySeedOffset + seed
);

hex.elevation = finalElevation;



        hex.moisture = PerlinUtility.Perlin(coord, perlinSettings.moistureFreq, perlinSettings.moistureSeedOffset + seed);
        hex.temperature = PerlinUtility.Perlin(coord, perlinSettings.tempFreq, perlinSettings.tempSeedOffset + seed);

        // Bioma inicial provisional
        if (hex.elevation < 0.08f)
            hex.terrainType = TerrainType.OceanDeep;
        else if (hex.elevation < 0.16f)
            hex.terrainType = TerrainType.OceanShallow;
        else if (hex.elevation < 0.22f)
            hex.terrainType = TerrainType.Beach;
        else if (hex.elevation < 0.38f)
            hex.terrainType = TerrainType.Plains;
        else if (hex.elevation < 0.52f)
            hex.terrainType = TerrainType.Valley;
        else if (hex.elevation < 0.68f)
            hex.terrainType = TerrainType.Forest;
        else if (hex.elevation < 0.82f)
            hex.terrainType = TerrainType.Hills;
        else
            hex.terrainType = TerrainType.Mountains;



        // Asignación lógica de vecinos (coordenadas)
        foreach (HexCoordinates neighbor in coord.GetAllNeighbors())
        {
            hex.neighborCoords.Add(neighbor);
        }

        worldMap[coord] = hex;
        return hex;
    }

    public List<HexData> GetChunkHexes(Vector2Int chunkCoord, int chunkSize)
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

    public bool TryGetHex(HexCoordinates coord, out HexData hex)
    {
        return worldMap.TryGetValue(coord, out hex);
    }

    public IEnumerable<HexData> GetAllHexes()
    {
        return worldMap.Values;
    }

    private TerrainType DetermineTerrainType(HexData hex)
    {
        float elevation = hex.elevation;

        if (elevation < 0.1f) return TerrainType.OceanDeep;
        if (elevation < 0.25f) return TerrainType.OceanShallow;

        // Cálculo de pendiente
        float slopeSum = 0f;
        int count = 0;

        foreach (var neighborCoord in hex.neighborCoords)
        {
            if (worldMap.TryGetValue(neighborCoord, out var neighbor))
            {
                slopeSum += Mathf.Abs(neighbor.elevation - elevation);
                count++;
            }
        }

        float avgSlope = (count > 0) ? slopeSum / count : 0f;

        if (avgSlope < 0.02f) return TerrainType.Plains;
        if (avgSlope < 0.06f) return TerrainType.Hills;
        if (elevation > 0.8f) return TerrainType.Mountains;
        if (avgSlope >= 0.06f && elevation < 0.5f) return TerrainType.Valley;

        return TerrainType.Plains; // Fallback
    }

    public static bool IsWater(TerrainType type)
    {
        return type == TerrainType.OceanDeep || type == TerrainType.OceanShallow;
    }



}```
