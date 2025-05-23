// 📁 HexObjectPlacer.cs
using UnityEngine;

public static class HexObjectPlacer
{
    public static float PlacementOffset = 0.01f;

    public static void PlaceOnHex(HexBehavior hex, GameObject prefab)
    {
        if (hex == null || prefab == null)
        {
            Debug.LogWarning("❌ HexBehavior o prefab nulo al intentar colocar objeto.");
            return;
        }

        GameObject instance = Object.Instantiate(prefab, Vector3.zero, Quaternion.identity);
        instance.name = $"Feature_{hex.coordinates.Q}_{hex.coordinates.R}";
        instance.transform.SetParent(hex.transform);
        instance.transform.rotation = Quaternion.identity;

        // FIX: Changed to SnapToHexTopFlat
        TerrainUtils.SnapToHexTopFlat(instance.transform, hex.GetComponent<HexRenderer>(), PlacementOffset);

        Debug.Log($"🌳 Objeto instanciado sobre {hex.name} en {instance.transform.position}");
    }
}