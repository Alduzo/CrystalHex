# HexMap – Código fuente consolidado

_Generado el Sat May 31 23:58:20 EST 2025_\n

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
    public Color GetColorFor(TerrainType terrain)
{
    var material = GetMaterialFor(terrain);
    if (material != null)
    {
        return material.color;
    }
    else
    {
        Debug.LogWarning($"⚠️ Material no encontrado para {terrain}. Usando color por defecto.");
        switch (terrain)
        {
            case TerrainType.Ocean:
                return Color.blue;
            case TerrainType.CoastalWater:
                return Color.cyan;
            case TerrainType.SandyBeach:
                return Color.yellow;
            case TerrainType.RockyBeach:
                return Color.gray;
            case TerrainType.Plains:
                return Color.green;
            case TerrainType.Hills:
                return new Color(0.5f, 0.8f, 0.3f);
            case TerrainType.Plateau:
                return Color.Lerp(Color.green, Color.gray, 0.5f);
            case TerrainType.Mountain:
                return Color.gray;
            case TerrainType.Valley:
                return Color.Lerp(Color.green, Color.yellow, 0.5f);
            default:
                return Color.white;
        }
    }
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
    {
        Instance = this;
        DontDestroyOnLoad(gameObject); // ← ¡Aquí!
    }
    else
    {
        Destroy(gameObject);
    }
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

## 📁 Core/GameManager.cs
```csharp
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Cámaras")]
    public Camera mainCamera;          // Cámara cercana (LOD 0)
    public Camera minimapCamera;       // Cámara media (LOD 1)
    public Camera globeCamera;         // Cámara lejana (LOD 2)

    [Header("Zoom Thresholds")]
    public float closeThreshold = 10f;
    public float mediumThreshold = 30f;
    public float globeThreshold = 50f;

    [Header("Referencias")]
    public ChunkManager chunkManager;
    public WorldMapManager worldMapManager;

    [SerializeField] public RawImage minimapImage;

    private bool minimapGenerated = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }
    }

    private void Start()
    {
        mainCamera.gameObject.SetActive(true);  // Siempre activa
        minimapCamera.gameObject.SetActive(false);  // Inicialmente desactiva
    }

    private void Update()
    {
        float currentZoom = mainCamera.transform.position.y;

        if (currentZoom <= closeThreshold)
        {
            // Nivel cercano - solo mainCamera activa
            if (minimapCamera.enabled) minimapCamera.enabled = false;
            if (globeCamera != null && globeCamera.enabled) globeCamera.enabled = false;
            minimapGenerated = false;
        }
        else if (currentZoom > closeThreshold && currentZoom <= mediumThreshold)
        {
            if (!minimapCamera.enabled) minimapCamera.enabled = true;

            if (!minimapGenerated)
            {
                Debug.Log("🗺 Generando minimapa procedural...");
                worldMapManager.GenerateMinimapTextureOrSphere();
                minimapGenerated = true;
            }
        }
        else if (currentZoom > mediumThreshold)
        {
            if (minimapCamera.enabled) minimapCamera.enabled = false;  // Desactiva minimapCamera a este nivel
            // Si llegas a tener una globeCamera, aquí podrías activar otra, pero vamos a dejarla desactivada por ahora
        }
    }
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
        var tiles = Object.FindObjectsByType<HexRenderer>(FindObjectsSortMode.None);
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

## 📁 Editor/WorldMapManagerEditor.cs
```csharp
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(WorldMapManager))]
public class WorldMapManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        WorldMapManager manager = (WorldMapManager)target;

if (GUILayout.Button("🔄 Regenerar Mundo"))
{
    manager.InitializeWorld();
    Debug.Log("🛠 Mundo regenerado manualmente desde botón.");
}



    }
}
```

---

## 📁 FastNoiseLite.cs
```csharp
// MIT License
//
// Copyright(c) 2023 Jordan Peck (jordan.me2@gmail.com)
// Copyright(c) 2023 Contributors
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files(the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and / or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions :
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
//
// .'',;:cldxkO00KKXXNNWWWNNXKOkxdollcc::::::;:::ccllloooolllllllllooollc:,'...        ...........',;cldxkO000Okxdlc::;;;,,;;;::cclllllll
// ..',;:ldxO0KXXNNNNNNNNXXK0kxdolcc::::::;;;,,,,,,;;;;;;;;;;:::cclllllc:;'....       ...........',;:ldxO0KXXXK0Okxdolc::;;;;::cllodddddo
// ...',:loxO0KXNNNNNXXKK0Okxdolc::;::::::::;;;,,'''''.....''',;:clllllc:;,'............''''''''',;:loxO0KXNNNNNXK0Okxdollccccllodxxxxxxd
// ....';:ldkO0KXXXKK00Okxdolcc:;;;;;::cclllcc:;;,''..... ....',;clooddolcc:;;;;,,;;;;;::::;;;;;;:cloxk0KXNWWWWWWNXKK0Okxddoooddxxkkkkkxx
// .....';:ldxkOOOOOkxxdolcc:;;;,,,;;:cllooooolcc:;'...      ..,:codxkkkxddooollloooooooollcc:::::clodkO0KXNWWWWWWNNXK00Okxxxxxxxxkkkkxxx
// . ....';:cloddddo___________,,,,;;:clooddddoolc:,...      ..,:ldx__00OOOkkk___kkkkkkxxdollc::::cclodkO0KXXNNNNNNXXK0OOkxxxxxxxxxxxxddd
// .......',;:cccc:|           |,,,;;:cclooddddoll:;'..     ..';cox|  \KKK000|   |KK00OOkxdocc___;::clldxxkO0KKKKK00Okkxdddddddddddddddoo
// .......'',,,,,''|   ________|',,;;::cclloooooolc:;'......___:ldk|   \KK000|   |XKKK0Okxolc|   |;;::cclodxxkkkkxxdoolllcclllooodddooooo
// ''......''''....|   |  ....'',,,,;;;::cclloooollc:;,''.'|   |oxk|    \OOO0|   |KKK00Oxdoll|___|;;;;;::ccllllllcc::;;,,;;;:cclloooooooo
// ;;,''.......... |   |_____',,;;;____:___cllo________.___|   |___|     \xkk|   |KK_______ool___:::;________;;;_______...'',;;:ccclllloo
// c:;,''......... |         |:::/     '   |lo/        |           |      \dx|   |0/       \d|   |cc/        |'/       \......',,;;:ccllo
// ol:;,'..........|    _____|ll/    __    |o/   ______|____    ___|   |   \o|   |/   ___   \|   |o/   ______|/   ___   \ .......'',;:clo
// dlc;,...........|   |::clooo|    /  |   |x\___   \KXKKK0|   |dol|   |\   \|   |   |   |   |   |d\___   \..|   |  /   /       ....',:cl
// xoc;'...  .....'|   |llodddd|    \__|   |_____\   \KKK0O|   |lc:|   |'\       |   |___|   |   |_____\   \.|   |_/___/...      ...',;:c
// dlc;'... ....',;|   |oddddddo\          |          |Okkx|   |::;|   |..\      |\         /|   |          | \         |...    ....',;:c
// ol:,'.......',:c|___|xxxddollc\_____,___|_________/ddoll|___|,,,|___|...\_____|:\ ______/l|___|_________/...\________|'........',;::cc
// c:;'.......';:codxxkkkkxxolc::;::clodxkOO0OOkkxdollc::;;,,''''',,,,''''''''''',,'''''',;:loxkkOOkxol:;,'''',,;:ccllcc:;,'''''',;::ccll
// ;,'.......',:codxkOO0OOkxdlc:;,,;;:cldxxkkxxdolc:;;,,''.....'',;;:::;;,,,'''''........,;cldkO0KK0Okdoc::;;::cloodddoolc:;;;;;::ccllooo
// .........',;:lodxOO0000Okdoc:,,',,;:clloddoolc:;,''.......'',;:clooollc:;;,,''.......',:ldkOKXNNXX0Oxdolllloddxxxxxxdolccccccllooodddd
// .    .....';:cldxkO0000Okxol:;,''',,;::cccc:;,,'.......'',;:cldxxkkxxdolc:;;,'.......';coxOKXNWWWNXKOkxddddxxkkkkkkxdoollllooddxxxxkkk
//       ....',;:codxkO000OOxdoc:;,''',,,;;;;,''.......',,;:clodkO00000Okxolc::;,,''..',;:ldxOKXNWWWNNK0OkkkkkkkkkkkxxddooooodxxkOOOOO000
//       ....',;;clodxkkOOOkkdolc:;,,,,,,,,'..........,;:clodxkO0KKXKK0Okxdolcc::;;,,,;;:codkO0XXNNNNXKK0OOOOOkkkkxxdoollloodxkO0KKKXXXXX
//
// VERSION: 1.1.1
// https://github.com/Auburn/FastNoiseLite

using System;
using System.Runtime.CompilerServices;

// Switch between using floats or doubles for input position
using FNLfloat = System.Single;
//using FNLfloat = System.Double;

public class FastNoiseLite
{
    private const short INLINE = 256; // MethodImplOptions.AggressiveInlining;
    private const short OPTIMISE = 512; // MethodImplOptions.AggressiveOptimization;

    public enum NoiseType 
    { 
        OpenSimplex2,
        OpenSimplex2S,
        Cellular,
        Perlin,
        ValueCubic,
        Value 
    };

    public enum RotationType3D 
    {
        None, 
        ImproveXYPlanes, 
        ImproveXZPlanes 
    };
    
    public enum FractalType 
    {
        None, 
        FBm, 
        Ridged, 
        PingPong, 
        DomainWarpProgressive, 
        DomainWarpIndependent 
    };

    public enum CellularDistanceFunction 
    {
        Euclidean, 
        EuclideanSq, 
        Manhattan, 
        Hybrid 
    };
    
    public enum CellularReturnType 
    {
        CellValue, 
        Distance, 
        Distance2, 
        Distance2Add, 
        Distance2Sub, 
        Distance2Mul, 
        Distance2Div 
    };

    public enum DomainWarpType 
    { 
        OpenSimplex2, 
        OpenSimplex2Reduced, 
        BasicGrid 
    };

    private enum TransformType3D 
    {
        None, 
        ImproveXYPlanes, 
        ImproveXZPlanes, 
        DefaultOpenSimplex2 
    };

    private int mSeed = 1337;
    private float mFrequency = 0.01f;
    private NoiseType mNoiseType = NoiseType.OpenSimplex2;
    private RotationType3D mRotationType3D = RotationType3D.None;
    private TransformType3D mTransformType3D = TransformType3D.DefaultOpenSimplex2;

    private FractalType mFractalType = FractalType.None;
    private int mOctaves = 3;
    private float mLacunarity = 2.0f;
    private float mGain = 0.5f;
    private float mWeightedStrength = 0.0f;
    private float mPingPongStrength = 2.0f;

    private float mFractalBounding = 1 / 1.75f;

    private CellularDistanceFunction mCellularDistanceFunction = CellularDistanceFunction.EuclideanSq;
    private CellularReturnType mCellularReturnType = CellularReturnType.Distance;
    private float mCellularJitterModifier = 1.0f;

    private DomainWarpType mDomainWarpType = DomainWarpType.OpenSimplex2;
    private TransformType3D mWarpTransformType3D = TransformType3D.DefaultOpenSimplex2;
    private float mDomainWarpAmp = 1.0f;

    /// <summary>
    /// Create new FastNoise object with optional seed
    /// </summary>
    public FastNoiseLite(int seed = 1337)
    {
        SetSeed(seed);
    }

    /// <summary>
    /// Sets seed used for all noise types
    /// </summary>
    /// <remarks>
    /// Default: 1337
    /// </remarks>
    public void SetSeed(int seed) { mSeed = seed; }

    /// <summary>
    /// Sets frequency for all noise types
    /// </summary>
    /// <remarks>
    /// Default: 0.01
    /// </remarks>
    public void SetFrequency(float frequency) { mFrequency = frequency; }

    /// <summary>
    /// Sets noise algorithm used for GetNoise(...)
    /// </summary>
    /// <remarks>
    /// Default: OpenSimplex2
    /// </remarks>
    public void SetNoiseType(NoiseType noiseType)
    {
        mNoiseType = noiseType;
        UpdateTransformType3D();
    }

    /// <summary>
    /// Sets domain rotation type for 3D Noise and 3D DomainWarp.
    /// Can aid in reducing directional artifacts when sampling a 2D plane in 3D
    /// </summary>
    /// <remarks>
    /// Default: None
    /// </remarks>
    public void SetRotationType3D(RotationType3D rotationType3D)
    {
        mRotationType3D = rotationType3D;
        UpdateTransformType3D();
        UpdateWarpTransformType3D();
    }

    /// <summary>
    /// Sets method for combining octaves in all fractal noise types
    /// </summary>
    /// <remarks>
    /// Default: None
    /// Note: FractalType.DomainWarp... only affects DomainWarp(...)
    /// </remarks>
    public void SetFractalType(FractalType fractalType) { mFractalType = fractalType; }

    /// <summary>
    /// Sets octave count for all fractal noise types 
    /// </summary>
    /// <remarks>
    /// Default: 3
    /// </remarks>
    public void SetFractalOctaves(int octaves)
    {
        mOctaves = octaves;
        CalculateFractalBounding();
    }

    /// <summary>
    /// Sets octave lacunarity for all fractal noise types
    /// </summary>
    /// <remarks>
    /// Default: 2.0
    /// </remarks>
    public void SetFractalLacunarity(float lacunarity) { mLacunarity = lacunarity; }

    /// <summary>
    /// Sets octave gain for all fractal noise types
    /// </summary>
    /// <remarks>
    /// Default: 0.5
    /// </remarks>
    public void SetFractalGain(float gain)
    {
        mGain = gain;
        CalculateFractalBounding();
    }

    /// <summary>
    /// Sets octave weighting for all none DomainWarp fratal types
    /// </summary>
    /// <remarks>
    /// Default: 0.0
    /// Note: Keep between 0...1 to maintain -1...1 output bounding
    /// </remarks>
    public void SetFractalWeightedStrength(float weightedStrength) { mWeightedStrength = weightedStrength; }

    /// <summary>
    /// Sets strength of the fractal ping pong effect
    /// </summary>
    /// <remarks>
    /// Default: 2.0
    /// </remarks>
    public void SetFractalPingPongStrength(float pingPongStrength) { mPingPongStrength = pingPongStrength; }


    /// <summary>
    /// Sets distance function used in cellular noise calculations
    /// </summary>
    /// <remarks>
    /// Default: Distance
    /// </remarks>
    public void SetCellularDistanceFunction(CellularDistanceFunction cellularDistanceFunction) { mCellularDistanceFunction = cellularDistanceFunction; }

    /// <summary>
    /// Sets return type from cellular noise calculations
    /// </summary>
    /// <remarks>
    /// Default: EuclideanSq
    /// </remarks>
    public void SetCellularReturnType(CellularReturnType cellularReturnType) { mCellularReturnType = cellularReturnType; }

    /// <summary>
    /// Sets the maximum distance a cellular point can move from it's grid position
    /// </summary>
    /// <remarks>
    /// Default: 1.0
    /// Note: Setting this higher than 1 will cause artifacts
    /// </remarks> 
    public void SetCellularJitter(float cellularJitter) { mCellularJitterModifier = cellularJitter; }


    /// <summary>
    /// Sets the warp algorithm when using DomainWarp(...)
    /// </summary>
    /// <remarks>
    /// Default: OpenSimplex2
    /// </remarks>
    public void SetDomainWarpType(DomainWarpType domainWarpType)
    {
        mDomainWarpType = domainWarpType;
        UpdateWarpTransformType3D();
    }


    /// <summary>
    /// Sets the maximum warp distance from original position when using DomainWarp(...)
    /// </summary>
    /// <remarks>
    /// Default: 1.0
    /// </remarks>
    public void SetDomainWarpAmp(float domainWarpAmp) { mDomainWarpAmp = domainWarpAmp; }


    /// <summary>
    /// 2D noise at given position using current settings
    /// </summary>
    /// <returns>
    /// Noise output bounded between -1...1
    /// </returns>
    [MethodImpl(OPTIMISE)]
    public float GetNoise(FNLfloat x, FNLfloat y)
    {
        TransformNoiseCoordinate(ref x, ref y);

        switch (mFractalType)
        {
            default:
                return GenNoiseSingle(mSeed, x, y);
            case FractalType.FBm:
                return GenFractalFBm(x, y);
            case FractalType.Ridged:
                return GenFractalRidged(x, y);
            case FractalType.PingPong:
                return GenFractalPingPong(x, y);
        }
    }

    /// <summary>
    /// 3D noise at given position using current settings
    /// </summary>
    /// <returns>
    /// Noise output bounded between -1...1
    /// </returns>
    [MethodImpl(OPTIMISE)]
    public float GetNoise(FNLfloat x, FNLfloat y, FNLfloat z)
    {
        TransformNoiseCoordinate(ref x, ref y, ref z);

        switch (mFractalType)
        {
            default:
                return GenNoiseSingle(mSeed, x, y, z);
            case FractalType.FBm:
                return GenFractalFBm(x, y, z);
            case FractalType.Ridged:
                return GenFractalRidged(x, y, z);
            case FractalType.PingPong:
                return GenFractalPingPong(x, y, z);
        }
    }


    /// <summary>
    /// 2D warps the input position using current domain warp settings
    /// </summary>
    /// <example>
    /// Example usage with GetNoise
    /// <code>DomainWarp(ref x, ref y)
    /// noise = GetNoise(x, y)</code>
    /// </example>
    [MethodImpl(OPTIMISE)]
    public void DomainWarp(ref FNLfloat x, ref FNLfloat y)
    {
        switch (mFractalType)
        {
            default:
                DomainWarpSingle(ref x, ref y);
                break;
            case FractalType.DomainWarpProgressive:
                DomainWarpFractalProgressive(ref x, ref y);
                break;
            case FractalType.DomainWarpIndependent:
                DomainWarpFractalIndependent(ref x, ref y);
                break;
        }
    }

    /// <summary>
    /// 3D warps the input position using current domain warp settings
    /// </summary>
    /// <example>
    /// Example usage with GetNoise
    /// <code>DomainWarp(ref x, ref y, ref z)
    /// noise = GetNoise(x, y, z)</code>
    /// </example>
    [MethodImpl(OPTIMISE)]
    public void DomainWarp(ref FNLfloat x, ref FNLfloat y, ref FNLfloat z)
    {
        switch (mFractalType)
        {
            default:
                DomainWarpSingle(ref x, ref y, ref z);
                break;
            case FractalType.DomainWarpProgressive:
                DomainWarpFractalProgressive(ref x, ref y, ref z);
                break;
            case FractalType.DomainWarpIndependent:
                DomainWarpFractalIndependent(ref x, ref y, ref z);
                break;
        }
    }


    private static readonly float[] Gradients2D =
    {
         0.130526192220052f,  0.99144486137381f,   0.38268343236509f,   0.923879532511287f,  0.608761429008721f,  0.793353340291235f,  0.793353340291235f,  0.608761429008721f,
         0.923879532511287f,  0.38268343236509f,   0.99144486137381f,   0.130526192220051f,  0.99144486137381f,  -0.130526192220051f,  0.923879532511287f, -0.38268343236509f,
         0.793353340291235f, -0.60876142900872f,   0.608761429008721f, -0.793353340291235f,  0.38268343236509f,  -0.923879532511287f,  0.130526192220052f, -0.99144486137381f,
        -0.130526192220052f, -0.99144486137381f,  -0.38268343236509f,  -0.923879532511287f, -0.608761429008721f, -0.793353340291235f, -0.793353340291235f, -0.608761429008721f,
        -0.923879532511287f, -0.38268343236509f,  -0.99144486137381f,  -0.130526192220052f, -0.99144486137381f,   0.130526192220051f, -0.923879532511287f,  0.38268343236509f,
        -0.793353340291235f,  0.608761429008721f, -0.608761429008721f,  0.793353340291235f, -0.38268343236509f,   0.923879532511287f, -0.130526192220052f,  0.99144486137381f,
         0.130526192220052f,  0.99144486137381f,   0.38268343236509f,   0.923879532511287f,  0.608761429008721f,  0.793353340291235f,  0.793353340291235f,  0.608761429008721f,
         0.923879532511287f,  0.38268343236509f,   0.99144486137381f,   0.130526192220051f,  0.99144486137381f,  -0.130526192220051f,  0.923879532511287f, -0.38268343236509f,
         0.793353340291235f, -0.60876142900872f,   0.608761429008721f, -0.793353340291235f,  0.38268343236509f,  -0.923879532511287f,  0.130526192220052f, -0.99144486137381f,
        -0.130526192220052f, -0.99144486137381f,  -0.38268343236509f,  -0.923879532511287f, -0.608761429008721f, -0.793353340291235f, -0.793353340291235f, -0.608761429008721f,
        -0.923879532511287f, -0.38268343236509f,  -0.99144486137381f,  -0.130526192220052f, -0.99144486137381f,   0.130526192220051f, -0.923879532511287f,  0.38268343236509f,
        -0.793353340291235f,  0.608761429008721f, -0.608761429008721f,  0.793353340291235f, -0.38268343236509f,   0.923879532511287f, -0.130526192220052f,  0.99144486137381f,
         0.130526192220052f,  0.99144486137381f,   0.38268343236509f,   0.923879532511287f,  0.608761429008721f,  0.793353340291235f,  0.793353340291235f,  0.608761429008721f,
         0.923879532511287f,  0.38268343236509f,   0.99144486137381f,   0.130526192220051f,  0.99144486137381f,  -0.130526192220051f,  0.923879532511287f, -0.38268343236509f,
         0.793353340291235f, -0.60876142900872f,   0.608761429008721f, -0.793353340291235f,  0.38268343236509f,  -0.923879532511287f,  0.130526192220052f, -0.99144486137381f,
        -0.130526192220052f, -0.99144486137381f,  -0.38268343236509f,  -0.923879532511287f, -0.608761429008721f, -0.793353340291235f, -0.793353340291235f, -0.608761429008721f,
        -0.923879532511287f, -0.38268343236509f,  -0.99144486137381f,  -0.130526192220052f, -0.99144486137381f,   0.130526192220051f, -0.923879532511287f,  0.38268343236509f,
        -0.793353340291235f,  0.608761429008721f, -0.608761429008721f,  0.793353340291235f, -0.38268343236509f,   0.923879532511287f, -0.130526192220052f,  0.99144486137381f,
         0.130526192220052f,  0.99144486137381f,   0.38268343236509f,   0.923879532511287f,  0.608761429008721f,  0.793353340291235f,  0.793353340291235f,  0.608761429008721f,
         0.923879532511287f,  0.38268343236509f,   0.99144486137381f,   0.130526192220051f,  0.99144486137381f,  -0.130526192220051f,  0.923879532511287f, -0.38268343236509f,
         0.793353340291235f, -0.60876142900872f,   0.608761429008721f, -0.793353340291235f,  0.38268343236509f,  -0.923879532511287f,  0.130526192220052f, -0.99144486137381f,
        -0.130526192220052f, -0.99144486137381f,  -0.38268343236509f,  -0.923879532511287f, -0.608761429008721f, -0.793353340291235f, -0.793353340291235f, -0.608761429008721f,
        -0.923879532511287f, -0.38268343236509f,  -0.99144486137381f,  -0.130526192220052f, -0.99144486137381f,   0.130526192220051f, -0.923879532511287f,  0.38268343236509f,
        -0.793353340291235f,  0.608761429008721f, -0.608761429008721f,  0.793353340291235f, -0.38268343236509f,   0.923879532511287f, -0.130526192220052f,  0.99144486137381f,
         0.130526192220052f,  0.99144486137381f,   0.38268343236509f,   0.923879532511287f,  0.608761429008721f,  0.793353340291235f,  0.793353340291235f,  0.608761429008721f,
         0.923879532511287f,  0.38268343236509f,   0.99144486137381f,   0.130526192220051f,  0.99144486137381f,  -0.130526192220051f,  0.923879532511287f, -0.38268343236509f,
         0.793353340291235f, -0.60876142900872f,   0.608761429008721f, -0.793353340291235f,  0.38268343236509f,  -0.923879532511287f,  0.130526192220052f, -0.99144486137381f,
        -0.130526192220052f, -0.99144486137381f,  -0.38268343236509f,  -0.923879532511287f, -0.608761429008721f, -0.793353340291235f, -0.793353340291235f, -0.608761429008721f,
        -0.923879532511287f, -0.38268343236509f,  -0.99144486137381f,  -0.130526192220052f, -0.99144486137381f,   0.130526192220051f, -0.923879532511287f,  0.38268343236509f,
        -0.793353340291235f,  0.608761429008721f, -0.608761429008721f,  0.793353340291235f, -0.38268343236509f,   0.923879532511287f, -0.130526192220052f,  0.99144486137381f,
         0.38268343236509f,   0.923879532511287f,  0.923879532511287f,  0.38268343236509f,   0.923879532511287f, -0.38268343236509f,   0.38268343236509f,  -0.923879532511287f,
        -0.38268343236509f,  -0.923879532511287f, -0.923879532511287f, -0.38268343236509f,  -0.923879532511287f,  0.38268343236509f,  -0.38268343236509f,   0.923879532511287f,
    };

    private static readonly float[] RandVecs2D =
    {
        -0.2700222198f, -0.9628540911f, 0.3863092627f, -0.9223693152f, 0.04444859006f, -0.999011673f, -0.5992523158f, -0.8005602176f, -0.7819280288f, 0.6233687174f, 0.9464672271f, 0.3227999196f, -0.6514146797f, -0.7587218957f, 0.9378472289f, 0.347048376f,
        -0.8497875957f, -0.5271252623f, -0.879042592f, 0.4767432447f, -0.892300288f, -0.4514423508f, -0.379844434f, -0.9250503802f, -0.9951650832f, 0.0982163789f, 0.7724397808f, -0.6350880136f, 0.7573283322f, -0.6530343002f, -0.9928004525f, -0.119780055f,
        -0.0532665713f, 0.9985803285f, 0.9754253726f, -0.2203300762f, -0.7665018163f, 0.6422421394f, 0.991636706f, 0.1290606184f, -0.994696838f, 0.1028503788f, -0.5379205513f, -0.84299554f, 0.5022815471f, -0.8647041387f, 0.4559821461f, -0.8899889226f,
        -0.8659131224f, -0.5001944266f, 0.0879458407f, -0.9961252577f, -0.5051684983f, 0.8630207346f, 0.7753185226f, -0.6315704146f, -0.6921944612f, 0.7217110418f, -0.5191659449f, -0.8546734591f, 0.8978622882f, -0.4402764035f, -0.1706774107f, 0.9853269617f,
        -0.9353430106f, -0.3537420705f, -0.9992404798f, 0.03896746794f, -0.2882064021f, -0.9575683108f, -0.9663811329f, 0.2571137995f, -0.8759714238f, -0.4823630009f, -0.8303123018f, -0.5572983775f, 0.05110133755f, -0.9986934731f, -0.8558373281f, -0.5172450752f,
        0.09887025282f, 0.9951003332f, 0.9189016087f, 0.3944867976f, -0.2439375892f, -0.9697909324f, -0.8121409387f, -0.5834613061f, -0.9910431363f, 0.1335421355f, 0.8492423985f, -0.5280031709f, -0.9717838994f, -0.2358729591f, 0.9949457207f, 0.1004142068f,
        0.6241065508f, -0.7813392434f, 0.662910307f, 0.7486988212f, -0.7197418176f, 0.6942418282f, -0.8143370775f, -0.5803922158f, 0.104521054f, -0.9945226741f, -0.1065926113f, -0.9943027784f, 0.445799684f, -0.8951327509f, 0.105547406f, 0.9944142724f,
        -0.992790267f, 0.1198644477f, -0.8334366408f, 0.552615025f, 0.9115561563f, -0.4111755999f, 0.8285544909f, -0.5599084351f, 0.7217097654f, -0.6921957921f, 0.4940492677f, -0.8694339084f, -0.3652321272f, -0.9309164803f, -0.9696606758f, 0.2444548501f,
        0.08925509731f, -0.996008799f, 0.5354071276f, -0.8445941083f, -0.1053576186f, 0.9944343981f, -0.9890284586f, 0.1477251101f, 0.004856104961f, 0.9999882091f, 0.9885598478f, 0.1508291331f, 0.9286129562f, -0.3710498316f, -0.5832393863f, -0.8123003252f,
        0.3015207509f, 0.9534596146f, -0.9575110528f, 0.2883965738f, 0.9715802154f, -0.2367105511f, 0.229981792f, 0.9731949318f, 0.955763816f, -0.2941352207f, 0.740956116f, 0.6715534485f, -0.9971513787f, -0.07542630764f, 0.6905710663f, -0.7232645452f,
        -0.290713703f, -0.9568100872f, 0.5912777791f, -0.8064679708f, -0.9454592212f, -0.325740481f, 0.6664455681f, 0.74555369f, 0.6236134912f, 0.7817328275f, 0.9126993851f, -0.4086316587f, -0.8191762011f, 0.5735419353f, -0.8812745759f, -0.4726046147f,
        0.9953313627f, 0.09651672651f, 0.9855650846f, -0.1692969699f, -0.8495980887f, 0.5274306472f, 0.6174853946f, -0.7865823463f, 0.8508156371f, 0.52546432f, 0.9985032451f, -0.05469249926f, 0.1971371563f, -0.9803759185f, 0.6607855748f, -0.7505747292f,
        -0.03097494063f, 0.9995201614f, -0.6731660801f, 0.739491331f, -0.7195018362f, -0.6944905383f, 0.9727511689f, 0.2318515979f, 0.9997059088f, -0.0242506907f, 0.4421787429f, -0.8969269532f, 0.9981350961f, -0.061043673f, -0.9173660799f, -0.3980445648f,
        -0.8150056635f, -0.5794529907f, -0.8789331304f, 0.4769450202f, 0.0158605829f, 0.999874213f, -0.8095464474f, 0.5870558317f, -0.9165898907f, -0.3998286786f, -0.8023542565f, 0.5968480938f, -0.5176737917f, 0.8555780767f, -0.8154407307f, -0.5788405779f,
        0.4022010347f, -0.9155513791f, -0.9052556868f, -0.4248672045f, 0.7317445619f, 0.6815789728f, -0.5647632201f, -0.8252529947f, -0.8403276335f, -0.5420788397f, -0.9314281527f, 0.363925262f, 0.5238198472f, 0.8518290719f, 0.7432803869f, -0.6689800195f,
        -0.985371561f, -0.1704197369f, 0.4601468731f, 0.88784281f, 0.825855404f, 0.5638819483f, 0.6182366099f, 0.7859920446f, 0.8331502863f, -0.553046653f, 0.1500307506f, 0.9886813308f, -0.662330369f, -0.7492119075f, -0.668598664f, 0.743623444f,
        0.7025606278f, 0.7116238924f, -0.5419389763f, -0.8404178401f, -0.3388616456f, 0.9408362159f, 0.8331530315f, 0.5530425174f, -0.2989720662f, -0.9542618632f, 0.2638522993f, 0.9645630949f, 0.124108739f, -0.9922686234f, -0.7282649308f, -0.6852956957f,
        0.6962500149f, 0.7177993569f, -0.9183535368f, 0.3957610156f, -0.6326102274f, -0.7744703352f, -0.9331891859f, -0.359385508f, -0.1153779357f, -0.9933216659f, 0.9514974788f, -0.3076565421f, -0.08987977445f, -0.9959526224f, 0.6678496916f, 0.7442961705f,
        0.7952400393f, -0.6062947138f, -0.6462007402f, -0.7631674805f, -0.2733598753f, 0.9619118351f, 0.9669590226f, -0.254931851f, -0.9792894595f, 0.2024651934f, -0.5369502995f, -0.8436138784f, -0.270036471f, -0.9628500944f, -0.6400277131f, 0.7683518247f,
        -0.7854537493f, -0.6189203566f, 0.06005905383f, -0.9981948257f, -0.02455770378f, 0.9996984141f, -0.65983623f, 0.751409442f, -0.6253894466f, -0.7803127835f, -0.6210408851f, -0.7837781695f, 0.8348888491f, 0.5504185768f, -0.1592275245f, 0.9872419133f,
        0.8367622488f, 0.5475663786f, -0.8675753916f, -0.4973056806f, -0.2022662628f, -0.9793305667f, 0.9399189937f, 0.3413975472f, 0.9877404807f, -0.1561049093f, -0.9034455656f, 0.4287028224f, 0.1269804218f, -0.9919052235f, -0.3819600854f, 0.924178821f,
        0.9754625894f, 0.2201652486f, -0.3204015856f, -0.9472818081f, -0.9874760884f, 0.1577687387f, 0.02535348474f, -0.9996785487f, 0.4835130794f, -0.8753371362f, -0.2850799925f, -0.9585037287f, -0.06805516006f, -0.99768156f, -0.7885244045f, -0.6150034663f,
        0.3185392127f, -0.9479096845f, 0.8880043089f, 0.4598351306f, 0.6476921488f, -0.7619021462f, 0.9820241299f, 0.1887554194f, 0.9357275128f, -0.3527237187f, -0.8894895414f, 0.4569555293f, 0.7922791302f, 0.6101588153f, 0.7483818261f, 0.6632681526f,
        -0.7288929755f, -0.6846276581f, 0.8729032783f, -0.4878932944f, 0.8288345784f, 0.5594937369f, 0.08074567077f, 0.9967347374f, 0.9799148216f, -0.1994165048f, -0.580730673f, -0.8140957471f, -0.4700049791f, -0.8826637636f, 0.2409492979f, 0.9705377045f,
        0.9437816757f, -0.3305694308f, -0.8927998638f, -0.4504535528f, -0.8069622304f, 0.5906030467f, 0.06258973166f, 0.9980393407f, -0.9312597469f, 0.3643559849f, 0.5777449785f, 0.8162173362f, -0.3360095855f, -0.941858566f, 0.697932075f, -0.7161639607f,
        -0.002008157227f, -0.9999979837f, -0.1827294312f, -0.9831632392f, -0.6523911722f, 0.7578824173f, -0.4302626911f, -0.9027037258f, -0.9985126289f, -0.05452091251f, -0.01028102172f, -0.9999471489f, -0.4946071129f, 0.8691166802f, -0.2999350194f, 0.9539596344f,
        0.8165471961f, 0.5772786819f, 0.2697460475f, 0.962931498f, -0.7306287391f, -0.6827749597f, -0.7590952064f, -0.6509796216f, -0.907053853f, 0.4210146171f, -0.5104861064f, -0.8598860013f, 0.8613350597f, 0.5080373165f, 0.5007881595f, -0.8655698812f,
        -0.654158152f, 0.7563577938f, -0.8382755311f, -0.545246856f, 0.6940070834f, 0.7199681717f, 0.06950936031f, 0.9975812994f, 0.1702942185f, -0.9853932612f, 0.2695973274f, 0.9629731466f, 0.5519612192f, -0.8338697815f, 0.225657487f, -0.9742067022f,
        0.4215262855f, -0.9068161835f, 0.4881873305f, -0.8727388672f, -0.3683854996f, -0.9296731273f, -0.9825390578f, 0.1860564427f, 0.81256471f, 0.5828709909f, 0.3196460933f, -0.9475370046f, 0.9570913859f, 0.2897862643f, -0.6876655497f, -0.7260276109f,
        -0.9988770922f, -0.047376731f, -0.1250179027f, 0.992154486f, -0.8280133617f, 0.560708367f, 0.9324863769f, -0.3612051451f, 0.6394653183f, 0.7688199442f, -0.01623847064f, -0.9998681473f, -0.9955014666f, -0.09474613458f, -0.81453315f, 0.580117012f,
        0.4037327978f, -0.9148769469f, 0.9944263371f, 0.1054336766f, -0.1624711654f, 0.9867132919f, -0.9949487814f, -0.100383875f, -0.6995302564f, 0.7146029809f, 0.5263414922f, -0.85027327f, -0.5395221479f, 0.841971408f, 0.6579370318f, 0.7530729462f,
        0.01426758847f, -0.9998982128f, -0.6734383991f, 0.7392433447f, 0.639412098f, -0.7688642071f, 0.9211571421f, 0.3891908523f, -0.146637214f, -0.9891903394f, -0.782318098f, 0.6228791163f, -0.5039610839f, -0.8637263605f, -0.7743120191f, -0.6328039957f,
    };

    private static readonly float[] Gradients3D =
    {
        0, 1, 1, 0,  0,-1, 1, 0,  0, 1,-1, 0,  0,-1,-1, 0,
        1, 0, 1, 0, -1, 0, 1, 0,  1, 0,-1, 0, -1, 0,-1, 0,
        1, 1, 0, 0, -1, 1, 0, 0,  1,-1, 0, 0, -1,-1, 0, 0,
        0, 1, 1, 0,  0,-1, 1, 0,  0, 1,-1, 0,  0,-1,-1, 0,
        1, 0, 1, 0, -1, 0, 1, 0,  1, 0,-1, 0, -1, 0,-1, 0,
        1, 1, 0, 0, -1, 1, 0, 0,  1,-1, 0, 0, -1,-1, 0, 0,
        0, 1, 1, 0,  0,-1, 1, 0,  0, 1,-1, 0,  0,-1,-1, 0,
        1, 0, 1, 0, -1, 0, 1, 0,  1, 0,-1, 0, -1, 0,-1, 0,
        1, 1, 0, 0, -1, 1, 0, 0,  1,-1, 0, 0, -1,-1, 0, 0,
        0, 1, 1, 0,  0,-1, 1, 0,  0, 1,-1, 0,  0,-1,-1, 0,
        1, 0, 1, 0, -1, 0, 1, 0,  1, 0,-1, 0, -1, 0,-1, 0,
        1, 1, 0, 0, -1, 1, 0, 0,  1,-1, 0, 0, -1,-1, 0, 0,
        0, 1, 1, 0,  0,-1, 1, 0,  0, 1,-1, 0,  0,-1,-1, 0,
        1, 0, 1, 0, -1, 0, 1, 0,  1, 0,-1, 0, -1, 0,-1, 0,
        1, 1, 0, 0, -1, 1, 0, 0,  1,-1, 0, 0, -1,-1, 0, 0,
        1, 1, 0, 0,  0,-1, 1, 0, -1, 1, 0, 0,  0,-1,-1, 0
    };

    private static readonly float[] RandVecs3D =
    {
        -0.7292736885f, -0.6618439697f, 0.1735581948f, 0, 0.790292081f, -0.5480887466f, -0.2739291014f, 0, 0.7217578935f, 0.6226212466f, -0.3023380997f, 0, 0.565683137f, -0.8208298145f, -0.0790000257f, 0, 0.760049034f, -0.5555979497f, -0.3370999617f, 0, 0.3713945616f, 0.5011264475f, 0.7816254623f, 0, -0.1277062463f, -0.4254438999f, -0.8959289049f, 0, -0.2881560924f, -0.5815838982f, 0.7607405838f, 0,
        0.5849561111f, -0.662820239f, -0.4674352136f, 0, 0.3307171178f, 0.0391653737f, 0.94291689f, 0, 0.8712121778f, -0.4113374369f, -0.2679381538f, 0, 0.580981015f, 0.7021915846f, 0.4115677815f, 0, 0.503756873f, 0.6330056931f, -0.5878203852f, 0, 0.4493712205f, 0.601390195f, 0.6606022552f, 0, -0.6878403724f, 0.09018890807f, -0.7202371714f, 0, -0.5958956522f, -0.6469350577f, 0.475797649f, 0,
        -0.5127052122f, 0.1946921978f, -0.8361987284f, 0, -0.9911507142f, -0.05410276466f, -0.1212153153f, 0, -0.2149721042f, 0.9720882117f, -0.09397607749f, 0, -0.7518650936f, -0.5428057603f, 0.3742469607f, 0, 0.5237068895f, 0.8516377189f, -0.02107817834f, 0, 0.6333504779f, 0.1926167129f, -0.7495104896f, 0, -0.06788241606f, 0.3998305789f, 0.9140719259f, 0, -0.5538628599f, -0.4729896695f, -0.6852128902f, 0,
        -0.7261455366f, -0.5911990757f, 0.3509933228f, 0, -0.9229274737f, -0.1782808786f, 0.3412049336f, 0, -0.6968815002f, 0.6511274338f, 0.3006480328f, 0, 0.9608044783f, -0.2098363234f, -0.1811724921f, 0, 0.06817146062f, -0.9743405129f, 0.2145069156f, 0, -0.3577285196f, -0.6697087264f, -0.6507845481f, 0, -0.1868621131f, 0.7648617052f, -0.6164974636f, 0, -0.6541697588f, 0.3967914832f, 0.6439087246f, 0,
        0.6993340405f, -0.6164538506f, 0.3618239211f, 0, -0.1546665739f, 0.6291283928f, 0.7617583057f, 0, -0.6841612949f, -0.2580482182f, -0.6821542638f, 0, 0.5383980957f, 0.4258654885f, 0.7271630328f, 0, -0.5026987823f, -0.7939832935f, -0.3418836993f, 0, 0.3202971715f, 0.2834415347f, 0.9039195862f, 0, 0.8683227101f, -0.0003762656404f, -0.4959995258f, 0, 0.791120031f, -0.08511045745f, 0.6057105799f, 0,
        -0.04011016052f, -0.4397248749f, 0.8972364289f, 0, 0.9145119872f, 0.3579346169f, -0.1885487608f, 0, -0.9612039066f, -0.2756484276f, 0.01024666929f, 0, 0.6510361721f, -0.2877799159f, -0.7023778346f, 0, -0.2041786351f, 0.7365237271f, 0.644859585f, 0, -0.7718263711f, 0.3790626912f, 0.5104855816f, 0, -0.3060082741f, -0.7692987727f, 0.5608371729f, 0, 0.454007341f, -0.5024843065f, 0.7357899537f, 0,
        0.4816795475f, 0.6021208291f, -0.6367380315f, 0, 0.6961980369f, -0.3222197429f, 0.641469197f, 0, -0.6532160499f, -0.6781148932f, 0.3368515753f, 0, 0.5089301236f, -0.6154662304f, -0.6018234363f, 0, -0.1635919754f, -0.9133604627f, -0.372840892f, 0, 0.52408019f, -0.8437664109f, 0.1157505864f, 0, 0.5902587356f, 0.4983817807f, -0.6349883666f, 0, 0.5863227872f, 0.494764745f, 0.6414307729f, 0,
        0.6779335087f, 0.2341345225f, 0.6968408593f, 0, 0.7177054546f, -0.6858979348f, 0.120178631f, 0, -0.5328819713f, -0.5205125012f, 0.6671608058f, 0, -0.8654874251f, -0.0700727088f, -0.4960053754f, 0, -0.2861810166f, 0.7952089234f, 0.5345495242f, 0, -0.04849529634f, 0.9810836427f, -0.1874115585f, 0, -0.6358521667f, 0.6058348682f, 0.4781800233f, 0, 0.6254794696f, -0.2861619734f, 0.7258696564f, 0,
        -0.2585259868f, 0.5061949264f, -0.8227581726f, 0, 0.02136306781f, 0.5064016808f, -0.8620330371f, 0, 0.200111773f, 0.8599263484f, 0.4695550591f, 0, 0.4743561372f, 0.6014985084f, -0.6427953014f, 0, 0.6622993731f, -0.5202474575f, -0.5391679918f, 0, 0.08084972818f, -0.6532720452f, 0.7527940996f, 0, -0.6893687501f, 0.0592860349f, 0.7219805347f, 0, -0.1121887082f, -0.9673185067f, 0.2273952515f, 0,
        0.7344116094f, 0.5979668656f, -0.3210532909f, 0, 0.5789393465f, -0.2488849713f, 0.7764570201f, 0, 0.6988182827f, 0.3557169806f, -0.6205791146f, 0, -0.8636845529f, -0.2748771249f, -0.4224826141f, 0, -0.4247027957f, -0.4640880967f, 0.777335046f, 0, 0.5257722489f, -0.8427017621f, 0.1158329937f, 0, 0.9343830603f, 0.316302472f, -0.1639543925f, 0, -0.1016836419f, -0.8057303073f, -0.5834887393f, 0,
        -0.6529238969f, 0.50602126f, -0.5635892736f, 0, -0.2465286165f, -0.9668205684f, -0.06694497494f, 0, -0.9776897119f, -0.2099250524f, -0.007368825344f, 0, 0.7736893337f, 0.5734244712f, 0.2694238123f, 0, -0.6095087895f, 0.4995678998f, 0.6155736747f, 0, 0.5794535482f, 0.7434546771f, 0.3339292269f, 0, -0.8226211154f, 0.08142581855f, 0.5627293636f, 0, -0.510385483f, 0.4703667658f, 0.7199039967f, 0,
        -0.5764971849f, -0.07231656274f, -0.8138926898f, 0, 0.7250628871f, 0.3949971505f, -0.5641463116f, 0, -0.1525424005f, 0.4860840828f, -0.8604958341f, 0, -0.5550976208f, -0.4957820792f, 0.667882296f, 0, -0.1883614327f, 0.9145869398f, 0.357841725f, 0, 0.7625556724f, -0.5414408243f, -0.3540489801f, 0, -0.5870231946f, -0.3226498013f, -0.7424963803f, 0, 0.3051124198f, 0.2262544068f, -0.9250488391f, 0,
        0.6379576059f, 0.577242424f, -0.5097070502f, 0, -0.5966775796f, 0.1454852398f, -0.7891830656f, 0, -0.658330573f, 0.6555487542f, -0.3699414651f, 0, 0.7434892426f, 0.2351084581f, 0.6260573129f, 0, 0.5562114096f, 0.8264360377f, -0.0873632843f, 0, -0.3028940016f, -0.8251527185f, 0.4768419182f, 0, 0.1129343818f, -0.985888439f, -0.1235710781f, 0, 0.5937652891f, -0.5896813806f, 0.5474656618f, 0,
        0.6757964092f, -0.5835758614f, -0.4502648413f, 0, 0.7242302609f, -0.1152719764f, 0.6798550586f, 0, -0.9511914166f, 0.0753623979f, -0.2992580792f, 0, 0.2539470961f, -0.1886339355f, 0.9486454084f, 0, 0.571433621f, -0.1679450851f, -0.8032795685f, 0, -0.06778234979f, 0.3978269256f, 0.9149531629f, 0, 0.6074972649f, 0.733060024f, -0.3058922593f, 0, -0.5435478392f, 0.1675822484f, 0.8224791405f, 0,
        -0.5876678086f, -0.3380045064f, -0.7351186982f, 0, -0.7967562402f, 0.04097822706f, -0.6029098428f, 0, -0.1996350917f, 0.8706294745f, 0.4496111079f, 0, -0.02787660336f, -0.9106232682f, -0.4122962022f, 0, -0.7797625996f, -0.6257634692f, 0.01975775581f, 0, -0.5211232846f, 0.7401644346f, -0.4249554471f, 0, 0.8575424857f, 0.4053272873f, -0.3167501783f, 0, 0.1045223322f, 0.8390195772f, -0.5339674439f, 0,
        0.3501822831f, 0.9242524096f, -0.1520850155f, 0, 0.1987849858f, 0.07647613266f, 0.9770547224f, 0, 0.7845996363f, 0.6066256811f, -0.1280964233f, 0, 0.09006737436f, -0.9750989929f, -0.2026569073f, 0, -0.8274343547f, -0.542299559f, 0.1458203587f, 0, -0.3485797732f, -0.415802277f, 0.840000362f, 0, -0.2471778936f, -0.7304819962f, -0.6366310879f, 0, -0.3700154943f, 0.8577948156f, 0.3567584454f, 0,
        0.5913394901f, -0.548311967f, -0.5913303597f, 0, 0.1204873514f, -0.7626472379f, -0.6354935001f, 0, 0.616959265f, 0.03079647928f, 0.7863922953f, 0, 0.1258156836f, -0.6640829889f, -0.7369967419f, 0, -0.6477565124f, -0.1740147258f, -0.7417077429f, 0, 0.6217889313f, -0.7804430448f, -0.06547655076f, 0, 0.6589943422f, -0.6096987708f, 0.4404473475f, 0, -0.2689837504f, -0.6732403169f, -0.6887635427f, 0,
        -0.3849775103f, 0.5676542638f, 0.7277093879f, 0, 0.5754444408f, 0.8110471154f, -0.1051963504f, 0, 0.9141593684f, 0.3832947817f, 0.131900567f, 0, -0.107925319f, 0.9245493968f, 0.3654593525f, 0, 0.377977089f, 0.3043148782f, 0.8743716458f, 0, -0.2142885215f, -0.8259286236f, 0.5214617324f, 0, 0.5802544474f, 0.4148098596f, -0.7008834116f, 0, -0.1982660881f, 0.8567161266f, -0.4761596756f, 0,
        -0.03381553704f, 0.3773180787f, -0.9254661404f, 0, -0.6867922841f, -0.6656597827f, 0.2919133642f, 0, 0.7731742607f, -0.2875793547f, -0.5652430251f, 0, -0.09655941928f, 0.9193708367f, -0.3813575004f, 0, 0.2715702457f, -0.9577909544f, -0.09426605581f, 0, 0.2451015704f, -0.6917998565f, -0.6792188003f, 0, 0.977700782f, -0.1753855374f, 0.1155036542f, 0, -0.5224739938f, 0.8521606816f, 0.02903615945f, 0,
        -0.7734880599f, -0.5261292347f, 0.3534179531f, 0, -0.7134492443f, -0.269547243f, 0.6467878011f, 0, 0.1644037271f, 0.5105846203f, -0.8439637196f, 0, 0.6494635788f, 0.05585611296f, 0.7583384168f, 0, -0.4711970882f, 0.5017280509f, -0.7254255765f, 0, -0.6335764307f, -0.2381686273f, -0.7361091029f, 0, -0.9021533097f, -0.270947803f, -0.3357181763f, 0, -0.3793711033f, 0.872258117f, 0.3086152025f, 0,
        -0.6855598966f, -0.3250143309f, 0.6514394162f, 0, 0.2900942212f, -0.7799057743f, -0.5546100667f, 0, -0.2098319339f, 0.85037073f, 0.4825351604f, 0, -0.4592603758f, 0.6598504336f, -0.5947077538f, 0, 0.8715945488f, 0.09616365406f, -0.4807031248f, 0, -0.6776666319f, 0.7118504878f, -0.1844907016f, 0, 0.7044377633f, 0.312427597f, 0.637304036f, 0, -0.7052318886f, -0.2401093292f, -0.6670798253f, 0,
        0.081921007f, -0.7207336136f, -0.6883545647f, 0, -0.6993680906f, -0.5875763221f, -0.4069869034f, 0, -0.1281454481f, 0.6419895885f, 0.7559286424f, 0, -0.6337388239f, -0.6785471501f, -0.3714146849f, 0, 0.5565051903f, -0.2168887573f, -0.8020356851f, 0, -0.5791554484f, 0.7244372011f, -0.3738578718f, 0, 0.1175779076f, -0.7096451073f, 0.6946792478f, 0, -0.6134619607f, 0.1323631078f, 0.7785527795f, 0,
        0.6984635305f, -0.02980516237f, -0.715024719f, 0, 0.8318082963f, -0.3930171956f, 0.3919597455f, 0, 0.1469576422f, 0.05541651717f, -0.9875892167f, 0, 0.708868575f, -0.2690503865f, 0.6520101478f, 0, 0.2726053183f, 0.67369766f, -0.68688995f, 0, -0.6591295371f, 0.3035458599f, -0.6880466294f, 0, 0.4815131379f, -0.7528270071f, 0.4487723203f, 0, 0.9430009463f, 0.1675647412f, -0.2875261255f, 0,
        0.434802957f, 0.7695304522f, -0.4677277752f, 0, 0.3931996188f, 0.594473625f, 0.7014236729f, 0, 0.7254336655f, -0.603925654f, 0.3301814672f, 0, 0.7590235227f, -0.6506083235f, 0.02433313207f, 0, -0.8552768592f, -0.3430042733f, 0.3883935666f, 0, -0.6139746835f, 0.6981725247f, 0.3682257648f, 0, -0.7465905486f, -0.5752009504f, 0.3342849376f, 0, 0.5730065677f, 0.810555537f, -0.1210916791f, 0,
        -0.9225877367f, -0.3475211012f, -0.167514036f, 0, -0.7105816789f, -0.4719692027f, -0.5218416899f, 0, -0.08564609717f, 0.3583001386f, 0.929669703f, 0, -0.8279697606f, -0.2043157126f, 0.5222271202f, 0, 0.427944023f, 0.278165994f, 0.8599346446f, 0, 0.5399079671f, -0.7857120652f, -0.3019204161f, 0, 0.5678404253f, -0.5495413974f, -0.6128307303f, 0, -0.9896071041f, 0.1365639107f, -0.04503418428f, 0,
        -0.6154342638f, -0.6440875597f, 0.4543037336f, 0, 0.1074204368f, -0.7946340692f, 0.5975094525f, 0, -0.3595449969f, -0.8885529948f, 0.28495784f, 0, -0.2180405296f, 0.1529888965f, 0.9638738118f, 0, -0.7277432317f, -0.6164050508f, -0.3007234646f, 0, 0.7249729114f, -0.00669719484f, 0.6887448187f, 0, -0.5553659455f, -0.5336586252f, 0.6377908264f, 0, 0.5137558015f, 0.7976208196f, -0.3160000073f, 0,
        -0.3794024848f, 0.9245608561f, -0.03522751494f, 0, 0.8229248658f, 0.2745365933f, -0.4974176556f, 0, -0.5404114394f, 0.6091141441f, 0.5804613989f, 0, 0.8036581901f, -0.2703029469f, 0.5301601931f, 0, 0.6044318879f, 0.6832968393f, 0.4095943388f, 0, 0.06389988817f, 0.9658208605f, -0.2512108074f, 0, 0.1087113286f, 0.7402471173f, -0.6634877936f, 0, -0.713427712f, -0.6926784018f, 0.1059128479f, 0,
        0.6458897819f, -0.5724548511f, -0.5050958653f, 0, -0.6553931414f, 0.7381471625f, 0.159995615f, 0, 0.3910961323f, 0.9188871375f, -0.05186755998f, 0, -0.4879022471f, -0.5904376907f, 0.6429111375f, 0, 0.6014790094f, 0.7707441366f, -0.2101820095f, 0, -0.5677173047f, 0.7511360995f, 0.3368851762f, 0, 0.7858573506f, 0.226674665f, 0.5753666838f, 0, -0.4520345543f, -0.604222686f, -0.6561857263f, 0,
        0.002272116345f, 0.4132844051f, -0.9105991643f, 0, -0.5815751419f, -0.5162925989f, 0.6286591339f, 0, -0.03703704785f, 0.8273785755f, 0.5604221175f, 0, -0.5119692504f, 0.7953543429f, -0.3244980058f, 0, -0.2682417366f, -0.9572290247f, -0.1084387619f, 0, -0.2322482736f, -0.9679131102f, -0.09594243324f, 0, 0.3554328906f, -0.8881505545f, 0.2913006227f, 0, 0.7346520519f, -0.4371373164f, 0.5188422971f, 0,
        0.9985120116f, 0.04659011161f, -0.02833944577f, 0, -0.3727687496f, -0.9082481361f, 0.1900757285f, 0, 0.91737377f, -0.3483642108f, 0.1925298489f, 0, 0.2714911074f, 0.4147529736f, -0.8684886582f, 0, 0.5131763485f, -0.7116334161f, 0.4798207128f, 0, -0.8737353606f, 0.18886992f, -0.4482350644f, 0, 0.8460043821f, -0.3725217914f, 0.3814499973f, 0, 0.8978727456f, -0.1780209141f, -0.4026575304f, 0,
        0.2178065647f, -0.9698322841f, -0.1094789531f, 0, -0.1518031304f, -0.7788918132f, -0.6085091231f, 0, -0.2600384876f, -0.4755398075f, -0.8403819825f, 0, 0.572313509f, -0.7474340931f, -0.3373418503f, 0, -0.7174141009f, 0.1699017182f, -0.6756111411f, 0, -0.684180784f, 0.02145707593f, -0.7289967412f, 0, -0.2007447902f, 0.06555605789f, -0.9774476623f, 0, -0.1148803697f, -0.8044887315f, 0.5827524187f, 0,
        -0.7870349638f, 0.03447489231f, 0.6159443543f, 0, -0.2015596421f, 0.6859872284f, 0.6991389226f, 0, -0.08581082512f, -0.10920836f, -0.9903080513f, 0, 0.5532693395f, 0.7325250401f, -0.396610771f, 0, -0.1842489331f, -0.9777375055f, -0.1004076743f, 0, 0.0775473789f, -0.9111505856f, 0.4047110257f, 0, 0.1399838409f, 0.7601631212f, -0.6344734459f, 0, 0.4484419361f, -0.845289248f, 0.2904925424f, 0
    };


    [MethodImpl(INLINE)]
    private static float FastMin(float a, float b) { return a < b ? a : b; }

    [MethodImpl(INLINE)]
    private static float FastMax(float a, float b) { return a > b ? a : b; }

    [MethodImpl(INLINE)]
    private static float FastAbs(float f) { return f < 0 ? -f : f; }

    [MethodImpl(INLINE)]
    private static float FastSqrt(float f) { return (float)Math.Sqrt(f); }

    [MethodImpl(INLINE)]
    private static int FastFloor(FNLfloat f) { return f >= 0 ? (int)f : (int)f - 1; }

    [MethodImpl(INLINE)]
    private static int FastRound(FNLfloat f) { return f >= 0 ? (int)(f + 0.5f) : (int)(f - 0.5f); }

    [MethodImpl(INLINE)]
    private static float Lerp(float a, float b, float t) { return a + t * (b - a); }

    [MethodImpl(INLINE)]
    private static float InterpHermite(float t) { return t * t * (3 - 2 * t); }

    [MethodImpl(INLINE)]
    private static float InterpQuintic(float t) { return t * t * t * (t * (t * 6 - 15) + 10); }

    [MethodImpl(INLINE)]
    private static float CubicLerp(float a, float b, float c, float d, float t)
    {
        float p = (d - c) - (a - b);
        return t * t * t * p + t * t * ((a - b) - p) + t * (c - a) + b;
    }

    [MethodImpl(INLINE)]
    private static float PingPong(float t)
    {
        t -= (int)(t * 0.5f) * 2;
        return t < 1 ? t : 2 - t;
    }

    private void CalculateFractalBounding()
    {
        float gain = FastAbs(mGain);
        float amp = gain;
        float ampFractal = 1.0f;
        for (int i = 1; i < mOctaves; i++)
        {
            ampFractal += amp;
            amp *= gain;
        }
        mFractalBounding = 1 / ampFractal;
    }

    // Hashing
    private const int PrimeX = 501125321;
    private const int PrimeY = 1136930381;
    private const int PrimeZ = 1720413743;

    [MethodImpl(INLINE)]
    private static int Hash(int seed, int xPrimed, int yPrimed)
    {
        int hash = seed ^ xPrimed ^ yPrimed;

        hash *= 0x27d4eb2d;
        return hash;
    }

    [MethodImpl(INLINE)]
    private static int Hash(int seed, int xPrimed, int yPrimed, int zPrimed)
    {
        int hash = seed ^ xPrimed ^ yPrimed ^ zPrimed;

        hash *= 0x27d4eb2d;
        return hash;
    }

    [MethodImpl(INLINE)]
    private static float ValCoord(int seed, int xPrimed, int yPrimed)
    {
        int hash = Hash(seed, xPrimed, yPrimed);

        hash *= hash;
        hash ^= hash << 19;
        return hash * (1 / 2147483648.0f);
    }

    [MethodImpl(INLINE)]
    private static float ValCoord(int seed, int xPrimed, int yPrimed, int zPrimed)
    {
        int hash = Hash(seed, xPrimed, yPrimed, zPrimed);

        hash *= hash;
        hash ^= hash << 19;
        return hash * (1 / 2147483648.0f);
    }

    [MethodImpl(INLINE)]
    private static float GradCoord(int seed, int xPrimed, int yPrimed, float xd, float yd)
    {
        int hash = Hash(seed, xPrimed, yPrimed);
        hash ^= hash >> 15;
        hash &= 127 << 1;

        float xg = Gradients2D[hash];
        float yg = Gradients2D[hash | 1];

        return xd * xg + yd * yg;
    }

    [MethodImpl(INLINE)]
    private static float GradCoord(int seed, int xPrimed, int yPrimed, int zPrimed, float xd, float yd, float zd)
    {
        int hash = Hash(seed, xPrimed, yPrimed, zPrimed);
        hash ^= hash >> 15;
        hash &= 63 << 2;

        float xg = Gradients3D[hash];
        float yg = Gradients3D[hash | 1];
        float zg = Gradients3D[hash | 2];

        return xd * xg + yd * yg + zd * zg;
    }

    [MethodImpl(INLINE)]
    private static void GradCoordOut(int seed, int xPrimed, int yPrimed, out float xo, out float yo)
    {
        int hash = Hash(seed, xPrimed, yPrimed) & (255 << 1);

        xo = RandVecs2D[hash];
        yo = RandVecs2D[hash | 1];
    }

    [MethodImpl(INLINE)]
    private static void GradCoordOut(int seed, int xPrimed, int yPrimed, int zPrimed, out float xo, out float yo, out float zo)
    {
        int hash = Hash(seed, xPrimed, yPrimed, zPrimed) & (255 << 2);

        xo = RandVecs3D[hash];
        yo = RandVecs3D[hash | 1];
        zo = RandVecs3D[hash | 2];
    }

    [MethodImpl(INLINE)]
    private static void GradCoordDual(int seed, int xPrimed, int yPrimed, float xd, float yd, out float xo, out float yo)
    {
        int hash = Hash(seed, xPrimed, yPrimed);
        int index1 = hash & (127 << 1);
        int index2 = (hash >> 7) & (255 << 1);

        float xg = Gradients2D[index1];
        float yg = Gradients2D[index1 | 1];
        float value = xd * xg + yd * yg;

        float xgo = RandVecs2D[index2];
        float ygo = RandVecs2D[index2 | 1];

        xo = value * xgo;
        yo = value * ygo;
    }

    [MethodImpl(INLINE)]
    private static void GradCoordDual(int seed, int xPrimed, int yPrimed, int zPrimed, float xd, float yd, float zd, out float xo, out float yo, out float zo)
    {
        int hash = Hash(seed, xPrimed, yPrimed, zPrimed);
        int index1 = hash & (63 << 2);
        int index2 = (hash >> 6) & (255 << 2);

        float xg = Gradients3D[index1];
        float yg = Gradients3D[index1 | 1];
        float zg = Gradients3D[index1 | 2];
        float value = xd * xg + yd * yg + zd * zg;

        float xgo = RandVecs3D[index2];
        float ygo = RandVecs3D[index2 | 1];
        float zgo = RandVecs3D[index2 | 2];

        xo = value * xgo;
        yo = value * ygo;
        zo = value * zgo;
    }


    // Generic noise gen

    private float GenNoiseSingle(int seed, FNLfloat x, FNLfloat y)
    {
        switch (mNoiseType)
        {
            case NoiseType.OpenSimplex2:
                return SingleSimplex(seed, x, y);
            case NoiseType.OpenSimplex2S:
                return SingleOpenSimplex2S(seed, x, y);
            case NoiseType.Cellular:
                return SingleCellular(seed, x, y);
            case NoiseType.Perlin:
                return SinglePerlin(seed, x, y);
            case NoiseType.ValueCubic:
                return SingleValueCubic(seed, x, y);
            case NoiseType.Value:
                return SingleValue(seed, x, y);
            default:
                return 0;
        }
    }

    private float GenNoiseSingle(int seed, FNLfloat x, FNLfloat y, FNLfloat z)
    {
        switch (mNoiseType)
        {
            case NoiseType.OpenSimplex2:
                return SingleOpenSimplex2(seed, x, y, z);
            case NoiseType.OpenSimplex2S:
                return SingleOpenSimplex2S(seed, x, y, z);
            case NoiseType.Cellular:
                return SingleCellular(seed, x, y, z);
            case NoiseType.Perlin:
                return SinglePerlin(seed, x, y, z);
            case NoiseType.ValueCubic:
                return SingleValueCubic(seed, x, y, z);
            case NoiseType.Value:
                return SingleValue(seed, x, y, z);
            default:
                return 0;
        }
    }


    // Noise Coordinate Transforms (frequency, and possible skew or rotation)

    [MethodImpl(INLINE)]
    private void TransformNoiseCoordinate(ref FNLfloat x, ref FNLfloat y)
    {
        x *= mFrequency;
        y *= mFrequency;

        switch (mNoiseType)
        {
            case NoiseType.OpenSimplex2:
            case NoiseType.OpenSimplex2S:
                {
                    const FNLfloat SQRT3 = (FNLfloat)1.7320508075688772935274463415059;
                    const FNLfloat F2 = 0.5f * (SQRT3 - 1);
                    FNLfloat t = (x + y) * F2;
                    x += t; 
                    y += t;
                }
                break;
            default:
                break;
        }
    }

    [MethodImpl(INLINE)]
    private void TransformNoiseCoordinate(ref FNLfloat x, ref FNLfloat y, ref FNLfloat z)
    {
        x *= mFrequency;
        y *= mFrequency;
        z *= mFrequency;

        switch (mTransformType3D)
        {
            case TransformType3D.ImproveXYPlanes:
                {
                    FNLfloat xy = x + y;
                    FNLfloat s2 = xy * -(FNLfloat)0.211324865405187;
                    z *= (FNLfloat)0.577350269189626;
                    x += s2 - z;
                    y = y + s2 - z;
                    z += xy * (FNLfloat)0.577350269189626;
                }
                break;
            case TransformType3D.ImproveXZPlanes:
                {
                    FNLfloat xz = x + z;
                    FNLfloat s2 = xz * -(FNLfloat)0.211324865405187;
                    y *= (FNLfloat)0.577350269189626;
                    x += s2 - y;
                    z += s2 - y;
                    y += xz * (FNLfloat)0.577350269189626;
                }
                break;
            case TransformType3D.DefaultOpenSimplex2:
                {
                    const FNLfloat R3 = (FNLfloat)(2.0 / 3.0);
                    FNLfloat r = (x + y + z) * R3; // Rotation, not skew
                    x = r - x;
                    y = r - y;
                    z = r - z;
                }
                break;
            default:
                break;
        }
    }

    private void UpdateTransformType3D()
    {
        switch (mRotationType3D)
        {
            case RotationType3D.ImproveXYPlanes:
                mTransformType3D = TransformType3D.ImproveXYPlanes;
                break;
            case RotationType3D.ImproveXZPlanes:
                mTransformType3D = TransformType3D.ImproveXZPlanes;
                break;
            default:
                switch (mNoiseType)
                {
                    case NoiseType.OpenSimplex2:
                    case NoiseType.OpenSimplex2S:
                        mTransformType3D = TransformType3D.DefaultOpenSimplex2;
                        break;
                    default:
                        mTransformType3D = TransformType3D.None;
                        break;
                }
                break;
        }
    }


    // Domain Warp Coordinate Transforms

    [MethodImpl(INLINE)]
    private void TransformDomainWarpCoordinate(ref FNLfloat x, ref FNLfloat y)
    {
        switch (mDomainWarpType)
        {
            case DomainWarpType.OpenSimplex2:
            case DomainWarpType.OpenSimplex2Reduced:
            {
                const FNLfloat SQRT3 = (FNLfloat)1.7320508075688772935274463415059;
                const FNLfloat F2 = 0.5f * (SQRT3 - 1);
                FNLfloat t = (x + y) * F2;
                x += t; y += t;
            }
            break;
            default:
                break;
        }
    }

    [MethodImpl(INLINE)]
    private void TransformDomainWarpCoordinate(ref FNLfloat x, ref FNLfloat y, ref FNLfloat z)
    {
        switch (mWarpTransformType3D)
        {
            case TransformType3D.ImproveXYPlanes:
                {
                    FNLfloat xy = x + y;
                    FNLfloat s2 = xy * -(FNLfloat)0.211324865405187;
                    z *= (FNLfloat)0.577350269189626;
                    x += s2 - z;
                    y = y + s2 - z;
                    z += xy * (FNLfloat)0.577350269189626;
                }
                break;
            case TransformType3D.ImproveXZPlanes:
                {
                    FNLfloat xz = x + z;
                    FNLfloat s2 = xz * -(FNLfloat)0.211324865405187;
                    y *= (FNLfloat)0.577350269189626;
                    x += s2 - y; z += s2 - y;
                    y += xz * (FNLfloat)0.577350269189626;
                }
                break;
            case TransformType3D.DefaultOpenSimplex2:
                {
                    const FNLfloat R3 = (FNLfloat)(2.0 / 3.0);
                    FNLfloat r = (x + y + z) * R3; // Rotation, not skew
                    x = r - x;
                    y = r - y;
                    z = r - z;
                }
                break;
            default:
                break;
        }
    }

    private void UpdateWarpTransformType3D()
    {
        switch (mRotationType3D)
        {
            case RotationType3D.ImproveXYPlanes:
                mWarpTransformType3D = TransformType3D.ImproveXYPlanes;
                break;
            case RotationType3D.ImproveXZPlanes:
                mWarpTransformType3D = TransformType3D.ImproveXZPlanes;
                break;
            default:
                switch (mDomainWarpType)
                {
                    case DomainWarpType.OpenSimplex2:
                    case DomainWarpType.OpenSimplex2Reduced:
                        mWarpTransformType3D = TransformType3D.DefaultOpenSimplex2;
                        break;
                    default:
                        mWarpTransformType3D = TransformType3D.None;
                        break;
                }
                break;
        }
    }


    // Fractal FBm

    private float GenFractalFBm(FNLfloat x, FNLfloat y)
    {
        int seed = mSeed;
        float sum = 0;
        float amp = mFractalBounding;

        for (int i = 0; i < mOctaves; i++)
        {
            float noise = GenNoiseSingle(seed++, x, y);
            sum += noise * amp;
            amp *= Lerp(1.0f, FastMin(noise + 1, 2) * 0.5f, mWeightedStrength);

            x *= mLacunarity;
            y *= mLacunarity;
            amp *= mGain;
        }

        return sum;
    }

    private float GenFractalFBm(FNLfloat x, FNLfloat y, FNLfloat z)
    {
        int seed = mSeed;
        float sum = 0;
        float amp = mFractalBounding;

        for (int i = 0; i < mOctaves; i++)
        {
            float noise = GenNoiseSingle(seed++, x, y, z);
            sum += noise * amp;
            amp *= Lerp(1.0f, (noise + 1) * 0.5f, mWeightedStrength);

            x *= mLacunarity;
            y *= mLacunarity;
            z *= mLacunarity;
            amp *= mGain;
        }

        return sum;
    }


    // Fractal Ridged

    private float GenFractalRidged(FNLfloat x, FNLfloat y)
    {
        int seed = mSeed;
        float sum = 0;
        float amp = mFractalBounding;

        for (int i = 0; i < mOctaves; i++)
        {
            float noise = FastAbs(GenNoiseSingle(seed++, x, y));
            sum += (noise * -2 + 1) * amp;
            amp *= Lerp(1.0f, 1 - noise, mWeightedStrength);

            x *= mLacunarity;
            y *= mLacunarity;
            amp *= mGain;
        }

        return sum;
    }

    private float GenFractalRidged(FNLfloat x, FNLfloat y, FNLfloat z)
    {
        int seed = mSeed;
        float sum = 0;
        float amp = mFractalBounding;

        for (int i = 0; i < mOctaves; i++)
        {
            float noise = FastAbs(GenNoiseSingle(seed++, x, y, z));
            sum += (noise * -2 + 1) * amp;
            amp *= Lerp(1.0f, 1 - noise, mWeightedStrength);

            x *= mLacunarity;
            y *= mLacunarity;
            z *= mLacunarity;
            amp *= mGain;
        }

        return sum;
    }


    // Fractal PingPong 

    private float GenFractalPingPong(FNLfloat x, FNLfloat y)
    {
        int seed = mSeed;
        float sum = 0;
        float amp = mFractalBounding;

        for (int i = 0; i < mOctaves; i++)
        {
            float noise = PingPong((GenNoiseSingle(seed++, x, y) + 1) * mPingPongStrength);
            sum += (noise - 0.5f) * 2 * amp;
            amp *= Lerp(1.0f, noise, mWeightedStrength);

            x *= mLacunarity;
            y *= mLacunarity;
            amp *= mGain;
        }

        return sum;
    }

    private float GenFractalPingPong(FNLfloat x, FNLfloat y, FNLfloat z)
    {
        int seed = mSeed;
        float sum = 0;
        float amp = mFractalBounding;

        for (int i = 0; i < mOctaves; i++)
        {
            float noise = PingPong((GenNoiseSingle(seed++, x, y, z) + 1) * mPingPongStrength);
            sum += (noise - 0.5f) * 2 * amp;
            amp *= Lerp(1.0f, noise, mWeightedStrength);

            x *= mLacunarity;
            y *= mLacunarity;
            z *= mLacunarity;
            amp *= mGain;
        }

        return sum;
    }


    // Simplex/OpenSimplex2 Noise

    private float SingleSimplex(int seed, FNLfloat x, FNLfloat y)
    {
        // 2D OpenSimplex2 case uses the same algorithm as ordinary Simplex.

        const float SQRT3 = 1.7320508075688772935274463415059f;
        const float G2 = (3 - SQRT3) / 6;

        /*
         * --- Skew moved to TransformNoiseCoordinate method ---
         * const FNfloat F2 = 0.5f * (SQRT3 - 1);
         * FNfloat s = (x + y) * F2;
         * x += s; y += s;
        */

        int i = FastFloor(x);
        int j = FastFloor(y);
        float xi = (float)(x - i);
        float yi = (float)(y - j);

        float t = (xi + yi) * G2;
        float x0 = (float)(xi - t);
        float y0 = (float)(yi - t);

        i *= PrimeX;
        j *= PrimeY;

        float n0, n1, n2;

        float a = 0.5f - x0 * x0 - y0 * y0;
        if (a <= 0) n0 = 0;
        else
        {
            n0 = (a * a) * (a * a) * GradCoord(seed, i, j, x0, y0);
        }

        float c = (float)(2 * (1 - 2 * G2) * (1 / G2 - 2)) * t + ((float)(-2 * (1 - 2 * G2) * (1 - 2 * G2)) + a);
        if (c <= 0) n2 = 0;
        else
        {
            float x2 = x0 + (2 * (float)G2 - 1);
            float y2 = y0 + (2 * (float)G2 - 1);
            n2 = (c * c) * (c * c) * GradCoord(seed, i + PrimeX, j + PrimeY, x2, y2);
        }

        if (y0 > x0)
        {
            float x1 = x0 + (float)G2;
            float y1 = y0 + ((float)G2 - 1);
            float b = 0.5f - x1 * x1 - y1 * y1;
            if (b <= 0) n1 = 0;
            else
            {
                n1 = (b * b) * (b * b) * GradCoord(seed, i, j + PrimeY, x1, y1);
            }
        }
        else
        {
            float x1 = x0 + ((float)G2 - 1);
            float y1 = y0 + (float)G2;
            float b = 0.5f - x1 * x1 - y1 * y1;
            if (b <= 0) n1 = 0;
            else
            {
                n1 = (b * b) * (b * b) * GradCoord(seed, i + PrimeX, j, x1, y1);
            }
        }

        return (n0 + n1 + n2) * 99.83685446303647f;
    }

    private float SingleOpenSimplex2(int seed, FNLfloat x, FNLfloat y, FNLfloat z)
    {
        // 3D OpenSimplex2 case uses two offset rotated cube grids.

        /*
         * --- Rotation moved to TransformNoiseCoordinate method ---
         * const FNfloat R3 = (FNfloat)(2.0 / 3.0);
         * FNfloat r = (x + y + z) * R3; // Rotation, not skew
         * x = r - x; y = r - y; z = r - z;
        */

        int i = FastRound(x);
        int j = FastRound(y);
        int k = FastRound(z);
        float x0 = (float)(x - i);
        float y0 = (float)(y - j);
        float z0 = (float)(z - k);

        int xNSign = (int)(-1.0f - x0) | 1;
        int yNSign = (int)(-1.0f - y0) | 1;
        int zNSign = (int)(-1.0f - z0) | 1;

        float ax0 = xNSign * -x0;
        float ay0 = yNSign * -y0;
        float az0 = zNSign * -z0;

        i *= PrimeX;
        j *= PrimeY;
        k *= PrimeZ;

        float value = 0;
        float a = (0.6f - x0 * x0) - (y0 * y0 + z0 * z0);

        for (int l = 0; ; l++)
        {
            if (a > 0)
            {
                value += (a * a) * (a * a) * GradCoord(seed, i, j, k, x0, y0, z0);
            }

            if (ax0 >= ay0 && ax0 >= az0)
            {
                float b = a + ax0 + ax0;
                if (b > 1) {
                    b -= 1;
                    value += (b * b) * (b * b) * GradCoord(seed, i - xNSign * PrimeX, j, k, x0 + xNSign, y0, z0);
                }
            }
            else if (ay0 > ax0 && ay0 >= az0)
            {
                float b = a + ay0 + ay0;
                if (b > 1)
                {
                    b -= 1;
                    value += (b * b) * (b * b) * GradCoord(seed, i, j - yNSign * PrimeY, k, x0, y0 + yNSign, z0);
                }
            }
            else
            {
                float b = a + az0 + az0;
                if (b > 1)
                {
                    b -= 1;
                    value += (b * b) * (b * b) * GradCoord(seed, i, j, k - zNSign * PrimeZ, x0, y0, z0 + zNSign);
                }
            }

            if (l == 1) break;

            ax0 = 0.5f - ax0;
            ay0 = 0.5f - ay0;
            az0 = 0.5f - az0;

            x0 = xNSign * ax0;
            y0 = yNSign * ay0;
            z0 = zNSign * az0;

            a += (0.75f - ax0) - (ay0 + az0);

            i += (xNSign >> 1) & PrimeX;
            j += (yNSign >> 1) & PrimeY;
            k += (zNSign >> 1) & PrimeZ;

            xNSign = -xNSign;
            yNSign = -yNSign;
            zNSign = -zNSign;

            seed = ~seed;
        }

        return value * 32.69428253173828125f;
    }


    // OpenSimplex2S Noise

    private float SingleOpenSimplex2S(int seed, FNLfloat x, FNLfloat y)
    {
        // 2D OpenSimplex2S case is a modified 2D simplex noise.

        const FNLfloat SQRT3 = (FNLfloat)1.7320508075688772935274463415059;
        const FNLfloat G2 = (3 - SQRT3) / 6;

        /*
         * --- Skew moved to TransformNoiseCoordinate method ---
         * const FNfloat F2 = 0.5f * (SQRT3 - 1);
         * FNfloat s = (x + y) * F2;
         * x += s; y += s;
        */

        int i = FastFloor(x);
        int j = FastFloor(y);
        float xi = (float)(x - i);
        float yi = (float)(y - j);

        i *= PrimeX;
        j *= PrimeY;
        int i1 = i + PrimeX;
        int j1 = j + PrimeY;

        float t = (xi + yi) * (float)G2;
        float x0 = xi - t;
        float y0 = yi - t;

        float a0 = (2.0f / 3.0f) - x0 * x0 - y0 * y0;
        float value = (a0 * a0) * (a0 * a0) * GradCoord(seed, i, j, x0, y0);

        float a1 = (float)(2 * (1 - 2 * G2) * (1 / G2 - 2)) * t + ((float)(-2 * (1 - 2 * G2) * (1 - 2 * G2)) + a0);
        float x1 = x0 - (float)(1 - 2 * G2);
        float y1 = y0 - (float)(1 - 2 * G2);
        value += (a1 * a1) * (a1 * a1) * GradCoord(seed, i1, j1, x1, y1);

        // Nested conditionals were faster than compact bit logic/arithmetic.
        float xmyi = xi - yi;
        if (t > G2)
        {
            if (xi + xmyi > 1)
            {
                float x2 = x0 + (float)(3 * G2 - 2);
                float y2 = y0 + (float)(3 * G2 - 1);
                float a2 = (2.0f / 3.0f) - x2 * x2 - y2 * y2;
                if (a2 > 0)
                {
                    value += (a2 * a2) * (a2 * a2) * GradCoord(seed, i + (PrimeX << 1), j + PrimeY, x2, y2);
                }
            }
            else
            {
                float x2 = x0 + (float)G2;
                float y2 = y0 + (float)(G2 - 1);
                float a2 = (2.0f / 3.0f) - x2 * x2 - y2 * y2;
                if (a2 > 0)
                {
                    value += (a2 * a2) * (a2 * a2) * GradCoord(seed, i, j + PrimeY, x2, y2);
                }
            }

            if (yi - xmyi > 1)
            {
                float x3 = x0 + (float)(3 * G2 - 1);
                float y3 = y0 + (float)(3 * G2 - 2);
                float a3 = (2.0f / 3.0f) - x3 * x3 - y3 * y3;
                if (a3 > 0)
                {
                    value += (a3 * a3) * (a3 * a3) * GradCoord(seed, i + PrimeX, j + (PrimeY << 1), x3, y3);
                }
            }
            else
            {
                float x3 = x0 + (float)(G2 - 1);
                float y3 = y0 + (float)G2;
                float a3 = (2.0f / 3.0f) - x3 * x3 - y3 * y3;
                if (a3 > 0)
                {
                    value += (a3 * a3) * (a3 * a3) * GradCoord(seed, i + PrimeX, j, x3, y3);
                }
            }
        }
        else
        {
            if (xi + xmyi < 0)
            {
                float x2 = x0 + (float)(1 - G2);
                float y2 = y0 - (float)G2;
                float a2 = (2.0f / 3.0f) - x2 * x2 - y2 * y2;
                if (a2 > 0)
                {
                    value += (a2 * a2) * (a2 * a2) * GradCoord(seed, i - PrimeX, j, x2, y2);
                }
            }
            else
            {
                float x2 = x0 + (float)(G2 - 1);
                float y2 = y0 + (float)G2;
                float a2 = (2.0f / 3.0f) - x2 * x2 - y2 * y2;
                if (a2 > 0)
                {
                    value += (a2 * a2) * (a2 * a2) * GradCoord(seed, i + PrimeX, j, x2, y2);
                }
            }

            if (yi < xmyi)
            {
                float x2 = x0 - (float)G2;
                float y2 = y0 - (float)(G2 - 1);
                float a2 = (2.0f / 3.0f) - x2 * x2 - y2 * y2;
                if (a2 > 0)
                {
                    value += (a2 * a2) * (a2 * a2) * GradCoord(seed, i, j - PrimeY, x2, y2);
                }
            }
            else
            {
                float x2 = x0 + (float)G2;
                float y2 = y0 + (float)(G2 - 1);
                float a2 = (2.0f / 3.0f) - x2 * x2 - y2 * y2;
                if (a2 > 0)
                {
                    value += (a2 * a2) * (a2 * a2) * GradCoord(seed, i, j + PrimeY, x2, y2);
                }
            }
        }

        return value * 18.24196194486065f;
    }

    private float SingleOpenSimplex2S(int seed, FNLfloat x, FNLfloat y, FNLfloat z)
    {
        // 3D OpenSimplex2S case uses two offset rotated cube grids.

        /*
         * --- Rotation moved to TransformNoiseCoordinate method ---
         * const FNfloat R3 = (FNfloat)(2.0 / 3.0);
         * FNfloat r = (x + y + z) * R3; // Rotation, not skew
         * x = r - x; y = r - y; z = r - z;
        */

        int i = FastFloor(x);
        int j = FastFloor(y);
        int k = FastFloor(z);
        float xi = (float)(x - i);
        float yi = (float)(y - j);
        float zi = (float)(z - k);

        i *= PrimeX;
        j *= PrimeY;
        k *= PrimeZ;
        int seed2 = seed + 1293373;

        int xNMask = (int)(-0.5f - xi);
        int yNMask = (int)(-0.5f - yi);
        int zNMask = (int)(-0.5f - zi);

        float x0 = xi + xNMask;
        float y0 = yi + yNMask;
        float z0 = zi + zNMask;
        float a0 = 0.75f - x0 * x0 - y0 * y0 - z0 * z0;
        float value = (a0 * a0) * (a0 * a0) * GradCoord(seed,
            i + (xNMask & PrimeX), j + (yNMask & PrimeY), k + (zNMask & PrimeZ), x0, y0, z0);

        float x1 = xi - 0.5f;
        float y1 = yi - 0.5f;
        float z1 = zi - 0.5f;
        float a1 = 0.75f - x1 * x1 - y1 * y1 - z1 * z1;
        value += (a1 * a1) * (a1 * a1) * GradCoord(seed2,
            i + PrimeX, j + PrimeY, k + PrimeZ, x1, y1, z1);

        float xAFlipMask0 = ((xNMask | 1) << 1) * x1;
        float yAFlipMask0 = ((yNMask | 1) << 1) * y1;
        float zAFlipMask0 = ((zNMask | 1) << 1) * z1;
        float xAFlipMask1 = (-2 - (xNMask << 2)) * x1 - 1.0f;
        float yAFlipMask1 = (-2 - (yNMask << 2)) * y1 - 1.0f;
        float zAFlipMask1 = (-2 - (zNMask << 2)) * z1 - 1.0f;

        bool skip5 = false;
        float a2 = xAFlipMask0 + a0;
        if (a2 > 0)
        {
            float x2 = x0 - (xNMask | 1);
            float y2 = y0;
            float z2 = z0;
            value += (a2 * a2) * (a2 * a2) * GradCoord(seed,
                i + (~xNMask & PrimeX), j + (yNMask & PrimeY), k + (zNMask & PrimeZ), x2, y2, z2);
        }
        else
        {
            float a3 = yAFlipMask0 + zAFlipMask0 + a0;
            if (a3 > 0)
            {
                float x3 = x0;
                float y3 = y0 - (yNMask | 1);
                float z3 = z0 - (zNMask | 1);
                value += (a3 * a3) * (a3 * a3) * GradCoord(seed,
                    i + (xNMask & PrimeX), j + (~yNMask & PrimeY), k + (~zNMask & PrimeZ), x3, y3, z3);
            }

            float a4 = xAFlipMask1 + a1;
            if (a4 > 0)
            {
                float x4 = (xNMask | 1) + x1;
                float y4 = y1;
                float z4 = z1;
                value += (a4 * a4) * (a4 * a4) * GradCoord(seed2,
                    i + (xNMask & (PrimeX * 2)), j + PrimeY, k + PrimeZ, x4, y4, z4);
                skip5 = true;
            }
        }

        bool skip9 = false;
        float a6 = yAFlipMask0 + a0;
        if (a6 > 0)
        {
            float x6 = x0;
            float y6 = y0 - (yNMask | 1);
            float z6 = z0;
            value += (a6 * a6) * (a6 * a6) * GradCoord(seed,
                i + (xNMask & PrimeX), j + (~yNMask & PrimeY), k + (zNMask & PrimeZ), x6, y6, z6);
        }
        else
        {
            float a7 = xAFlipMask0 + zAFlipMask0 + a0;
            if (a7 > 0)
            {
                float x7 = x0 - (xNMask | 1);
                float y7 = y0;
                float z7 = z0 - (zNMask | 1);
                value += (a7 * a7) * (a7 * a7) * GradCoord(seed,
                    i + (~xNMask & PrimeX), j + (yNMask & PrimeY), k + (~zNMask & PrimeZ), x7, y7, z7);
            }

            float a8 = yAFlipMask1 + a1;
            if (a8 > 0)
            {
                float x8 = x1;
                float y8 = (yNMask | 1) + y1;
                float z8 = z1;
                value += (a8 * a8) * (a8 * a8) * GradCoord(seed2,
                    i + PrimeX, j + (yNMask & (PrimeY << 1)), k + PrimeZ, x8, y8, z8);
                skip9 = true;
            }
        }

        bool skipD = false;
        float aA = zAFlipMask0 + a0;
        if (aA > 0)
        {
            float xA = x0;
            float yA = y0;
            float zA = z0 - (zNMask | 1);
            value += (aA * aA) * (aA * aA) * GradCoord(seed,
                i + (xNMask & PrimeX), j + (yNMask & PrimeY), k + (~zNMask & PrimeZ), xA, yA, zA);
        }
        else
        {
            float aB = xAFlipMask0 + yAFlipMask0 + a0;
            if (aB > 0)
            {
                float xB = x0 - (xNMask | 1);
                float yB = y0 - (yNMask | 1);
                float zB = z0;
                value += (aB * aB) * (aB * aB) * GradCoord(seed,
                    i + (~xNMask & PrimeX), j + (~yNMask & PrimeY), k + (zNMask & PrimeZ), xB, yB, zB);
            }

            float aC = zAFlipMask1 + a1;
            if (aC > 0)
            {
                float xC = x1;
                float yC = y1;
                float zC = (zNMask | 1) + z1;
                value += (aC * aC) * (aC * aC) * GradCoord(seed2,
                    i + PrimeX, j + PrimeY, k + (zNMask & (PrimeZ << 1)), xC, yC, zC);
                skipD = true;
            }
        }

        if (!skip5)
        {
            float a5 = yAFlipMask1 + zAFlipMask1 + a1;
            if (a5 > 0)
            {
                float x5 = x1;
                float y5 = (yNMask | 1) + y1;
                float z5 = (zNMask | 1) + z1;
                value += (a5 * a5) * (a5 * a5) * GradCoord(seed2,
                    i + PrimeX, j + (yNMask & (PrimeY << 1)), k + (zNMask & (PrimeZ << 1)), x5, y5, z5);
            }
        }

        if (!skip9)
        {
            float a9 = xAFlipMask1 + zAFlipMask1 + a1;
            if (a9 > 0)
            {
                float x9 = (xNMask | 1) + x1;
                float y9 = y1;
                float z9 = (zNMask | 1) + z1;
                value += (a9 * a9) * (a9 * a9) * GradCoord(seed2,
                    i + (xNMask & (PrimeX * 2)), j + PrimeY, k + (zNMask & (PrimeZ << 1)), x9, y9, z9);
            }
        }

        if (!skipD)
        {
            float aD = xAFlipMask1 + yAFlipMask1 + a1;
            if (aD > 0)
            {
                float xD = (xNMask | 1) + x1;
                float yD = (yNMask | 1) + y1;
                float zD = z1;
                value += (aD * aD) * (aD * aD) * GradCoord(seed2,
                    i + (xNMask & (PrimeX << 1)), j + (yNMask & (PrimeY << 1)), k + PrimeZ, xD, yD, zD);
            }
        }

        return value * 9.046026385208288f;
    }


    // Cellular Noise

    private float SingleCellular(int seed, FNLfloat x, FNLfloat y)
    {
        int xr = FastRound(x);
        int yr = FastRound(y);

        float distance0 = float.MaxValue;
        float distance1 = float.MaxValue;
        int closestHash = 0;

        float cellularJitter = 0.43701595f * mCellularJitterModifier;

        int xPrimed = (xr - 1) * PrimeX;
        int yPrimedBase = (yr - 1) * PrimeY;

        switch (mCellularDistanceFunction)
        {
            default:
            case CellularDistanceFunction.Euclidean:
            case CellularDistanceFunction.EuclideanSq:
                for (int xi = xr - 1; xi <= xr + 1; xi++)
                {
                    int yPrimed = yPrimedBase;

                    for (int yi = yr - 1; yi <= yr + 1; yi++)
                    {
                        int hash = Hash(seed, xPrimed, yPrimed);
                        int idx = hash & (255 << 1);

                        float vecX = (float)(xi - x) + RandVecs2D[idx] * cellularJitter;
                        float vecY = (float)(yi - y) + RandVecs2D[idx | 1] * cellularJitter;

                        float newDistance = vecX * vecX + vecY * vecY;

                        distance1 = FastMax(FastMin(distance1, newDistance), distance0);
                        if (newDistance < distance0)
                        {
                            distance0 = newDistance;
                            closestHash = hash;
                        }
                        yPrimed += PrimeY;
                    }
                    xPrimed += PrimeX;
                }
                break;
            case CellularDistanceFunction.Manhattan:
                for (int xi = xr - 1; xi <= xr + 1; xi++)
                {
                    int yPrimed = yPrimedBase;

                    for (int yi = yr - 1; yi <= yr + 1; yi++)
                    {
                        int hash = Hash(seed, xPrimed, yPrimed);
                        int idx = hash & (255 << 1);

                        float vecX = (float)(xi - x) + RandVecs2D[idx] * cellularJitter;
                        float vecY = (float)(yi - y) + RandVecs2D[idx | 1] * cellularJitter;

                        float newDistance = FastAbs(vecX) + FastAbs(vecY);

                        distance1 = FastMax(FastMin(distance1, newDistance), distance0);
                        if (newDistance < distance0)
                        {
                            distance0 = newDistance;
                            closestHash = hash;
                        }
                        yPrimed += PrimeY;
                    }
                    xPrimed += PrimeX;
                }
                break;
            case CellularDistanceFunction.Hybrid:
                for (int xi = xr - 1; xi <= xr + 1; xi++)
                {
                    int yPrimed = yPrimedBase;

                    for (int yi = yr - 1; yi <= yr + 1; yi++)
                    {
                        int hash = Hash(seed, xPrimed, yPrimed);
                        int idx = hash & (255 << 1);

                        float vecX = (float)(xi - x) + RandVecs2D[idx] * cellularJitter;
                        float vecY = (float)(yi - y) + RandVecs2D[idx | 1] * cellularJitter;

                        float newDistance = (FastAbs(vecX) + FastAbs(vecY)) + (vecX * vecX + vecY * vecY);

                        distance1 = FastMax(FastMin(distance1, newDistance), distance0);
                        if (newDistance < distance0)
                        {
                            distance0 = newDistance;
                            closestHash = hash;
                        }
                        yPrimed += PrimeY;
                    }
                    xPrimed += PrimeX;
                }
                break;
        }

        if (mCellularDistanceFunction == CellularDistanceFunction.Euclidean && mCellularReturnType >= CellularReturnType.Distance)
        {
            distance0 = FastSqrt(distance0);

            if (mCellularReturnType >= CellularReturnType.Distance2)
            {
                distance1 = FastSqrt(distance1);
            }
        }

        switch (mCellularReturnType)
        {
            case CellularReturnType.CellValue:
                return closestHash * (1 / 2147483648.0f);
            case CellularReturnType.Distance:
                return distance0 - 1;
            case CellularReturnType.Distance2:
                return distance1 - 1;
            case CellularReturnType.Distance2Add:
                return (distance1 + distance0) * 0.5f - 1;
            case CellularReturnType.Distance2Sub:
                return distance1 - distance0 - 1;
            case CellularReturnType.Distance2Mul:
                return distance1 * distance0 * 0.5f - 1;
            case CellularReturnType.Distance2Div:
                return distance0 / distance1 - 1;
            default:
                return 0;
        }
    }

    private float SingleCellular(int seed, FNLfloat x, FNLfloat y, FNLfloat z)
    {
        int xr = FastRound(x);
        int yr = FastRound(y);
        int zr = FastRound(z);

        float distance0 = float.MaxValue;
        float distance1 = float.MaxValue;
        int closestHash = 0;

        float cellularJitter = 0.39614353f * mCellularJitterModifier;

        int xPrimed = (xr - 1) * PrimeX;
        int yPrimedBase = (yr - 1) * PrimeY;
        int zPrimedBase = (zr - 1) * PrimeZ;

        switch (mCellularDistanceFunction)
        {
            case CellularDistanceFunction.Euclidean:
            case CellularDistanceFunction.EuclideanSq:
                for (int xi = xr - 1; xi <= xr + 1; xi++)
                {
                    int yPrimed = yPrimedBase;

                    for (int yi = yr - 1; yi <= yr + 1; yi++)
                    {
                        int zPrimed = zPrimedBase;

                        for (int zi = zr - 1; zi <= zr + 1; zi++)
                        {
                            int hash = Hash(seed, xPrimed, yPrimed, zPrimed);
                            int idx = hash & (255 << 2);

                            float vecX = (float)(xi - x) + RandVecs3D[idx] * cellularJitter;
                            float vecY = (float)(yi - y) + RandVecs3D[idx | 1] * cellularJitter;
                            float vecZ = (float)(zi - z) + RandVecs3D[idx | 2] * cellularJitter;

                            float newDistance = vecX * vecX + vecY * vecY + vecZ * vecZ;

                            distance1 = FastMax(FastMin(distance1, newDistance), distance0);
                            if (newDistance < distance0)
                            {
                                distance0 = newDistance;
                                closestHash = hash;
                            }
                            zPrimed += PrimeZ;
                        }
                        yPrimed += PrimeY;
                    }
                    xPrimed += PrimeX;
                }
                break;
            case CellularDistanceFunction.Manhattan:
                for (int xi = xr - 1; xi <= xr + 1; xi++)
                {
                    int yPrimed = yPrimedBase;

                    for (int yi = yr - 1; yi <= yr + 1; yi++)
                    {
                        int zPrimed = zPrimedBase;

                        for (int zi = zr - 1; zi <= zr + 1; zi++)
                        {
                            int hash = Hash(seed, xPrimed, yPrimed, zPrimed);
                            int idx = hash & (255 << 2);

                            float vecX = (float)(xi - x) + RandVecs3D[idx] * cellularJitter;
                            float vecY = (float)(yi - y) + RandVecs3D[idx | 1] * cellularJitter;
                            float vecZ = (float)(zi - z) + RandVecs3D[idx | 2] * cellularJitter;

                            float newDistance = FastAbs(vecX) + FastAbs(vecY) + FastAbs(vecZ);

                            distance1 = FastMax(FastMin(distance1, newDistance), distance0);
                            if (newDistance < distance0)
                            {
                                distance0 = newDistance;
                                closestHash = hash;
                            }
                            zPrimed += PrimeZ;
                        }
                        yPrimed += PrimeY;
                    }
                    xPrimed += PrimeX;
                }
                break;
            case CellularDistanceFunction.Hybrid:
                for (int xi = xr - 1; xi <= xr + 1; xi++)
                {
                    int yPrimed = yPrimedBase;

                    for (int yi = yr - 1; yi <= yr + 1; yi++)
                    {
                        int zPrimed = zPrimedBase;

                        for (int zi = zr - 1; zi <= zr + 1; zi++)
                        {
                            int hash = Hash(seed, xPrimed, yPrimed, zPrimed);
                            int idx = hash & (255 << 2);

                            float vecX = (float)(xi - x) + RandVecs3D[idx] * cellularJitter;
                            float vecY = (float)(yi - y) + RandVecs3D[idx | 1] * cellularJitter;
                            float vecZ = (float)(zi - z) + RandVecs3D[idx | 2] * cellularJitter;

                            float newDistance = (FastAbs(vecX) + FastAbs(vecY) + FastAbs(vecZ)) + (vecX * vecX + vecY * vecY + vecZ * vecZ);

                            distance1 = FastMax(FastMin(distance1, newDistance), distance0);
                            if (newDistance < distance0)
                            {
                                distance0 = newDistance;
                                closestHash = hash;
                            }
                            zPrimed += PrimeZ;
                        }
                        yPrimed += PrimeY;
                    }
                    xPrimed += PrimeX;
                }
                break;
            default:
                break;
        }

        if (mCellularDistanceFunction == CellularDistanceFunction.Euclidean && mCellularReturnType >= CellularReturnType.Distance)
        {
            distance0 = FastSqrt(distance0);

            if (mCellularReturnType >= CellularReturnType.Distance2)
            {
                distance1 = FastSqrt(distance1);
            }
        }

        switch (mCellularReturnType)
        {
            case CellularReturnType.CellValue:
                return closestHash * (1 / 2147483648.0f);
            case CellularReturnType.Distance:
                return distance0 - 1;
            case CellularReturnType.Distance2:
                return distance1 - 1;
            case CellularReturnType.Distance2Add:
                return (distance1 + distance0) * 0.5f - 1;
            case CellularReturnType.Distance2Sub:
                return distance1 - distance0 - 1;
            case CellularReturnType.Distance2Mul:
                return distance1 * distance0 * 0.5f - 1;
            case CellularReturnType.Distance2Div:
                return distance0 / distance1 - 1;
            default:
                return 0;
        }
    }


    // Perlin Noise

    private float SinglePerlin(int seed, FNLfloat x, FNLfloat y)
    {
        int x0 = FastFloor(x);
        int y0 = FastFloor(y);

        float xd0 = (float)(x - x0);
        float yd0 = (float)(y - y0);
        float xd1 = xd0 - 1;
        float yd1 = yd0 - 1;

        float xs = InterpQuintic(xd0);
        float ys = InterpQuintic(yd0);

        x0 *= PrimeX;
        y0 *= PrimeY;
        int x1 = x0 + PrimeX;
        int y1 = y0 + PrimeY;

        float xf0 = Lerp(GradCoord(seed, x0, y0, xd0, yd0), GradCoord(seed, x1, y0, xd1, yd0), xs);
        float xf1 = Lerp(GradCoord(seed, x0, y1, xd0, yd1), GradCoord(seed, x1, y1, xd1, yd1), xs);

        return Lerp(xf0, xf1, ys) * 1.4247691104677813f;
    }

    private float SinglePerlin(int seed, FNLfloat x, FNLfloat y, FNLfloat z)
    {
        int x0 = FastFloor(x);
        int y0 = FastFloor(y);
        int z0 = FastFloor(z);

        float xd0 = (float)(x - x0);
        float yd0 = (float)(y - y0);
        float zd0 = (float)(z - z0);
        float xd1 = xd0 - 1;
        float yd1 = yd0 - 1;
        float zd1 = zd0 - 1;

        float xs = InterpQuintic(xd0);
        float ys = InterpQuintic(yd0);
        float zs = InterpQuintic(zd0);

        x0 *= PrimeX;
        y0 *= PrimeY;
        z0 *= PrimeZ;
        int x1 = x0 + PrimeX;
        int y1 = y0 + PrimeY;
        int z1 = z0 + PrimeZ;

        float xf00 = Lerp(GradCoord(seed, x0, y0, z0, xd0, yd0, zd0), GradCoord(seed, x1, y0, z0, xd1, yd0, zd0), xs);
        float xf10 = Lerp(GradCoord(seed, x0, y1, z0, xd0, yd1, zd0), GradCoord(seed, x1, y1, z0, xd1, yd1, zd0), xs);
        float xf01 = Lerp(GradCoord(seed, x0, y0, z1, xd0, yd0, zd1), GradCoord(seed, x1, y0, z1, xd1, yd0, zd1), xs);
        float xf11 = Lerp(GradCoord(seed, x0, y1, z1, xd0, yd1, zd1), GradCoord(seed, x1, y1, z1, xd1, yd1, zd1), xs);

        float yf0 = Lerp(xf00, xf10, ys);
        float yf1 = Lerp(xf01, xf11, ys);

        return Lerp(yf0, yf1, zs) * 0.964921414852142333984375f;
    }


    // Value Cubic Noise

    private float SingleValueCubic(int seed, FNLfloat x, FNLfloat y)
    {
        int x1 = FastFloor(x);
        int y1 = FastFloor(y);

        float xs = (float)(x - x1);
        float ys = (float)(y - y1);

        x1 *= PrimeX;
        y1 *= PrimeY;
        int x0 = x1 - PrimeX;
        int y0 = y1 - PrimeY;
        int x2 = x1 + PrimeX;
        int y2 = y1 + PrimeY;
        int x3 = x1 + unchecked(PrimeX * 2);
        int y3 = y1 + unchecked(PrimeY * 2);

        return CubicLerp(
            CubicLerp(ValCoord(seed, x0, y0), ValCoord(seed, x1, y0), ValCoord(seed, x2, y0), ValCoord(seed, x3, y0),
            xs),
            CubicLerp(ValCoord(seed, x0, y1), ValCoord(seed, x1, y1), ValCoord(seed, x2, y1), ValCoord(seed, x3, y1),
            xs),
            CubicLerp(ValCoord(seed, x0, y2), ValCoord(seed, x1, y2), ValCoord(seed, x2, y2), ValCoord(seed, x3, y2),
            xs),
            CubicLerp(ValCoord(seed, x0, y3), ValCoord(seed, x1, y3), ValCoord(seed, x2, y3), ValCoord(seed, x3, y3),
            xs),
            ys) * (1 / (1.5f * 1.5f));
    }

    private float SingleValueCubic(int seed, FNLfloat x, FNLfloat y, FNLfloat z)
    {
        int x1 = FastFloor(x);
        int y1 = FastFloor(y);
        int z1 = FastFloor(z);

        float xs = (float)(x - x1);
        float ys = (float)(y - y1);
        float zs = (float)(z - z1);

        x1 *= PrimeX;
        y1 *= PrimeY;
        z1 *= PrimeZ;

        int x0 = x1 - PrimeX;
        int y0 = y1 - PrimeY;
        int z0 = z1 - PrimeZ;
        int x2 = x1 + PrimeX;
        int y2 = y1 + PrimeY;
        int z2 = z1 + PrimeZ;
        int x3 = x1 + unchecked(PrimeX * 2);
        int y3 = y1 + unchecked(PrimeY * 2);
        int z3 = z1 + unchecked(PrimeZ * 2);


        return CubicLerp(
            CubicLerp(
            CubicLerp(ValCoord(seed, x0, y0, z0), ValCoord(seed, x1, y0, z0), ValCoord(seed, x2, y0, z0), ValCoord(seed, x3, y0, z0), xs),
            CubicLerp(ValCoord(seed, x0, y1, z0), ValCoord(seed, x1, y1, z0), ValCoord(seed, x2, y1, z0), ValCoord(seed, x3, y1, z0), xs),
            CubicLerp(ValCoord(seed, x0, y2, z0), ValCoord(seed, x1, y2, z0), ValCoord(seed, x2, y2, z0), ValCoord(seed, x3, y2, z0), xs),
            CubicLerp(ValCoord(seed, x0, y3, z0), ValCoord(seed, x1, y3, z0), ValCoord(seed, x2, y3, z0), ValCoord(seed, x3, y3, z0), xs),
            ys),
            CubicLerp(
            CubicLerp(ValCoord(seed, x0, y0, z1), ValCoord(seed, x1, y0, z1), ValCoord(seed, x2, y0, z1), ValCoord(seed, x3, y0, z1), xs),
            CubicLerp(ValCoord(seed, x0, y1, z1), ValCoord(seed, x1, y1, z1), ValCoord(seed, x2, y1, z1), ValCoord(seed, x3, y1, z1), xs),
            CubicLerp(ValCoord(seed, x0, y2, z1), ValCoord(seed, x1, y2, z1), ValCoord(seed, x2, y2, z1), ValCoord(seed, x3, y2, z1), xs),
            CubicLerp(ValCoord(seed, x0, y3, z1), ValCoord(seed, x1, y3, z1), ValCoord(seed, x2, y3, z1), ValCoord(seed, x3, y3, z1), xs),
            ys),
            CubicLerp(
            CubicLerp(ValCoord(seed, x0, y0, z2), ValCoord(seed, x1, y0, z2), ValCoord(seed, x2, y0, z2), ValCoord(seed, x3, y0, z2), xs),
            CubicLerp(ValCoord(seed, x0, y1, z2), ValCoord(seed, x1, y1, z2), ValCoord(seed, x2, y1, z2), ValCoord(seed, x3, y1, z2), xs),
            CubicLerp(ValCoord(seed, x0, y2, z2), ValCoord(seed, x1, y2, z2), ValCoord(seed, x2, y2, z2), ValCoord(seed, x3, y2, z2), xs),
            CubicLerp(ValCoord(seed, x0, y3, z2), ValCoord(seed, x1, y3, z2), ValCoord(seed, x2, y3, z2), ValCoord(seed, x3, y3, z2), xs),
            ys),
            CubicLerp(
            CubicLerp(ValCoord(seed, x0, y0, z3), ValCoord(seed, x1, y0, z3), ValCoord(seed, x2, y0, z3), ValCoord(seed, x3, y0, z3), xs),
            CubicLerp(ValCoord(seed, x0, y1, z3), ValCoord(seed, x1, y1, z3), ValCoord(seed, x2, y1, z3), ValCoord(seed, x3, y1, z3), xs),
            CubicLerp(ValCoord(seed, x0, y2, z3), ValCoord(seed, x1, y2, z3), ValCoord(seed, x2, y2, z3), ValCoord(seed, x3, y2, z3), xs),
            CubicLerp(ValCoord(seed, x0, y3, z3), ValCoord(seed, x1, y3, z3), ValCoord(seed, x2, y3, z3), ValCoord(seed, x3, y3, z3), xs),
            ys),
            zs) * (1 / (1.5f * 1.5f * 1.5f));
    }


    // Value Noise

    private float SingleValue(int seed, FNLfloat x, FNLfloat y)
    {
        int x0 = FastFloor(x);
        int y0 = FastFloor(y);

        float xs = InterpHermite((float)(x - x0));
        float ys = InterpHermite((float)(y - y0));

        x0 *= PrimeX;
        y0 *= PrimeY;
        int x1 = x0 + PrimeX;
        int y1 = y0 + PrimeY;

        float xf0 = Lerp(ValCoord(seed, x0, y0), ValCoord(seed, x1, y0), xs);
        float xf1 = Lerp(ValCoord(seed, x0, y1), ValCoord(seed, x1, y1), xs);

        return Lerp(xf0, xf1, ys);
    }

    private float SingleValue(int seed, FNLfloat x, FNLfloat y, FNLfloat z)
    {
        int x0 = FastFloor(x);
        int y0 = FastFloor(y);
        int z0 = FastFloor(z);

        float xs = InterpHermite((float)(x - x0));
        float ys = InterpHermite((float)(y - y0));
        float zs = InterpHermite((float)(z - z0));

        x0 *= PrimeX;
        y0 *= PrimeY;
        z0 *= PrimeZ;
        int x1 = x0 + PrimeX;
        int y1 = y0 + PrimeY;
        int z1 = z0 + PrimeZ;

        float xf00 = Lerp(ValCoord(seed, x0, y0, z0), ValCoord(seed, x1, y0, z0), xs);
        float xf10 = Lerp(ValCoord(seed, x0, y1, z0), ValCoord(seed, x1, y1, z0), xs);
        float xf01 = Lerp(ValCoord(seed, x0, y0, z1), ValCoord(seed, x1, y0, z1), xs);
        float xf11 = Lerp(ValCoord(seed, x0, y1, z1), ValCoord(seed, x1, y1, z1), xs);

        float yf0 = Lerp(xf00, xf10, ys);
        float yf1 = Lerp(xf01, xf11, ys);

        return Lerp(yf0, yf1, zs);
    }


    // Domain Warp

    private void DoSingleDomainWarp(int seed, float amp, float freq, FNLfloat x, FNLfloat y, ref FNLfloat xr, ref FNLfloat yr)
    {
        switch (mDomainWarpType)
        {
            case DomainWarpType.OpenSimplex2:
                SingleDomainWarpSimplexGradient(seed, amp * 38.283687591552734375f, freq, x, y, ref xr, ref yr, false);
                break;
            case DomainWarpType.OpenSimplex2Reduced:
                SingleDomainWarpSimplexGradient(seed, amp * 16.0f, freq, x, y, ref xr, ref yr, true);
                break;
            case DomainWarpType.BasicGrid:
                SingleDomainWarpBasicGrid(seed, amp, freq, x, y, ref xr, ref yr);
                break;
        }
    }

    private void DoSingleDomainWarp(int seed, float amp, float freq, FNLfloat x, FNLfloat y, FNLfloat z, ref FNLfloat xr, ref FNLfloat yr, ref FNLfloat zr)
    {
        switch (mDomainWarpType)
        {
            case DomainWarpType.OpenSimplex2:
                SingleDomainWarpOpenSimplex2Gradient(seed, amp * 32.69428253173828125f, freq, x, y, z, ref xr, ref yr, ref zr, false);
                break;
            case DomainWarpType.OpenSimplex2Reduced:
                SingleDomainWarpOpenSimplex2Gradient(seed, amp * 7.71604938271605f, freq, x, y, z, ref xr, ref yr, ref zr, true);
                break;
            case DomainWarpType.BasicGrid:
                SingleDomainWarpBasicGrid(seed, amp, freq, x, y, z, ref xr, ref yr, ref zr);
                break;
        }
    }


    // Domain Warp Single Wrapper

    private void DomainWarpSingle(ref FNLfloat x, ref FNLfloat y)
    {
        int seed = mSeed;
        float amp = mDomainWarpAmp * mFractalBounding;
        float freq = mFrequency;

        FNLfloat xs = x;
        FNLfloat ys = y;
        TransformDomainWarpCoordinate(ref xs, ref ys);

        DoSingleDomainWarp(seed, amp, freq, xs, ys, ref x, ref y);
    }

    private void DomainWarpSingle(ref FNLfloat x, ref FNLfloat y, ref FNLfloat z)
    {
        int seed = mSeed;
        float amp = mDomainWarpAmp * mFractalBounding;
        float freq = mFrequency;

        FNLfloat xs = x;
        FNLfloat ys = y;
        FNLfloat zs = z;
        TransformDomainWarpCoordinate(ref xs, ref ys, ref zs);

        DoSingleDomainWarp(seed, amp, freq, xs, ys, zs, ref x, ref y, ref z);
    }


    // Domain Warp Fractal Progressive

    private void DomainWarpFractalProgressive(ref FNLfloat x, ref FNLfloat y)
    {
        int seed = mSeed;
        float amp = mDomainWarpAmp * mFractalBounding;
        float freq = mFrequency;

        for (int i = 0; i < mOctaves; i++)
        {
            FNLfloat xs = x;
            FNLfloat ys = y;
            TransformDomainWarpCoordinate(ref xs, ref ys);

            DoSingleDomainWarp(seed, amp, freq, xs, ys, ref x, ref y);

            seed++;
            amp *= mGain;
            freq *= mLacunarity;
        }
    }

    private void DomainWarpFractalProgressive(ref FNLfloat x, ref FNLfloat y, ref FNLfloat z)
    {
        int seed = mSeed;
        float amp = mDomainWarpAmp * mFractalBounding;
        float freq = mFrequency;

        for (int i = 0; i < mOctaves; i++)
        {
            FNLfloat xs = x;
            FNLfloat ys = y;
            FNLfloat zs = z;
            TransformDomainWarpCoordinate(ref xs, ref ys, ref zs);

            DoSingleDomainWarp(seed, amp, freq, xs, ys, zs, ref x, ref y, ref z);

            seed++;
            amp *= mGain;
            freq *= mLacunarity;
        }
    }


    // Domain Warp Fractal Independant
    private void DomainWarpFractalIndependent(ref FNLfloat x, ref FNLfloat y)
    {
        FNLfloat xs = x;
        FNLfloat ys = y;
        TransformDomainWarpCoordinate(ref xs, ref ys);

        int seed = mSeed;
        float amp = mDomainWarpAmp * mFractalBounding;
        float freq = mFrequency;

        for (int i = 0; i < mOctaves; i++)
        {
            DoSingleDomainWarp(seed, amp, freq, xs, ys, ref x, ref y);

            seed++;
            amp *= mGain;
            freq *= mLacunarity;
        }
    }

    private void DomainWarpFractalIndependent(ref FNLfloat x, ref FNLfloat y, ref FNLfloat z)
    {
        FNLfloat xs = x;
        FNLfloat ys = y;
        FNLfloat zs = z;
        TransformDomainWarpCoordinate(ref xs, ref ys, ref zs);

        int seed = mSeed;
        float amp = mDomainWarpAmp * mFractalBounding;
        float freq = mFrequency;

        for (int i = 0; i < mOctaves; i++)
        {
            DoSingleDomainWarp(seed, amp, freq, xs, ys, zs, ref x, ref y, ref z);

            seed++;
            amp *= mGain;
            freq *= mLacunarity;
        }
    }


    // Domain Warp Basic Grid

    private void SingleDomainWarpBasicGrid(int seed, float warpAmp, float frequency, FNLfloat x, FNLfloat y, ref FNLfloat xr, ref FNLfloat yr)
    {
        FNLfloat xf = x * frequency;
        FNLfloat yf = y * frequency;

        int x0 = FastFloor(xf);
        int y0 = FastFloor(yf);

        float xs = InterpHermite((float)(xf - x0));
        float ys = InterpHermite((float)(yf - y0));

        x0 *= PrimeX;
        y0 *= PrimeY;
        int x1 = x0 + PrimeX;
        int y1 = y0 + PrimeY;

        int hash0 = Hash(seed, x0, y0) & (255 << 1);
        int hash1 = Hash(seed, x1, y0) & (255 << 1);

        float lx0x = Lerp(RandVecs2D[hash0], RandVecs2D[hash1], xs);
        float ly0x = Lerp(RandVecs2D[hash0 | 1], RandVecs2D[hash1 | 1], xs);

        hash0 = Hash(seed, x0, y1) & (255 << 1);
        hash1 = Hash(seed, x1, y1) & (255 << 1);

        float lx1x = Lerp(RandVecs2D[hash0], RandVecs2D[hash1], xs);
        float ly1x = Lerp(RandVecs2D[hash0 | 1], RandVecs2D[hash1 | 1], xs);

        xr += Lerp(lx0x, lx1x, ys) * warpAmp;
        yr += Lerp(ly0x, ly1x, ys) * warpAmp;
    }

    private void SingleDomainWarpBasicGrid(int seed, float warpAmp, float frequency, FNLfloat x, FNLfloat y, FNLfloat z, ref FNLfloat xr, ref FNLfloat yr, ref FNLfloat zr)
    {
        FNLfloat xf = x * frequency;
        FNLfloat yf = y * frequency;
        FNLfloat zf = z * frequency;

        int x0 = FastFloor(xf);
        int y0 = FastFloor(yf);
        int z0 = FastFloor(zf);

        float xs = InterpHermite((float)(xf - x0));
        float ys = InterpHermite((float)(yf - y0));
        float zs = InterpHermite((float)(zf - z0));

        x0 *= PrimeX;
        y0 *= PrimeY;
        z0 *= PrimeZ;
        int x1 = x0 + PrimeX;
        int y1 = y0 + PrimeY;
        int z1 = z0 + PrimeZ;

        int hash0 = Hash(seed, x0, y0, z0) & (255 << 2);
        int hash1 = Hash(seed, x1, y0, z0) & (255 << 2);

        float lx0x = Lerp(RandVecs3D[hash0], RandVecs3D[hash1], xs);
        float ly0x = Lerp(RandVecs3D[hash0 | 1], RandVecs3D[hash1 | 1], xs);
        float lz0x = Lerp(RandVecs3D[hash0 | 2], RandVecs3D[hash1 | 2], xs);

        hash0 = Hash(seed, x0, y1, z0) & (255 << 2);
        hash1 = Hash(seed, x1, y1, z0) & (255 << 2);

        float lx1x = Lerp(RandVecs3D[hash0], RandVecs3D[hash1], xs);
        float ly1x = Lerp(RandVecs3D[hash0 | 1], RandVecs3D[hash1 | 1], xs);
        float lz1x = Lerp(RandVecs3D[hash0 | 2], RandVecs3D[hash1 | 2], xs);

        float lx0y = Lerp(lx0x, lx1x, ys);
        float ly0y = Lerp(ly0x, ly1x, ys);
        float lz0y = Lerp(lz0x, lz1x, ys);

        hash0 = Hash(seed, x0, y0, z1) & (255 << 2);
        hash1 = Hash(seed, x1, y0, z1) & (255 << 2);

        lx0x = Lerp(RandVecs3D[hash0], RandVecs3D[hash1], xs);
        ly0x = Lerp(RandVecs3D[hash0 | 1], RandVecs3D[hash1 | 1], xs);
        lz0x = Lerp(RandVecs3D[hash0 | 2], RandVecs3D[hash1 | 2], xs);

        hash0 = Hash(seed, x0, y1, z1) & (255 << 2);
        hash1 = Hash(seed, x1, y1, z1) & (255 << 2);

        lx1x = Lerp(RandVecs3D[hash0], RandVecs3D[hash1], xs);
        ly1x = Lerp(RandVecs3D[hash0 | 1], RandVecs3D[hash1 | 1], xs);
        lz1x = Lerp(RandVecs3D[hash0 | 2], RandVecs3D[hash1 | 2], xs);

        xr += Lerp(lx0y, Lerp(lx0x, lx1x, ys), zs) * warpAmp;
        yr += Lerp(ly0y, Lerp(ly0x, ly1x, ys), zs) * warpAmp;
        zr += Lerp(lz0y, Lerp(lz0x, lz1x, ys), zs) * warpAmp;
    }


    // Domain Warp Simplex/OpenSimplex2
    private void SingleDomainWarpSimplexGradient(int seed, float warpAmp, float frequency, FNLfloat x, FNLfloat y, ref FNLfloat xr, ref FNLfloat yr, bool outGradOnly)
    {
        const float SQRT3 = 1.7320508075688772935274463415059f;
        const float G2 = (3 - SQRT3) / 6;

        x *= frequency;
        y *= frequency;

        /*
         * --- Skew moved to TransformNoiseCoordinate method ---
         * const FNfloat F2 = 0.5f * (SQRT3 - 1);
         * FNfloat s = (x + y) * F2;
         * x += s; y += s;
        */

        int i = FastFloor(x);
        int j = FastFloor(y);
        float xi = (float)(x - i);
        float yi = (float)(y - j);

        float t = (xi + yi) * G2;
        float x0 = (float)(xi - t);
        float y0 = (float)(yi - t);

        i *= PrimeX;
        j *= PrimeY;

        float vx, vy;
        vx = vy = 0;

        float a = 0.5f - x0 * x0 - y0 * y0;
        if (a > 0)
        {
            float aaaa = (a * a) * (a * a);
            float xo, yo;
            if (outGradOnly)
                GradCoordOut(seed, i, j, out xo, out yo);
            else
                GradCoordDual(seed, i, j, x0, y0, out xo, out yo);
            vx += aaaa * xo;
            vy += aaaa * yo;
        }

        float c = (float)(2 * (1 - 2 * G2) * (1 / G2 - 2)) * t + ((float)(-2 * (1 - 2 * G2) * (1 - 2 * G2)) + a);
        if (c > 0)
        {
            float x2 = x0 + (2 * (float)G2 - 1);
            float y2 = y0 + (2 * (float)G2 - 1);
            float cccc = (c * c) * (c * c);
            float xo, yo;
            if (outGradOnly)
                GradCoordOut(seed, i + PrimeX, j + PrimeY, out xo, out yo);
            else
                GradCoordDual(seed, i + PrimeX, j + PrimeY, x2, y2, out xo, out yo);
            vx += cccc * xo;
            vy += cccc * yo;
        }

        if (y0 > x0)
        {
            float x1 = x0 + (float)G2;
            float y1 = y0 + ((float)G2 - 1);
            float b = 0.5f - x1 * x1 - y1 * y1;
            if (b > 0)
            {
                float bbbb = (b * b) * (b * b);
                float xo, yo;
                if (outGradOnly)
                    GradCoordOut(seed, i, j + PrimeY, out xo, out yo);
                else
                    GradCoordDual(seed, i, j + PrimeY, x1, y1, out xo, out yo);
                vx += bbbb * xo;
                vy += bbbb * yo;
            }
        }
        else
        {
            float x1 = x0 + ((float)G2 - 1);
            float y1 = y0 + (float)G2;
            float b = 0.5f - x1 * x1 - y1 * y1;
            if (b > 0)
            {
                float bbbb = (b * b) * (b * b);
                float xo, yo;
                if (outGradOnly)
                    GradCoordOut(seed, i + PrimeX, j, out xo, out yo);
                else
                    GradCoordDual(seed, i + PrimeX, j, x1, y1, out xo, out yo);
                vx += bbbb * xo;
                vy += bbbb * yo;
            }
        }

        xr += vx * warpAmp;
        yr += vy * warpAmp;
    }

    private void SingleDomainWarpOpenSimplex2Gradient(int seed, float warpAmp, float frequency, FNLfloat x, FNLfloat y, FNLfloat z, ref FNLfloat xr, ref FNLfloat yr, ref FNLfloat zr, bool outGradOnly)
    {
        x *= frequency;
        y *= frequency;
        z *= frequency;

        /*
         * --- Rotation moved to TransformDomainWarpCoordinate method ---
         * const FNfloat R3 = (FNfloat)(2.0 / 3.0);
         * FNfloat r = (x + y + z) * R3; // Rotation, not skew
         * x = r - x; y = r - y; z = r - z;
        */

        int i = FastRound(x);
        int j = FastRound(y);
        int k = FastRound(z);
        float x0 = (float)x - i;
        float y0 = (float)y - j;
        float z0 = (float)z - k;

        int xNSign = (int)(-x0 - 1.0f) | 1;
        int yNSign = (int)(-y0 - 1.0f) | 1;
        int zNSign = (int)(-z0 - 1.0f) | 1;

        float ax0 = xNSign * -x0;
        float ay0 = yNSign * -y0;
        float az0 = zNSign * -z0;

        i *= PrimeX;
        j *= PrimeY;
        k *= PrimeZ;

        float vx, vy, vz;
        vx = vy = vz = 0;

        float a = (0.6f - x0 * x0) - (y0 * y0 + z0 * z0);
        for (int l = 0; ; l++)
        {
            if (a > 0)
            {
                float aaaa = (a * a) * (a * a);
                float xo, yo, zo;
                if (outGradOnly)
                    GradCoordOut(seed, i, j, k, out xo, out yo, out zo);
                else
                    GradCoordDual(seed, i, j, k, x0, y0, z0, out xo, out yo, out zo);
                vx += aaaa * xo;
                vy += aaaa * yo;
                vz += aaaa * zo;
            }

            float b = a;
            int i1 = i;
            int j1 = j;
            int k1 = k;
            float x1 = x0;
            float y1 = y0;
            float z1 = z0;

            if (ax0 >= ay0 && ax0 >= az0)
            {
                x1 += xNSign;
                b = b + ax0 + ax0;
                i1 -= xNSign * PrimeX;
            }
            else if (ay0 > ax0 && ay0 >= az0)
            {
                y1 += yNSign;
                b = b + ay0 + ay0;
                j1 -= yNSign * PrimeY;
            }
            else
            {
                z1 += zNSign;
                b = b + az0 + az0;
                k1 -= zNSign * PrimeZ;
            }

            if (b > 1)
            {
                b -= 1;
                float bbbb = (b * b) * (b * b);
                float xo, yo, zo;
                if (outGradOnly)
                    GradCoordOut(seed, i1, j1, k1, out xo, out yo, out zo);
                else
                    GradCoordDual(seed, i1, j1, k1, x1, y1, z1, out xo, out yo, out zo);
                vx += bbbb * xo;
                vy += bbbb * yo;
                vz += bbbb * zo;
            }

            if (l == 1) break;

            ax0 = 0.5f - ax0;
            ay0 = 0.5f - ay0;
            az0 = 0.5f - az0;

            x0 = xNSign * ax0;
            y0 = yNSign * ay0;
            z0 = zNSign * az0;

            a += (0.75f - ax0) - (ay0 + az0);

            i += (xNSign >> 1) & PrimeX;
            j += (yNSign >> 1) & PrimeY;
            k += (zNSign >> 1) & PrimeZ;

            xNSign = -xNSign;
            yNSign = -yNSign;
            zNSign = -zNSign;

            seed += 1293373;
        }

        xr += vx * warpAmp;
        yr += vy * warpAmp;
        zr += vz * warpAmp;
    }
}```

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
    SyncNeighbors(); // Backup dinámico
    if (neighbors.Count == 0)
    {
        Debug.LogWarning($"{name} has no neighbors!");
        return;
    }
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
        // Eliminar llamada a AssignNeighborReferences
        var hexData = WorldMapManager.Instance.GetOrGenerateHex(coordinates);
        if (hexData.neighborRefs == null || hexData.neighborRefs.Count == 0)
        {
            Debug.LogWarning($"{name} has no neighbors! Skipping influence evaluation.");
            return;
        }

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

public void SyncNeighbors()
{
    neighbors.Clear();

    var hexData = WorldMapManager.Instance.GetOrGenerateHex(coordinates);
    WorldMapManager.Instance.EnsureNeighborsAssigned(hexData);

    foreach (var neighborData in hexData.neighborRefs)
    {
        Vector2Int chunkCoord = ChunkManager.WorldToChunkCoord(neighborData.coordinates);
        if (ChunkManager.Instance.loadedChunks.TryGetValue(chunkCoord, out var neighborChunk))
        {
            var behaviors = neighborChunk.GetComponentsInChildren<HexBehavior>();
            foreach (var neighborBehavior in behaviors)
            {
                if (neighborBehavior.coordinates.Equals(neighborData.coordinates))
                {
                    neighbors.Add(neighborBehavior);
                    break;
                }
            }
        }
    }
}


}```

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
        // 'J' is now dedicated to vertical zoom.
        float rotationDelta = 0f;
        if (Input.GetKey(KeyCode.U)) rotationDelta = -1f; // U: gira hacia la izquierda

        if (Input.GetKey(KeyCode.I)) rotationDelta = 1f;  // I: gira hacia la derecha


        if (rotationDelta != 0f)
        {
            AdjustRotation(rotationDelta);
        }

        // --- VERTICAL ZOOM (Y-level) (E/R Keys) ---
        float yMoveDelta = 0f;
        if (Input.GetKey(KeyCode.L)) yMoveDelta = -1f; // E to zoom in (move camera down)
        if (Input.GetKey(KeyCode.O)) yMoveDelta = 1f;  // R to zoom out (move camera up)

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

    try
    {
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

                HexBehavior behavior = hex.GetComponent<HexBehavior>();
                if (behavior != null)
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
                              //  Debug.LogWarning("⚠️ ChunkMapGameConfig not found in Resources.");
                            }
                            renderer.SetVisualByHex(hexData);
                    }

                    // Asignar vecinos con la nueva lógica proactiva
                    WorldMapManager.Instance.RefreshNeighborsFor(hexData);

                    var existingCollider = parent.GetComponent<MeshCollider>();
                    if (existingCollider != null)
                    {
                        Object.Destroy(existingCollider);
                        Debug.Log($"🧹 Eliminado MeshCollider sobrante de {parent.name}");
                    }

                    GameObject testPrefab = Resources.Load<GameObject>("TerrainObjects/Leaf_Oak");
                    if (testPrefab == null)
                    {
                        Debug.LogWarning("⚠️ No se encontró el prefab en Resources/TerrainObjects/Leaf_Oak");
                    }
                    else if (Random.value < 0.05f)
                    {
                        CoroutineDispatcher.Instance?.RunCoroutine(DelayedPlaceFeature(behavior, testPrefab));
                    }
                }
                else
                {
                    Debug.LogError($"❌ Hex instanciado sin HexBehavior: {hex.name}");
                }
            }
        }
    }
    catch (System.Exception ex)
    {
        Debug.LogError($"❌ Error generando chunk {chunkCoord}: {ex}");
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
        WorldMapManager.Instance.EnsureNeighborsAssigned(hexData);

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

    public void InitializeChunks(int range)
    {
        Debug.Log("🚀 Inicializando chunks alrededor del centro con PerlinSettings actualizado...");
        loadedChunks.Clear();

        for (int x = -range; x <= range; x++)
        {
            for (int y = -range; y <= range; y++)
            {
                Vector2Int coord = new Vector2Int(x, y);
                GameObject chunk = ChunkGenerator.GenerateChunk(coord, chunkSize, hexPrefab);
                loadedChunks[coord] = chunk;
            }
        }
        Debug.Log($"🌍 {loadedChunks.Count} chunks generados.");
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
            List<HexRenderer> newHexes = new List<HexRenderer>();

            foreach (var coord in chunksToKeep)
            {
                if (loadedChunks.TryGetValue(coord, out var chunk))
                {
                    HexRenderer[] hexes = chunk.GetComponentsInChildren<HexRenderer>();
                    if (hexes.Length > 0)
                    {
                        Debug.Log($"🔍 Chunk en {coord} tiene {hexes.Length} hexes.");
                        newHexes.AddRange(hexes);
                    }
                }
            }

            Debug.Log($"🔍 Se encontraron {newHexes.Count} nuevos HexRenderer.");
            HexBorderManager.Instance?.AddBordersForChunk(newHexes);

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
                HexRenderer[] hexes = chunk.GetComponentsInChildren<HexRenderer>();
                HexBorderManager.Instance?.RemoveBordersForChunk(hexes);
                HexBorderManager.Instance?.RemoveBordersForChunk(hexes);

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
    public void LoadChunksAtDetail(int detailLevel)
{
/*    foreach (var chunk in loadedChunks.Values)
    {
        Destroy(chunk);
    }
    loadedChunks.Clear();

    int size = chunkSize;
    GameObject prefab = hexPrefab;

    if (detailLevel == 0)
    {
        // Detalle máximo, chunks completos
        size = chunkSize;
        prefab = hexPrefab;
    }
    else if (detailLevel == 1)
    {
        size = chunkSize * 2;
        prefab = Resources.Load<GameObject>("LowPolyHexPrefab");  // Asegúrate que existe
    }
    else if (detailLevel == 2)
    {
        // Minimapa nivel 2: chunks low-poly con elevación y color por tile
        size = chunkSize;
        prefab = Resources.Load<GameObject>("LowPolyHexPrefab");
    }
    else
    {
        // Nivel 3 (proyección global) no carga chunks
        return;
    }

    for (int x = -2; x <= 2; x++)
    {
        for (int y = -2; y <= 2; y++)
        {
            Vector2Int coord = new Vector2Int(x, y);
            GameObject chunk = ChunkGenerator.GenerateChunk(coord, size, prefab);
            loadedChunks[coord] = chunk;
        }
    }

    Debug.Log($"🌍 Chunks generados para LOD {detailLevel}");
*/}

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

## 📁 Map/HexBorderManager.cs
```csharp
using UnityEngine;
using System.Collections.Generic;

public class HexBorderManager : MonoBehaviour
{
    public static HexBorderManager Instance;

    [Header("Border Settings")]
    [SerializeField] private bool bordersVisible = true;
    [SerializeField] private float heightOffset = 0.1f;
    [SerializeField] private Color borderColor = Color.white;
    [SerializeField] private float lineWidth = 0.05f;
    [SerializeField] private float outerRadius = 1f;

    private Dictionary<HexRenderer, LineRenderer> borderLines = new();

    public static bool IsVisible => Instance != null && Instance.bordersVisible;
    public static float HeightOffset => Instance != null ? Instance.heightOffset : 0.1f;
    public static Color BorderColor => Instance != null ? Instance.borderColor : Color.white;
    public static float LineWidth => Instance != null ? Instance.lineWidth : 0.05f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
            Debug.Log("✅ HexBorderManager iniciado correctamente.");
        }
    }

    public void AddBordersForChunk(IEnumerable<HexRenderer> hexes)
    {
        int count = 0;
        foreach (var hex in hexes)
        {
            if (borderLines.ContainsKey(hex)) continue;

            GameObject lineObj = new GameObject($"HexBorder_{hex.name}");
            lineObj.transform.SetParent(this.transform);

            LineRenderer lr = lineObj.AddComponent<LineRenderer>();
            lr.useWorldSpace = true;
            lr.loop = true;
            lr.positionCount = 7;
            lr.material = new Material(Shader.Find("Unlit/Color"));
            lr.widthMultiplier = lineWidth;
            lr.material.color = borderColor;
            lr.startColor = borderColor;
            lr.endColor = borderColor;

            Vector3 center = new Vector3(hex.transform.position.x,
                hex.transform.position.y + hex.columnHeight * hex.heightScale + heightOffset,
                hex.transform.position.z);

            Vector3[] corners = new Vector3[7];
            for (int i = 0; i < 7; i++)
            {
                float angle = Mathf.Deg2Rad * (60f * i);
                corners[i] = new Vector3(
                    center.x + outerRadius * Mathf.Cos(angle),
                    center.y,
                    center.z + outerRadius * Mathf.Sin(angle)
                );
            }

            lr.SetPositions(corners);
            lr.enabled = bordersVisible;

            borderLines.Add(hex, lr);
            count++;
        }
        Debug.Log($"🟢 Agregados {count} bordes para el chunk.");
    }

    public void RemoveBordersForChunk(IEnumerable<HexRenderer> hexes)
    {
        int count = 0;
        foreach (var hex in hexes)
        {
            if (borderLines.TryGetValue(hex, out var lr))
            {
                Destroy(lr.gameObject);
                borderLines.Remove(hex);
                count++;
            }
        }
        Debug.Log($"🔴 Eliminados {count} bordes del chunk.");
    }

    public void ToggleBorders()
    {
        bordersVisible = !bordersVisible;
        foreach (var lr in borderLines.Values)
        {
            lr.enabled = bordersVisible;
        }
        Debug.Log($"🔲 Bordes {(bordersVisible ? "ACTIVADOS" : "DESACTIVADOS")}");
    }

    public void SetBordersVisibility(bool visible)
    {
        bordersVisible = visible;
        foreach (var lr in borderLines.Values)
        {
            lr.enabled = visible;
        }
    }

    public void RefreshBorders()
    {
        foreach (var lr in borderLines.Values)
        {
            lr.widthMultiplier = lineWidth;
            lr.material.color = borderColor;
            lr.startColor = borderColor;
            lr.endColor = borderColor;
        }
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
    bool isEven = Q % 2 == 0;

    switch (direction)
    {
        case HexDirection.NE: return isEven ? new HexCoordinates(Q + 1, R) : new HexCoordinates(Q + 1, R + 1);
        case HexDirection.E:  return new HexCoordinates(Q + 1, R);
        case HexDirection.SE: return isEven ? new HexCoordinates(Q + 1, R - 1) : new HexCoordinates(Q + 1, R);
        case HexDirection.SW: return isEven ? new HexCoordinates(Q - 1, R - 1) : new HexCoordinates(Q - 1, R);
        case HexDirection.W:  return new HexCoordinates(Q - 1, R);
        case HexDirection.NW: return isEven ? new HexCoordinates(Q - 1, R) : new HexCoordinates(Q - 1, R + 1);
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
    Ocean,
    CoastalWater,
    SandyBeach,
    RockyBeach,
    Plains,
    LowHills,
    Hills,
    Valley,
    Mountain,
    Plateau,
    Peak
}

public enum HexType { Natural, Rural, Urban }
public enum ResourceType { Minerals, Wood, Food, Water, Energy }

public class HexData
{
    public HexCoordinates coordinates;

    // Capa estática
    public float elevation;
    public float slope;
    public float moisture;
    public float temperature;
    public TerrainType terrainType;
    public bool neighborsAssigned = false;




    // Capa dinámica
    public HexType hexType = HexType.Natural;
    public bool isExplored = false;
    public Dictionary<ResourceType, float> extractedResources = new();

    // Vecinos (sólo coordenadas, útil para persistencia o reconstrucción rápida)
    public List<HexCoordinates> neighborCoords = new();

    // En runtime, puede poblarse dinámicamente con referencias (si es necesario)
    public List<HexData> neighborRefs = new();

        // Capa de agua (propiedades dinámicas)
    public float waterAmount = 0f;    // Cantidad de agua acumulada
    public bool isRiver = false;      // Indica si forma parte de un río
    public bool isLake = false;       // Indica si forma parte de un lago

    
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
    [Range(0.01f, 5f)] public float heightScale = 20f;

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
      //  Debug.Log($"{name} – Mesh vertices: {_mesh.vertexCount}, assigned to MeshCollider: {_mc.sharedMesh != null}");

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
    
public void SetVisualByHex(HexData hexData)
{
    if (hexData.isRiver)
    {
        // Asignar color azul para agua
        topColor = Color.blue;
        sideColor = new Color(0, 0, 0.5f);  // Lado más oscuro para agua
    }
    else
    {
        // Asignar color según terrainType o por defecto
        switch (hexData.terrainType)
        {
            case TerrainType.Plains:
                topColor = Color.green; sideColor = Color.black;
                break;
            case TerrainType.Mountain:
                topColor = Color.gray; sideColor = Color.black;
                break;
            // Agrega más casos según necesidad
            default:
                topColor = Color.magenta; sideColor = Color.black;
                break;
        }
    }

    BuildMesh();  // Actualiza el mesh con nuevos colores
}


    
}```

---

## 📁 Map/PerlinSettings.cs
```csharp
using UnityEngine;

[System.Serializable]
public class PerlinSettings : MonoBehaviour
{


public float globalFreq = 0.001f;
public float globalAmplitude = 30f;

    [Header("Base Elevation (Continente)")]


    [Range(0.01f, 1f)]
    public float continentFreq = 0.02f;

    [Range(0f, 01f)]
    public float continentalFlattenFactor = 0.5f;  // Valor inicial 0.5
    public float baseFreq = 0.005f;            // Frecuencia base para el continente
    public float baseOffset = 50f;
    public float regionalNoise = 10f;

    public float baseAmplitude = 150f;          // Altura máxima del continente
    public int octaves = 6;                    // Octavas para el fractal Perlin

    public float lacunarity = 2.5f;            // Factor de frecuencia
    public float persistence = 0.4f;           // Factor de amplitud


    [Header("Montañas (Ridge Noise)")]
    public float ridgeFreq = 0.02f;            // Frecuencia del Ridge (montañas)
    public float ridgeAmplitude = 30f;         // Altura máxima de montañas
    public float mountainThreshold = 0.6f;     // Umbral para que empiecen las montañas

    [Header("Ríos (Inversión de Perlin o Worley)")]
    public float riverFreq = 0.01f;            // Frecuencia del ruido de ríos
    public float riverDepth = 10f;             // Profundidad de los ríos

    [Header("Otros")]
    public int seed = 200500;                  // Semilla para variabilidad
    public float moistureFreq = 0.03f;         // Opcional, para humedad
    public float tempFreq = 0.015f;            // Opcional, para temperatura

[Range(0.001f, 1f)]
public float waterFreq = 0.02f;
    [Range(0.1f, 10f)]
    public float waterAmplitude = 1f;  // Opcional, para controlar la "altura" o "cantidad"

public float waterLevel = 1f;  // Opcional, para controlar la "altura" o "cantidad"


    [Header("Anomaly Settings (Opcional)")]
    [Range(0f, 1f)] public float anomalyThreshold = 0.15f;
    [Range(0f, 1f)] public float anomalyStrength = 0.25f;
    public float anomalyFrequency = 0.01f;
}
```

---

## 📁 Map/PerlinSettingsController.cs
```csharp
/*using UnityEngine;

// Control visual para editar PerlinSettings desde el inspector.
public class PerlinSettingsController : MonoBehaviour
{
    [Header("Perlin Settings Asset")]
    public PerlinSettings perlinSettings; // Asigna aquí el asset en el inspector

    [Header("Editable Settings")]
    [Range(0.0001f, 125f)] public float elevationFreq = 125f;
    [Range(0.001f, 1f)] public float moistureFreq = 0.03f;
    [Range(0.001f, 1f)] public float tempFreq = 0.015f;
    public int seed = 100000;

    [Header("Perlin Fractal Settings")]
    [Range(1, 10)] public int octaves = 6;
    [Range(1f, 4f)] public float lacunarity = 2.5f;
    [Range(0.1f, 1f)] public float persistence = 0.4f;

    [Header("Anomaly Settings")]
    [Range(0f, 1f)] public float anomalyStrength = 0.25f;
    [Range(0f, 1f)] public float anomalyThreshold = 0.15f;
    public float anomalyFrequency = 0.1f;

  void OnValidate()
{
    if (perlinSettings != null)
    {
        perlinSettings.elevationFreq = elevationFreq;
        perlinSettings.moistureFreq = moistureFreq;
        perlinSettings.tempFreq = tempFreq;
        perlinSettings.seed = seed;
        perlinSettings.octaves = octaves;
        perlinSettings.lacunarity = lacunarity;
        perlinSettings.persistence = persistence;
        perlinSettings.anomalyStrength = anomalyStrength;
        perlinSettings.anomalyThreshold = anomalyThreshold;
        perlinSettings.anomalyFrequency = anomalyFrequency;

#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(perlinSettings);
        UnityEditor.AssetDatabase.SaveAssets();
#endif

        // 🚀 Llama a regenerar incluso fuera de Play
        if (WorldMapManager.Instance != null)
        {
            WorldMapManager.Instance.InitializeWorld();
            Debug.Log("🌍 Mundo regenerado automáticamente tras cambiar PerlinSettings.");
        }
    }
}



}
*/```

---

## 📁 Map/PerlinUtility.cs
```csharp
using UnityEngine;

// contiene Lógica matemática pura (Perlin, FractalPerlin, RidgePerlin).

public static class PerlinUtility
{
   public static float Perlin(HexCoordinates coord, float frequency, int seedOffset, int mapWidth, int mapHeight)
{
    float scale = 300f / Mathf.Min(mapWidth, mapHeight);  // 300 es el denominador base
    float scaledFreq = frequency * scale;

    float nx = (coord.Q * 73856093 + seedOffset * 19349663) * scaledFreq;
    float ny = (coord.R * 83492791 + seedOffset * 83492791) * scaledFreq;
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
            float nx = (coord.Q  + seedOffset ) * frequency;
            float ny = (coord.R  + seedOffset ) * frequency;
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
            (coord.Q   + seedOffset) * anomalyFreq,
            (coord.R  + seedOffset) * anomalyFreq
        );

        if (noise > 1f - anomalyThreshold)
            return baseElevation + anomalyStrength;

        if (noise < anomalyThreshold)
            return baseElevation - anomalyStrength;

        return baseElevation;
    }

    public static float RidgePerlin(HexCoordinates coord, float frequency, int seedOffset)
    {
        float nx = (coord.Q + seedOffset) * frequency;
        float ny = (coord.R + seedOffset) * frequency;
        float p = Mathf.PerlinNoise(nx, ny);
        return Mathf.Pow(1f - Mathf.Abs(2f * p - 1f), 2f); // Crea crestas, escarpado
    }

    // 🆕 (Opcional) Método para remapear valores de [0,1] a [-1,1] o cualquier rango
    public static float Remap(float value, float fromMin, float fromMax, float toMin, float toMax)
    {
        return (value - fromMin) / (fromMax - fromMin) * (toMax - toMin) + toMin;
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

## 📁 Map/WaterManager.cs
```csharp
using System.Collections.Generic;
using UnityEngine;

public class WaterManager : MonoBehaviour
{
    public static WaterManager Instance;

    [Header("Water Flow Settings")]
    [SerializeField] private float initialRainAmount = 10f;
    [SerializeField] private float waterFlowSpeed = 1f;
    [SerializeField] private float riverThreshold = 5f;
    [SerializeField] private float lakeThreshold = 10f;
    [SerializeField] private int simulationTicks = 10;

    [Header("Visual Settings")]
    [SerializeField] private Material riverMaterial;
    [SerializeField] private Material lakeMaterial;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

   public void SimulateWaterFlow()
{
    foreach (var kvp in WorldMapManager.Instance.GetAllHexes())
    {
        HexData hex = kvp;

        // 🌊 Flujo inicial basado en humedad y pendiente
        float moistureFactor = hex.moisture;
        float slopeFactor = Mathf.Clamp01(hex.slope * 10f);  // Normaliza pendiente (0-1)

        // 🌊 Capacidad de absorción según tipo de terreno
        float absorption = 1f;
        switch (hex.terrainType)
        {
            case TerrainType.Plains:
            case TerrainType.Valley:
            case TerrainType.Plateau:
                absorption = 0.7f;  // Más capacidad de absorción
                break;
            case TerrainType.Hills:
                absorption = 0.4f;  // Menos absorción
                break;
            case TerrainType.Mountain:
                absorption = 0.2f;  // Muy baja absorción
                break;
        }

        // 💧 Calcular flujo inicial (retención y pendiente)
        float baseWater = moistureFactor * initialRainAmount * absorption;
        float flowBoost = slopeFactor * initialRainAmount * (1f - absorption);  // Complementa absorción

        hex.waterAmount = baseWater + flowBoost;

        // 🏞 Visualización opcional: marcar ríos o lagos si excede umbral
        if (hex.waterAmount > 1.5f)  // Umbral ajustable
        {
            hex.isRiver = true;
            hex.isLake = false;
        }
        else if (hex.waterAmount > 0.8f)  // Menor acumulación
        {
            hex.isRiver = false;
            hex.isLake = true;
        }
        else
        {
            hex.isRiver = false;
            hex.isLake = false;
        }
    }

    Debug.Log("🌊 Simulación de flujo de agua completada con lógica mejorada.");
}


    // Proporcionar materiales para que ChunkGenerator los aplique
    public Material GetRiverMaterial() => riverMaterial;
    public Material GetLakeMaterial() => lakeMaterial;

    // Configuración para UI
    public void SetRainAmount(float value) => initialRainAmount = value;
    public void SetFlowSpeed(float value) => waterFlowSpeed = value;
    public void SetRiverThreshold(float value) => riverThreshold = value;
    public void SetLakeThreshold(float value) => lakeThreshold = value;
    public void SetSimulationTicks(int value) => simulationTicks = value;
}
```

---

## 📁 Map/WorldMapManager.cs
```csharp
using System.Collections.Generic;
using UnityEngine;



public class WorldMapManager : MonoBehaviour



{
    public static WorldMapManager Instance { get; private set; }
    public static FastNoiseLite Noise { get; private set; }

    public PerlinSettings perlinSettings;

    public ChunkMapGameConfig chunkMapConfig;

    [Header("MiniMap Settings")]
    public int minimapResolution = 256;  // Resolución del minimapa
    public UnityEngine.UI.RawImage minimapImage;  // Asigna un RawImage en el Canvas para mostrar minimapa



    [Header("Map Settings")]
    public int mapWidth = 1000;
    public int mapHeight = 1000;



    private Dictionary<HexCoordinates, HexData> worldMap = new();

    private ChunkManager chunkManager;



    private void Start()
    {
        if (perlinSettings == null)
        {
            Debug.LogError("WorldManager: PerlinSettings no ha sido asignado.");
        }
        else
        {
            Debug.Log($"WorldManager: PerlinSettings cargado. ElevationFreq: {perlinSettings.baseFreq}, Seed: {perlinSettings.seed}, Octaves: {perlinSettings.octaves}");
            InitializeWorld();  // 🚀 Regenera automáticamente al iniciar Play
        }
    }

    private void Awake()
    {
        Instance = this;

        if (perlinSettings == null)
        {
            Debug.LogError("WorldManager: PerlinSettings no ha sido asignado en el Inspector.");
        }
        else
        {
            Debug.Log($"WorldManager: PerlinSettings cargado. BaseFreq: {perlinSettings.baseFreq}, Seed: {perlinSettings.seed}, Octaves: {perlinSettings.octaves}");
        }

        // Inicializa FastNoiseLite con el seed y parámetros de PerlinSettings
        Noise = new FastNoiseLite();
        Noise.SetSeed(perlinSettings.seed);
        Noise.SetFrequency(perlinSettings.baseFreq);
        Noise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
        Noise.SetFractalOctaves(perlinSettings.octaves);
        Noise.SetFractalLacunarity(perlinSettings.lacunarity);
        Noise.SetFractalGain(perlinSettings.persistence);

        Debug.Log("🌍 FastNoiseLite inicializado con semilla y parámetros.");
    }


    public void InitializeWorld()
    {
        Resources.UnloadUnusedAssets();
        // perlinSettings = Resources.Load<PerlinSettings>("NewPerlinSettings");
        if (perlinSettings == null)
        {
            Debug.LogError("❌ No se pudo cargar NewPerlinSettings desde Resources.");
            return;
        }

        Debug.Log($"🔄 PerlinSettings recargado dinámicamente. Seed: {perlinSettings.seed}");
        ResetWorld();

        Debug.Log("🌍 Mundo regenerado completamente.");
        if (ChunkManager.Instance != null)
        {
            ChunkManager.Instance.InitializeChunks(2);  // Cambia el rango según quieras
            Debug.Log("🌍 Mundo inicial regenerado con chunks.");
        }

    }

    public void ResetWorld()
    {
        Debug.Log("🧹 Limpiando worldMap y chunks...");
        worldMap.Clear();

        foreach (Transform child in transform)
            Destroy(child.gameObject);

        if (ChunkManager.Instance != null)
        {
            foreach (var chunk in ChunkManager.Instance.loadedChunks.Values)
                Destroy(chunk);
            ChunkManager.Instance.loadedChunks.Clear();
        }

        Debug.Log("✅ ResetWorld completado.");
    }

    // ✅ MÉTODOS CLAVE COMPLETOS Y SIN CAMBIOS

   public HexData GetOrGenerateHex(HexCoordinates coord)
{
    if (worldMap.TryGetValue(coord, out var existing))
        return existing;

    HexData hex = new HexData();
    hex.coordinates = coord;

    hex.elevation = CalculateElevation(coord.Q, coord.R, mapWidth, mapHeight);
    hex.slope = CalculateSlopeMagnitude(coord.Q, coord.R, 0.01f, mapWidth, mapHeight);

    // Frecuencia de montañas
float mountainZoneNoise = PerlinUtility.Perlin(coord, 0.02f, 2024, mapWidth, mapHeight);
if (mountainZoneNoise > 0.95f)
{
    hex.elevation += 50f;  
    float localMountainNoise = PerlinUtility.Perlin(coord, 0.1f, 2025, mapWidth, mapHeight);
    hex.elevation += localMountainNoise * 10f;  // Relieve extra local
}

    hex.moisture = PerlinUtility.Perlin(coord, perlinSettings.moistureFreq, perlinSettings.seed, mapWidth, mapHeight);
    hex.temperature = PerlinUtility.Perlin(coord, perlinSettings.tempFreq, perlinSettings.seed, mapWidth, mapHeight);
    /* float waterNoise = PerlinUtility.Perlin(coord, perlinSettings.waterFreq, perlinSettings.seed);
        hex.waterAmount = waterNoise * perlinSettings.waterAmplitude;
        hex.isRiver = hex.waterAmount > 0.5f;  // Umbral de prueba para visualizar ríos */


    // Asignación inicial de agua
        hex.waterAmount = Mathf.Max(0, hex.moisture * 10f - hex.slope * 20f);  // Base simple: humedad - pendiente
    if (hex.waterAmount > 1f)
    {
        hex.isRiver = true;
        hex.isLake = false;  // Esto lo podemos ajustar si detectamos zonas bajas con acumulación
    }

    foreach (HexCoordinates neighbor in coord.GetAllNeighbors())
        hex.neighborCoords.Add(neighbor);

    hex.terrainType = DetermineTerrainType(hex);
    
    //Condiciones rugosidad para tipos de terreno hills y Montañas

    if (hex.terrainType == TerrainType.Hills || hex.terrainType == TerrainType.LowHills)
        {
            float extraNoise = PerlinUtility.RidgePerlin(hex.coordinates, 0.1f, perlinSettings.seed);
            hex.elevation += extraNoise * 3f;  // Aumenta variabilidad para Hills
        }
        else if (hex.terrainType == TerrainType.Mountain)
        {
            float extraNoise = PerlinUtility.RidgePerlin(hex.coordinates, 0.05f, perlinSettings.seed);
            hex.elevation += extraNoise * 5f;  // Aumenta más variabilidad para Mountains
        }

    // Forzar menor rugosidad (aplanar) plains, plateau y valleys 
        if (hex.terrainType == TerrainType.Plains || hex.terrainType == TerrainType.Plateau || hex.terrainType == TerrainType.Valley)
        {

            hex.elevation = Mathf.Lerp(hex.elevation, Mathf.Round(hex.elevation), 0.8f);  // Suaviza variabilidad
            hex.slope *= 0.05f;  // Reduce pendiente
        }
    worldMap[coord] = hex;
    

    return hex;
}


    public bool TryGetHex(HexCoordinates coord, out HexData hex) =>
        worldMap.TryGetValue(coord, out hex);

    public void EnsureNeighborsAssigned(HexData hex)
    {
        if (hex.neighborsAssigned)
            return;

        hex.neighborRefs.Clear();
        foreach (var coord in hex.neighborCoords)
        {
            if (worldMap.TryGetValue(coord, out var neighbor))
                hex.neighborRefs.Add(neighbor);
        }
        hex.neighborsAssigned = true;
    }

    public void AssignNeighborsForChunk(List<HexData> chunkHexes)
    {
        foreach (var hex in chunkHexes)
            EnsureNeighborsAssigned(hex);
    }

    public void RefreshNeighborsFor(HexData hex)
    {
        foreach (HexCoordinates coord in hex.neighborCoords)
        {
            if (worldMap.TryGetValue(coord, out var neighbor))
            {
                if (!hex.neighborRefs.Contains(neighbor))
                    hex.neighborRefs.Add(neighbor);
                if (!neighbor.neighborRefs.Contains(hex))
                    neighbor.neighborRefs.Add(hex);
            }
        }
        hex.neighborsAssigned = true;
    }

    public HexBehavior GetHexBehavior(HexCoordinates coord)
    {
        if (TryGetHex(coord, out var hexData))
        {
            Vector2Int chunkCoord = ChunkManager.WorldToChunkCoord(coord);
            if (ChunkManager.Instance.loadedChunks.TryGetValue(chunkCoord, out var chunk))
            {
                var behaviors = chunk.GetComponentsInChildren<HexBehavior>();
                foreach (var behavior in behaviors)
                {
                    if (behavior.coordinates.Equals(coord))
                        return behavior;
                }
            }
        }
        return null;
    }

    public IEnumerable<HexData> GetAllHexes() => worldMap.Values;

    // Métodos originales: CalculateElevation, CalculateSlopeMagnitude, DetermineTerrainType, IsWater

  public float CalculateElevation(int x, int y, int mapWidth, int mapHeight)
{
    // 🌍 Mascara continental usando Perlin global (controla zonas continentales)
    float continentMask = Mathf.PerlinNoise(
        (x / (float)mapWidth) * perlinSettings.continentFreq + perlinSettings.seed,
        (y / (float)mapHeight) * perlinSettings.continentFreq + perlinSettings.seed
    );
float baseNoise = Noise.GetNoise(x, y);
    baseNoise = Mathf.InverseLerp(-1f, 1f, baseNoise);  // Ajustar el rango del ruido


    float baseElevation = (continentMask * perlinSettings.baseAmplitude) + perlinSettings.baseOffset;

    // 🌊 Control del nivel de agua
    float waterLevel = 16f;
    if (baseElevation < waterLevel)
    {
        baseElevation = waterLevel - (waterLevel - baseElevation) * perlinSettings.continentalFlattenFactor;
    }

 // 🌄 🌎 NUEVO RUIDO REGIONAL A GRAN ESCALA 🌎 🌄
   float globalRegionNoise = Noise.GetNoise(x * perlinSettings.globalFreq, y * perlinSettings.globalFreq);
baseElevation += globalRegionNoise * perlinSettings.globalAmplitude;

        // 🌄 Ruido adicional para microvariaciones (simula subzonas altas/bajas)
        float regionalNoise = Noise.GetNoise(x + 2000, y + 2000) * 10f;  // Ajusta el *10f según amplitud deseada
    baseElevation += regionalNoise;

    // 🌄 Detalle fino para variabilidad local
    float detailNoise = Noise.GetNoise(x + 4000, y + 4000) * 2f;
    baseElevation += detailNoise;

    // 🌄 Eliminamos la lógica de MountainThreshold (opcional)
    // Esto permite que la elevación se distribuya más naturalmente
    // y no dependa de un umbral único.
    // Si se desea mantener control, podríamos hacer:
    // baseElevation += Mathf.Max(0, regionalNoise - perlinSettings.mountainThreshold) * perlinSettings.ridgeAmplitude;

    // 🌊 Reducción por ríos
    float riverNoise = 1f - Noise.GetNoise(x + 6000, y + 6000);
    baseElevation -= riverNoise * (perlinSettings.riverDepth * 0.3f);

    // Variabilidad aleatoria muy fina
    baseElevation += Random.Range(-0.2f, 0.2f);

    return baseElevation;
}


   public float CalculateSlopeMagnitude(int x, int y, float epsilon, int mapWidth, int mapHeight)
{
    float centerElevation = CalculateElevation(x, y, mapWidth, mapHeight);
    float elevXPlus = CalculateElevation(x + 3, y, mapWidth, mapHeight);
    float elevXMinus = CalculateElevation(x - 3, y, mapWidth, mapHeight);
    float elevYPlus = CalculateElevation(x, y + 3, mapWidth, mapHeight);
    float elevYMinus = CalculateElevation(x, y - 3, mapWidth, mapHeight);

    float slopeX = (elevXPlus - elevXMinus) / 6f;
    float slopeY = (elevYPlus - elevYMinus) / 6f;

    float rawSlope = Mathf.Sqrt(slopeX * slopeX + slopeY * slopeY);

    float slopeNoise = Mathf.Abs(Noise.GetNoise(x + 5000, y + 5000)) * 0.005f;  // Ruido mínimo
    return rawSlope * 1.1f + slopeNoise;  // Reduzco a 10% extra solo
}




    public static bool IsWater(TerrainType type) =>
        type == TerrainType.Ocean || type == TerrainType.CoastalWater;

  private TerrainType DetermineTerrainType(HexData hex)
{
    float elevation = hex.elevation;
    float slope = hex.slope;
    int x = hex.coordinates.Q;
    int y = hex.coordinates.R;

    if (elevation < -10f) return TerrainType.Ocean;
    if (elevation < 12f) return TerrainType.CoastalWater;

    float valleyNoise = Noise.GetNoise(x + 8000, y + 8000);
    if (valleyNoise > 0.6f && elevation >= 14f && elevation < 50f)
        return TerrainType.Valley;

       if (elevation >= 11f && elevation < 13f)
{
    if (slope < 0.2f)  // Menos restrictivo, pendiente más suave
    {
        // Aumentar probabilidad de SandyBeach al 98%
        float pseudoRandom = PerlinUtility.Perlin(new HexCoordinates(hex.coordinates.Q + 10000, hex.coordinates.R + 10000), 0.1f, 9999, mapWidth, mapHeight);
        if (pseudoRandom < 0.98f)
            return TerrainType.SandyBeach;
        else
            return TerrainType.RockyBeach;
    }
    else
    {
        // Opcional: Mantener zonas de mayor pendiente como SandyBeach también
       // return TerrainType.SandyBeach;  // O puedes decidir Rocky si quieres
    }
}


    if (elevation >= 13f && elevation < 22f) return TerrainType.Plains;            // Plains extendido
    if (elevation >= 22f && elevation < 24f) return TerrainType.LowHills;         // Nuevo rango intermedio
    if (elevation >= 24f && elevation < 26f) return TerrainType.Hills;           // Hills
    if (elevation >= 26f && elevation < 30f) return TerrainType.Plateau;         // Plateau extendido
    if (elevation >= 30f) return TerrainType.Mountain;                           // Mountain más amplio

    if (elevation >= -1f && elevation < 40f && slope >= 0.01f && slope < 0.31f) return TerrainType.Valley;

    return TerrainType.Plains;  // Fallback solo si no coincide
}








   public void GenerateMinimapTextureOrSphere()
{
    Debug.Log("🗺 Generando minimapa procedural actualizado...");

    int resolution = minimapResolution;  // Usa la resolución configurable
    Texture2D texture = new Texture2D(resolution, resolution);

    for (int y = 0; y < resolution; y++)
    {
        for (int x = 0; x < resolution; x++)
        {
            int worldX = Mathf.RoundToInt((float)x / resolution * mapWidth);
            int worldY = Mathf.RoundToInt((float)y / resolution * mapHeight);

            // Calcula elevación y pendiente con métodos actualizados
            float elevation = CalculateElevation(worldX, worldY, mapWidth, mapHeight);
            float slope = CalculateSlopeMagnitude(worldX, worldY, 0.01f, mapWidth, mapHeight);

            // Usa moisture y temperature si quieres añadir variabilidad
            float moisture = PerlinUtility.Perlin(new HexCoordinates(worldX, worldY), perlinSettings.moistureFreq, perlinSettings.seed, mapWidth, mapHeight);
            float temperature = PerlinUtility.Perlin(new HexCoordinates(worldX, worldY), perlinSettings.tempFreq, perlinSettings.seed, mapWidth, mapHeight);

            // Determina tipo de terreno
            TerrainType terrain = DetermineTerrainType(new HexData { elevation = elevation, slope = slope, moisture = moisture, temperature = temperature });

            // Obtén color base del terreno
            Color color = chunkMapConfig.GetColorFor(terrain);

            // 🌊 Opcional: si el agua inicial está configurada, marcar zonas con humedad alta y pendiente baja
            float waterAmount = Mathf.Max(0, moisture * 10f - slope * 20f);
            if (waterAmount > 1f)
                color = Color.Lerp(color, Color.blue, 0.5f);  // Mezcla con azul para ríos
            else if (waterAmount > 0.5f)
                color = Color.Lerp(color, Color.cyan, 0.3f);  // Mezcla con cyan para lagos

            // 🌄 Opcional: marcar zonas montañosas con un tinte gris o marrón
            if (elevation > 25f)
                color = Color.Lerp(color, new Color(0.4f, 0.3f, 0.3f), 0.4f);  // Mezcla con marrón-gris

            texture.SetPixel(x, y, color);
        }
    }

    texture.Apply();

    if (GameManager.Instance != null && GameManager.Instance.minimapImage != null)
    {
        GameManager.Instance.minimapImage.texture = texture;
        GameManager.Instance.minimapImage.gameObject.SetActive(true);
        Debug.Log("🗺 Minimapa generado y asignado al RawImage.");
    }
    else
    {
        Debug.LogWarning("⚠️ MinimapImage no asignado en GameManager.");
    }
}




}
```

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

    Vector3 hexPos = hexRenderer.transform.position;

    objectTransform.position = new Vector3(
        hexPos.x,
        targetY,
        hexPos.z
    );

    Debug.Log($"📍 {objectTransform.name} alineado a ({hexPos.x:F2}, {targetY:F2}, {hexPos.z:F2}) sobre {hexRenderer.name} (VisualTopY={hexRenderer.VisualTopY:F3}, offset={verticalOffset:F3})");
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

/// <summary>
/// Devuelve la altura visual superior del hexágono, considerando su escala y altura base.
/// </summary>
public static float GetHexVisualTopY(HexRenderer hexRenderer, float verticalOffset = 0f)
{
    if (hexRenderer == null)
    {
        Debug.LogWarning("⚠️ HexRenderer es null en GetHexVisualTopY.");
        return 0f;
    }

    return hexRenderer.transform.position.y + hexRenderer.columnHeight * hexRenderer.heightScale + verticalOffset;
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

    if (Input.GetKeyDown(KeyCode.W)) MoveTo(currentCoordinates.GetNeighbor(HexDirection.NW));   // Norte visual
    if (Input.GetKeyDown(KeyCode.E)) MoveTo(currentCoordinates.GetNeighbor(HexDirection.NE));   // Noreste
    if (Input.GetKeyDown(KeyCode.D)) MoveTo(currentCoordinates.GetNeighbor(HexDirection.SE));   // Sureste
    if (Input.GetKeyDown(KeyCode.S)) MoveTo(currentCoordinates.GetNeighbor(HexDirection.SW));   // Sur
    if (Input.GetKeyDown(KeyCode.A)) MoveTo(currentCoordinates.GetNeighbor(HexDirection.W));    // Suroeste
    if (Input.GetKeyDown(KeyCode.Q)) MoveTo(currentCoordinates.GetNeighbor(HexDirection.E));    // Noroeste
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
