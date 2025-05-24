using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class HexBorderManager : MonoBehaviour
{
    private static HexBorderManager instance;

    [Header("Border Settings")]
    [SerializeField] private bool bordersVisible = true;
    [SerializeField] private float heightOffset = 0.1f;
    [SerializeField] private Color borderColor = Color.white;
    [SerializeField] private float lineWidth = 0.05f;
    [SerializeField] private float outerRadius = 1f;

    private List<LineRenderer> borderLines = new List<LineRenderer>();

    public static bool IsVisible => instance != null && instance.bordersVisible;
    public static float HeightOffset => instance != null ? instance.heightOffset : 0.1f;
    public static Color BorderColor => instance != null ? instance.borderColor : Color.white;
    public static float LineWidth => instance != null ? instance.lineWidth : 0.05f;

    private IEnumerator Start()
{
    yield return new WaitForSeconds(1f);  // O ajusta según tiempo de carga
    GenerateBorders();
}


    private void GenerateBorders()
    {
        borderLines.Clear();

        HexRenderer[] hexes = FindObjectsOfType<HexRenderer>();
        foreach (HexRenderer hex in hexes)
        {
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

            borderLines.Add(lr);
        }

        Debug.Log($"✅ Generados {borderLines.Count} bordes hexagonales.");
    }

    public void ToggleBorders()
    {
        bordersVisible = !bordersVisible;
        foreach (var lr in borderLines)
        {
            lr.enabled = bordersVisible;
        }
        Debug.Log($"🔲 Bordes {(bordersVisible ? "ACTIVADOS" : "DESACTIVADOS")}");
    }

    public void SetBordersVisibility(bool visible)
    {
        bordersVisible = visible;
        foreach (var lr in borderLines)
        {
            lr.enabled = visible;
        }
    }

    public void RefreshBorders()
    {
        foreach (var lr in borderLines)
        {
            lr.widthMultiplier = lineWidth;
            lr.material.color = borderColor;
            lr.startColor = borderColor;
            lr.endColor = borderColor;

            // Aquí puedes regenerar posiciones si cambian offsets o radii
        }
    }
}
