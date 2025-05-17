using UnityEngine;

public static class PerlinUtility
{
    public static float Perlin(HexCoordinates coord, float frequency, int seedOffset)
    {
        float nx = (coord.Q + seedOffset) * frequency;
        float ny = (coord.R + seedOffset) * frequency;
        return Mathf.PerlinNoise(nx, ny);
    }
}
