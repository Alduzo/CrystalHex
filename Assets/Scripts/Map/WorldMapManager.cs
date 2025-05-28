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
    float baseElevation = (Noise.GetNoise(x, y) * perlinSettings.baseAmplitude * 0.5f) + perlinSettings.baseOffset;  // Reduzco a 50%

    float detailNoise = Noise.GetNoise(x + 1000, y + 1000) * 0.3f;  // Suavizo detalle
    baseElevation += detailNoise;

    float ridge = Noise.GetNoise(x + 1000, y + 1000) * (perlinSettings.ridgeAmplitude * 0.3f);  // Solo 30%
    if (baseElevation > perlinSettings.mountainThreshold)
        baseElevation += ridge * (baseElevation - perlinSettings.mountainThreshold);

    float river = 1f - Noise.GetNoise(x + 3000, y + 3000);
    baseElevation -= river * (perlinSettings.riverDepth * 0.3f);  // Ríos suaves

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

    // Agua
    if (elevation < -2f) return TerrainType.Ocean;
    if (elevation < 0f) return TerrainType.CoastalWater;

    // Playas
    if (elevation >= 0f && elevation < 1f && slope < 0.05f)
        return TerrainType.SandyBeach;
    if (elevation >= 0f && elevation < 1f && slope >= 0.05f)
        return TerrainType.RockyBeach;

    // Llanuras: Muy suaves y bajas
    if (elevation >= 1f && elevation < 8f && slope < 0.05f)
        return TerrainType.Plains;

    // Colinas: Moderada elevación y pendiente
    if (elevation >= 8f && elevation < 25f && slope >= 0.05f && slope < 0.2f)
        return TerrainType.Hills;

    // Mesetas: Altura media pero poca pendiente
    if (elevation >= 25f && elevation < 50f && slope < 0.1f)
        return TerrainType.Plateau;

    // Montañas: Elevación alta y pendiente marcada
    if (elevation >= 50f && slope >= 0.2f)
        return TerrainType.Mountain;

    // Valles: Pendiente moderada y elevación media-baja
    if (elevation >= -1f && elevation < 20f && slope >= 0.02f && slope < 0.2f)
        return TerrainType.Valley;

    // Por defecto
    return TerrainType.Plains;
}





   public void GenerateMinimapTextureOrSphere()
    {
        Debug.Log("🗺 Generando minimapa procedural...");

        int resolution = 256;  // Resolución básica
        Texture2D texture = new Texture2D(resolution, resolution);

        for (int y = 0; y < resolution; y++)
        {
            for (int x = 0; x < resolution; x++)
            {
                int worldX = Mathf.RoundToInt((float)x / resolution * mapWidth);
                int worldY = Mathf.RoundToInt((float)y / resolution * mapHeight);

                float elevation = CalculateElevation(worldX, worldY, mapWidth, mapHeight);
                float slope = CalculateSlopeMagnitude(worldX, worldY, 0.01f, mapWidth, mapHeight);
                TerrainType terrain = DetermineTerrainType(new HexData { elevation = elevation, slope = slope });

                Color color = chunkMapConfig.GetColorFor(terrain);
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
