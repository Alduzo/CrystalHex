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
        StartCoroutine(DelayedInit());
    }

    public void InitializeChunks(int range)
    {
        Debug.Log("🚀 Inicializando chunks alrededor del centro con PerlinSettings actualizado...");
        loadedChunks.Clear();

        for (int x = -range; x <= range; x++)
        {
            for (int y = -range; y <= range; y++)
            {
                Vector2Int coord = new Vector2Int(x, y);
                GameObject chunk = ChunkGenerator.GenerateChunk(coord, chunkSize, hexPrefab);
                loadedChunks[coord] = chunk;
            }
        }
        Debug.Log($"🌍 {loadedChunks.Count} chunks generados.");
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
                }
            }

        }

        if (anyNewChunks)
        {
            ReassignAllChunkBehaviorNeighbors();
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
                HexBorderManager.Instance?.RemoveBordersForChunk(hexes);

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
    public void LoadChunksAtDetail(int detailLevel)
{
/*    foreach (var chunk in loadedChunks.Values)
    {
        Destroy(chunk);
    }
    loadedChunks.Clear();

    int size = chunkSize;
    GameObject prefab = hexPrefab;

    if (detailLevel == 0)
    {
        // Detalle máximo, chunks completos
        size = chunkSize;
        prefab = hexPrefab;
    }
    else if (detailLevel == 1)
    {
        size = chunkSize * 2;
        prefab = Resources.Load<GameObject>("LowPolyHexPrefab");  // Asegúrate que existe
    }
    else if (detailLevel == 2)
    {
        // Minimapa nivel 2: chunks low-poly con elevación y color por tile
        size = chunkSize;
        prefab = Resources.Load<GameObject>("LowPolyHexPrefab");
    }
    else
    {
        // Nivel 3 (proyección global) no carga chunks
        return;
    }

    for (int x = -2; x <= 2; x++)
    {
        for (int y = -2; y <= 2; y++)
        {
            Vector2Int coord = new Vector2Int(x, y);
            GameObject chunk = ChunkGenerator.GenerateChunk(coord, size, prefab);
            loadedChunks[coord] = chunk;
        }
    }

    Debug.Log($"🌍 Chunks generados para LOD {detailLevel}");
*/}

}
