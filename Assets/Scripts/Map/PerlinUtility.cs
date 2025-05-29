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
