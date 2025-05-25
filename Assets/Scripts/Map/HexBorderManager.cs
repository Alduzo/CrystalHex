using UnityEngine;
using System.Collections.Generic;

public class HexBorderManager : MonoBehaviour
{
    public static HexBorderManager Instance;

    [Header("Border Settings")]
    [SerializeField] private bool bordersVisible = true;
    [SerializeField] private float heightOffset = 0.1f;
    [SerializeField] private Color borderColor = Color.white;
    [SerializeField] private float lineWidth = 0.05f;
    [SerializeField] private float outerRadius = 1f;

    private Dictionary<HexRenderer, LineRenderer> borderLines = new();

    public static bool IsVisible => Instance != null && Instance.bordersVisible;
    public static float HeightOffset => Instance != null ? Instance.heightOffset : 0.1f;
    public static Color BorderColor => Instance != null ? Instance.borderColor : Color.white;
    public static float LineWidth => Instance != null ? Instance.lineWidth : 0.05f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
            Debug.Log("✅ HexBorderManager iniciado correctamente.");
        }
    }

    public void AddBordersForChunk(IEnumerable<HexRenderer> hexes)
    {
        int count = 0;
        foreach (var hex in hexes)
        {
            if (borderLines.ContainsKey(hex)) continue;

            GameObject lineObj = new GameObject($"HexBorder_{hex.name}");
            lineObj.transform.SetParent(this.transform);

            LineRenderer lr = lineObj.AddComponent<LineRenderer>();
            lr.useWorldSpace = true;
            lr.loop = true;
            lr.positionCount = 7;
            lr.material = new Material(Shader.Find("Unlit/Color"));
            lr.widthMultiplier = lineWidth;
            lr.material.color = borderColor;
            lr.startColor = borderColor;
            lr.endColor = borderColor;

            Vector3 center = new Vector3(hex.transform.position.x,
                hex.transform.position.y + hex.columnHeight * hex.heightScale + heightOffset,
                hex.transform.position.z);

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

            lr.SetPositions(corners);
            lr.enabled = bordersVisible;

            borderLines.Add(hex, lr);
            count++;
        }
        Debug.Log($"🟢 Agregados {count} bordes para el chunk.");
    }

    public void RemoveBordersForChunk(IEnumerable<HexRenderer> hexes)
    {
        int count = 0;
        foreach (var hex in hexes)
        {
            if (borderLines.TryGetValue(hex, out var lr))
            {
                Destroy(lr.gameObject);
                borderLines.Remove(hex);
                count++;
            }
        }
        Debug.Log($"🔴 Eliminados {count} bordes del chunk.");
    }

    public void ToggleBorders()
    {
        bordersVisible = !bordersVisible;
        foreach (var lr in borderLines.Values)
        {
            lr.enabled = bordersVisible;
        }
        Debug.Log($"🔲 Bordes {(bordersVisible ? "ACTIVADOS" : "DESACTIVADOS")}");
    }

    public void SetBordersVisibility(bool visible)
    {
        bordersVisible = visible;
        foreach (var lr in borderLines.Values)
        {
            lr.enabled = visible;
        }
    }

    public void RefreshBorders()
    {
        foreach (var lr in borderLines.Values)
        {
            lr.widthMultiplier = lineWidth;
            lr.material.color = borderColor;
            lr.startColor = borderColor;
            lr.endColor = borderColor;
        }
    }
}
