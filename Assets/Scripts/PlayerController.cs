using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    public ChunkManager chunkManager;  // Asignar desde el inspector
    private Vector2Int lastChunkCoord;

    public float moveCooldown = 0.2f;
    private float lastMoveTime;

    public HexCoordinates currentCoordinates;
    public float outerRadius = 1f;

    private Vector2Int currentChunkCoord;

void Start()
{
    Vector3 pos = transform.position;

    // 🔥 Ajuste: calcula coordenadas usando el centro del render
    Renderer rend = GetComponentInChildren<Renderer>();
    if (rend != null)
    {
        pos = rend.bounds.center;  // O usa pos = rend.bounds.center - rend.bounds.extents.y * Vector3.up si quieres la base
    }

    currentCoordinates = HexCoordinates.FromWorldPosition(pos, outerRadius);
    Vector3 tilePos = HexCoordinates.ToWorldPosition(currentCoordinates, outerRadius);
    float topY = HexRenderer.GetVisualTopYAt(currentCoordinates);
    transform.position = new Vector3(tilePos.x, topY, tilePos.z);

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

    if (Input.GetKeyDown(KeyCode.W)) MoveTo(currentCoordinates.GetNeighbor(HexDirection.NW));   // Norte visual
    if (Input.GetKeyDown(KeyCode.E)) MoveTo(currentCoordinates.GetNeighbor(HexDirection.NE));   // Noreste
    if (Input.GetKeyDown(KeyCode.D)) MoveTo(currentCoordinates.GetNeighbor(HexDirection.SE));   // Sureste
    if (Input.GetKeyDown(KeyCode.S)) MoveTo(currentCoordinates.GetNeighbor(HexDirection.SW));   // Sur
    if (Input.GetKeyDown(KeyCode.A)) MoveTo(currentCoordinates.GetNeighbor(HexDirection.W));    // Suroeste
    if (Input.GetKeyDown(KeyCode.Q)) MoveTo(currentCoordinates.GetNeighbor(HexDirection.E));    // Noroeste
}




    void MoveTo(HexCoordinates newCoord)
    {
        currentCoordinates = newCoord;
        transform.position = HexCoordinates.ToWorldPosition(newCoord, outerRadius);
        lastMoveTime = Time.time;
    }

void UpdateChunkLoading(bool force = false)
{
    Vector3 pos = transform.position;
    Renderer rend = GetComponentInChildren<Renderer>();
    if (rend != null)
    {
        pos = rend.bounds.center;
    }

    HexCoordinates playerHex = HexCoordinates.FromWorldPosition(pos, outerRadius);
    Vector2Int chunkCoord = ChunkGenerator.GetChunkCoordFromHex(playerHex);

    if (force || chunkCoord != currentChunkCoord)
    {
        currentChunkCoord = chunkCoord;
        ChunkManager.Instance.UpdateChunks(currentChunkCoord);
    }
}
}

