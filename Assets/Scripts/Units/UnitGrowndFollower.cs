using UnityEngine;

[DisallowMultipleComponent]
public class UnitGroundFollower : MonoBehaviour
{
    public LayerMask terrainLayer;
    [SerializeField] private float minHeightThreshold = 0.01f;

    private void LateUpdate()
    {
        Vector3 pos = transform.position;
        HexCoordinates hexCoord = HexCoordinates.FromWorldPosition(pos, HexRenderer.SharedOuterRadius);
        float topY = HexRenderer.GetVisualTopYAt(hexCoord);

        Renderer rend = GetComponentInChildren<Renderer>();
        float objectBottom = rend != null ? rend.bounds.center.y - rend.bounds.extents.y : pos.y;
        float adjustment = topY - objectBottom;

        if (Mathf.Abs(adjustment) > minHeightThreshold)
        {
            transform.position = new Vector3(pos.x, pos.y + adjustment, pos.z);
            Debug.DrawLine(transform.position, transform.position + Vector3.down * 1f, Color.magenta, 2f);
        }
    }
}
