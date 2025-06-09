using UnityEngine;

public static class HexMetrics
{
    public const float outerRadius = 1f;
    public const float innerRadius = outerRadius * 0.866025404f;

    public const float elevationStep = 1f;
    public const int terracesPerSlope = 2;
    public const int terraceSteps = terracesPerSlope * 2 + 1;
    public const float horizontalTerraceStepSize = 1f / terraceSteps;
    public const float verticalTerraceStepSize = 1f / (terracesPerSlope + 1);

   public static Vector3[] corners = {
    new Vector3(outerRadius, 0f, 0f),
    new Vector3(0.5f * outerRadius, 0f, innerRadius),
    new Vector3(-0.5f * outerRadius, 0f, innerRadius),
    new Vector3(-outerRadius, 0f, 0f),
    new Vector3(-0.5f * outerRadius, 0f, -innerRadius),
    new Vector3(0.5f * outerRadius, 0f, -innerRadius),
    new Vector3(outerRadius, 0f, 0f) // Repetido para cerrar el ciclo
};


    public static Vector3 TerraceLerp(Vector3 a, Vector3 b, int step)
    {
        float h = step * horizontalTerraceStepSize;
        float v = ((step + 1) / 2) * verticalTerraceStepSize;
        return new Vector3(
            Mathf.Lerp(a.x, b.x, h),
            Mathf.Lerp(a.y, b.y, v),
            Mathf.Lerp(a.z, b.z, h)
        );
    }

    public static Color TerraceLerp(Color a, Color b, int step)
    {
        float h = step * horizontalTerraceStepSize;
        return Color.Lerp(a, b, h);
    }

    public static Vector3 GetFirstCorner(HexDirection direction)
    {
        return corners[(int)direction];
    }

    public static Vector3 GetSecondCorner(HexDirection direction)
    {
        return corners[(int)direction + 1];
    }

    public static Vector3 GetBridge(HexDirection direction)
    {
        return (corners[(int)direction] + corners[(int)direction + 1]) * 0.5f;
    }

    public static Vector3 Perturb(Vector3 position)
    {
        float sampleX = position.x * 0.1f + 100f;
        float sampleZ = position.z * 0.1f + 100f;
        float offsetX = (Mathf.PerlinNoise(sampleX, sampleZ) * 2f - 1f) * 0.1f;  // Ajusta magnitud
        float offsetZ = (Mathf.PerlinNoise(sampleZ, sampleX) * 2f - 1f) * 0.1f;
        position.x += offsetX;
        position.z += offsetZ;
        return position;
    }
    public static Color GetColorByTerrainType(TerrainType type)
    {
        switch (type)
        {
            case TerrainType.Plains: return Color.green;
            case TerrainType.Mountain: return Color.gray;
            // Agrega más casos según necesidad
            default: return Color.magenta;  // O un color por defecto
        }
    }



}
