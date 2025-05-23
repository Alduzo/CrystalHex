/*using UnityEngine;
using System.Collections;
using System.Collections.Generic;


[RequireComponent(typeof(MeshFilter), typeof(MeshCollider))]
public class TerrainMeshCollider : MonoBehaviour
{
    public void ApplyCollider()
    {
        MeshCollider mc = GetComponent<MeshCollider>();
        if (mc == null)
        {
            mc = gameObject.AddComponent<MeshCollider>();
        }

        Mesh combinedMesh = GetComponent<MeshFilter>().sharedMesh;
        mc.sharedMesh = combinedMesh;

        if (combinedMesh != null)
        {
            Debug.Log($"✅ MeshCollider asignado con mesh de {combinedMesh.vertexCount} vértices en {name}");
        }
        else
        {
            Debug.LogWarning($"⚠️ No se pudo asignar collider porque el mesh combinado es null en {name}");
        }
    }



    public void CombineHexMeshes()
    {
        MeshFilter[] meshFilters = GetComponentsInChildren<MeshFilter>();
        Debug.Log($"🔍 Encontrados {meshFilters.Length} MeshFilters en hijos del chunk {name}");

        List<CombineInstance> combineList = new List<CombineInstance>();

        foreach (var mf in meshFilters)
        {
            if (mf == GetComponent<MeshFilter>())
            {
                Debug.Log($"🟡 Saltando MeshFilter del chunk root: {mf.name}");
                continue;
            }

            if (mf.sharedMesh == null)
            {
                Debug.LogWarning($"❌ MeshFilter sin mesh asignado: {mf.name}");
                continue;
            }

            CombineInstance ci = new CombineInstance
            {
                mesh = mf.sharedMesh,
                transform = mf.transform.localToWorldMatrix
            };
            combineList.Add(ci);
        }

        if (combineList.Count == 0)
        {
            Debug.LogWarning($"❌ No hay meshes válidos para combinar en los hijos de {name}.");
            return;
        }

        Mesh combinedMesh = new Mesh();
        combinedMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        combinedMesh.CombineMeshes(combineList.ToArray(), true, true);

        GetComponent<MeshFilter>().sharedMesh = combinedMesh;
        Debug.Log($"✅ Mesh combinado con {combineList.Count} sub-meshes en {name}.");
    }



}
*/