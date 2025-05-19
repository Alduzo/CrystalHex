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
