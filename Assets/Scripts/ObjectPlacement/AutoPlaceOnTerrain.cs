using UnityEngine;
using System.Collections;

[DisallowMultipleComponent]
public class AutoPlaceOnTerrain : MonoBehaviour
{
    [SerializeField] private string terrainLayerName = "Terrain";
    [SerializeField] private float heightOffset = 0.5f;
    [SerializeField] private int maxAttempts = 30;
    [SerializeField] private float retryDelay = 0.1f;
    [SerializeField] private bool debug = false;

    private IEnumerator Start()
    {
        int attempts = 0;

        while (attempts < maxAttempts)
        {
            if (TryPlaceOnTerrain())
            {
                if (debug) Debug.Log($"✅ {gameObject.name} colocado sobre el terreno.");
                yield break;
            }

            attempts++;
            yield return new WaitForSeconds(retryDelay);
        }

        Debug.LogWarning($"⚠️ {gameObject.name} no pudo colocarse sobre el terreno tras {maxAttempts} intentos.");
    }

    private bool TryPlaceOnTerrain()
    {
        Vector3 probePosition = new Vector3(transform.position.x, 100f, transform.position.z);
        if (Physics.Raycast(probePosition, Vector3.down, out RaycastHit hit, 200f, LayerMask.GetMask(terrainLayerName)))
        {
            transform.position = new Vector3(
                transform.position.x,
                hit.collider.bounds.max.y + heightOffset,
                transform.position.z
            );
            return true;
        }

        return false;
    }
}
