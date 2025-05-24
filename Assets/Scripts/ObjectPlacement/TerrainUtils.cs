// 📁 TerrainUtils.cs
using UnityEngine;

public static class TerrainUtils
{
    /// <summary>
    /// Snaps a transform to the visual top of a hexagonal tile.
    /// Adjusts the Y-position to be on top of the hex, plus an optional vertical offset.
    /// This method calculates the offset from the object's pivot to its visual bottom.
    /// </summary>
    /// <param name="objectTransform">The transform of the object to snap.</param>
    /// <param name="hexRenderer">The HexRenderer of the target hex.</param>
    /// <param name="verticalOffset">Additional offset above the hex's visual top.</param>
    public static void SnapToHexTopFlat(Transform objectTransform, HexRenderer hexRenderer, float verticalOffset)
{
    if (objectTransform == null)
    {
        Debug.LogWarning("⚠️ SnapToHexTopFlat falló: objectTransform es null.");
        return;
    }

    if (hexRenderer == null)
    {
        Debug.LogWarning($"⚠️ SnapToHexTopFlat falló: hexRenderer es null para {objectTransform.name}.");
        return;
    }

    Renderer objectRenderer = objectTransform.GetComponentInChildren<Renderer>();
    if (objectRenderer == null)
    {
        Debug.LogWarning($"❌ {objectTransform.name} no tiene Renderer hijo visible. No se puede alinear.");
        return;
    }

    float objectBottomOffset = objectRenderer.bounds.center.y - objectRenderer.bounds.extents.y - objectTransform.position.y;
    float targetY = hexRenderer.VisualTopY - objectBottomOffset + verticalOffset;

    Vector3 hexPos = hexRenderer.transform.position;

    objectTransform.position = new Vector3(
        hexPos.x,
        targetY,
        hexPos.z
    );

    Debug.Log($"📍 {objectTransform.name} alineado a ({hexPos.x:F2}, {targetY:F2}, {hexPos.z:F2}) sobre {hexRenderer.name} (VisualTopY={hexRenderer.VisualTopY:F3}, offset={verticalOffset:F3})");
}



    /// <summary>
    /// Calculates the world position of a hex's center at a given Y-height.
    /// </summary>
    /// <param name="coordinates">The HexCoordinates of the hex.</param>
    /// <param name="yHeight">The desired Y-coordinate (e.g., hex.VisualTopY).</param>
    /// <param name="outerRadius">The outer radius of the hex, typically HexRenderer.SharedOuterRadius.</param>
    /// <returns>The Vector3 world position.</returns>
    public static Vector3 GetHexWorldPosition(HexCoordinates coordinates, float yHeight, float outerRadius)
    {
        Vector3 worldPos = HexCoordinates.ToWorldPosition(coordinates, outerRadius);
        worldPos.y = yHeight;
        return worldPos;
    }

    public static void SnapToHexCenterY(Transform objectTransform, HexRenderer hexRenderer, float verticalOffset)
    {
        if (objectTransform == null || hexRenderer == null)
        {
            Debug.LogWarning("⚠️ SnapToHexCenterY falló: Transform o HexRenderer es null.");
            return;
        }

        Vector3 pos = objectTransform.position;
        pos.y = hexRenderer.transform.position.y + verticalOffset;
        objectTransform.position = pos;

        Debug.Log($"📍 {objectTransform.name} colocado en Y={pos.y:F2} sobre {hexRenderer.name}.");
    }

public static void SnapToHexCenterXYZ(Transform objectTransform, HexRenderer hexRenderer, float verticalOffset)
{
    if (objectTransform == null || hexRenderer == null)
    {
        Debug.LogWarning("⚠️ SnapToHexCenterXYZ falló: Transform o HexRenderer es null.");
        return;
    }

    Vector3 hexPos = hexRenderer.transform.position;
    Vector3 newPos = new Vector3(hexPos.x, hexPos.y + verticalOffset, hexPos.z);
    objectTransform.position = newPos;

    Debug.Log($"📍 {objectTransform.name} colocado en ({newPos.x:F2}, {newPos.y:F2}, {newPos.z:F2}) sobre {hexRenderer.name}.");
}

/// <summary>
/// Devuelve la altura visual superior del hexágono, considerando su escala y altura base.
/// </summary>
public static float GetHexVisualTopY(HexRenderer hexRenderer, float verticalOffset = 0f)
{
    if (hexRenderer == null)
    {
        Debug.LogWarning("⚠️ HexRenderer es null en GetHexVisualTopY.");
        return 0f;
    }

    return hexRenderer.transform.position.y + hexRenderer.columnHeight * hexRenderer.heightScale + verticalOffset;
}


}