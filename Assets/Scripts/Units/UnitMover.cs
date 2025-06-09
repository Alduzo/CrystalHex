using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitMover : MonoBehaviour
{
    public float moveSpeed = 2f;
    public float rotationSpeed = 5f;
    public float groundCheckRadius = 0.5f;
    public LayerMask groundLayer;

    private Queue<Vector3> path = new Queue<Vector3>();
    private Vector3 currentTarget;
    private bool isMoving = false;

    [SerializeField] private Transform footTransform;

    private void Update()
    {
        if (isMoving)
        {
            MoveAlongPath();
        }
    }

    public void MoveTo(Vector3 destination)
    {
        path = new Queue<Vector3>(new List<Vector3> { destination });
        currentTarget = path.Dequeue();
        isMoving = true;
    }

    public void SetPath(List<Vector3> waypoints)
    {
        path = new Queue<Vector3>(waypoints);
        if (path.Count > 0)
        {
            currentTarget = path.Dequeue();
            isMoving = true;
        }
    }

    private void MoveAlongPath()
    {
        Vector3 direction = (currentTarget - transform.position).normalized;
        float distance = Vector3.Distance(transform.position, currentTarget);

        if (distance > 0.1f)
        {
            transform.position += direction * moveSpeed * Time.deltaTime;
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
        else
        {
            if (path.Count > 0)
            {
                currentTarget = path.Dequeue();
            }
            else
            {
                isMoving = false;
            }
        }
    }

    private void LateUpdate()
    {
        AdjustToGround();
    }

    private void AdjustToGround()
{
    Vector3 pos = transform.position;

    // 1️⃣ Saber en qué hex estamos (para lógica de juego)
    HexCoordinates hexCoord = HexCoordinates.FromWorldPosition(pos, HexRenderer.SharedOuterRadius);

    // 2️⃣ Obtener HexRenderer real debajo (para alineación visual)
    HexRenderer hex = TerrainUtils.GetHexBelow(footTransform, 2f, groundLayer);

    if (hex != null)
    {
        TerrainUtils.SnapToHexTopFlat(transform, hex, 0.01f);
    }
    else
    {
        // Fallback si no hay hex cargado visualmente (e.g. al borde del mapa)
        float tileTopY = HexRenderer.GetVisualTopYAt(hexCoord);
        Renderer rend = GetComponentInChildren<Renderer>();
        float objectBottom = rend != null ? rend.bounds.center.y - rend.bounds.extents.y : pos.y;
        float adjustment = tileTopY - objectBottom;

        if (Mathf.Abs(adjustment) > 0.01f)
        {
            transform.position = new Vector3(pos.x, pos.y + adjustment, pos.z);
        }
    }
}

}
