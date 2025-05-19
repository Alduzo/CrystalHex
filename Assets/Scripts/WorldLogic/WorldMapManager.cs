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



}