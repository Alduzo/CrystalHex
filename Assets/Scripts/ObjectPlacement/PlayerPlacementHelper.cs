using UnityEngine;
using System.Collections;

public class PlayerPlacementHelper : MonoBehaviour
{
    [SerializeField] private string terrainLayerName = "Terrain";
    [SerializeField] private float heightOffset = 0.1f;
    [SerializeField] private int maxAttempts = 30;
    [SerializeField] private float retryDelay = 0.1f;
    [SerializeField] private float placementDetectionRadius = .75f;

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
    var hex = TerrainUtils.GetHexBelow(transform, 2f, LayerMask.GetMask(terrainLayerName));
    if (hex != null)
    {
        TerrainUtils.SnapToHexTopFlat(transform, hex, heightOffset);
        return true;
    }

    Debug.LogWarning($"{name} no encontró hex debajo usando Raycast.");
    return false;
}

}
