using UnityEngine;

[DisallowMultipleComponent]
public class UnitGroundFollower : MonoBehaviour
{
    // Changed from string to LayerMask, assign this in the Inspector!
    public LayerMask terrainLayer; 
    [SerializeField] private float minHeightThreshold = 0.01f; 


    private void LateUpdate()
    {
        HexRenderer hex = GetClosestHexBelow();
        if (hex == null) return;

        Renderer rend = GetComponentInChildren<Renderer>();
        float topY = hex.VisualTopY;

        float objectBottom = rend.bounds.center.y - rend.bounds.extents.y;
        float adjustment = topY - objectBottom;

        // Solo ajustar si hay diferencia real
        if (Mathf.Abs(adjustment) > minHeightThreshold)
        {
            transform.position += new Vector3(0f, adjustment, 0f);
            Debug.DrawLine(transform.position, transform.position + Vector3.down * 1f, Color.magenta, 2f);
        }
    }

    private HexRenderer GetClosestHexBelow()
    {
        // Increased radius for more reliable detection
        Collider[] hits = Physics.OverlapSphere(transform.position, 1.5f, terrainLayer); 
        foreach (var col in hits)
        {
            var hex = col.GetComponentInParent<HexRenderer>();
            if (hex != null) return hex;
        }

        return null;
    }
}