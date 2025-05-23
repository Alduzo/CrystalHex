using UnityEngine;

public class HexBorderVisualizer : MonoBehaviour
{
    public static bool ShowBorders = true;

    [SerializeField] private float heightOffset = 0.1f;
    [SerializeField] private Color borderColor = Color.white;

    public static float HeightOffset => Instance?.heightOffset ?? 0.1f;
    public static Color BorderColor => Instance?.borderColor ?? Color.white;

    private static HexBorderVisualizer Instance;

    void Awake()
    {
        Instance = this;
    }

    public void ToggleDebugBorders()
    {
        ShowBorders = !ShowBorders;
        Debug.Log($"🧩 Hex Border Gizmos: {(ShowBorders ? "ON" : "OFF")}");
    }
}
