using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
public class HexRenderer : MonoBehaviour
{
    public static float SharedOuterRadius = 1f;

    [Header("Hex Settings")]
    public float innerRadius = 0.5f;
    public static float outerRadiusShared = 1f;
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

    const int terracesPerSlope = 2;
    const int terraceSteps = terracesPerSlope * 2 + 1;
    const float horizontalTerraceStepSize = 1f / terraceSteps;
    const float verticalTerraceStepSize = 1f / (terracesPerSlope + 1);

    void Awake() => BuildMesh();
    void OnEnable() => BuildMesh();

#if UNITY_EDITOR
    void OnValidate() {
        if (!Application.isPlaying) BuildMesh();
    }
#endif

    public void SetHeight(float h) {
        columnHeight = h;
        BuildMesh();
    }

    public void SetColor(Color color) {
        topColor = color;
        BuildMesh();
    }

    void BuildMesh() {
        if (_mf == null) _mf = GetComponent<MeshFilter>();
        if (_mc == null) _mc = GetComponent<MeshCollider>();
        if (_mr == null) _mr = GetComponent<MeshRenderer>();
        if (_mesh == null) _mesh = new Mesh { name = "HexColumn" };

        _mesh.Clear();

        List<Vector3> v = new();
        List<int> t = new();
        List<Color> c = new();

        float displayHeight = columnHeight * heightScale;

        Vector3 centerTop = new Vector3(0f, displayHeight, 0f);
        Vector3 centerBottom = Vector3.zero;

        int centerTopIndex = v.Count;
        v.Add(centerTop);
        c.Add(topColor);

        for (int i = 0; i < 6; i++) {
            Vector3 corner = GetFlatPoint(i, displayHeight);
            v.Add(corner);
            c.Add(topColor);
        }

        for (int i = 0; i < 6; i++) {
            int current = centerTopIndex + 1 + i;
            int next = i == 5 ? centerTopIndex + 1 : current + 1;
            t.AddRange(new[] { centerTopIndex, current, next });
        }

        int centerBottomIndex = v.Count;
        v.Add(centerBottom);
        c.Add(sideColor);

        for (int i = 0; i < 6; i++) {
            Vector3 corner = GetFlatPoint(i, 0f);
            v.Add(corner);
            c.Add(sideColor);
        }

        for (int i = 0; i < 6; i++) {
            int current = centerBottomIndex + 1 + i;
            int next = i == 5 ? centerBottomIndex + 1 : current + 1;
            t.AddRange(new[] { centerBottomIndex, next, current });
        }

        for (int i = 0; i < 6; i++) {
            Vector3 bottomA = GetFlatPoint(i, 0f);
            Vector3 topA = GetFlatPoint(i, displayHeight);
            Vector3 bottomB = GetFlatPoint((i + 1) % 6, 0f);
            Vector3 topB = GetFlatPoint((i + 1) % 6, displayHeight);

            TriangulateTerracedEdge(v, t, c, bottomA, topA, bottomB, topB);
        }

        TriangulateCorner(v, t, c);

        _mesh.SetVertices(v);
        _mesh.SetColors(c);
        _mesh.SetTriangles(t, 0);
        _mesh.RecalculateNormals();

        _mf.sharedMesh = _mesh;
        _mc.sharedMesh = _mesh;
        if (_mr.sharedMaterial == null && material != null)
            _mr.sharedMaterial = material;
    }

    void TriangulateTerracedEdge(List<Vector3> v, List<int> t, List<Color> c, Vector3 beginBottom, Vector3 beginTop, Vector3 endBottom, Vector3 endTop) {
        Vector3 v00 = beginBottom;
        Vector3 v01 = beginTop;
        Vector3 v10, v11;

        for (int i = 1; i <= terraceSteps; i++) {
            float hStep = i * horizontalTerraceStepSize;
            float vStep = i * verticalTerraceStepSize;

            v10 = Vector3.Lerp(beginBottom, endBottom, hStep);
            v11 = Vector3.Lerp(beginTop, endTop, hStep);
            v11.y = Mathf.Lerp(beginTop.y, endTop.y, vStep);

            int baseIndex = v.Count;

            v.Add(v00); c.Add(sideColor);
            v.Add(v01); c.Add(topColor);
            v.Add(v10); c.Add(sideColor);
            v.Add(v11); c.Add(topColor);

            t.AddRange(new[] { baseIndex, baseIndex + 1, baseIndex + 2 });
            t.AddRange(new[] { baseIndex + 2, baseIndex + 1, baseIndex + 3 });

            v00 = v10;
            v01 = v11;
        }
    }

    void TriangulateCorner(List<Vector3> v, List<int> t, List<Color> c) {
        // Ejemplo simple de una esquina (puede mejorarse si se tienen vecinos):
        Vector3 a = GetFlatPoint(0, columnHeight * heightScale);
        Vector3 b = GetFlatPoint(1, columnHeight * heightScale);
        Vector3 cPos = GetFlatPoint(2, columnHeight * heightScale);

        int start = v.Count;
        v.Add(a); c.Add(topColor);
        v.Add(b); c.Add(topColor);
        v.Add(cPos); c.Add(topColor);

        t.AddRange(new[] { start, start + 1, start + 2 });
    }

    Vector3 GetFlatPoint(int index, float y) {
        float angle = 60f * index * Mathf.Deg2Rad;
        return new Vector3(
            SharedOuterRadius * Mathf.Cos(angle),
            y,
            SharedOuterRadius * Mathf.Sin(angle)
        );
    }
}
