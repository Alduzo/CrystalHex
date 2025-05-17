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
        hex.elevation = PerlinUtility.Perlin(coord, perlinSettings.elevationFreq, perlinSettings.elevationSeedOffset + seed);
        hex.moisture = PerlinUtility.Perlin(coord, perlinSettings.moistureFreq, perlinSettings.moistureSeedOffset + seed);
        hex.temperature = PerlinUtility.Perlin(coord, perlinSettings.tempFreq, perlinSettings.tempSeedOffset + seed);

        // Bioma inicial provisional
        hex.terrainType = hex.elevation < 0.25f ? TerrainType.Water :
                          hex.elevation > 0.8f ? TerrainType.Mountains :
                          hex.moisture > 0.6f ? TerrainType.Forest :
                          TerrainType.Plains;

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

}