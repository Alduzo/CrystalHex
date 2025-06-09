using UnityEngine;

[DisallowMultipleComponent]
public class AutoPlaceOnTerrain : MonoBehaviour
{
    [SerializeField] private string terrainLayerName = "Terrain";
    [SerializeField] private float heightOffset = 0.25f;
    [SerializeField] private float placementDetectionRadius = .750f;
    [SerializeField] private bool debug = false;

    public bool TryPlace()
    {
        Debug.Log($"📌 {name} está intentando colocarse desde posición: {transform.position}");

        Collider[] colliders = Physics.OverlapSphere(transform.position, placementDetectionRadius, LayerMask.GetMask(terrainLayerName));
        Debug.Log($"🔎 {name}: {colliders.Length} colisionadores detectados en layer {terrainLayerName}");

        foreach (var col in colliders)
        {
            Debug.Log($" - 🎯 Collider: {col.name}");

            HexRenderer hex = col.GetComponentInParent<HexRenderer>();
            if (hex == null)
            {
                Debug.Log($" - ⛔ No es HexRenderer");
                continue;
            }

            Debug.Log($" - ✅ HexRenderer válido: {hex.name}");

            TerrainUtils.SnapToHexCenterXYZ(transform, hex, heightOffset);
            return true;
        }

        Debug.LogWarning($"⚠️ {name} no pudo colocarse sobre ningún Hex válido.");
        return false;
    }
}
