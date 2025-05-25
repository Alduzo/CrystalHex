using System.Collections.Generic;
using UnityEngine;

public class WorldMapManager : MonoBehaviour
{
    public static WorldMapManager Instance;

    [Header("World Settings")]
    public PerlinSettings perlinSettings;
    [Header("Terrain Type Thresholds")]
    [SerializeField] private float oceanThreshold = -50f;
    [SerializeField] private float coastalWaterThreshold = -5f;

    [SerializeField] private float sandyBeachMinElevation = -5f;
    [SerializeField] private float sandyBeachMaxElevation = 1f;
    [SerializeField] private float sandyBeachMinSlope = 0f;
    [SerializeField] private float sandyBeachMaxSlope = 0.05f;

    [SerializeField] private float rockyBeachMinElevation = -5f;
    [SerializeField] private float rockyBeachMaxElevation = 1f;
    [SerializeField] private float rockyBeachMinSlope = 0.05f;
    [SerializeField] private float rockyBeachMaxSlope = 1f;

    [SerializeField] private float plainsMinElevation = 1f;
    [SerializeField] private float plainsMaxElevation = 15f;
    [SerializeField] private float plainsMinSlope = 0f;
    [SerializeField] private float plainsMaxSlope = 0.1f;

    [SerializeField] private float hillsMinElevation = 15f;
    [SerializeField] private float hillsMaxElevation = 80f;
    [SerializeField] private float hillsMinSlope = 0.1f;
    [SerializeField] private float hillsMaxSlope = 0.4f;

    [SerializeField] private float plateauMinElevation = 80f;
    [SerializeField] private float plateauMaxElevation = 120f;
    [SerializeField] private float plateauMinSlope = 0f;
    [SerializeField] private float plateauMaxSlope = 0.1f;

    [SerializeField] private float mountainMinElevation = 120f;
    [SerializeField] private float mountainMaxElevation = 200f; // Puedes ajustarlo si quieres un rango abierto
    [SerializeField] private float mountainMinSlope = 0.3f;
    [SerializeField] private float mountainMaxSlope = 1f;

    [SerializeField] private float valleyMinElevation = -10f;
    [SerializeField] private float valleyMaxElevation = 20f;
    [SerializeField] private float valleyMinSlope = 0.05f;
    [SerializeField] private float valleyMaxSlope = 0.4f;



    [Header("Map Settings")]
    public int mapWidth = 1000;
    public int mapHeight = 1000;

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

    public bool TryGetHex(HexCoordinates coord, out HexData hex) =>
        worldMap.TryGetValue(coord, out hex);

    public IEnumerable<HexData> GetAllHexes() => worldMap.Values;

    public void AssignNeighborReferences(HexData hex)
    {
        hex.neighborRefs.Clear();
        foreach (var coord in hex.neighborCoords)
        {
            if (worldMap.TryGetValue(coord, out var neighbor))
                hex.neighborRefs.Add(neighbor);
        }
    }

    public void AssignNeighborsForChunk(List<HexData> chunkHexes)
    {
        foreach (var hex in chunkHexes)
            AssignNeighborReferences(hex);
    }

   private TerrainType DetermineTerrainType(HexData hex)
{
    float elevation = hex.elevation;
    float slope = hex.slope;

    if (elevation < oceanThreshold) return TerrainType.Ocean;
    if (elevation < coastalWaterThreshold) return TerrainType.CoastalWater;

    if (elevation >= sandyBeachMinElevation && elevation < sandyBeachMaxElevation &&
        slope >= sandyBeachMinSlope && slope < sandyBeachMaxSlope)
        return TerrainType.SandyBeach;

    if (elevation >= rockyBeachMinElevation && elevation < rockyBeachMaxElevation &&
        slope >= rockyBeachMinSlope && slope < rockyBeachMaxSlope)
        return TerrainType.RockyBeach;

    if (elevation >= plainsMinElevation && elevation < plainsMaxElevation &&
        slope >= plainsMinSlope && slope < plainsMaxSlope)
        return TerrainType.Plains;

    if (elevation >= hillsMinElevation && elevation < hillsMaxElevation &&
        slope >= hillsMinSlope && slope < hillsMaxSlope)
        return TerrainType.Hills;

    if (elevation >= plateauMinElevation && elevation < plateauMaxElevation &&
        slope >= plateauMinSlope && slope < plateauMaxSlope)
        return TerrainType.Plateau;

    if (elevation >= mountainMinElevation && elevation < mountainMaxElevation &&
        slope >= mountainMinSlope && slope < mountainMaxSlope)
        return TerrainType.Mountain;

    if (elevation >= valleyMinElevation && elevation < valleyMaxElevation &&
        slope >= valleyMinSlope && slope < valleyMaxSlope)
        return TerrainType.Valley;

    return TerrainType.Valley; // Catch-all por defecto
}



    public static bool IsWater(TerrainType type) =>
        type == TerrainType.Ocean || type == TerrainType.CoastalWater;

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
}
