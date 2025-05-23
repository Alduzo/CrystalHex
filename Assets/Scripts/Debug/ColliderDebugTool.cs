// 📁 ColliderDebugTool.cs
using UnityEngine;

public class ColliderDebugTool : MonoBehaviour
{
    [SerializeField] private float sphereRadius = 0.5f;
    [SerializeField] private Color sphereColor = Color.red;
    [SerializeField] private float refreshRate = 0.5f;

    private float lastCheckTime;

    void OnDrawGizmos()
    {
        Gizmos.color = sphereColor;
        Gizmos.DrawWireSphere(transform.position, sphereRadius);
    }

    void OnDrawGizmosSelected()
{
    Gizmos.color = Color.red;
    Gizmos.DrawWireSphere(transform.position, 5.0f);
}

    void Update()
    {
        if (Time.time - lastCheckTime > refreshRate)
        {
            lastCheckTime = Time.time;
            CheckColliders();
        }
    }

    void CheckColliders()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, sphereRadius);
        Debug.Log($"Colliders overlapping {gameObject.name}: {colliders.Length}");

        // FIX: Replaced obsolete FindObjectsOfType with FindObjectsByType
        HexRenderer[] hexRenderers = Object.FindObjectsByType<HexRenderer>(FindObjectsSortMode.None);
        Debug.Log($"Total HexRenderers in scene: {hexRenderers.Length}");
    }
}