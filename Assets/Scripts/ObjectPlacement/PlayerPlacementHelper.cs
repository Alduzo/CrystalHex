using UnityEngine;
using System.Collections;

public class PlayerPlacementHelper : MonoBehaviour
{
    [SerializeField] private string terrainLayerName = "Terrain";
    [SerializeField] private float heightOffset = 0.5f;
    [SerializeField] private int maxAttempts = 30;
    [SerializeField] private float retryDelay = 0.1f;

    private IEnumerator Start()
    {
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

        Debug.LogWarning($"⚠️ {gameObject.name} no pudo colocarse sobre el terreno tras {maxAttempts} intentos.");
    }

    public bool TryPlace()
    {
        Vector3 probePosition = new Vector3(transform.position.x, 100f, transform.position.z);
        if (Physics.Raycast(probePosition, Vector3.down, out RaycastHit hit, 200f, LayerMask.GetMask(terrainLayerName)))
        {
            Debug.Log($"🔍 Raycast hit {hit.collider.name} at {hit.point}, bounds.max.y = {hit.collider.bounds.max.y}");

            transform.position = new Vector3
            (
                transform.position.x,
                hit.collider.bounds.max.y + heightOffset,
                transform.position.z
            );

            return true;
        }

        return false;
    }
}
