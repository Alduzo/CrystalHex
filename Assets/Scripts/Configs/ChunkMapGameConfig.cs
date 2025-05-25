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
}
