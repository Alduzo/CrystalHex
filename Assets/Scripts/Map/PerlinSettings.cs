using UnityEngine;

// Configuración de parámetros centralizados como asset.
[CreateAssetMenu(menuName = "World/PerlinSettings")]
public class PerlinSettings : ScriptableObject
{
    public float elevationFreq = 6f;              // Frecuencia para elevación
    public float moistureFreq = 0.03f;           // Frecuencia para humedad
    public float tempFreq = 0.015f;              // Frecuencia para temperatura
    public int seed = 200500;                    // Semilla para PerlinNoise

    [Header("Perlin Fractal Settings")]
    public int octaves = 8;                      // Cantidad de octavas
    public float lacunarity = 2.5f;              // Lacunaridad
    public float persistence = 0.4f;             // Persistencia

    [Header("Anomaly Settings")]
    [Range(0f, 1f)] public float anomalyThreshold = 0.15f;   // Umbral de anomalía
    [Range(0f, 1f)] public float anomalyStrength = 0.25f;    // Fuerza de anomalía
    public float anomalyFrequency = 0.1f;                    // Frecuencia de anomalía
}
