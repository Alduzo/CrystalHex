using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChunkManager : MonoBehaviour
{
    public static ChunkManager Instance;

    public GameObject hexPrefab;
    public int chunkSize = 10;
    public int loadRadius = 1; // Aumentado a 1 para tener vecinos

    [Range(0, 10)]
    public int unloadRadius = 2; // Si es 0, no descarga nada


    public Dictionary<Vector2Int, GameObject> loadedChunks = new();


   


   private void Awake()
{
    Instance = this;
    StartCoroutine(DelayedInit());
}

private IEnumerator DelayedInit()
{
    yield return new WaitUntil(() => WorldMapManager.Instance != null);

    Vector2Int initialCoord = new Vector2Int(0, 0);
    if (!loadedChunks.ContainsKey(initialCoord))
    {
        GameObject chunk = ChunkGenerator.GenerateChunk(initialCoord, chunkSize, hexPrefab);
        loadedChunks.Add(initialCoord, chunk);
        Debug.Log("🌱 Chunk inicial generado en (0,0)");

        if (CoroutineDispatcher.Instance != null)
        {
            CoroutineDispatcher.Instance.RunCoroutine(DelayApplyCollider(chunk));
        }
        else
        {
            Debug.LogWarning("⚠️ CoroutineDispatcher no está presente en escena.");
        }
    }
}






    public void UpdateChunks(Vector2Int playerChunkCoord)
    {
        HashSet<Vector2Int> chunksToKeep = new();
        List<Vector2Int> toUnload = new();

        bool anyNewChunks = false;

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
                    anyNewChunks = true;

                    // ✅ Aplicar MeshCollider con retraso leve (usando CoroutineDispatcher)
                    if (CoroutineDispatcher.Instance != null)
                    {
                        CoroutineDispatcher.Instance.RunCoroutine(DelayApplyCollider(chunk));
                    }
                    else
                    {
                        Debug.LogWarning("⚠️ CoroutineDispatcher no está presente en escena.");
                    }


                }
            }
        }

        // ✅ Reasignar vecinos visuales luego de cargar nuevos chunks
        if (anyNewChunks)
        {
            ReassignAllChunkBehaviorNeighbors();
        }

        if (unloadRadius > 0)
        {
            foreach (var coord in loadedChunks.Keys)
            {
                int dist = Mathf.Max(
                    Mathf.Abs(coord.x - playerChunkCoord.x),
                    Mathf.Abs(coord.y - playerChunkCoord.y)
                );

                if (dist > loadRadius + unloadRadius)
                {
                    toUnload.Add(coord);
                }
            }
        }


        foreach (var coord in toUnload)
        {
            if (loadedChunks.TryGetValue(coord, out var chunk))
            {
                Destroy(chunk);
                loadedChunks.Remove(coord);
            }
        }

    }

    public static Vector2Int WorldToChunkCoord(HexCoordinates coordinates)
    {
        int chunkX = Mathf.FloorToInt((float)coordinates.Q / Instance.chunkSize);
        int chunkY = Mathf.FloorToInt((float)coordinates.R / Instance.chunkSize);
        return new Vector2Int(chunkX, chunkY);
    }

    public void ReassignAllChunkBehaviorNeighbors()
    {
        foreach (var chunk in loadedChunks.Values)
        {
            var behaviors = chunk.GetComponentsInChildren<HexBehavior>();
            foreach (var behavior in behaviors)
            {
                ChunkGenerator.AssignBehaviorNeighborsFromWorldMap(behavior);
            }
        }
    }
    public IEnumerator DelayApplyCollider(GameObject parent)
    {
        yield return new WaitForSeconds(0.1f); // Espera a que se complete la generación de meshes

        var terrainCollider = parent.GetComponent<TerrainMeshCollider>();
        if (terrainCollider != null)
        {
            terrainCollider.ApplyCollider();
            Debug.Log($"✅ MeshCollider aplicado al chunk {parent.name}");
        }
        else
        {
            Debug.LogWarning($"⚠️ No se encontró TerrainMeshCollider en {parent.name}");
        }
    }

}
