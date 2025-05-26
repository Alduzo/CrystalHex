using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChunkManager : MonoBehaviour
{
    public static ChunkManager Instance;

    public GameObject hexPrefab;
    public int chunkSize = 10;
    public int loadRadius = 1;

    [Range(0, 10)]
    public int unloadRadius = 2;

    public Dictionary<Vector2Int, GameObject> loadedChunks = new();

    private void Awake()
    {
        Instance = this;
        // 🔥 Opción de desactivar generación automática inicial:
        // StartCoroutine(DelayedInit());
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
        }
    }

    public void InitializeChunks(Vector2Int initialCoord)
    {
        Debug.Log("🚀 Inicializando chunks con PerlinSettings actualizado...");
        if (loadedChunks.Count > 0)
        {
            Debug.Log("♻️ Limpiando chunks cargados previamente...");
            foreach (var oldChunk in loadedChunks.Values)
                Destroy(oldChunk);
            loadedChunks.Clear();
        }

        GameObject newChunk = ChunkGenerator.GenerateChunk(initialCoord, chunkSize, hexPrefab);
        loadedChunks[initialCoord] = newChunk;
        Debug.Log($"🌱 Chunk inicial generado en {initialCoord}");
    }

    public void UpdateChunks(Vector2Int playerChunkCoord)
    {
        HashSet<Vector2Int> chunksToKeep = new();
        List<Vector2Int> toUnload = new();

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
                else
                {
                    loadedChunks[coord].SetActive(true);
                }
            }
        }

        if (chunksToKeep.Count > 0)
        {
            List<HexRenderer> newHexes = new List<HexRenderer>();

            foreach (var coord in chunksToKeep)
            {
                if (loadedChunks.TryGetValue(coord, out var chunk))
                {
                    HexRenderer[] hexes = chunk.GetComponentsInChildren<HexRenderer>();
                    if (hexes.Length > 0)
                    {
                        Debug.Log($"🔍 Chunk en {coord} tiene {hexes.Length} hexes.");
                        newHexes.AddRange(hexes);
                    }

                    var behaviors = chunk.GetComponentsInChildren<HexBehavior>();
                    foreach (var behavior in behaviors)
                    {
                        behavior.SyncNeighbors();
                    }
                }
            }

            Debug.Log($"🔍 Se encontraron {newHexes.Count} nuevos HexRenderer.");
            HexBorderManager.Instance?.AddBordersForChunk(newHexes);
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
                HexRenderer[] hexes = chunk.GetComponentsInChildren<HexRenderer>();
                HexBorderManager.Instance?.RemoveBordersForChunk(hexes);
                chunk.SetActive(false);
                Debug.Log($"📦 Chunk en {coord} descargado visualmente (SetActive(false)).");
            }
        }
    }

    public static Vector2Int WorldToChunkCoord(HexCoordinates coordinates)
    {
        int chunkX = Mathf.FloorToInt((float)coordinates.Q / Instance.chunkSize);
        int chunkY = Mathf.FloorToInt((float)coordinates.R / Instance.chunkSize);
        return new Vector2Int(chunkX, chunkY);
    }

    public void RegenerateAllChunks()
    {
        Debug.Log("🔁 Regenerando todos los chunks...");
        foreach (var chunk in loadedChunks.Values)
        {
            Destroy(chunk);
        }
        loadedChunks.Clear();

        Vector2Int initialCoord = new Vector2Int(0, 0);
        GameObject newChunk = ChunkGenerator.GenerateChunk(initialCoord, chunkSize, hexPrefab);
        loadedChunks[initialCoord] = newChunk;
        Debug.Log("🌍 Chunk inicial regenerado manualmente.");
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
}
