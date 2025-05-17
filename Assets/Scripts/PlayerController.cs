using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;

    private Vector2Int currentChunkCoord;

IEnumerator Start()
{
    yield return new WaitUntil(() => ChunkManager.Instance != null);

    HexCoordinates playerHex = HexCoordinates.FromWorldPosition(transform.position, HexRenderer.SharedOuterRadius);
    Vector2Int chunkCoord = ChunkManager.WorldToChunkCoord(playerHex);
    currentChunkCoord = chunkCoord;
    ChunkManager.Instance.UpdateChunks(chunkCoord);
}



    void Update()
    {
        float h = 0f;
        float v = 0f;

        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) h = -1f;
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) h = 1f;
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) v = 1f;
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) v = -1f;


        Vector3 move = new Vector3(h, v, 0f) * moveSpeed * Time.deltaTime;
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

    
}
