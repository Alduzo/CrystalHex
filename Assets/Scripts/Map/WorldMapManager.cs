using System.Collections.Generic;
using UnityEngine;



public class WorldMapManager : MonoBehaviour



{
    public static WorldMapManager Instance { get; private set; }
    public static FastNoiseLite Noise { get; private set; }

    public PerlinSettings perlinSettings;

    public ChunkMapGameConfig chunkMapConfig;

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








   public Texture2D GenerateMinimapTexture(int resolution)
{
    Debug.Log("🗺 Generando minimapa procedural completo...");

    Texture2D minimapTexture = new Texture2D(resolution, resolution);
    minimapTexture.filterMode = FilterMode.Point;

    int minQ = 0, maxQ = mapWidth;
    int minR = 0, maxR = mapHeight;

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





}
