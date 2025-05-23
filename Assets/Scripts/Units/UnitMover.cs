using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[DisallowMultipleComponent]
public class UnitMover : MonoBehaviour
{
    public float moveSpeed = 5f;
    // Changed from string to LayerMask, assign this in the Inspector!
    public LayerMask terrainLayer; 
    [SerializeField] private float minHeightThreshold = 0.01f;

    private bool isMoving = false;
    private Queue<HexBehavior> currentPath = new();
    public HexBehavior currentHex;

    void Start()
    {
        SnapToHexVisualTop();
        currentHex = GetClosestHexBelow();
    }

    void LateUpdate() // Using LateUpdate to ensure all movement for the frame is done first
    {
        AdjustToGround();
    }

    void Update()
    {
        if (!isMoving && currentPath.Count > 0)
            StartCoroutine(FollowPath(currentPath));
    }

    public void MoveTo(HexBehavior targetHex)
    {
        if (currentHex == null || targetHex == null) return;

        List<HexBehavior> path = HexPathfinder.Instance.FindPath(currentHex, targetHex);

        if (path == null || path.Count == 0)
        {
            Debug.LogWarning("⚠️ No se pudo calcular ruta.");
            return;
        }

        HexPathfinder.Instance.debugPath = path;
        currentPath = new Queue<HexBehavior>(path);
    }

    private IEnumerator FollowPath(Queue<HexBehavior> path)
    {
        isMoving = true;

        while (path.Count > 0)
        {
            HexBehavior step = path.Dequeue();
            HexRenderer hex = step.GetComponent<HexRenderer>();
            float topY = hex.VisualTopY;

            Renderer rend = GetComponentInChildren<Renderer>();
            float objectBottom = rend.bounds.center.y - rend.bounds.extents.y;
            float adjustment = topY + (rend.bounds.extents.y - (transform.position.y - objectBottom));

            Vector3 target = new Vector3(
                step.transform.position.x,
                topY + (rend.bounds.extents.y - (transform.position.y - objectBottom)),
                step.transform.position.z
            );

            while (Vector3.Distance(transform.position, target) > 0.05f)
            {
                transform.position = Vector3.MoveTowards(transform.position, target, moveSpeed * Time.deltaTime);
                yield return null;
            }

            currentHex = step;
            yield return null;
        }

        isMoving = false;
    }

    private HexBehavior GetClosestHexBelow()
    {
        // Increased radius for more reliable detection
        Collider[] colliders = Physics.OverlapSphere(transform.position, 1.5f, terrainLayer); 
        foreach (var col in colliders)
        {
            var hex = col.GetComponentInParent<HexRenderer>();
            if (hex != null)
            {
                return hex.GetComponentInParent<HexBehavior>();
            }
        }
        return null;
    }

    private HexRenderer GetClosestHexBelowRenderer()
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

    private void SnapToHexVisualTop()
    {
        // Increased radius for more reliable detection
        Collider[] colliders = Physics.OverlapSphere(transform.position, 1.5f, terrainLayer); 
        foreach (var col in colliders)
        {
            var hex = col.GetComponentInParent<HexRenderer>();
            if (hex != null)
            {
                Renderer rend = GetComponentInChildren<Renderer>();
                if (rend == null) return;

                float topY = hex.VisualTopY;
                float objectBottom = rend.bounds.center.y - rend.bounds.extents.y;
                float adjustment = topY - objectBottom;

                if (Mathf.Abs(adjustment) > minHeightThreshold)
                {
                    transform.position += new Vector3(0f, adjustment, 0f);
                    Debug.DrawLine(transform.position, transform.position + Vector3.up * 1f, Color.cyan, 2f);
                }
                return;
            }
        }
    }

    private void AdjustToGround()
    {
        HexRenderer hex = GetClosestHexBelowRenderer();
        if (hex == null) return;

        Renderer rend = GetComponentInChildren<Renderer>();
        if (rend == null) return;

        float topY = hex.VisualTopY;
        float objectBottom = rend.bounds.center.y - rend.bounds.extents.y;
        float adjustment = topY - objectBottom;

        if (Mathf.Abs(adjustment) > minHeightThreshold)
        {
            transform.position += new Vector3(0f, adjustment, 0f);
            Debug.DrawLine(transform.position, transform.position + Vector3.down * 1f, Color.green, 0.1f);
        }
    }
}