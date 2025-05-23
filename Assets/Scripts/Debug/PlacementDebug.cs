using UnityEngine;

public class PlacementDebug : MonoBehaviour
{
    void Start()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, 5.0f, LayerMask.GetMask("Terrain"));
        Debug.Log($"🧪 [{name}] ve {colliders.Length} colisionadores de terreno.");
        foreach (var col in colliders)
        {
            Debug.Log($" - Hex: {col.name}, MeshCollider: {col.GetComponent<MeshCollider>() != null}");
        }

        var rend = GetComponentInChildren<Renderer>();
        if (rend == null)
        {
            Debug.LogWarning($"❌ {name} no tiene Renderer hijo visible.");
        }
        else
        {
            Debug.Log($"✅ {name} tiene Renderer hijo activo: {rend.gameObject.name}");
        }
    }
}
