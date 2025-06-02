using UnityEngine;

[System.Serializable]
public class PerlinSettings : MonoBehaviour
{


public float globalFreq = 0.001f;
public float globalAmplitude = 30f;

    [Header("Base Elevation (Continente)")]


    [Range(0.01f, 1f)]
    public float continentFreq = 0.02f;

    [Range(0f, 5f)]
    public float continentalFlattenFactor = 0.5f;  // Valor inicial 0.5
    public float baseFreq = 0.005f;            // Frecuencia base para el continente
    public float baseOffset = 50f;
    public float regionalNoise = 10f;

    public float baseAmplitude = 150f;          // Altura máxima del continente
    public int octaves = 6;                    // Octavas para el fractal Perlin

    public float lacunarity = 2.5f;            // Factor de frecuencia
    public float persistence = 0.4f;           // Factor de amplitud

    [Header("Montañas (FractalPerlin)")]
public float mountainFreq = 0.02f;  // Frecuencia para FractalPerlin
public float mountainAmplitude = 50f;  // Magnitud máxima del efecto montaña


    [Header("Montañas (Ridge Noise)")]
    public float ridgeFreq = 0.02f;            // Frecuencia del Ridge (montañas)
    public float ridgeAmplitude = 30f;         // Altura máxima de montañas
    public float mountainThreshold = 0.6f;     // Umbral para que empiecen las montañas

    [Header("Ríos (Inversión de Perlin o Worley)")]
    public float riverFreq = 0.01f;            // Frecuencia del ruido de ríos
    public float riverDepth = 10f;             // Profundidad de los ríos

    [Header("Otros")]
    public int seed = 200500;                  // Semilla para variabilidad
    public float moistureFreq = 0.03f;         // Opcional, para humedad
    public float tempFreq = 0.015f;            // Opcional, para temperatura

[Range(0.001f, 1f)]
public float waterFreq = 0.02f;
    [Range(0.1f, 10f)]
    public float waterAmplitude = 1f;  // Opcional, para controlar la "altura" o "cantidad"

public float waterLevel = 1f;  // Opcional, para controlar la "altura" o "cantidad"


    [Header("Anomaly Settings (Opcional)")]
    [Range(0f, 1f)] public float anomalyThreshold = 0.15f;
    [Range(0f, 1f)] public float anomalyStrength = 0.25f;
    public float anomalyFrequency = 0.01f;
}
