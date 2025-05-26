using System.Collections.Generic;
using UnityEngine;

public class WorldMapManager : MonoBehaviour
{
    public static WorldMapManager Instance;

    [Header("World Settings")]
    public PerlinSettings perlinSettings;  // Ahora solo cargado dinámicamente



    [Header("Map Settings")]
    public int mapWidth = 1000;
    public int mapHeight = 1000;

    private Dictionary<HexCoordinates, HexData> worldMap = new();

    private void Awake()
    {
        Instance = this;
        Debug.Log("🔄 WorldMapManager Awake. Limpiando worldMap inicial.");
        worldMap.Clear();
    }

    public void InitializeWorld()
    {
        Resources.UnloadUnusedAssets();
        perlinSettings = Resources.Load<PerlinSettings>("NewPerlinSettings");
        if (perlinSettings == null)
        {
            Debug.LogError("❌ No se pudo cargar NewPerlinSettings desde Resources.");
            return;
        }

        Debug.Log($"🔄 PerlinSettings recargado dinámicamente. Seed: {perlinSettings.seed}");
        ResetWorld();
        ChunkManager.Instance?.InitializeChunks(Vector2Int.zero);
        Debug.Log("🌍 Mundo regenerado completamente.");
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
        float scaleContinent = 0.0005f;
        float scaleMountains = 0.005f;
        float scaleDetail = 0.05f;

        float valueA = PerlinUtility.FractalPerlin(new HexCoordinates(x, y), scaleContinent, 3, 1.5f, 0.4f, perlinSettings.seed);
        float maskMountain = Mathf.Clamp01(Mathf.InverseLerp(0.2f, 0.8f, valueA));
        float valueB = PerlinUtility.RidgePerlin(new HexCoordinates(x, y), scaleMountains, perlinSettings.seed) * maskMountain;
        float valueC = PerlinUtility.Perlin(new HexCoordinates(x, y), scaleDetail, perlinSettings.seed);

        float elevation = (valueA * 150f) + (valueB * 100f) + (valueC * 10f);

        float oceanEdgePercentage = 0.05f;
        float xNorm = (float)x / mapWidth;
        if (xNorm < oceanEdgePercentage || xNorm > (1f - oceanEdgePercentage))
        {
            float blend = xNorm < oceanEdgePercentage ? 1f - (xNorm / oceanEdgePercentage) :
                                                       (xNorm - (1f - oceanEdgePercentage)) / oceanEdgePercentage;
            elevation = Mathf.Lerp(elevation, -150f, blend);
        }

        float iceEdgePercentage = 0.1f;
        float yNorm = (float)y / mapHeight;
        if (yNorm < iceEdgePercentage || yNorm > (1f - iceEdgePercentage))
        {
            float blend = yNorm < iceEdgePercentage ? 1f - (yNorm / iceEdgePercentage) :
                                                      (yNorm - (1f - iceEdgePercentage)) / iceEdgePercentage;
            elevation = Mathf.Lerp(elevation, 5f, blend);
        }

        return elevation;
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

        if (elevation < -50f) return TerrainType.Ocean;
        if (elevation < -5f) return TerrainType.CoastalWater;

        if (elevation >= -5f && elevation < 1f && slope >= 0f && slope < 0.05f)
            return TerrainType.SandyBeach;
        if (elevation >= -5f && elevation < 1f && slope >= 0.05f && slope < 1f)
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
