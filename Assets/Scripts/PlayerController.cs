using UnityEngine;
using System.Collections;


public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;

    private Vector2Int currentChunkCoord;

    IEnumerator Start()
    {
        yield return new WaitUntil(() => ChunkManager.Instance != null);

        // Espera a que WorldMapManager esté inicializado
        yield return new WaitUntil(() => WorldMapManager.Instance != null);

        HexCoordinates spawnCoord = FindNonWaterSpawnTile();
        Vector3 spawnPosition = HexCoordinates.ToWorldPosition(spawnCoord, HexRenderer.SharedOuterRadius);
        transform.position = spawnPosition;

        Vector2Int chunkCoord = ChunkManager.WorldToChunkCoord(spawnCoord);
        currentChunkCoord = chunkCoord;
        ChunkManager.Instance.UpdateChunks(chunkCoord);
    }




    void Update()
    {
        float h = 0f;
        float v = 0f;

        if (Input.GetKey(KeyCode.LeftArrow)) h = -1f;
        if (Input.GetKey(KeyCode.RightArrow)) h = 1f;
        if (Input.GetKey(KeyCode.UpArrow)) v = 1f;
        if (Input.GetKey(KeyCode.DownArrow)) v = -1f;



        Vector3 move = new Vector3(h, 0f, v) * moveSpeed * Time.deltaTime;
        transform.position += move;

        UpdateChunkLoading();
    }

    void UpdateChunkLoading(bool force = false)
    {
        HexCoordinates playerHex = HexCoordinates.FromWorldPosition(transform.position, HexRenderer.SharedOuterRadius);
        Vector2Int chunkCoord = ChunkManager.WorldToChunkCoord(playerHex);

        if (force || chunkCoord != currentChunkCoord)
        {
            Debug.Log("Player moved to chunk " + chunkCoord);
            currentChunkCoord = chunkCoord;
            ChunkManager.Instance.UpdateChunks(chunkCoord);
        }
    }
    HexCoordinates FindNonWaterSpawnTile()
    {
        foreach (var hex in WorldMapManager.Instance.GetAllHexes())
        {
            if (hex.terrainType != TerrainType.OceanDeep && hex.terrainType != TerrainType.OceanShallow)
            {
                return hex.coordinates;
            }
        }

        Debug.LogWarning("No suitable land tile found! Defaulting to (0,0).");
        return new HexCoordinates(0, 0);
    }


}
