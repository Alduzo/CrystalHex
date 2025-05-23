using UnityEngine;

public class PerlinSettingsController : MonoBehaviour
{
    public PerlinSettings perlinSettings;

    [Range(0.001f, 15f)] public float elevationFreq = 0.02f;
    [Range(0.001f, 1f)] public float moistureFreq = 0.03f;
    [Range(0.001f, 1f)] public float tempFreq = 0.015f;

    public int elevationSeedOffset = 1000;
    public int moistureSeedOffset = 2000;
    public int tempSeedOffset = 3000;

    void OnValidate()
    {
        if (perlinSettings != null)
        {
            perlinSettings.elevationFreq = elevationFreq;
            perlinSettings.moistureFreq = moistureFreq;
            perlinSettings.tempFreq = tempFreq;

            perlinSettings.elevationSeedOffset = elevationSeedOffset;
            perlinSettings.moistureSeedOffset = moistureSeedOffset;
            perlinSettings.tempSeedOffset = tempSeedOffset;
        }
    }

    void Start()
    {
        if (perlinSettings != null)
        {
            elevationFreq = perlinSettings.elevationFreq;
            moistureFreq = perlinSettings.moistureFreq;
            tempFreq = perlinSettings.tempFreq;

            elevationSeedOffset = perlinSettings.elevationSeedOffset;
            moistureSeedOffset = perlinSettings.moistureSeedOffset;
            tempSeedOffset = perlinSettings.tempSeedOffset;
        }
    }

    [Header("Anomaly Settings")]
    [Range(0f, 1f)]
    public float anomalyStrength = 0.25f;

    [Range(0f, 1f)]
    public float anomalyThreshold = 0.15f;

    public float anomalyFrequency = 0.1f;
    public int anomalySeedOffset = 5000;

}
