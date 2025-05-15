using UnityEngine;

public enum MapShape { Square, Hexagonal, Random }  // ✅ Ahora correctamente declarado FUERA de la clase

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

    [Header("Visual Diversity")]
    public MapShape mapShape = MapShape.Hexagonal; // ✅ Usa el enum declarado fuera
    [Range(0.01f, 0.2f)]
    public float terrainDiversity = 0.05f;

    [Header("Multipliers")]
    [Range(0f, 1f)] public float slowTilePercent = 0.05f;
    [Range(0f, 1f)] public float fastTilePercent = 0.05f;

    [Header("Debug Options")]
    public bool enableDebugLabels = true;

    [Header("Terrain Materials")]
    [SerializeField]
    public Material[] terrainMaterials;

}
