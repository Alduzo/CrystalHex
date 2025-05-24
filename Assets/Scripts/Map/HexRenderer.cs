// 📁 HexRenderer.cs (Updated)
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
public class HexRenderer : MonoBehaviour
{
    public static float SharedOuterRadius = 1f;

    [Header("Hex Settings")]
    public float innerRadius = 0.5f;
    public float columnHeight = 0f;

    [Header("Visual")]
    public Material material;
    public Color topColor = Color.magenta;
    public Color sideColor = Color.black;

    [Header("Scale Settings")]
    [Range(0.01f, 1f)] public float heightScale = 0.25f;

    Mesh _mesh;
    MeshFilter _mf;
    MeshCollider _mc;
    MeshRenderer _mr;

    void Start()
    {
        if (Application.isPlaying)
        {
            BuildMesh();
        }
    }

    void Awake()
    {
        InitializeComponents();
        // Always build mesh in Play Mode
        if (Application.isPlaying)
        {
            BuildMesh();
        }
    }

    void OnEnable()
    {
        InitializeComponents();
        // Always build mesh in Play Mode
        if (Application.isPlaying)
        {
            BuildMesh();
        }
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        // Only execute when the game is not playing AND in a valid scene state
        if (!Application.isPlaying && _mf != null && _mc != null && _mr != null)
        {
            UnityEditor.EditorApplication.delayCall += () =>
            {
                if (this != null) BuildMesh();
            };
        }
    }
#endif

    private void InitializeComponents()
    {
        if (_mf == null) _mf = GetComponent<MeshFilter>();
        if (_mc == null) _mc = GetComponent<MeshCollider>();
        if (_mr == null) _mr = GetComponent<MeshRenderer>();
        // Ensure mesh is initialized. If it was cleared or destroyed in editor, create a new one.
        if (_mesh == null) _mesh = new Mesh { name = "HexSimple" };
    }

    public void SetHeight(float elevationHeight)
    {
        columnHeight = elevationHeight;
        BuildMesh();
    }

    public void SetColor(Color color)
    {
        topColor = color;
        BuildMesh();
    }

    void BuildMesh()
    {
        if (_mf != null) _mf.sharedMesh = null;
        if (_mc != null) _mc.sharedMesh = null;
        InitializeComponents();

        if (_mesh != null) _mesh.Clear();
        else _mesh = new Mesh { name = "HexSimple" };

        List<Vector3> v = new();
        List<int> t = new();
        List<Color> c = new();

        // TOP
        float yTop = columnHeight * heightScale;
        float yBottom = 0f; // For the base of the column

        // Center vertex
        v.Add(new Vector3(0, yTop, 0)); c.Add(topColor);
        // Vertices for the top surface (outer ring)
        for (int i = 0; i < 6; i++)
        {
            v.Add(GetFlatPoint(i, yTop)); c.Add(topColor);
        }
        // Triangles for the top surface
        for (int i = 0; i < 6; i++)
        {
            t.AddRange(new[] { 0, i + 1, (i + 1) % 6 + 1 });
        }

        // SIDES
        for (int i = 0; i < 6; i++)
        {
            Vector3 topA = GetFlatPoint(i, yTop);
            Vector3 topB = GetFlatPoint((i + 1) % 6, yTop);
            Vector3 bottomA = GetFlatPoint(i, yBottom);
            Vector3 bottomB = GetFlatPoint((i + 1) % 6, yBottom);

            int baseIndex = v.Count;

            v.Add(bottomA); c.Add(sideColor);
            v.Add(topA); c.Add(topColor);
            v.Add(bottomB); c.Add(sideColor);
            v.Add(topB); c.Add(topColor);

            t.AddRange(new[] { baseIndex, baseIndex + 1, baseIndex + 2 });
            t.AddRange(new[] { baseIndex + 2, baseIndex + 1, baseIndex + 3 });
        }

        _mesh.SetVertices(v);
        _mesh.SetColors(c);
        _mesh.SetTriangles(t, 0);
        _mesh.RecalculateNormals();

        if (_mf != null && Application.isPlaying)
        {
            _mf.sharedMesh = _mesh;
        }

        if (_mc != null && Application.isPlaying)
        {
            _mc.sharedMesh = _mesh;
            _mc.convex = false;
        }

        if (_mr != null && _mr.sharedMaterial == null && material != null)
        {
            _mr.sharedMaterial = material;
        }
        Debug.Log($"{name} – Mesh vertices: {_mesh.vertexCount}, assigned to MeshCollider: {_mc.sharedMesh != null}");

    }

    Vector3 GetFlatPoint(int index, float y)
    {
        float angle = 60f * index * Mathf.Deg2Rad;
        return new Vector3(SharedOuterRadius * Mathf.Cos(angle), y, SharedOuterRadius * Mathf.Sin(angle));
    }

    public float VisualTopY
    {
        get { return transform.position.y + columnHeight * heightScale; }


    }
    
   void OnDrawGizmos()
    {
        if (!HexBorderManager.IsVisible) return;


        Gizmos.color = HexBorderManager.BorderColor;

        float outerRadius = HexRenderer.SharedOuterRadius;
        float yOffset = HexBorderManager.HeightOffset;
        Vector3 center = transform.position + Vector3.up * yOffset;

        for (int i = 0; i < 6; i++)
        {
            float angle1 = Mathf.Deg2Rad * (60f * i);
            float angle2 = Mathf.Deg2Rad * (60f * (i + 1));

            Vector3 corner1 = new Vector3(
                center.x + outerRadius * Mathf.Cos(angle1),
                center.y,
                center.z + outerRadius * Mathf.Sin(angle1)
            );
            Vector3 corner2 = new Vector3(
                center.x + outerRadius * Mathf.Cos(angle2),
                center.y,
                center.z + outerRadius * Mathf.Sin(angle2)
            );

            Gizmos.DrawLine(corner1, corner2);
        }
    }


    
}