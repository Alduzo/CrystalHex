using UnityEngine;

// Configuración de parámetros centralizados como asset.

[CreateAssetMenu(menuName = "World/PerlinSettings")]
public class PerlinSettings : ScriptableObject
{
    public float elevationFreq = 0.02f;
    public float moistureFreq = 0.03f;
    public float tempFreq = 0.015f;
    public int seed = 1000;

    [Header("Perlin Fractal Settings")]
    public int octaves = 6;
    public float lacunarity = 2.5f;
    public float persistence = 0.4f;

    [Header("Anomaly Settings")]
    [Range(0f, 1f)] public float anomalyThreshold = 0.15f;
    [Range(0f, 1f)] public float anomalyStrength = 0.25f;
    public float anomalyFrequency = 0.1f;
}
