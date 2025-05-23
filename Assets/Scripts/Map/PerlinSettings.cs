using UnityEngine;

[CreateAssetMenu(menuName = "World/PerlinSettings")]
public class PerlinSettings : ScriptableObject
{
    public float elevationFreq = 0.02f;
    public int elevationSeedOffset = 1000;

[Header("Anomaly Settings")]
    [Range(0f, 1f)] public float anomalyThreshold = 0.15f;
    [Range(0f, 1f)] public float anomalyStrength = 0.25f;
    public float anomalyFrequency = 0.1f;
    public int anomalySeedOffset = 5000;
    public float moistureFreq = 0.03f;
    public int moistureSeedOffset = 2000;

    public float tempFreq = 0.015f;
    public int tempSeedOffset = 3000;


}
