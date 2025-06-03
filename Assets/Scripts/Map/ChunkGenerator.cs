using UnityEngine;
using System.Collections;

public class ChunkGenerator
{

public static int ChunkSize { get; set; }

    public static GameObject GenerateChunk(Vector2Int chunkCoord, int chunkSize, GameObject hexPrefab)
{
    GameObject parent = new GameObject($"Chunk_{chunkCoord.x}_{chunkCoord.y}");
    parent.layer = LayerMask.NameToLayer("Terrain");
    parent.tag = "Chunk";
    Debug.Log($"✅ Chunk generado: {parent.name} | Posición: {parent.transform.position}");
    Chunk chunkComponent = parent.AddComponent<Chunk>();

    // 🔥 Cargar ChunkMapGameConfig solo una vez por chunk
    var config = Resources.Load<ChunkMapGameConfig>("ChunkMapGameConfig");
    if (config == null)
    {
        Debug.LogError("⚠️ No se encontró ChunkMapGameConfig en Resources.");
        return parent;  // Detenemos si no existe
    }

    try
    {
        for (int dx = 0; dx < chunkSize; dx++)
        {
            for (int dy = 0; dy < chunkSize; dy++)
            {
                int globalQ = chunkCoord.x * chunkSize + dx;
                int globalR = chunkCoord.y * chunkSize + dy;

                if (globalQ < -WorldMapManager.MaxMapWidth / 2 || globalQ >= WorldMapManager.MaxMapWidth / 2 ||
                    globalR < -WorldMapManager.MaxMapHeight / 2 || globalR >= WorldMapManager.MaxMapHeight / 2)
                {
                    continue;
                }

                HexCoordinates hexCoord = new HexCoordinates(globalQ, globalR);
                Vector3 worldPos = HexCoordinates.ToWorldPosition(hexCoord, HexRenderer.SharedOuterRadius);

                var hexData = WorldMapManager.Instance.GetOrGenerateHex(hexCoord);
                if (hexData == null)
                {
                    Debug.LogWarning($"⚠️ No se pudo generar HexData para {hexCoord}. Se omite.");
                    continue;
                }


                GameObject hex = Object.Instantiate(hexPrefab, worldPos, Quaternion.identity, parent.transform);
                hex.name = $"Hex_{globalQ}_{globalR}";
                hex.layer = LayerMask.NameToLayer("Terrain");
                SetLayerRecursively(hex, LayerMask.NameToLayer("Terrain"));

                chunkComponent.hexDataList.Add(hexData);

                var renderer = hex.GetComponent<HexRenderer>();
                if (renderer != null)
                {
                    float elevationHeight = hexData.elevation * config.elevationScale;
                    renderer.SetHeight(elevationHeight);
                    Material mat = config.GetMaterialFor(hexData.terrainType);
                    if (mat != null)
                        renderer.GetComponent<MeshRenderer>().material = mat;

                    renderer.SetVisualByHex(hexData);

                    // 💧 Lógica de agua
                    if (WorldMapManager.IsWater(hexData.terrainType) || hexData.isRiver || hexData.isLake)
                    {
                        Vector3 waterPos = HexCoordinates.ToWorldPosition(hexCoord, HexRenderer.SharedOuterRadius);
                        var waterPrefab = WorldMapManager.Instance.waterTilePrefab;

                        if (waterPrefab != null)
                        {
                            float topY = hex.transform.position.y + renderer.columnHeight * renderer.heightScale;
                            float targetWaterLevel = Mathf.Min(WorldMapManager.GlobalWaterLevel, topY + 0.1f);
                            float waterHeight = targetWaterLevel - topY;

                            if (WorldMapManager.IsWater(hexData.terrainType) && waterHeight > 0)
                            {
                                Vector3 scale = new Vector3(HexRenderer.SharedOuterRadius * 2f, waterHeight / 2f, HexRenderer.SharedOuterRadius * 2f);
                                Vector3 position = new Vector3(waterPos.x, topY + waterHeight / 2f, waterPos.z);
                                GameObject waterTile = Object.Instantiate(waterPrefab, position, Quaternion.Euler(90, 0, 0), parent.transform);
                                waterTile.name = $"OceanWater_{hexCoord.Q}_{hexCoord.R}";
                                waterTile.transform.localScale = scale;
                            }
                            else if (hexData.isLake)
                            {
                                waterPos.y = topY + 0.1f;
                                GameObject waterTile = Object.Instantiate(waterPrefab, waterPos, Quaternion.Euler(90, 0, 0), parent.transform);
                                waterTile.name = $"Lake_{hexCoord.Q}_{hexCoord.R}";
                                waterTile.transform.localScale = new Vector3(HexRenderer.SharedOuterRadius * 2f, 1f, HexRenderer.SharedOuterRadius * 2f);
                            }
                            else if (hexData.isRiver)
                            {
                                waterPos.y = topY + 0.05f;
                                GameObject waterTile = Object.Instantiate(waterPrefab, waterPos, Quaternion.Euler(90, 0, 0), parent.transform);
                                waterTile.name = $"River_{hexCoord.Q}_{hexCoord.R}";
                                waterTile.transform.localScale = new Vector3(HexRenderer.SharedOuterRadius * 2f, 1f, HexRenderer.SharedOuterRadius * 2f);
                            }
                        }
                    }
                }

                HexBehavior behavior = hex.GetComponent<HexBehavior>();
                if (behavior != null)
                {
                    behavior.coordinates = hexCoord;
                }

                // ⚠️ Comentar temporalmente RefreshNeighborsFor
                // WorldMapManager.Instance.RefreshNeighborsFor(hexData);

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

public static Vector2Int GetChunkCoordFromWorldPos(Vector3 worldPos)
{
    int chunkX = Mathf.FloorToInt(worldPos.x / ChunkSize);
    int chunkY = Mathf.FloorToInt(worldPos.z / ChunkSize);  // Usa worldPos.z si el eje Y es elevación
    return new Vector2Int(chunkX, chunkY);
}



}

