using System.Collections.Generic;
using UnityEngine;

public class HexPathfinder : MonoBehaviour
{
    public static HexPathfinder Instance;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public List<HexBehavior> FindPath(HexBehavior start, HexBehavior goal)
    {
        var openSet = new PriorityQueue<HexBehavior>();
        var cameFrom = new Dictionary<HexBehavior, HexBehavior>();
        var gScore = new Dictionary<HexBehavior, float>();
        var fScore = new Dictionary<HexBehavior, float>();

        openSet.Enqueue(start, 0);
        gScore[start] = 0f;
        fScore[start] = Heuristic(start, goal);

        while (openSet.Count > 0)
        {
            HexBehavior current = openSet.Dequeue();

            if (current == goal)
                return ReconstructPath(cameFrom, current);

            foreach (HexBehavior neighbor in current.neighbors)
            {
                float tentativeG = gScore[current] + 1f; // cost between neighbors

                if (!gScore.ContainsKey(neighbor) || tentativeG < gScore[neighbor])
                {
                    cameFrom[neighbor] = current;
                    gScore[neighbor] = tentativeG;
                    fScore[neighbor] = tentativeG + Heuristic(neighbor, goal);

                    if (!openSet.Contains(neighbor))
                        openSet.Enqueue(neighbor, fScore[neighbor]);
                }
            }
        }

        Debug.LogWarning("⚠️ No se encontró un camino.");
        return null;
    }

    private float Heuristic(HexBehavior a, HexBehavior b)
    {
        return Vector3.Distance(a.transform.position, b.transform.position);
    }

    private List<HexBehavior> ReconstructPath(Dictionary<HexBehavior, HexBehavior> cameFrom, HexBehavior current)
    {
        List<HexBehavior> totalPath = new List<HexBehavior> { current };
        while (cameFrom.ContainsKey(current))
        {
            current = cameFrom[current];
            totalPath.Insert(0, current);
        }

        // Debug info
        Debug.Log("✅ Ruta encontrada con " + totalPath.Count + " pasos.");
        foreach (var step in totalPath)
            Debug.Log(" ➜ " + step.name);

        return totalPath;
    }

    // Opcional: visualización con Gizmos
    public List<HexBehavior> debugPath;
    private void OnDrawGizmos()
    {
        if (debugPath == null || debugPath.Count < 2) return;

        Gizmos.color = Color.green;
        for (int i = 0; i < debugPath.Count - 1; i++)
        {
            Gizmos.DrawLine(debugPath[i].transform.position + Vector3.up * 0.2f,
                            debugPath[i + 1].transform.position + Vector3.up * 0.2f);
        }
    }
}
