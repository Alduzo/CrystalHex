using System.Collections.Generic;
using UnityEngine;



public class WorldMapManager : MonoBehaviour



{
    public static WorldMapManager Instance { get; private set; }
    public static FastNoiseLite Noise { get; private set; } 

    public PerlinSettings perlinSettings; 
   




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
        
        hex.moisture = PerlinUtility.Perlin(coord, perlinSettings.moistureFreq, perlinSettings.seed);
        hex.temperature = PerlinUtility.Perlin(coord, perlinSettings.tempFreq, perlinSettings.seed);

        foreach (HexCoordinates neighbor in coord.GetAllNeighbors())
            hex.neighborCoords.Add(neighbor);

        hex.terrainType = DetermineTerrainType(hex);
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
    // Remapear baseNoise a [0,1] para suavizar variaciones
    float rawNoise = Noise.GetNoise(x, y);  // [-1,1]
    float remappedNoise = (rawNoise + 1f) * 0.5f;  // [0,1]

    // Elevación base controlada y elevada por baseOffset
    float baseElevation = (remappedNoise * perlinSettings.baseAmplitude) + perlinSettings.baseOffset;

    // RidgeNoise separado para montañas con amplitud reducida
    float ridgeNoise = (Noise.GetNoise(x + 10, y + 10) + 1f) * 0.5f;  // [0,1]
    float ridge = ridgeNoise * perlinSettings.ridgeAmplitude * 0.2f;  // Reduzco el impacto
    if (baseElevation > perlinSettings.mountainThreshold)
    {
        float ridgeContribution = baseElevation - perlinSettings.mountainThreshold;
        baseElevation += ridge * ridgeContribution * 0.3f;  // Suaviza
    }

    // RiverNoise suavizado para no restar tanto
    float riverNoise = (Noise.GetNoise(x + 20, y + 20) + 1f) * 0.5f;  // [0,1]
    float river = 1f - riverNoise;
    baseElevation -= river * perlinSettings.riverDepth * 0.1f;  // Reduce impacto río

    // Opcional: Limitar altura máxima
    baseElevation = Mathf.Clamp(baseElevation, 0f, 50f);  // Ajusta según amplitud realista

    return baseElevation;
}



    public float CalculateSlopeMagnitude(int x, int y, float epsilon, int mapWidth, int mapHeight)
    {
        float centerElevation = CalculateElevation(x, y, mapWidth, mapHeight);
        float elevXPlus = CalculateElevation(x + 1, y, mapWidth, mapHeight);
        float elevXMinus = CalculateElevation(x - 1, y, mapWidth, mapHeight);
        float elevYPlus = CalculateElevation(x, y + 1, mapWidth, mapHeight);
        float elevYMinus = CalculateElevation(x, y - 1, mapWidth, mapHeight);

        float slopeX = (elevXPlus - elevXMinus) / 2f;
        float slopeY = (elevYPlus - elevYMinus) / 2f;

        return Mathf.Sqrt(slopeX * slopeX + slopeY * slopeY);
    }

    public static bool IsWater(TerrainType type) =>
        type == TerrainType.Ocean || type == TerrainType.CoastalWater;

    private TerrainType DetermineTerrainType(HexData hex)
    {
        float elevation = hex.elevation;
        float slope = hex.slope;

        if (elevation < -2f) return TerrainType.Ocean;
        if (elevation < -1f) return TerrainType.CoastalWater;

        if (elevation >= -5f && elevation < 1f && slope >= 0f && slope < 0.15f)
            return TerrainType.SandyBeach;
        if (elevation >= -5f && elevation < 1f && slope >= 0.15f && slope < 1f)
            return TerrainType.RockyBeach;
        if (elevation >= 1f && elevation < 15f && slope >= 0f && slope < 0.1f)
            return TerrainType.Plains;
        if (elevation >= 15f && elevation < 80f && slope >= 0.1f && slope < 0.4f)
            return TerrainType.Hills;
        if (elevation >= 80f && elevation < 120f && slope >= 0f && slope < 0.1f)
            return TerrainType.Plateau;
        if (elevation >= 120f && elevation < 200f && slope >= 0.3f && slope < 1f)
            return TerrainType.Mountain;
        if (elevation >= -10f && elevation < 20f && slope >= 0.05f && slope < 0.4f)
            return TerrainType.Valley;

        return TerrainType.Valley;  // Por defecto
    }
}
