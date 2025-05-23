using UnityEngine;
using UnityEditor;

public class HexTileValidator : MonoBehaviour
{
    [MenuItem("Tools/Validate HexTiles In Scene")]
    public static void ValidateHexTiles()
    {
        var tiles = FindObjectsOfType<HexRenderer>();
        Debug.Log($"🔍 Validando {tiles.Length} HexTiles...");

        foreach (var tile in tiles)
        {
            var go = tile.gameObject;
            string name = go.name;

            if (go.layer != LayerMask.NameToLayer("Terrain"))
                Debug.LogWarning($"⚠️ {name} NO está en la capa Terrain (está en {LayerMask.LayerToName(go.layer)})");

            var mf = go.GetComponent<MeshFilter>();
            if (mf == null || mf.sharedMesh == null)
                Debug.LogWarning($"❌ {name} no tiene MeshFilter válido");

            var mc = go.GetComponent<MeshCollider>();
            if (mc == null || mc.sharedMesh == null)
                Debug.LogWarning($"❌ {name} no tiene MeshCollider válido");

            if (mf != null && mf.sharedMesh != null && mc != null && mc.sharedMesh != null)
                Debug.Log($"✅ {name} tiene collider y mesh correctamente configurados.");
        }
    }
}
