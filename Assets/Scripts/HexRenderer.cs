using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class HexRenderer : MonoBehaviour
{
    private Mesh _mesh;
    private MeshFilter _meshFilter;
    private MeshRenderer _meshRenderer;

    [Header("Hex Settings")]
    public float innerRadius = 0.5f;
    public static float SharedOuterRadius = 1f;
    public float height = 0f;

    [Header("Visual Settings")]
    public Material material;
    public Color baseColor = Color.green;
    private Color _baseColor; // active color used for drawing
    public Color edgeColor = Color.black;

    private List<Face> _faces;
    private List<Color> _colors;

    private void Awake()
    {
        SetupComponents();
        DrawMesh();
        _baseColor = baseColor;

    }

    private void OnEnable()
    {
        SetupComponents();
        DrawMesh();
    }

    public void OnValidate()
    {
#if UNITY_EDITOR
        if (!Application.isPlaying) return;
#endif
        SetupComponents();
        DrawMesh();
    }

    private void SetupComponents()
    {
        if (_meshFilter == null) _meshFilter = GetComponent<MeshFilter>();
        if (_meshRenderer == null) _meshRenderer = GetComponent<MeshRenderer>();
        if (_mesh == null)
        {
            _mesh = new Mesh { name = "HexMesh_Runtime" };
            GetComponent<MeshFilter>().mesh = _mesh;
            GetComponent<MeshCollider>().sharedMesh = _mesh;
        }

        if (_meshRenderer.sharedMaterial == null && material != null)
            _meshRenderer.sharedMaterial = material;
    }

    public void DrawMesh()
    {
        _faces = new List<Face>();
        DrawFaces();
        CombineFaces();
        if (TryGetComponent(out MeshCollider col))
        {
            col.sharedMesh = null; // reset first
            col.sharedMesh = _mesh; // reassign updated mesh
        }

    }

    private void DrawFaces()
    {
        for (int point = 0; point < 6; point++)
        {
            _faces.Add(CreateFace(innerRadius, SharedOuterRadius, height, height, point));
        }
    }

    private Face CreateFace(float innerRad, float outerRad, float heightA, float heightB, int point, bool reverse = false)
    {
        Vector3 pointA = GetPoint(innerRad, heightB, point);
        Vector3 pointB = GetPoint(innerRad, heightB, (point < 5) ? point + 1 : 0);
        Vector3 pointC = GetPoint(outerRad, heightA, (point < 5) ? point + 1 : 0);
        Vector3 pointD = GetPoint(outerRad, heightA, point);

        List<Vector3> vertices = new() { pointA, pointB, pointC, pointD };
        List<int> triangles = new() { 0, 1, 2, 2, 3, 0 };
        List<Vector2> uvs = new() {
            new Vector2(0, 0),
            new Vector2(1, 0),
            new Vector2(1, 1),
            new Vector2(0, 1)
        };

        if (reverse) vertices.Reverse();

        return new Face(vertices, triangles, uvs);
    }

    protected Vector3 GetPoint(float radius, float height, int index)
    {
        float angle_deg = 60 * index;
        float angle_rad = Mathf.Deg2Rad * angle_deg;
        return new Vector3(radius * Mathf.Cos(angle_rad), radius * Mathf.Sin(angle_rad), height);
    }

    private void CombineFaces()
    {
        List<Vector3> vertices = new();
        List<int> triangles = new();
        List<Vector2> uvs = new();
        _colors = new List<Color>();

        for (int i = 0; i < _faces.Count; i++)
        {
            int offset = vertices.Count;
            vertices.AddRange(_faces[i].vertices);
            uvs.AddRange(_faces[i].uvs);

            // Add color: center vertices are lighter, outer ones are darker
            _colors.Add(_baseColor);    // A
            _colors.Add(_baseColor);    // B
            _colors.Add(edgeColor);     // C
            _colors.Add(edgeColor);     // D


            foreach (int t in _faces[i].triangles)
                triangles.Add(t + offset);
        }

        _mesh.Clear();
        _mesh.SetVertices(vertices);
        _mesh.SetTriangles(triangles, 0);
        _mesh.SetUVs(0, uvs);
        _mesh.SetColors(_colors);
        _mesh.RecalculateNormals();

        Debug.Log($"HexRenderer: Mesh generated with {_mesh.vertexCount} vertices and {_mesh.triangles.Length} triangles");
    }

    public struct Face
    {
        public List<Vector3> vertices { get; private set; }
        public List<int> triangles { get; private set; }
        public List<Vector2> uvs { get; private set; }

        public Face(List<Vector3> vertices, List<int> triangles, List<Vector2> uvs)
        {
            this.vertices = vertices;
            this.triangles = triangles;
            this.uvs = uvs;
        }
    }
    public void SetColor(Color newColor)
{
    _baseColor = newColor;
    DrawMesh();
}

public void ResetColor()
{
    _baseColor = baseColor;
    DrawMesh();
}

        #if UNITY_EDITOR
private void Update()
{
    if (Application.isPlaying && Input.GetKeyDown(KeyCode.R))
    {
        DrawMesh();
    }
}
#endif
// Following lines commented where for activating hover and clicked states for Tiles.
// private void OnMouseEnter()
//{
//    _baseColor = edgeColor; // Use a highlight or hover color
 //   DrawMesh();
//}

//private void OnMouseExit()
//{
//    _baseColor = baseColor; // Reset to default
//    DrawMesh();
//}

//private void OnMouseDown()
//{
//    Debug.Log($"Hex clicked: {gameObject.name}");
    
    // Placeholder: simulate crystal planting
//    _baseColor = Color.cyan;
//    DrawMesh();
//}



}
