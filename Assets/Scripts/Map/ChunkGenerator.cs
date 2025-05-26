using UnityEngine;
using System.Collections;

public class ChunkGenerator
{


    public static GameObject GenerateChunk(Vector2Int chunkCoord, int chunkSize, GameObject hexPrefab)
{
    GameObject parent = new GameObject($"Chunk_{chunkCoord.x}_{chunkCoord.y}");
    parent.layer = LayerMask.NameToLayer("Terrain");
    parent.tag = "Chunk";
    Debug.Log($"✅ Chunk generado: {parent.name} | Posición: {parent.transform.position}");

    try
    {
        for (int dx = 0; dx < chunkSize; dx++)
        {
            for (int dy = 0; dy < chunkSize; dy++)
            {
                int globalQ = chunkCoord.x * chunkSize + dx;
                int globalR = chunkCoord.y * chunkSize + dy;

                HexCoordinates hexCoord = new HexCoordinates(globalQ, globalR);
                Vector3 worldPos = HexCoordinates.ToWorldPosition(hexCoord, HexRenderer.SharedOuterRadius);

                GameObject hex = Object.Instantiate(hexPrefab, worldPos, Quaternion.identity, parent.transform);
                hex.name = $"Hex_{globalQ}_{globalR}";
                hex.layer = LayerMask.NameToLayer("Terrain");
                SetLayerRecursively(hex, LayerMask.NameToLayer("Terrain"));

                HexBehavior behavior = hex.GetComponent<HexBehavior>();
                if (behavior != null)
                {
                    behavior.coordinates = hexCoord;

                    var hexData = WorldMapManager.Instance.GetOrGenerateHex(hexCoord);
                    var renderer = hex.GetComponent<HexRenderer>();

                    if (renderer != null)
                    {
                        var config = Resources.Load<ChunkMapGameConfig>("ChunkMapGameConfig");
                        if (config != null)
                        {
                            float elevationHeight = hexData.elevation * config.elevationScale;
                            renderer.SetHeight(elevationHeight);

                            Material mat = config.GetMaterialFor(hexData.terrainType);
                            if (mat != null)
                                renderer.GetComponent<MeshRenderer>().material = mat;
                        }
                        else
                        {
                            Debug.LogWarning("⚠️ ChunkMapGameConfig not found in Resources.");
                        }
                    }

                    // Asignar vecinos con la nueva lógica proactiva
                    WorldMapManager.Instance.RefreshNeighborsFor(hexData);

                    var existingCollider = parent.GetComponent<MeshCollider>();
                    if (existingCollider != null)
                    {
                        Object.Destroy(existingCollider);
                        Debug.Log($"🧹 Eliminado MeshCollider sobrante de {parent.name}");
                    }

                    GameObject testPrefab = Resources.Load<GameObject>("TerrainObjects/Leaf_Oak");
                    if (testPrefab == null)
                    {
                        Debug.LogWarning("⚠️ No se encontró el prefab en Resources/TerrainObjects/Leaf_Oak");
                    }
                    else if (Random.value < 0.05f)
                    {
                        CoroutineDispatcher.Instance?.RunCoroutine(DelayedPlaceFeature(behavior, testPrefab));
                    }
                }
                else
                {
                    Debug.LogError($"❌ Hex instanciado sin HexBehavior: {hex.name}");
                }
            }
        }
    }
    catch (System.Exception ex)
    {
        Debug.LogError($"❌ Error generando chunk {chunkCoord}: {ex}");
    }

    return parent;
}



    public static void AssignNeighbors(GameObject chunkRoot)
    {
        HexBehavior[] hexes = chunkRoot.GetComponentsInChildren<HexBehavior>();

        foreach (HexBehavior hex in hexes)
        {
            hex.neighbors.Clear();
            foreach (HexBehavior other in hexes)
            {
                if (hex == other) continue;

                int dq = Mathf.Abs(hex.coordinates.Q - other.coordinates.Q);
                int dr = Mathf.Abs(hex.coordinates.R - other.coordinates.R);

                if ((dq == 1 && dr == 0) || (dq == 0 && dr == 1) || (dq == 1 && dr == 1))
                {
                    hex.neighbors.Add(other);
                }
            }
        }
    }


    public static void AssignBehaviorNeighborsFromWorldMap(HexBehavior behavior)
    {
        var hexData = WorldMapManager.Instance.GetOrGenerateHex(behavior.coordinates);
        WorldMapManager.Instance.EnsureNeighborsAssigned(hexData);

        foreach (var neighborData in hexData.neighborRefs)
        {
            if (WorldMapManager.Instance.TryGetHex(neighborData.coordinates, out var nData))
            {
                Vector2Int neighborChunkCoord = ChunkManager.WorldToChunkCoord(nData.coordinates);
                if (ChunkManager.Instance.loadedChunks.TryGetValue(neighborChunkCoord, out var neighborChunk))
                {
                    var behaviorList = neighborChunk.GetComponentsInChildren<HexBehavior>();
                    foreach (var other in behaviorList)
                    {
                        if (other.coordinates.Equals(nData.coordinates))
                        {
                            behavior.neighbors.Add(other);
                            break;

                        }
                    }
                }
            }
        }
    }


    private static IEnumerator DelayedPlaceFeature(HexBehavior hex, GameObject prefab)
    {
        yield return new WaitForSeconds(0.1f); // Puedes ajustar el tiempo si sigue fallando

        if (hex != null && prefab != null)
        {
            HexObjectPlacer.PlaceOnHex(hex, prefab);
        }


    }
    private static void SetLayerRecursively(GameObject obj, int layer)
    {
        obj.layer = layer;
        foreach (Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject, layer);
        }
    }

    public static GameObject PlaceOnHex(HexBehavior hex, GameObject prefab)
    {
        GameObject obj = Object.Instantiate(prefab);
        obj.name = $"Feature_{hex.coordinates.Q}_{hex.coordinates.R}";

        return obj;
    }



}

