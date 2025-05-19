using UnityEngine;

public static class ChunkGenerator
{
    public static GameObject GenerateChunk(Vector2Int chunkCoord, int chunkSize, GameObject hexPrefab)
    {
        GameObject parent = new GameObject($"Chunk_{chunkCoord.x}_{chunkCoord.y}");

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

                HexBehavior behavior = hex.GetComponent<HexBehavior>();
                if (behavior != null)
                {
                    behavior.coordinates = hexCoord;

                    var hexData = WorldMapManager.Instance.GetOrGenerateHex(hexCoord);
                    // Obtener renderer y aplicar visualización de altura y color
                    var renderer = hex.GetComponent<HexRenderer>();
                    if (renderer != null)
                    {
                        var config = Resources.Load<ChunkMapGameConfig>("ChunkMapGameConfig");
                        if (config != null)  // ← buena práctica por si no se encuentra el asset
                        {
                            float elevationHeight = hexData.elevation * config.elevationScale;
                            renderer.SetHeight(elevationHeight);

                            Material mat = config.GetMaterialFor(hexData.terrainType);
                            if (mat != null)
                                renderer.GetComponent<MeshRenderer>().material = mat;
                        }
                        else
                        {
                            Debug.LogWarning("ChunkMapGameConfig not found in Resources.");
                        }
                    }

                    WorldMapManager.Instance.AssignNeighborReferences(hexData);

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
            }
        }

        // ✅ Asignar vecinos visuales entre chunks (cuando ya todos existen)
        HexBehavior[] behaviors = parent.GetComponentsInChildren<HexBehavior>();
        foreach (var behavior in behaviors)
        {
            AssignBehaviorNeighborsFromWorldMap(behavior);
            Debug.Log($"Assigning neighbors to {behavior.name}, found {behavior.neighbors.Count}");

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
        WorldMapManager.Instance.AssignNeighborReferences(hexData);

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



}

