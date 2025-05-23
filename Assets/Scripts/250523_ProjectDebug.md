# HexMap – Código fuente consolidado

_Generado el Fri May 23 10:35:19 EST 2025_\n

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

## 📁 Configs/GameConfig.cs
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

## 📁 Core/CoroutineDispatcher.cs
```csharp
using System.Collections;
using UnityEngine;

public class CoroutineDispatcher : MonoBehaviour
{
    public static CoroutineDispatcher Instance;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void RunCoroutine(IEnumerator coroutine)
    {
        StartCoroutine(coroutine);
    }
    
}
```

---

## 📁 Core/GameEnums.cs
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

## 📁 Core/Singletons/CrystalSelectorUI.cs
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

## 📁 Core/TickManager.cs
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
          ///  Debug.Log($"Tick occurred at time {Time.time}, current interval: {tickInterval}");
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

       /// Debug.Log($"Game speed set to {speed} with interval {tickInterval}");
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

## 📁 Debug/ColliderDebugTool.cs
```csharp
// 📁 ColliderDebugTool.cs
using UnityEngine;

public class ColliderDebugTool : MonoBehaviour
{
    [SerializeField] private float sphereRadius = 0.5f;
    [SerializeField] private Color sphereColor = Color.red;
    [SerializeField] private float refreshRate = 0.5f;

    private float lastCheckTime;

    void OnDrawGizmos()
    {
        Gizmos.color = sphereColor;
        Gizmos.DrawWireSphere(transform.position, sphereRadius);
    }

    void OnDrawGizmosSelected()
{
    Gizmos.color = Color.red;
    Gizmos.DrawWireSphere(transform.position, 5.0f);
}

    void Update()
    {
        if (Time.time - lastCheckTime > refreshRate)
        {
            lastCheckTime = Time.time;
            CheckColliders();
        }
    }

    void CheckColliders()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, sphereRadius);
        Debug.Log($"Colliders overlapping {gameObject.name}: {colliders.Length}");

        // FIX: Replaced obsolete FindObjectsOfType with FindObjectsByType
        HexRenderer[] hexRenderers = Object.FindObjectsByType<HexRenderer>(FindObjectsSortMode.None);
        Debug.Log($"Total HexRenderers in scene: {hexRenderers.Length}");
    }
}```

---

## 📁 Debug/HexTileValidator.cs
```csharp
using UnityEngine;
using UnityEditor;

public class HexTileValidator : MonoBehaviour
{
    [MenuItem("Tools/Validate HexTiles In Scene")]
    public static void ValidateHexTiles()
    {
        var tiles = FindObjectsOfType<HexRenderer>();
        Debug.Log($"🔍 Validando {tiles.Length} HexTiles...");

        foreach (var tile in tiles)
        {
            var go = tile.gameObject;
            string name = go.name;

            if (go.layer != LayerMask.NameToLayer("Terrain"))
                Debug.LogWarning($"⚠️ {name} NO está en la capa Terrain (está en {LayerMask.LayerToName(go.layer)})");

            var mf = go.GetComponent<MeshFilter>();
            if (mf == null || mf.sharedMesh == null)
                Debug.LogWarning($"❌ {name} no tiene MeshFilter válido");

            var mc = go.GetComponent<MeshCollider>();
            if (mc == null || mc.sharedMesh == null)
                Debug.LogWarning($"❌ {name} no tiene MeshCollider válido");

            if (mf != null && mf.sharedMesh != null && mc != null && mc.sharedMesh != null)
                Debug.Log($"✅ {name} tiene collider y mesh correctamente configurados.");
        }
    }
}
```

---

## 📁 Debug/PlacementDebug.cs
```csharp
using UnityEngine;

public class PlacementDebug : MonoBehaviour
{
    void Start()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, 5.0f, LayerMask.GetMask("Terrain"));
        Debug.Log($"🧪 [{name}] ve {colliders.Length} colisionadores de terreno.");
        foreach (var col in colliders)
        {
            Debug.Log($" - Hex: {col.name}, MeshCollider: {col.GetComponent<MeshCollider>() != null}");
        }

        var rend = GetComponentInChildren<Renderer>();
        if (rend == null)
        {
            Debug.LogWarning($"❌ {name} no tiene Renderer hijo visible.");
        }
        else
        {
            Debug.Log($"✅ {name} tiene Renderer hijo activo: {rend.gameObject.name}");
        }
    }
}
```

---

## 📁 HexBehavior.cs
```csharp
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Collections;

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
        StartCoroutine(DelayedRegister());

    }

    private IEnumerator DelayedRegister()
    {
        yield return new WaitForSeconds(0.2f); // Asegura que todos los vecinos estén asignados
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

## 📁 Input/CameraController.cs
```csharp

/*
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float moveSpeed = 10f;
    public float zoomSpeed = 5f;
    public float minZoom = 3f;
    public float maxZoom = 15f;

    public float rotationSpeed = 50f; // velocidad de rotación en grados por segundo
    public Transform rotationPivot;   // el punto alrededor del cual girará la cámara (puede ser el jugador)


    private Camera cam;

    void Start()
    {
        cam = Camera.main;
    }

    void Update()
    {
        // Movimiento horizontal/vertical / flechas)
        float moveX = 0f;
        float moveZ = 0f;

        if (Input.GetKey(KeyCode.RightArrow)) moveX = 1f;
        if (Input.GetKey(KeyCode.LeftArrow)) moveX = -1f;
        if (Input.GetKey(KeyCode.UpArrow)) moveZ = 1f;
        if (Input.GetKey(KeyCode.DownArrow)) moveZ = -1f;

        // Movimiento vertical en Y (H/N)
        float moveY = 0f;
        if (Input.GetKey(KeyCode.H)) moveY = 1f;
        if (Input.GetKey(KeyCode.N)) moveY = -1f;



        Vector3 move = new Vector3(moveX, moveY, moveZ) * moveSpeed * Time.deltaTime;
        transform.position += move;

        // Zoom with scroll wheel
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0f)
        {
            cam.orthographicSize -= scroll * zoomSpeed;
            cam.orthographicSize = Mathf.Clamp(cam.orthographicSize, minZoom, maxZoom);
        }
        
        // Rotación horizontal con teclas J y K
        float rotationInput = 0f;
        if (Input.GetKey(KeyCode.K)) rotationInput = 1f;
        if (Input.GetKey(KeyCode.J)) rotationInput = -1f;

        if (Mathf.Abs(rotationInput) > 0f)
        {
            Vector3 pivot = rotationPivot != null ? rotationPivot.position : transform.position;
            // Rotar alrededor del pivot sin cambiar tilt
            transform.RotateAround(pivot, Vector3.up, rotationInput * rotationSpeed * Time.deltaTime);

            // Corrige la inclinación (mantener vista en horizontal)
            Vector3 angles = transform.eulerAngles;
            angles.x = 45f; // O el ángulo que desees mantener
            angles.z = 0f;
            transform.eulerAngles = angles;

        }

    }
}
*/```

---

## 📁 Input/HexMapCamera.cs
```csharp
﻿using UnityEngine;

/// <summary>
/// Component that controls the singleton camera that navigates the hex map.
/// </summary>
public class HexMapCamera : MonoBehaviour
{
    // Existing fields for runtime control (These will now operate relative to the new initial setup)
    [SerializeField]
    float stickMinZoom, stickMaxZoom;

    [SerializeField]
    float swivelMinZoom, swivelMaxZoom;

    [SerializeField]
    float moveSpeedMinZoom, moveSpeedMaxZoom;

    [SerializeField]
    float rotationSpeed;

    [Header("Vertical Zoom Settings")]
    [SerializeField]
    float yZoomSpeed = 10f;
    [SerializeField]
    float yMinHeight = 5f;
    [SerializeField]
    float yMaxHeight = 50f;

    [Header("Initial Camera Placement (Relative to Player)")]
    [Tooltip("Offset from player's position for camera's starting X, Y, Z. (0.81, 5, -21.57 for your request)")]
    [SerializeField]
    private Vector3 initialOffsetFromPlayer = new Vector3(0.81f, 5f, -21.57f); // Custom offset for 3rd person view
    [Tooltip("Height above player to look at. Adjust for a better view target (e.g., player's head).")]
    [SerializeField]
    private float lookAtHeightOffset = 1.5f; // Adjust this to look slightly above the player's base

    // References to child transforms for camera mechanics
    Transform swivel, stick;

    // Internal state variables
    float zoom = 1f; // Will be set to a default that works with the new perspective
    float rotationAngle; // Tracks the camera's Y-rotation for manual adjustments
    static HexMapCamera instance; // Singleton instance

    public static bool Locked
    {
        set => instance.enabled = !value;
    }

    // ValidatePosition is still available if you need to call it from other scripts,
    // but its internal implementation might need adjustment depending on how it's used.
    public static void ValidatePosition()
    {
        // This method's behavior might need to be adjusted if it was used to reposition
        // the camera based on an old system. For now, it remains a placeholder.
    }

    void Awake()
    {
        // Get references to child transforms (swivel and stick)
        swivel = transform.GetChild(0);
        stick = swivel.GetChild(0);

        // Reset swivel and stick to neutral local positions and rotations.
        // Their main purpose will now be for runtime adjustments (like mouse scroll zoom affecting distance/pitch)
        // relative to the new initial camera world position and rotation set in Start().
        swivel.localPosition = Vector3.zero;
        swivel.localRotation = Quaternion.identity;
        stick.localPosition = Vector3.zero;

        // Set 'zoom' to a default value that works well with the new 3rd person perspective.
        // This might require experimentation in the editor. 0.5f is a common midpoint.
        zoom = 0.5f;
        // Apply the initial zoom state to stick/swivel immediately in Awake.
        // This ensures the stick and swivel are at a consistent state when the game starts.
        AdjustZoom(0); // Calling with 0 delta just applies the current 'zoom' value.
    }

    void OnEnable()
    {
        // Set the singleton instance when the component is enabled
        instance = this;
    }

    void Start()
    {
        // Position and orient the camera based on the player's position and desired offset.
        // This is called in Start() to ensure the player object is fully initialized.
        SetInitialCameraPositionAndLook();
    }

    void Update()
    {
        // --- ZOOM (Mouse Scroll Wheel) ---
        float zoomDelta = Input.GetAxis("Mouse ScrollWheel");
        if (zoomDelta != 0f)
        {
            AdjustZoom(zoomDelta);
        }

        // --- ROTATION (Q Key for Left Rotation) ---
        // 'E' is now dedicated to vertical zoom.
        float rotationDelta = 0f;
        if (Input.GetKey(KeyCode.Q)) rotationDelta = -1f; // Q rotates left

        if (rotationDelta != 0f)
        {
            AdjustRotation(rotationDelta);
        }

        // --- VERTICAL ZOOM (Y-level) (E/R Keys) ---
        float yMoveDelta = 0f;
        if (Input.GetKey(KeyCode.E)) yMoveDelta = -1f; // E to zoom in (move camera down)
        if (Input.GetKey(KeyCode.R)) yMoveDelta = 1f;  // R to zoom out (move camera up)

        if (yMoveDelta != 0f)
        {
            AdjustYHeight(yMoveDelta);
        }

        // --- CAMERA MOVEMENT (Arrow Keys for Horizontal Panning) ---
        float xDelta = 0f;
        if (Input.GetKey(KeyCode.RightArrow)) xDelta = 1f;
        if (Input.GetKey(KeyCode.LeftArrow)) xDelta = -1f;

        float zDelta = 0f;
        if (Input.GetKey(KeyCode.UpArrow)) zDelta = 1f;
        if (Input.GetKey(KeyCode.DownArrow)) zDelta = -1f;

        if (xDelta != 0f || zDelta != 0f)
        {
            AdjustPosition(xDelta, zDelta);
        }

        // --- Manual Focus On Player (C Key) ---
        // When 'C' is pressed, re-apply the initial camera position and look-at.
        // This effectively snaps the camera back to the defined 3rd person view.
        if (Input.GetKeyDown(KeyCode.C))
        {
            SetInitialCameraPositionAndLook();
        }
    }

    /// <summary>
    /// Adjusts the camera's distance and pitch (angle of view) based on the 'zoom' value.
    /// This uses the swivel and stick child transforms.
    /// </summary>
    /// <param name="delta">Amount to change the zoom by. Set to 0 to just apply current 'zoom' value.</param>
    void AdjustZoom(float delta)
    {
        zoom = Mathf.Clamp01(zoom + delta);

        // Interpolate distance for the camera's stick (zoom in/out distance)
        float distance = Mathf.Lerp(stickMinZoom, stickMaxZoom, zoom);
        stick.localPosition = new Vector3(0f, 0f, distance);

        // Interpolate angle for the camera's swivel (pitch/tilt)
        float angle = Mathf.Lerp(swivelMinZoom, swivelMaxZoom, zoom);
        swivel.localRotation = Quaternion.Euler(angle, 0f, 0f);
    }

    /// <summary>
    /// Adjusts the camera's Y-axis rotation (yaw).
    /// </summary>
    /// <param name="delta">Positive for clockwise, negative for counter-clockwise.</param>
    void AdjustRotation(float delta)
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

    /// <summary>
    /// Moves the camera horizontally in world space.
    /// </summary>
    void AdjustPosition(float xDelta, float zDelta)
    {
        Vector3 direction = transform.localRotation * new Vector3(xDelta, 0f, zDelta).normalized;
        float damping = Mathf.Max(Mathf.Abs(xDelta), Mathf.Abs(zDelta));
        float distance = Mathf.Lerp(moveSpeedMinZoom, moveSpeedMaxZoom, zoom) * damping * Time.deltaTime;

        Vector3 position = transform.localPosition;
        position += direction * distance;
        transform.localPosition = position;
    }

    /// <summary>
    /// Adjusts the camera's Y height (vertical position) within defined limits.
    /// </summary>
    /// <param name="delta">Positive to move up, negative to move down.</param>
    private void AdjustYHeight(float delta)
    {
        Vector3 position = transform.position;
        position.y = Mathf.Clamp(position.y + delta * yZoomSpeed * Time.deltaTime, yMinHeight, yMaxHeight);
        transform.position = position;
    }

    /// <summary>
    /// Sets the camera's initial world position relative to the player and makes it look at the player.
    /// This method is called once in Start() and can be triggered manually via the 'C' key.
    /// </summary>
    private void SetInitialCameraPositionAndLook()
    {
        GameObject playerObject = GameObject.FindWithTag("Player");
        if (playerObject != null)
        {
            // Set camera's world position based on player's position and the specified offset
            transform.position = playerObject.transform.position + initialOffsetFromPlayer;

            // Make the camera look at the player's position, with an optional height offset
            // (e.g., to aim at the player's head instead of their feet).
            transform.LookAt(playerObject.transform.position + Vector3.up * lookAtHeightOffset);

            // After directly setting the camera's rotation using LookAt,
            // update the 'rotationAngle' variable to match the camera's current Y-rotation.
            // This ensures that subsequent manual rotations (using 'Q' key) continue correctly
            // from the new orientation.
            rotationAngle = transform.localRotation.eulerAngles.y;
        }
        else
        {
            Debug.LogWarning("HexMapCamera: Player GameObject not found! Make sure your player has the tag 'Player' and is active in the scene.");
        }
    }
}```

---

## 📁 Map/ChunkGenerator.cs
```csharp
using UnityEngine;
using System.Collections;

public class ChunkGenerator
{


    public static GameObject GenerateChunk(Vector2Int chunkCoord, int chunkSize, GameObject hexPrefab)
    {
        GameObject parent = new GameObject($"Chunk_{chunkCoord.x}_{chunkCoord.y}");
        parent.layer = LayerMask.NameToLayer("Terrain");
        parent.tag = "Chunk";
        Debug.Log($"✅ Chunk generado: {parent.name} | Posición: {parent.transform.position}");

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

                hex.layer = LayerMask.NameToLayer("Terrain");

                SetLayerRecursively(hex, LayerMask.NameToLayer("Terrain"));


                // Diagnóstico detallado
                Debug.Log($"🧪 Instanciado {hex.name} con componentes:");
                Debug.Log($"↳ HexBehavior: {hex.GetComponent<HexBehavior>() != null}");
                Debug.Log($"↳ HexRenderer: {hex.GetComponent<HexRenderer>() != null}");

                HexBehavior behavior = hex.GetComponent<HexBehavior>();
                if (behavior != null)
                {
                    try
                    {
                        behavior.coordinates = hexCoord;

                        var hexData = WorldMapManager.Instance.GetOrGenerateHex(hexCoord);

                        var renderer = hex.GetComponent<HexRenderer>();
                        if (renderer != null)
                        {
                            var config = Resources.Load<ChunkMapGameConfig>("ChunkMapGameConfig");
                            if (config != null)
                            {
                                float elevationHeight = hexData.elevation * config.elevationScale;
                                renderer.SetHeight(elevationHeight);

                                Material mat = config.GetMaterialFor(hexData.terrainType);
                                if (mat != null)
                                    renderer.GetComponent<MeshRenderer>().material = mat;
                            }
                            else
                            {
                                Debug.LogWarning("⚠️ ChunkMapGameConfig not found in Resources.");
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

                        // 🧹 Elimina cualquier collider sobrante en el chunk
                        var existingCollider = parent.GetComponent<MeshCollider>();
                        if (existingCollider != null)
                        {
                            Object.Destroy(existingCollider);
                            Debug.Log($"🧹 Eliminado MeshCollider sobrante de {parent.name}");
                        }


                         // ✅ Colocar árbol solo en algunos hexágonos para evitar saturación
                        GameObject testPrefab = Resources.Load<GameObject>("TerrainObjects/Leaf_Oak");

                    if (testPrefab == null)
                    {
                        Debug.LogWarning("⚠️ No se encontró el prefab en Resources/TerrainObjects/Leaf_Oak");
                    }
                    else if ((globalQ + globalR) % 5 == 0)
                    {
                        CoroutineDispatcher.Instance?.RunCoroutine(DelayedPlaceFeature(behavior, testPrefab));
                    }

                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogError($"❌ Error en procesamiento de {hex.name}:\n{ex}");
                    }
                }
                else
                {
                    Debug.LogError($"❌ Hex instanciado sin HexBehavior: {hex.name}");
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

    private static IEnumerator DelayedPlaceFeature(HexBehavior hex, GameObject prefab)
    {
        yield return new WaitForSeconds(0.1f); // Puedes ajustar el tiempo si sigue fallando

        if (hex != null && prefab != null)
        {
            HexObjectPlacer.PlaceOnHex(hex, prefab);
        }
        
    
}
    private static void SetLayerRecursively(GameObject obj, int layer)
{
    obj.layer = layer;
    foreach (Transform child in obj.transform)
    {
        SetLayerRecursively(child.gameObject, layer);
    }
}

public static GameObject PlaceOnHex(HexBehavior hex, GameObject prefab)
{
    GameObject obj = Object.Instantiate(prefab);
    obj.name = $"Feature_{hex.coordinates.Q}_{hex.coordinates.R}";

    return obj;
}


}

```

---

## 📁 Map/ChunkGizmoDrawer.cs
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

## 📁 Map/ChunkManager.cs
```csharp
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChunkManager : MonoBehaviour
{
    public static ChunkManager Instance;

    public GameObject hexPrefab;
    public int chunkSize = 10;
    public int loadRadius = 1;

    [Range(0, 10)]
    public int unloadRadius = 2;

    public Dictionary<Vector2Int, GameObject> loadedChunks = new();

    private void Awake()
    {
        Instance = this;
        StartCoroutine(DelayedInit());
    }

    private IEnumerator DelayedInit()
    {
        yield return new WaitUntil(() => WorldMapManager.Instance != null);

        Vector2Int initialCoord = new Vector2Int(0, 0);
        if (!loadedChunks.ContainsKey(initialCoord))
        {
            GameObject chunk = ChunkGenerator.GenerateChunk(initialCoord, chunkSize, hexPrefab);
            loadedChunks.Add(initialCoord, chunk);
            Debug.Log("🌱 Chunk inicial generado en (0,0)");
        }
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

## 📁 Map/ChunkMapConfigController.cs
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

## 📁 Map/CrystalGenerator.cs
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

## 📁 Map/CrystalMesh.cs
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

## 📁 Map/HexCoordinates.cs
```csharp
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]

public enum HexDirection
{
    NE, E, SE, SW, W, NW
}


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

    public HexCoordinates GetNeighbor(HexDirection direction)
{
    switch (direction)
    {
        case HexDirection.NE: return new HexCoordinates(Q + 1, R);
        case HexDirection.E:  return new HexCoordinates(Q + 1, R - 1);
        case HexDirection.SE: return new HexCoordinates(Q,     R - 1);
        case HexDirection.SW: return new HexCoordinates(Q - 1, R);
        case HexDirection.W:  return new HexCoordinates(Q - 1, R + 1);
        case HexDirection.NW: return new HexCoordinates(Q,     R + 1);
        default: return this;
    }
}

}```

---

## 📁 Map/HexData.cs
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

## 📁 Map/HexRenderer.cs
```csharp
// 📁 HexRenderer.cs (Updated)
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
public class HexRenderer : MonoBehaviour
{
    public static float SharedOuterRadius = 1f;

    [Header("Hex Settings")]
    public float innerRadius = 0.5f;
    public float columnHeight = 0f;

    [Header("Visual")]
    public Material material;
    public Color topColor = Color.magenta;
    public Color sideColor = Color.black;

    [Header("Scale Settings")]
    [Range(0.01f, 1f)] public float heightScale = 0.25f;

    Mesh _mesh;
    MeshFilter _mf;
    MeshCollider _mc;
    MeshRenderer _mr;

    void Start()
    {
        if (Application.isPlaying)
        {
            BuildMesh();
        }
    }

    void Awake()
    {
        InitializeComponents();
        // Always build mesh in Play Mode
        if (Application.isPlaying)
        {
            BuildMesh();
        }
    }

    void OnEnable()
    {
        InitializeComponents();
        // Always build mesh in Play Mode
        if (Application.isPlaying)
        {
            BuildMesh();
        }
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        // Only execute when the game is not playing AND in a valid scene state
        if (!Application.isPlaying && _mf != null && _mc != null && _mr != null)
        {
            UnityEditor.EditorApplication.delayCall += () =>
            {
                if (this != null) BuildMesh();
            };
        }
    }
#endif

    private void InitializeComponents()
    {
        if (_mf == null) _mf = GetComponent<MeshFilter>();
        if (_mc == null) _mc = GetComponent<MeshCollider>();
        if (_mr == null) _mr = GetComponent<MeshRenderer>();
        // Ensure mesh is initialized. If it was cleared or destroyed in editor, create a new one.
        if (_mesh == null) _mesh = new Mesh { name = "HexSimple" };
    }

    public void SetHeight(float elevationHeight)
    {
        columnHeight = elevationHeight;
        BuildMesh();
    }

    public void SetColor(Color color)
    {
        topColor = color;
        BuildMesh();
    }

    void BuildMesh()
    {
        if (_mf != null) _mf.sharedMesh = null;
        if (_mc != null) _mc.sharedMesh = null;
        InitializeComponents();

        if (_mesh != null) _mesh.Clear();
        else _mesh = new Mesh { name = "HexSimple" };

        List<Vector3> v = new();
        List<int> t = new();
        List<Color> c = new();

        // TOP
        float yTop = columnHeight * heightScale;
        float yBottom = 0f; // For the base of the column

        // Center vertex
        v.Add(new Vector3(0, yTop, 0)); c.Add(topColor);
        // Vertices for the top surface (outer ring)
        for (int i = 0; i < 6; i++)
        {
            v.Add(GetFlatPoint(i, yTop)); c.Add(topColor);
        }
        // Triangles for the top surface
        for (int i = 0; i < 6; i++)
        {
            t.AddRange(new[] { 0, i + 1, (i + 1) % 6 + 1 });
        }

        // SIDES
        for (int i = 0; i < 6; i++)
        {
            Vector3 topA = GetFlatPoint(i, yTop);
            Vector3 topB = GetFlatPoint((i + 1) % 6, yTop);
            Vector3 bottomA = GetFlatPoint(i, yBottom);
            Vector3 bottomB = GetFlatPoint((i + 1) % 6, yBottom);

            int baseIndex = v.Count;

            v.Add(bottomA); c.Add(sideColor);
            v.Add(topA); c.Add(topColor);
            v.Add(bottomB); c.Add(sideColor);
            v.Add(topB); c.Add(topColor);

            t.AddRange(new[] { baseIndex, baseIndex + 1, baseIndex + 2 });
            t.AddRange(new[] { baseIndex + 2, baseIndex + 1, baseIndex + 3 });
        }

        _mesh.SetVertices(v);
        _mesh.SetColors(c);
        _mesh.SetTriangles(t, 0);
        _mesh.RecalculateNormals();

        if (_mf != null && Application.isPlaying)
        {
            _mf.sharedMesh = _mesh;
        }

        if (_mc != null && Application.isPlaying)
        {
            _mc.sharedMesh = _mesh;
            _mc.convex = false;
        }

        if (_mr != null && _mr.sharedMaterial == null && material != null)
        {
            _mr.sharedMaterial = material;
        }
        Debug.Log($"{name} – Mesh vertices: {_mesh.vertexCount}, assigned to MeshCollider: {_mc.sharedMesh != null}");

    }

    Vector3 GetFlatPoint(int index, float y)
    {
        float angle = 60f * index * Mathf.Deg2Rad;
        return new Vector3(SharedOuterRadius * Mathf.Cos(angle), y, SharedOuterRadius * Mathf.Sin(angle));
    }

    public float VisualTopY
    {
        get { return transform.position.y + columnHeight * heightScale; }


    }
    
}```

---

## 📁 Map/PerlinSettings.cs
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

## 📁 Map/PerlinSettingsController.cs
```csharp
using UnityEngine;

public class PerlinSettingsController : MonoBehaviour
{
    public PerlinSettings perlinSettings;

    [Range(0.001f, 15f)] public float elevationFreq = 0.02f;
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

## 📁 Map/PerlinUtility.cs
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

## 📁 Map/TerrainMeshCollider.cs
```csharp
/*using UnityEngine;
using System.Collections;
using System.Collections.Generic;


[RequireComponent(typeof(MeshFilter), typeof(MeshCollider))]
public class TerrainMeshCollider : MonoBehaviour
{
    public void ApplyCollider()
    {
        MeshCollider mc = GetComponent<MeshCollider>();
        if (mc == null)
        {
            mc = gameObject.AddComponent<MeshCollider>();
        }

        Mesh combinedMesh = GetComponent<MeshFilter>().sharedMesh;
        mc.sharedMesh = combinedMesh;

        if (combinedMesh != null)
        {
            Debug.Log($"✅ MeshCollider asignado con mesh de {combinedMesh.vertexCount} vértices en {name}");
        }
        else
        {
            Debug.LogWarning($"⚠️ No se pudo asignar collider porque el mesh combinado es null en {name}");
        }
    }



    public void CombineHexMeshes()
    {
        MeshFilter[] meshFilters = GetComponentsInChildren<MeshFilter>();
        Debug.Log($"🔍 Encontrados {meshFilters.Length} MeshFilters en hijos del chunk {name}");

        List<CombineInstance> combineList = new List<CombineInstance>();

        foreach (var mf in meshFilters)
        {
            if (mf == GetComponent<MeshFilter>())
            {
                Debug.Log($"🟡 Saltando MeshFilter del chunk root: {mf.name}");
                continue;
            }

            if (mf.sharedMesh == null)
            {
                Debug.LogWarning($"❌ MeshFilter sin mesh asignado: {mf.name}");
                continue;
            }

            CombineInstance ci = new CombineInstance
            {
                mesh = mf.sharedMesh,
                transform = mf.transform.localToWorldMatrix
            };
            combineList.Add(ci);
        }

        if (combineList.Count == 0)
        {
            Debug.LogWarning($"❌ No hay meshes válidos para combinar en los hijos de {name}.");
            return;
        }

        Mesh combinedMesh = new Mesh();
        combinedMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        combinedMesh.CombineMeshes(combineList.ToArray(), true, true);

        GetComponent<MeshFilter>().sharedMesh = combinedMesh;
        Debug.Log($"✅ Mesh combinado con {combineList.Count} sub-meshes en {name}.");
    }



}
*/```

---

## 📁 Map/TerrainPlacementHelper.cs
```csharp
using UnityEngine;

public class TerrainPlacementHelper : MonoBehaviour
{
    [Header("Configuración")]
    public LayerMask terrainLayer; // Asegúrate que el terreno esté en este layer
    public float placementHeightOffset = 0.5f;

    public bool PlacePrefabOnTerrain(GameObject prefab, Vector3 targetXZPosition)
    {
        // Dispara un rayo desde arriba para encontrar el punto sobre el terreno
        Ray ray = new Ray(new Vector3(targetXZPosition.x, 100f, targetXZPosition.z), Vector3.down);
        if (Physics.Raycast(ray, out RaycastHit hit, 200f, terrainLayer))
        {
            Vector3 placePos = hit.point + Vector3.up * placementHeightOffset;
            Instantiate(prefab, placePos, Quaternion.identity);
            return true;
        }

        Debug.LogWarning("No se encontró terreno en esa posición.");
        return false;
    }
}
```

---

## 📁 Map/WorldMapManager.cs
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

---

## 📁 ObjectPlacement/AutoPlaceOnTerrain.cs
```csharp
using UnityEngine;

[DisallowMultipleComponent]
public class AutoPlaceOnTerrain : MonoBehaviour
{
    [SerializeField] private string terrainLayerName = "Terrain";
    [SerializeField] private float heightOffset = 0.25f;
    [SerializeField] private float placementDetectionRadius = 5.0f;
    [SerializeField] private bool debug = false;

    public bool TryPlace()
    {
        Debug.Log($"📌 {name} está intentando colocarse desde posición: {transform.position}");

        Collider[] colliders = Physics.OverlapSphere(transform.position, placementDetectionRadius, LayerMask.GetMask(terrainLayerName));
        Debug.Log($"🔎 {name}: {colliders.Length} colisionadores detectados en layer {terrainLayerName}");

        foreach (var col in colliders)
        {
            Debug.Log($" - 🎯 Collider: {col.name}");

            HexRenderer hex = col.GetComponentInParent<HexRenderer>();
            if (hex == null)
            {
                Debug.Log($" - ⛔ No es HexRenderer");
                continue;
            }

            Debug.Log($" - ✅ HexRenderer válido: {hex.name}");

            TerrainUtils.SnapToHexCenterXYZ(transform, hex, heightOffset);
            return true;
        }

        Debug.LogWarning($"⚠️ {name} no pudo colocarse sobre ningún Hex válido.");
        return false;
    }
}
```

---

## 📁 ObjectPlacement/HexObjectPlacer.cs
```csharp
// 📁 HexObjectPlacer.cs
using UnityEngine;

public static class HexObjectPlacer
{
    public static float PlacementOffset = 0.01f;

    public static void PlaceOnHex(HexBehavior hex, GameObject prefab)
    {
        if (hex == null || prefab == null)
        {
            Debug.LogWarning("❌ HexBehavior o prefab nulo al intentar colocar objeto.");
            return;
        }

        GameObject instance = Object.Instantiate(prefab, Vector3.zero, Quaternion.identity);
        instance.name = $"Feature_{hex.coordinates.Q}_{hex.coordinates.R}";
        instance.transform.SetParent(hex.transform);
        instance.transform.rotation = Quaternion.identity;

        // FIX: Changed to SnapToHexTopFlat
        TerrainUtils.SnapToHexTopFlat(instance.transform, hex.GetComponent<HexRenderer>(), PlacementOffset);

        Debug.Log($"🌳 Objeto instanciado sobre {hex.name} en {instance.transform.position}");
    }
}```

---

## 📁 ObjectPlacement/PlayerPlacementHelper.cs
```csharp
using UnityEngine;
using System.Collections;

public class PlayerPlacementHelper : MonoBehaviour
{
    [SerializeField] private string terrainLayerName = "Terrain";
    [SerializeField] private float heightOffset = 0.3f;
    [SerializeField] private int maxAttempts = 30;
    [SerializeField] private float retryDelay = 0.1f;
    [SerializeField] private float placementDetectionRadius = 5.0f;

    private IEnumerator Start()
    {
        yield return new WaitUntil(() => WorldMapManager.Instance != null);
        yield return new WaitUntil(() => ChunkManager.Instance != null);
        yield return new WaitUntil(() => ChunkManager.Instance.loadedChunks.Count > 0);
        yield return new WaitForSeconds(3f); // Let hexes initialize

        int attempts = 0;
        while (attempts < maxAttempts)
        {
            if (TryPlace())
            {
                Debug.Log($"✅ {gameObject.name} colocado sobre el terreno.");
                yield break;
            }

            attempts++;
            yield return new WaitForSeconds(retryDelay);
        }

        Debug.LogWarning($"⚠️ {gameObject.name} no pudo colocarse sobre ningún Hex válido tras {maxAttempts} intentos.");
    }

    public bool TryPlace()
    {
        Debug.Log($"📌 {name} está intentando colocarse desde posición: {transform.position}");

        Collider[] colliders = Physics.OverlapSphere(transform.position, placementDetectionRadius, LayerMask.GetMask(terrainLayerName));
        Debug.Log($"🔎 {name}: {colliders.Length} colisionadores detectados en layer {terrainLayerName}");

        foreach (var col in colliders)
        {
            Debug.Log($" - 🎯 Collider: {col.name}");

            var hex = col.GetComponentInParent<HexRenderer>();
            if (hex == null)
            {
                Debug.Log($" - ⛔ No es HexRenderer");
                continue;
            }

            Debug.Log($" - ✅ HexRenderer válido: {hex.name}");

            TerrainUtils.SnapToHexCenterXYZ(transform, hex, heightOffset);
            return true;
        }

        return false;
    }
}
```

---

## 📁 ObjectPlacement/TerrainUtils.cs
```csharp
// 📁 TerrainUtils.cs
using UnityEngine;

public static class TerrainUtils
{
    /// <summary>
    /// Snaps a transform to the visual top of a hexagonal tile.
    /// Adjusts the Y-position to be on top of the hex, plus an optional vertical offset.
    /// This method calculates the offset from the object's pivot to its visual bottom.
    /// </summary>
    /// <param name="objectTransform">The transform of the object to snap.</param>
    /// <param name="hexRenderer">The HexRenderer of the target hex.</param>
    /// <param name="verticalOffset">Additional offset above the hex's visual top.</param>
    public static void SnapToHexTopFlat(Transform objectTransform, HexRenderer hexRenderer, float verticalOffset)
    {
        if (objectTransform == null)
        {
            Debug.LogWarning("⚠️ SnapToHexTopFlat falló: objectTransform es null.");
            return;
        }

        if (hexRenderer == null)
        {
            Debug.LogWarning($"⚠️ SnapToHexTopFlat falló: hexRenderer es null para {objectTransform.name}.");
            return;
        }

        Renderer objectRenderer = objectTransform.GetComponentInChildren<Renderer>();
        if (objectRenderer == null)
        {
            Debug.LogWarning($"❌ {objectTransform.name} no tiene Renderer hijo visible. No se puede alinear.");
            return;
        }

        float objectBottomOffset = objectRenderer.bounds.center.y - objectRenderer.bounds.extents.y - objectTransform.position.y;
        float targetY = hexRenderer.VisualTopY - objectBottomOffset + verticalOffset;

        Vector3 newPos = objectTransform.position;
        newPos.y = targetY;
        objectTransform.position = newPos;

        Debug.Log($"📍 {objectTransform.name} alineado a Y={targetY:F3} sobre {hexRenderer.name} (VisualTopY={hexRenderer.VisualTopY:F3}, offset={verticalOffset:F3})");
    }


    /// <summary>
    /// Calculates the world position of a hex's center at a given Y-height.
    /// </summary>
    /// <param name="coordinates">The HexCoordinates of the hex.</param>
    /// <param name="yHeight">The desired Y-coordinate (e.g., hex.VisualTopY).</param>
    /// <param name="outerRadius">The outer radius of the hex, typically HexRenderer.SharedOuterRadius.</param>
    /// <returns>The Vector3 world position.</returns>
    public static Vector3 GetHexWorldPosition(HexCoordinates coordinates, float yHeight, float outerRadius)
    {
        Vector3 worldPos = HexCoordinates.ToWorldPosition(coordinates, outerRadius);
        worldPos.y = yHeight;
        return worldPos;
    }

    public static void SnapToHexCenterY(Transform objectTransform, HexRenderer hexRenderer, float verticalOffset)
    {
        if (objectTransform == null || hexRenderer == null)
        {
            Debug.LogWarning("⚠️ SnapToHexCenterY falló: Transform o HexRenderer es null.");
            return;
        }

        Vector3 pos = objectTransform.position;
        pos.y = hexRenderer.transform.position.y + verticalOffset;
        objectTransform.position = pos;

        Debug.Log($"📍 {objectTransform.name} colocado en Y={pos.y:F2} sobre {hexRenderer.name}.");
    }

public static void SnapToHexCenterXYZ(Transform objectTransform, HexRenderer hexRenderer, float verticalOffset)
{
    if (objectTransform == null || hexRenderer == null)
    {
        Debug.LogWarning("⚠️ SnapToHexCenterXYZ falló: Transform o HexRenderer es null.");
        return;
    }

    Vector3 hexPos = hexRenderer.transform.position;
    Vector3 newPos = new Vector3(hexPos.x, hexPos.y + verticalOffset, hexPos.z);
    objectTransform.position = newPos;

    Debug.Log($"📍 {objectTransform.name} colocado en ({newPos.x:F2}, {newPos.y:F2}, {newPos.z:F2}) sobre {hexRenderer.name}.");
}


}```

---

## 📁 PlayerController.cs
```csharp
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    public float moveCooldown = 0.2f;
    private float lastMoveTime;

    public HexCoordinates currentCoordinates;
    public float outerRadius = 1f;

    private Vector2Int currentChunkCoord;

    void Start()
    {
        currentCoordinates = HexCoordinates.FromWorldPosition(transform.position, outerRadius);
        transform.position = HexCoordinates.ToWorldPosition(currentCoordinates, outerRadius);
        UpdateChunkLoading(true);
    }

    void Update()
    {
        HandleKeyboardMovement();
        UpdateChunkLoading();
    }

    void HandleKeyboardMovement()
    {
        if (Time.time - lastMoveTime < moveCooldown)
            return;

        if (Input.GetKeyDown(KeyCode.W)) MoveTo(currentCoordinates.GetNeighbor(HexDirection.NE));
        if (Input.GetKeyDown(KeyCode.E)) MoveTo(currentCoordinates.GetNeighbor(HexDirection.E));
        if (Input.GetKeyDown(KeyCode.D)) MoveTo(currentCoordinates.GetNeighbor(HexDirection.SE));
        if (Input.GetKeyDown(KeyCode.S)) MoveTo(currentCoordinates.GetNeighbor(HexDirection.SW));
        if (Input.GetKeyDown(KeyCode.A)) MoveTo(currentCoordinates.GetNeighbor(HexDirection.W));
        if (Input.GetKeyDown(KeyCode.Q)) MoveTo(currentCoordinates.GetNeighbor(HexDirection.NW));
    }

    void MoveTo(HexCoordinates newCoord)
    {
        currentCoordinates = newCoord;
        transform.position = HexCoordinates.ToWorldPosition(newCoord, outerRadius);
        lastMoveTime = Time.time;
    }

    void UpdateChunkLoading(bool force = false)
    {
        Vector2Int chunkCoord = ChunkManager.WorldToChunkCoord(currentCoordinates);

        if (force || chunkCoord != currentChunkCoord)
        {
            currentChunkCoord = chunkCoord;
            ChunkManager.Instance.UpdateChunks(chunkCoord);
        }
    }
}

```

---

## 📁 UI/GameSpeedDropdownToggle.cs
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

## 📁 UI/GameSpeedUI.cs
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

## 📁 Units/HexPathfinder.cs
```csharp
using System.Collections.Generic;
using UnityEngine;

public class HexPathfinder : MonoBehaviour
{
    public static HexPathfinder Instance;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public List<HexBehavior> FindPath(HexBehavior start, HexBehavior goal)
    {
        var openSet = new PriorityQueue<HexBehavior>();
        var cameFrom = new Dictionary<HexBehavior, HexBehavior>();
        var gScore = new Dictionary<HexBehavior, float>();
        var fScore = new Dictionary<HexBehavior, float>();

        openSet.Enqueue(start, 0);
        gScore[start] = 0f;
        fScore[start] = Heuristic(start, goal);

        while (openSet.Count > 0)
        {
            HexBehavior current = openSet.Dequeue();

            if (current == goal)
                return ReconstructPath(cameFrom, current);

            foreach (HexBehavior neighbor in current.neighbors)
            {
                float tentativeG = gScore[current] + 1f; // cost between neighbors

                if (!gScore.ContainsKey(neighbor) || tentativeG < gScore[neighbor])
                {
                    cameFrom[neighbor] = current;
                    gScore[neighbor] = tentativeG;
                    fScore[neighbor] = tentativeG + Heuristic(neighbor, goal);

                    if (!openSet.Contains(neighbor))
                        openSet.Enqueue(neighbor, fScore[neighbor]);
                }
            }
        }

        Debug.LogWarning("⚠️ No se encontró un camino.");
        return null;
    }

    private float Heuristic(HexBehavior a, HexBehavior b)
    {
        return Vector3.Distance(a.transform.position, b.transform.position);
    }

    private List<HexBehavior> ReconstructPath(Dictionary<HexBehavior, HexBehavior> cameFrom, HexBehavior current)
    {
        List<HexBehavior> totalPath = new List<HexBehavior> { current };
        while (cameFrom.ContainsKey(current))
        {
            current = cameFrom[current];
            totalPath.Insert(0, current);
        }

        // Debug info
        Debug.Log("✅ Ruta encontrada con " + totalPath.Count + " pasos.");
        foreach (var step in totalPath)
            Debug.Log(" ➜ " + step.name);

        return totalPath;
    }

    // Opcional: visualización con Gizmos
    public List<HexBehavior> debugPath;
    private void OnDrawGizmos()
    {
        if (debugPath == null || debugPath.Count < 2) return;

        Gizmos.color = Color.green;
        for (int i = 0; i < debugPath.Count - 1; i++)
        {
            Gizmos.DrawLine(debugPath[i].transform.position + Vector3.up * 0.2f,
                            debugPath[i + 1].transform.position + Vector3.up * 0.2f);
        }
    }
}
```

---

## 📁 Units/PriorityQueue.cs
```csharp
using System;
using System.Collections.Generic;

public class PriorityQueue<T>
{
    private readonly List<(T item, float priority)> elements = new();

    public int Count => elements.Count;

    public void Enqueue(T item, float priority)
    {
        elements.Add((item, priority));
    }

    public T Dequeue()
    {
        int bestIndex = 0;

        for (int i = 1; i < elements.Count; i++)
        {
            if (elements[i].priority < elements[bestIndex].priority)
                bestIndex = i;
        }

        T bestItem = elements[bestIndex].item;
        elements.RemoveAt(bestIndex);
        return bestItem;
    }

    public bool Contains(T item)
    {
        return elements.Exists(e => EqualityComparer<T>.Default.Equals(e.item, item));
    }
}
```

---

## 📁 Units/UnitGrowndFollower.cs
```csharp
using UnityEngine;

[DisallowMultipleComponent]
public class UnitGroundFollower : MonoBehaviour
{
    // Changed from string to LayerMask, assign this in the Inspector!
    public LayerMask terrainLayer; 
    [SerializeField] private float minHeightThreshold = 0.01f; 


    private void LateUpdate()
    {
        HexRenderer hex = GetClosestHexBelow();
        if (hex == null) return;

        Renderer rend = GetComponentInChildren<Renderer>();
        float topY = hex.VisualTopY;

        float objectBottom = rend.bounds.center.y - rend.bounds.extents.y;
        float adjustment = topY - objectBottom;

        // Solo ajustar si hay diferencia real
        if (Mathf.Abs(adjustment) > minHeightThreshold)
        {
            transform.position += new Vector3(0f, adjustment, 0f);
            Debug.DrawLine(transform.position, transform.position + Vector3.down * 1f, Color.magenta, 2f);
        }
    }

    private HexRenderer GetClosestHexBelow()
    {
        // Increased radius for more reliable detection
        Collider[] hits = Physics.OverlapSphere(transform.position, 1.5f, terrainLayer); 
        foreach (var col in hits)
        {
            var hex = col.GetComponentInParent<HexRenderer>();
            if (hex != null) return hex;
        }

        return null;
    }
}```

---

## 📁 Units/UnitHighlightFollower.cs
```csharp
using UnityEngine;

public class UnitighlightFollower : MonoBehaviour
{
    [SerializeField] private Transform target; // Jugador u objeto a seguir
    // Changed from string to LayerMask, assign this in the Inspector!
    public LayerMask terrainLayer; 
    [SerializeField] private float detectionRadius = 1.5f; // Increased radius for more reliable detection
    [SerializeField] private float verticalOffset = 0.01f;

    private void LateUpdate()
    {
        if (target == null) return;

        HexRenderer hex = GetClosestHexBelow(target.position);
        if (hex != null)
        {
            TerrainUtils.SnapToHexTopFlat(transform, hex, verticalOffset);
            Debug.DrawLine(transform.position, transform.position + Vector3.up, Color.magenta, 0.2f);
        }
    }

    private HexRenderer GetClosestHexBelow(Vector3 position)
    {
        Collider[] hits = Physics.OverlapSphere(position, detectionRadius, terrainLayer);
        foreach (var col in hits)
        {
            HexRenderer hex = col.GetComponentInParent<HexRenderer>();
            if (hex != null) return hex;
        }
        return null;
    }
}```

---

## 📁 Units/UnitMover.cs
```csharp
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[DisallowMultipleComponent]
public class UnitMover : MonoBehaviour
{
    public float moveSpeed = 5f;
    // Changed from string to LayerMask, assign this in the Inspector!
    public LayerMask terrainLayer; 
    [SerializeField] private float minHeightThreshold = 0.01f;

    private bool isMoving = false;
    private Queue<HexBehavior> currentPath = new();
    public HexBehavior currentHex;

    void Start()
    {
        SnapToHexVisualTop();
        currentHex = GetClosestHexBelow();
    }

    void LateUpdate() // Using LateUpdate to ensure all movement for the frame is done first
    {
        AdjustToGround();
    }

    void Update()
    {
        if (!isMoving && currentPath.Count > 0)
            StartCoroutine(FollowPath(currentPath));
    }

    public void MoveTo(HexBehavior targetHex)
    {
        if (currentHex == null || targetHex == null) return;

        List<HexBehavior> path = HexPathfinder.Instance.FindPath(currentHex, targetHex);

        if (path == null || path.Count == 0)
        {
            Debug.LogWarning("⚠️ No se pudo calcular ruta.");
            return;
        }

        HexPathfinder.Instance.debugPath = path;
        currentPath = new Queue<HexBehavior>(path);
    }

    private IEnumerator FollowPath(Queue<HexBehavior> path)
    {
        isMoving = true;

        while (path.Count > 0)
        {
            HexBehavior step = path.Dequeue();
            HexRenderer hex = step.GetComponent<HexRenderer>();
            float topY = hex.VisualTopY;

            Renderer rend = GetComponentInChildren<Renderer>();
            float objectBottom = rend.bounds.center.y - rend.bounds.extents.y;
            float adjustment = topY + (rend.bounds.extents.y - (transform.position.y - objectBottom));

            Vector3 target = new Vector3(
                step.transform.position.x,
                topY + (rend.bounds.extents.y - (transform.position.y - objectBottom)),
                step.transform.position.z
            );

            while (Vector3.Distance(transform.position, target) > 0.05f)
            {
                transform.position = Vector3.MoveTowards(transform.position, target, moveSpeed * Time.deltaTime);
                yield return null;
            }

            currentHex = step;
            yield return null;
        }

        isMoving = false;
    }

    private HexBehavior GetClosestHexBelow()
    {
        // Increased radius for more reliable detection
        Collider[] colliders = Physics.OverlapSphere(transform.position, 1.5f, terrainLayer); 
        foreach (var col in colliders)
        {
            var hex = col.GetComponentInParent<HexRenderer>();
            if (hex != null)
            {
                return hex.GetComponentInParent<HexBehavior>();
            }
        }
        return null;
    }

    private HexRenderer GetClosestHexBelowRenderer()
    {
        // Increased radius for more reliable detection
        Collider[] hits = Physics.OverlapSphere(transform.position, 1.5f, terrainLayer); 
        foreach (var col in hits)
        {
            var hex = col.GetComponentInParent<HexRenderer>();
            if (hex != null) return hex;
        }
        return null;
    }

    private void SnapToHexVisualTop()
    {
        // Increased radius for more reliable detection
        Collider[] colliders = Physics.OverlapSphere(transform.position, 1.5f, terrainLayer); 
        foreach (var col in colliders)
        {
            var hex = col.GetComponentInParent<HexRenderer>();
            if (hex != null)
            {
                Renderer rend = GetComponentInChildren<Renderer>();
                if (rend == null) return;

                float topY = hex.VisualTopY;
                float objectBottom = rend.bounds.center.y - rend.bounds.extents.y;
                float adjustment = topY - objectBottom;

                if (Mathf.Abs(adjustment) > minHeightThreshold)
                {
                    transform.position += new Vector3(0f, adjustment, 0f);
                    Debug.DrawLine(transform.position, transform.position + Vector3.up * 1f, Color.cyan, 2f);
                }
                return;
            }
        }
    }

    private void AdjustToGround()
    {
        HexRenderer hex = GetClosestHexBelowRenderer();
        if (hex == null) return;

        Renderer rend = GetComponentInChildren<Renderer>();
        if (rend == null) return;

        float topY = hex.VisualTopY;
        float objectBottom = rend.bounds.center.y - rend.bounds.extents.y;
        float adjustment = topY - objectBottom;

        if (Mathf.Abs(adjustment) > minHeightThreshold)
        {
            transform.position += new Vector3(0f, adjustment, 0f);
            Debug.DrawLine(transform.position, transform.position + Vector3.down * 1f, Color.green, 0.1f);
        }
    }
}```

---

## 📁 Units/UnitSelector.cs
```csharp
// 📁 UnitSelector.cs
using UnityEngine;
using System;

public class UnitSelector : MonoBehaviour
{
    public LayerMask terrainLayer; // Layer for terrain detection (e.g., "Terrain")
    public LayerMask unitLayer;    // Layer for unit detection (e.g., "Player" or a specific "Unit" layer)
    [SerializeField] private float hoverRaycastDistance = 100f;

    private GameObject selectedUnit;
    private HexBehavior lastHoveredHex; // To track the currently hovered hex

    // Public static events that other scripts can subscribe to
    public static event Action<UnitMover, bool> OnUnitSelected; // unit and isSelected (true/false)
    public static event Action<HexBehavior> OnUnitHovered;       // hex that is currently hovered (can be null)

    void Update()
    {
        HandleSelectionInput();
        HandleHoverDetection(); // Continuously check for hovered hex
    }

    private void HandleSelectionInput()
{
    if (Input.GetMouseButtonDown(0))
    {
        Debug.Log("🖱️ Clic izquierdo detectado.");

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        Debug.DrawRay(ray.origin, ray.direction * hoverRaycastDistance, Color.yellow, 2f);

        if (Physics.Raycast(ray, out hit, hoverRaycastDistance, unitLayer))
        {
            Debug.Log($"🎯 Raycast impactó: {hit.collider.name} (Layer: {LayerMask.LayerToName(hit.collider.gameObject.layer)})");

            // Intenta encontrar UnitMover en varias partes de la jerarquía
            UnitMover clickedUnit = hit.collider.GetComponent<UnitMover>()
                                   ?? hit.collider.GetComponentInChildren<UnitMover>()
                                   ?? hit.collider.GetComponentInParent<UnitMover>();

            if (clickedUnit != null)
            {
                Debug.Log($"✅ Unidad con UnitMover encontrada: {clickedUnit.gameObject.name}");
                SelectUnit(clickedUnit);
            }
            else
            {
                Debug.LogWarning("⚠️ Raycast impactó algo en unitLayer, pero no encontró UnitMover en la jerarquía.");
            }
        }
        else
        {
            Debug.Log("👀 Raycast no impactó ningún objeto en la capa de unidades.");
            DeselectUnit();
        }
    }

    if (Input.GetMouseButtonDown(1) && selectedUnit != null)
    {
        Debug.Log("🖱️ Clic derecho detectado. Intentando mover unidad seleccionada...");

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, hoverRaycastDistance, terrainLayer))
        {
            HexBehavior hex = hit.collider.GetComponentInParent<HexBehavior>();
            if (hex != null)
            {
                UnitMover mover = selectedUnit.GetComponent<UnitMover>();
                if (mover != null)
                {
                    Debug.Log($"🏃 Moviendo unidad a Hex ({hex.coordinates.Q}, {hex.coordinates.R})");
                    mover.MoveTo(hex);
                }
                else
                {
                    Debug.LogWarning("⚠️ Unidad seleccionada no tiene UnitMover.");
                }
            }
            else
            {
                Debug.LogWarning("⚠️ El objeto clickeado no tiene HexBehavior en su jerarquía.");
            }
        }
        else
        {
            Debug.Log("❌ Clic derecho no impactó el terreno.");
        }
    }
}


    /// <summary>
    /// Detects which hex the mouse is currently hovering over and invokes the OnUnitHovered event.
    /// </summary>
    private void HandleHoverDetection()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        HexBehavior currentHovered = null;

        if (Camera.main != null && Physics.Raycast(ray, out hit, hoverRaycastDistance, terrainLayer))
        {
            currentHovered = hit.collider.GetComponentInParent<HexBehavior>();
        }

        // Only invoke the event if the hovered hex has changed
        if (currentHovered != lastHoveredHex)
        {
            lastHoveredHex = currentHovered;
            OnUnitHovered?.Invoke(lastHoveredHex); // Invoke the event, passing null if no hex is hovered
        }
    }

    private void SelectUnit(UnitMover unit)
    {
        if (selectedUnit != null && selectedUnit != unit.gameObject)
        {
            // Deselect previous unit if a different one is selected
            OnUnitSelected?.Invoke(selectedUnit.GetComponent<UnitMover>(), false);
        }

        selectedUnit = unit.gameObject;
        OnUnitSelected?.Invoke(unit, true); // Select new unit
        Debug.Log("✅ Unidad seleccionada: " + selectedUnit.name);
    }

    private void DeselectUnit()
    {
        if (selectedUnit != null)
        {
            OnUnitSelected?.Invoke(selectedUnit.GetComponent<UnitMover>(), false);
            selectedUnit = null;
            Debug.Log("❌ Unidad deseleccionada.");
        }
    }
}```
