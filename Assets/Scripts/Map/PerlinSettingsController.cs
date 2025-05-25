using UnityEngine;

// Control visual para editar PerlinSettings desde el inspector.
public class PerlinSettingsController : MonoBehaviour
{
    [Header("Perlin Settings Asset")]
    public PerlinSettings perlinSettings; // Asigna aquí el asset en el inspector

    [Header("Editable Settings")]
    [Range(0.001f, 15f)] public float elevationFreq = 0.02f;
    [Range(0.001f, 1f)] public float moistureFreq = 0.03f;
    [Range(0.001f, 1f)] public float tempFreq = 0.015f;
    public int seed = 1000;

    [Header("Perlin Fractal Settings")]
    [Range(1, 10)] public int octaves = 6;
    [Range(1f, 4f)] public float lacunarity = 2.5f;
    [Range(0.1f, 1f)] public float persistence = 0.4f;

    [Header("Anomaly Settings")]
    [Range(0f, 1f)] public float anomalyStrength = 0.25f;
    [Range(0f, 1f)] public float anomalyThreshold = 0.15f;
    public float anomalyFrequency = 0.1f;

    void OnValidate()
    {
        if (perlinSettings != null)
        {
            perlinSettings.elevationFreq = elevationFreq;
            perlinSettings.moistureFreq = moistureFreq;
            perlinSettings.tempFreq = tempFreq;
            perlinSettings.seed = seed;

            // Aplicar parámetros avanzados de Perlin
            perlinSettings.octaves = octaves;
            perlinSettings.lacunarity = lacunarity;
            perlinSettings.persistence = persistence;

            perlinSettings.anomalyStrength = anomalyStrength;
            perlinSettings.anomalyThreshold = anomalyThreshold;
            perlinSettings.anomalyFrequency = anomalyFrequency;

#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(perlinSettings);
#endif
        }
    }
}
