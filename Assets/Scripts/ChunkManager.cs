// ChunkManager.cs
using System.Collections.Generic;
using UnityEngine;

public class ChunkManager : MonoBehaviour
{
    public static ChunkManager Instance;

    public int chunkSize = 6; // e.g. 10x10 hexes per chunk
    public int loadRadius = 0; // how many chunks around the player to load
    public int unloadBufferRadius = 2; // ← aquí
    public GameObject hexPrefab;

    private Dictionary<Vector2Int, GameObject> loadedChunks = new();

    private void Awake()
    {
        Instance = this;
    }

    public void UpdateChunks(Vector2Int playerChunkCoord)
    {
        HashSet<Vector2Int> chunksToKeep = new();

        for (int dx = -loadRadius; dx <= loadRadius; dx++)
        {
            for (int dy = -loadRadius; dy <= loadRadius; dy++)
            {
                Vector2Int coord = new Vector2Int(playerChunkCoord.x + dx, playerChunkCoord.y + dy);
                chunksToKeep.Add(coord);

                if (!loadedChunks.ContainsKey(coord))
                {
                    GameObject chunk = ChunkGenerator.GenerateChunk(coord, chunkSize, hexPrefab);
                    loadedChunks[coord] = chunk;
                }
            }
        }

        // Unload chunks that are no longer in range
        List<Vector2Int> toUnload = new();
        foreach (var kvp in loadedChunks)
        {
            Vector2Int diff = kvp.Key - playerChunkCoord;
            if (Mathf.Abs(diff.x) > unloadBufferRadius || Mathf.Abs(diff.y) > unloadBufferRadius)
            {
                Destroy(kvp.Value);
                toUnload.Add(kvp.Key);
            }

        }
        foreach (var coord in toUnload)
        {
            loadedChunks.Remove(coord);
        }
    }

   public static Vector2Int WorldToChunkCoord(HexCoordinates hex)
    {
        int qChunk = Mathf.FloorToInt(hex.Q / (float)Instance.chunkSize);
        int rChunk = Mathf.FloorToInt(hex.R / (float)Instance.chunkSize);
        return new Vector2Int(qChunk, rChunk);
    }

}
