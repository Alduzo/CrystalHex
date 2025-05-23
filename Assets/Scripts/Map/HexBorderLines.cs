using UnityEngine;
using System.Collections;


[RequireComponent(typeof(LineRenderer))]
[RequireComponent(typeof(HexRenderer))]
public class HexBorderLines : MonoBehaviour
{
    [Header("Visual Settings")]
    [SerializeField] private float outerRadius = 1f;
    [SerializeField] private float lineWidth = 0.05f;
    [SerializeField] private float heightOffset = 0.1f;
    [SerializeField] private Color lineColor = Color.white;

    private LineRenderer lineRenderer;

private IEnumerator Start()
{
    yield return new WaitForSeconds(0.5f); // o WaitForEndOfFrame si prefieres

    InitializeLine();
}


    void OnValidate()
    {
        if (Application.isPlaying && lineRenderer != null)
        {
            InitializeLine();
        }
    }

    private void InitializeLine()
    {
        if (lineRenderer == null)
            lineRenderer = GetComponent<LineRenderer>();

        HexRenderer hex = GetComponent<HexRenderer>();
        if (hex == null)
        {
            Debug.LogWarning("⚠️ HexRenderer no encontrado en el objeto para dibujar borde.");
            return;
        }

        lineRenderer.positionCount = 7;
        lineRenderer.loop = true;
        lineRenderer.useWorldSpace = true;
        lineRenderer.material = new Material(Shader.Find("Unlit/Color"));
        lineRenderer.material.color = lineColor;
        lineRenderer.widthMultiplier = lineWidth;
        lineRenderer.startColor = lineColor;
        lineRenderer.endColor = lineColor;

        Renderer meshRenderer = hex.GetComponentInChildren<Renderer>();
        if (meshRenderer == null)
        {
            Debug.LogWarning("⚠️ No se encontró Renderer para calcular la altura del borde.");
            return;
        }

        float topY = meshRenderer.bounds.max.y + heightOffset;
        Vector3 center = new Vector3(transform.position.x, topY, transform.position.z);

        Debug.Log($"✅ Bordes para {name} colocados en Y={topY}");




        Vector3[] corners = new Vector3[7];
        for (int i = 0; i < 7; i++)
        {
            float angle = Mathf.Deg2Rad * (60f * i);
            corners[i] = new Vector3(
                center.x + outerRadius * Mathf.Cos(angle),
                center.y,
                center.z + outerRadius * Mathf.Sin(angle)
            );
        }

        lineRenderer.SetPositions(corners);
    }

private float CalculateVisualTopY(HexRenderer hex)
{
    Renderer meshRenderer = hex.GetComponentInChildren<Renderer>();
    if (meshRenderer == null)
    {
        Debug.LogWarning("⚠️ HexRenderer no tiene Renderer visible.");
        return hex.transform.position.y;
    }

    float top = meshRenderer.bounds.max.y;
    return top;
}


}
