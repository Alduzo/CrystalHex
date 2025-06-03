using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using JetBrains.Annotations;


public class WorldMapManager : MonoBehaviour



{
   public static int MaxMapWidth => Instance.mapWidth;
    public static int MaxMapHeight => Instance.mapHeight;
    public static WorldMapManager Instance { get; private set; }
    public static FastNoiseLite Noise { get; private set; }

    public PerlinSettings perlinSettings;

    public ChunkMapGameConfig chunkMapConfig;
    public GameObject waterTilePrefab;  // Asignar en el inspector

     public GameObject hexTilePrefab;
    public const float GlobalWaterLevel = 16f;




    [Header("MiniMap Settings")]
    // public MinimapController minimapController;  // Asigna desde Inspector
    public int minimapResolution = 256;  // Resolución del minimapa
    // Depreciado por ajustes a MInimap 1 de junio public UnityEngine.UI.RawImage minimapImage;  // Asigna un RawImage en el Canvas para mostrar minimapa



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
        
       /*     if (minimapController != null)
    {
        minimapController.GenerateMinimap();
    } */
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

private void GenerateHexDataGrid()
{
    Debug.Log("🔄 Generando HexData grid...");
    int minQ = -mapWidth / 2;
int maxQ = mapWidth / 2;
int minR = -mapHeight / 2;
int maxR = mapHeight / 2;

for (int q = minQ; q < maxQ; q++)
{
    for (int r = minR; r < maxR; r++)

        {
HexCoordinates coord = new HexCoordinates(q, r);
            if (!worldMap.ContainsKey(coord))
            {
                HexData data = GetOrGenerateHex(coord);  // Ya asigna datos base
                worldMap[coord] = data;
            }
        }
    }
    Debug.Log($"✅ HexData grid generado con {worldMap.Count} entradas.");
}

private void AssignNeighborsGlobal()
{
    Debug.Log("🔄 Asignando vecinos globalmente...");
    foreach (var hex in worldMap.Values)
    {
        hex.neighborRefs.Clear();
        foreach (var coord in hex.neighborCoords)
        {
            if (worldMap.TryGetValue(coord, out var neighbor))
                hex.neighborRefs.Add(neighbor);
        }
        hex.neighborsAssigned = true;
    }
    Debug.Log("✅ Vecinos asignados a todos los HexData.");
}

private void InstantiateHexTiles()
{
    Debug.Log("🖼 Instanciando visuales HexTile...");
    foreach (var data in worldMap.Values)
    {
        Vector2Int chunkCoord = ChunkManager.WorldToChunkCoord(data.coordinates);
        if (ChunkManager.Instance.loadedChunks.TryGetValue(chunkCoord, out var chunk))
        {
            var hexGO = Instantiate(hexTilePrefab, chunk.transform);
            hexGO.transform.position = HexCoordinates.ToWorldPosition(data.coordinates, HexRenderer.SharedOuterRadius);
            var behavior = hexGO.GetComponent<HexBehavior>();
            if (behavior != null)
            {
                behavior.Initialize(data);
            }
            else
            {
                Debug.LogWarning($"⚠️ Prefab para {data.coordinates} no tiene HexBehavior.");
            }
        }
        else
        {
            Debug.LogWarning($"⚠️ Chunk no encontrado para {data.coordinates}. Skipping tile.");
        }
    }
    Debug.Log("✅ Visuales instanciados.");
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
 // GenerateHexDataGrid();
//AssignNeighborsGlobal();
//InstantiateHexTiles();

Debug.Log("🌍 Mundo regenerado con flujo por fases.");

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

        AssignWaterFeatures(hex);

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
    float continentMask = Mathf.PerlinNoise(
        (x / (float)mapWidth) * perlinSettings.continentFreq + perlinSettings.seed,
        (y / (float)mapHeight) * perlinSettings.continentFreq + perlinSettings.seed
    );

    float baseNoise = Noise.GetNoise(x, y);
    baseNoise = Mathf.InverseLerp(-1f, 1f, baseNoise);

    float baseElevation = (continentMask * perlinSettings.baseAmplitude) + perlinSettings.baseOffset;

    float waterLevel = WorldMapManager.GlobalWaterLevel
;
    if (baseElevation < waterLevel * 1.2f)
    {
        baseElevation = waterLevel - (waterLevel - baseElevation) * perlinSettings.continentalFlattenFactor;
    }

    // 🌎 Global region + detail noise (controlados)
    float globalRegionNoise = Noise.GetNoise(x * perlinSettings.globalFreq, y * perlinSettings.globalFreq);
    baseElevation += globalRegionNoise * (perlinSettings.globalAmplitude * 0.5f);  // Reduzco magnitud
    float regionalNoise = Noise.GetNoise(x + 2000, y + 2000) * 5f;  // Reduzco a 5
    baseElevation += regionalNoise;
    float detailNoise = Noise.GetNoise(x + 4000, y + 4000) * 1f;  // Reduzco a 1
    baseElevation += detailNoise;

    // 🏔 Montañas (FractalPerlin) solo sobre umbral
   if (baseElevation > waterLevel + 8f)
{
    float mountainNoise = PerlinUtility.FractalPerlin(
        new HexCoordinates(x, y),
        perlinSettings.mountainFreq,  // Control desde inspector
        5, 2.0f, 0.5f,
        perlinSettings.seed + 5555);
    float mountainHeight = Mathf.Lerp(0, perlinSettings.mountainAmplitude, mountainNoise);
    baseElevation += mountainHeight * ((baseElevation - (waterLevel + 5f)) / 40f);
}

    // 🏔 Picos (RidgeNoise) solo sobre umbral alto y progresivo
    if (baseElevation > waterLevel + 15f)
    {
        float ridgeNoise = PerlinUtility.RidgePerlin(new HexCoordinates(x, y), 0.1f, perlinSettings.seed);
        if (ridgeNoise > 0.5f)
        {
            float ridgeEffect = (ridgeNoise - 0.5f) / 0.5f * 5f;  // Pico moderado
            baseElevation += ridgeEffect * ((baseElevation - (waterLevel + 15f)) / 15f);  // Transición gradual
        }
    }

    // 🌊 Ríos y cañones (ruidos suaves)
    float riverNoise = 1f - Noise.GetNoise(x + 6000, y + 6000);
    baseElevation -= riverNoise * (perlinSettings.riverDepth * 0.2f);  // Suavizado
    float canyonNoise = 1f - PerlinUtility.FractalPerlin(
        new HexCoordinates(x, y),
        0.02f, 4, 2.0f, 0.5f,
        perlinSettings.seed + 9999);
    float canyonDepth = Mathf.Lerp(0, 10f, canyonNoise);  // Suavizado
    baseElevation -= canyonDepth;

    baseElevation += Random.Range(-0.01f, 0.01f);  // Variabilidad mínima

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
        type == TerrainType.Ocean || type == TerrainType.CoastalWater ;

    private TerrainType DetermineTerrainType(HexData hex)
{
    float elevation = hex.elevation;
    float slope = hex.slope;
    int x = hex.coordinates.Q;
    int y = hex.coordinates.R;

    float waterLevel = WorldMapManager.GlobalWaterLevel;


        if (elevation < waterLevel - 5f) return TerrainType.Ocean;
        if (elevation >= waterLevel - 5f && elevation < waterLevel) return TerrainType.CoastalWater;

if (elevation >= waterLevel && elevation < waterLevel + 3f)
{
    if (slope < 2.2f)  // Ajusta pendiente para más SandyBeach
    {
        float pseudoRandom = PerlinUtility.Perlin(
            new HexCoordinates(x + 10000, y + 10000), 0.1f, 9999, mapWidth, mapHeight);
        if (pseudoRandom < 0.99f)
            return TerrainType.SandyBeach;
        else
            return TerrainType.RockyBeach;
    }
    else
    {
        return TerrainType.RockyBeach;
    }
}


    // 🌄 Valles
    float valleyNoise = Noise.GetNoise(x + 8000, y + 8000);
    if (valleyNoise > 0.3f && elevation >= waterLevel + 3f && elevation < waterLevel + 50f)
        return TerrainType.Valley;

    // 🌿 Tierra firme (ajustada al nuevo CalculateElevation)
    if (elevation >= waterLevel + 3f && elevation < waterLevel + 10f) return TerrainType.Plains;
    if (elevation >= waterLevel + 10f && elevation < waterLevel + 16f) return TerrainType.LowHills;
    if (elevation >= waterLevel + 16f && elevation < waterLevel + 23f) return TerrainType.Hills;
    if (elevation >= waterLevel + 23f && elevation < waterLevel + 28f) return TerrainType.Plateau;
    if (elevation >= waterLevel + 30f) return TerrainType.Mountain;

    // 🌄 Alternativa fallback
    if (elevation >= -1f && elevation < 50f && slope >= 0.01f && slope < 0.31f)
        return TerrainType.Valley;

    return TerrainType.Plains;
}



public Texture2D GenerateMinimapLightweight(int resolution)
{
    Debug.Log("🗺 Generando minimapa lightweight...");
    Texture2D minimapTexture = new Texture2D(resolution, resolution);
    minimapTexture.filterMode = FilterMode.Point;

    float scaleX = (float)mapWidth / resolution;
    float scaleY = (float)mapHeight / resolution;

    for (int y = 0; y < resolution; y++)
    {
        for (int x = 0; x < resolution; x++)
        {
            int worldX = Mathf.FloorToInt(x * scaleX);
            int worldY = Mathf.FloorToInt(y * scaleY);

            float elevation = CalculateElevation(worldX, worldY, mapWidth, mapHeight);
            Color color = chunkMapConfig.GetColorFor(DetermineTerrainTypeSimple(elevation));

            minimapTexture.SetPixel(x, y, color);
        }
    }

    minimapTexture.Apply();
    return minimapTexture;
}

private TerrainType DetermineTerrainTypeSimple(float elevation)
{
    if (elevation < -10f) return TerrainType.Ocean;
    if (elevation < 12f) return TerrainType.CoastalWater;
    if (elevation < 13f) return TerrainType.SandyBeach;
    if (elevation < 22f) return TerrainType.Plains;
    if (elevation < 26f) return TerrainType.Hills;
    if (elevation < 30f) return TerrainType.Plateau;
    return TerrainType.Mountain;
}



/*

   public Texture2D GenerateMinimapTexture(int resolution)
{
    Debug.Log("🗺 Generando minimapa procedural completo...");

    Texture2D minimapTexture = new Texture2D(resolution, resolution);
    minimapTexture.filterMode = FilterMode.Point;

    int minQ = -mapWidth / 2, maxQ = mapWidth / 2;
int minR = -mapHeight / 2, maxR = mapHeight / 2;

    float scaleQ = (float)resolution / (maxQ - minQ);
    float scaleR = (float)resolution / (maxR - minR);

    // Fondo base gris
    for (int y = 0; y < resolution; y++)
        for (int x = 0; x < resolution; x++)
            minimapTexture.SetPixel(x, y, Color.gray);

    for (int q = minQ; q < maxQ; q++)
    {
        for (int r = minR; r < maxR; r++)
        {
            HexCoordinates coord = new HexCoordinates(q, r);
            HexData hex = GetOrGenerateHex(coord);  // Genera datos directamente
            int pixelX = Mathf.RoundToInt((q - minQ) * scaleQ);
            int pixelY = Mathf.RoundToInt((r - minR) * scaleR);

            if (pixelX >= 0 && pixelX < resolution && pixelY >= 0 && pixelY < resolution)
            {
                minimapTexture.SetPixel(pixelX, pixelY, GetColorForHex(hex));
            }
        }
    }

    minimapTexture.Apply();
    return minimapTexture;
}

*/


    private Color GetColorForHex(HexData hex)
    {
        if (hex.isRiver) return Color.blue;
        if (hex.isLake) return new Color(0, 0.4f, 1f);

        return hex.terrainType switch
        {
            TerrainType.Ocean => new Color(0, 0.5f, 1f),
            TerrainType.CoastalWater => new Color(0.2f, 0.6f, 1f),
            TerrainType.SandyBeach => new Color(1f, 0.9f, 0.6f),
            TerrainType.RockyBeach => new Color(0.8f, 0.7f, 0.5f),
            TerrainType.Plains => Color.green,
            TerrainType.LowHills => new Color(0.5f, 0.8f, 0.4f),
            TerrainType.Hills => new Color(0.4f, 0.7f, 0.3f),
            TerrainType.Valley => new Color(0.6f, 0.8f, 0.3f),
            TerrainType.Mountain => Color.gray,
            TerrainType.Plateau => new Color(0.7f, 0.5f, 0.3f),
            TerrainType.Peak => Color.white,
            _ => Color.yellow,
        };
    }

  private void AssignWaterFeatures(HexData hex)
{
    float waterLevel = GlobalWaterLevel;

    // Si la elevación está justo debajo del nivel del agua y con poca pendiente, podría ser lago
    if (hex.elevation < waterLevel - 0.5f && hex.slope < 0.2f && hex.moisture > 0.5f)
    {
        hex.isLake = true;
        hex.isRiver = false;
    }
    // Si tiene pendiente moderada y humedad alta, podría ser río
    else if (hex.slope > 0.1f && hex.slope < 0.5f && hex.moisture > 0.4f)
    {
        hex.isRiver = true;
        hex.isLake = false;
    }
    else
    {
        hex.isRiver = false;
        hex.isLake = false;
    }
}





}
