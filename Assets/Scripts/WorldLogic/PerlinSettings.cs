using UnityEngine;

[CreateAssetMenu(menuName = "World/PerlinSettings")]
public class PerlinSettings : ScriptableObject
{
    public float elevationFreq = 0.02f;
    public int elevationSeedOffset = 1000;

    public float moistureFreq = 0.03f;
    public int moistureSeedOffset = 2000;

    public float tempFreq = 0.015f;
    public int tempSeedOffset = 3000;
}
