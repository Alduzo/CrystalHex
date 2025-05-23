using UnityEngine;
using System.Collections;

public class PlayerPlacementHelper : MonoBehaviour
{
    [SerializeField] private string terrainLayerName = "Terrain";
    [SerializeField] private float heightOffset = 0.3f;
    [SerializeField] private int maxAttempts = 30;
    [SerializeField] private float retryDelay = 0.1f;
    [SerializeField] private float placementDetectionRadius = 5.0f;

    private IEnumerator Start()
    {
        yield return new WaitUntil(() => WorldMapManager.Instance != null);
        yield return new WaitUntil(() => ChunkManager.Instance != null);
        yield return new WaitUntil(() => ChunkManager.Instance.loadedChunks.Count > 0);
        yield return new WaitForSeconds(3f); // Let hexes initialize

        int attempts = 0;
        while (attempts < maxAttempts)
        {
            if (TryPlace())
            {
                Debug.Log($"✅ {gameObject.name} colocado sobre el terreno.");
                yield break;
            }

            attempts++;
            yield return new WaitForSeconds(retryDelay);
        }

        Debug.LogWarning($"⚠️ {gameObject.name} no pudo colocarse sobre ningún Hex válido tras {maxAttempts} intentos.");
    }

    public bool TryPlace()
    {
        Debug.Log($"📌 {name} está intentando colocarse desde posición: {transform.position}");

        Collider[] colliders = Physics.OverlapSphere(transform.position, placementDetectionRadius, LayerMask.GetMask(terrainLayerName));
        Debug.Log($"🔎 {name}: {colliders.Length} colisionadores detectados en layer {terrainLayerName}");

        foreach (var col in colliders)
        {
            Debug.Log($" - 🎯 Collider: {col.name}");

            var hex = col.GetComponentInParent<HexRenderer>();
            if (hex == null)
            {
                Debug.Log($" - ⛔ No es HexRenderer");
                continue;
            }

            Debug.Log($" - ✅ HexRenderer válido: {hex.name}");

            TerrainUtils.SnapToHexCenterXYZ(transform, hex, heightOffset);
            return true;
        }

        return false;
    }
}
