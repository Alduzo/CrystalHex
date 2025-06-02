using UnityEngine;

public struct EdgeVertices
{
    public Vector3 v1, v2, v3, v4, v5;

    public EdgeVertices(Vector3 corner1, Vector3 corner2)
    {
        v1 = corner1;
        v2 = Vector3.Lerp(corner1, corner2, 0.25f);
        v3 = Vector3.Lerp(corner1, corner2, 0.5f);
        v4 = Vector3.Lerp(corner1, corner2, 0.75f);
        v5 = corner2;
    }

    public EdgeVertices(Vector3 corner1, Vector3 corner2, float outerStep)
    {
        v1 = corner1;
        v2 = Vector3.Lerp(corner1, corner2, outerStep);
        v3 = Vector3.Lerp(corner1, corner2, 0.5f);
        v4 = Vector3.Lerp(corner1, corner2, 1f - outerStep);
        v5 = corner2;
    }

    public static EdgeVertices TerraceLerp(EdgeVertices a, EdgeVertices b, int step)
    {
        return new EdgeVertices(
            HexMetrics.TerraceLerp(a.v1, b.v1, step),
            HexMetrics.TerraceLerp(a.v2, b.v2, step),
            HexMetrics.TerraceLerp(a.v3, b.v3, step),
            HexMetrics.TerraceLerp(a.v4, b.v4, step),
            HexMetrics.TerraceLerp(a.v5, b.v5, step)
        );
    }

    public EdgeVertices(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4, Vector3 v5)
    {
        this.v1 = v1;
        this.v2 = v2;
        this.v3 = v3;
        this.v4 = v4;
        this.v5 = v5;
    }
}
