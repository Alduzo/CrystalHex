using UnityEngine;

public class UnitighlightFollower : MonoBehaviour
{
    [SerializeField] private Transform target; // Jugador u objeto a seguir
    // Changed from string to LayerMask, assign this in the Inspector!
    public LayerMask terrainLayer; 
    [SerializeField] private float detectionRadius = 1.5f; // Increased radius for more reliable detection
    [SerializeField] private float verticalOffset = 0.01f;

    private void LateUpdate()
    {
        if (target == null) return;

        HexRenderer hex = GetClosestHexBelow(target.position);
        if (hex != null)
        {
            TerrainUtils.SnapToHexTopFlat(transform, hex, verticalOffset);
            Debug.DrawLine(transform.position, transform.position + Vector3.up, Color.magenta, 0.2f);
        }
    }

    private HexRenderer GetClosestHexBelow(Vector3 position)
    {
        Collider[] hits = Physics.OverlapSphere(position, detectionRadius, terrainLayer);
        foreach (var col in hits)
        {
            HexRenderer hex = col.GetComponentInParent<HexRenderer>();
            if (hex != null) return hex;
        }
        return null;
    }
}