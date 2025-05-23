using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    public float moveCooldown = 0.2f;
    private float lastMoveTime;

    public HexCoordinates currentCoordinates;
    public float outerRadius = 1f;

    private Vector2Int currentChunkCoord;

    void Start()
    {
        currentCoordinates = HexCoordinates.FromWorldPosition(transform.position, outerRadius);
        transform.position = HexCoordinates.ToWorldPosition(currentCoordinates, outerRadius);
        UpdateChunkLoading(true);
    }

    void Update()
    {
        HandleKeyboardMovement();
        UpdateChunkLoading();
    }

    void HandleKeyboardMovement()
    {
        if (Time.time - lastMoveTime < moveCooldown)
            return;

        if (Input.GetKeyDown(KeyCode.W)) MoveTo(currentCoordinates.GetNeighbor(HexDirection.NE));
        if (Input.GetKeyDown(KeyCode.E)) MoveTo(currentCoordinates.GetNeighbor(HexDirection.E));
        if (Input.GetKeyDown(KeyCode.D)) MoveTo(currentCoordinates.GetNeighbor(HexDirection.SE));
        if (Input.GetKeyDown(KeyCode.S)) MoveTo(currentCoordinates.GetNeighbor(HexDirection.SW));
        if (Input.GetKeyDown(KeyCode.A)) MoveTo(currentCoordinates.GetNeighbor(HexDirection.W));
        if (Input.GetKeyDown(KeyCode.Q)) MoveTo(currentCoordinates.GetNeighbor(HexDirection.NW));
    }

    void MoveTo(HexCoordinates newCoord)
    {
        currentCoordinates = newCoord;
        transform.position = HexCoordinates.ToWorldPosition(newCoord, outerRadius);
        lastMoveTime = Time.time;
    }

    void UpdateChunkLoading(bool force = false)
    {
        Vector2Int chunkCoord = ChunkManager.WorldToChunkCoord(currentCoordinates);

        if (force || chunkCoord != currentChunkCoord)
        {
            currentChunkCoord = chunkCoord;
            ChunkManager.Instance.UpdateChunks(chunkCoord);
        }
    }
}

