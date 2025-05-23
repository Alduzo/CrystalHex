using UnityEngine;

public class TerrainPlacementHelper : MonoBehaviour
{
    [Header("Configuración")]
    public LayerMask terrainLayer; // Asegúrate que el terreno esté en este layer
    public float placementHeightOffset = 0.5f;

    public bool PlacePrefabOnTerrain(GameObject prefab, Vector3 targetXZPosition)
    {
        // Dispara un rayo desde arriba para encontrar el punto sobre el terreno
        Ray ray = new Ray(new Vector3(targetXZPosition.x, 100f, targetXZPosition.z), Vector3.down);
        if (Physics.Raycast(ray, out RaycastHit hit, 200f, terrainLayer))
        {
            Vector3 placePos = hit.point + Vector3.up * placementHeightOffset;
            Instantiate(prefab, placePos, Quaternion.identity);
            return true;
        }

        Debug.LogWarning("No se encontró terreno en esa posición.");
        return false;
    }
}
