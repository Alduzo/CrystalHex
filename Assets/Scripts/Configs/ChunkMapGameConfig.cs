using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ChunkMapGameConfig", menuName = "Configs/Chunk Map Game Config")]
public class ChunkMapGameConfig : ScriptableObject
{
    [System.Serializable]
    public struct TerrainMaterialPair
    {
        public TerrainType terrainType;
        public Material material;
    }

    public TerrainMaterialPair[] terrainMaterials;

    public float elevationScale = 1f;

    private Dictionary<TerrainType, Material> _materialMap;

    public Material GetMaterialFor(TerrainType type)
    {
        if (_materialMap == null)
        {
            _materialMap = new Dictionary<TerrainType, Material>();
            foreach (var pair in terrainMaterials)
            {
                _materialMap[pair.terrainType] = pair.material;
            }
        }

        return _materialMap.TryGetValue(type, out var mat) ? mat : null;
    }
    public Color GetColorFor(TerrainType terrain)
{
    var material = GetMaterialFor(terrain);
    if (material != null)
    {
        return material.color;
    }
    else
    {
        Debug.LogWarning($"⚠️ Material no encontrado para {terrain}. Usando color por defecto.");
        switch (terrain)
        {
            case TerrainType.Ocean:
                return Color.blue;
            case TerrainType.CoastalWater:
                return Color.cyan;
            case TerrainType.SandyBeach:
                return Color.yellow;
            case TerrainType.RockyBeach:
                return Color.gray;
            case TerrainType.Plains:
                return Color.green;
            case TerrainType.Hills:
                return new Color(0.5f, 0.8f, 0.3f);
            case TerrainType.Plateau:
                return Color.Lerp(Color.green, Color.gray, 0.5f);
            case TerrainType.Mountain:
                return Color.gray;
            case TerrainType.Valley:
                return Color.Lerp(Color.green, Color.yellow, 0.5f);
            default:
                return Color.white;
        }
    }
}


}
