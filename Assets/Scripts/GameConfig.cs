using UnityEngine;

[CreateAssetMenu(fileName = "GameConfig", menuName = "Configs/GameConfig")]
public class GameConfig : ScriptableObject
{
    [Header("Terrain Generation")]
    public int initialRadius = 5;
    public int maxExpansionRadius = 10;
    public int maxTiles = 500;

    [Header("Map Bounds (Used for Expansion)")]
    public int minX = -10;
    public int maxX = 10;
    public int minY = -10;
    public int maxY = 10;

    [Header("Debug Options")]
    public bool enableDebugLabels = true;
}
