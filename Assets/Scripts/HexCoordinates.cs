using UnityEngine;

[System.Serializable]
public struct HexCoordinates
{
    public readonly int Q; // eje x axial
    public readonly int R; // eje z axial (se llama R en muchos sistemas)

    public int S => -Q - R; // coordenada cúbica implícita

    public HexCoordinates(int q, int r)
    {
        this.Q = q;
        this.R = r;
    }

    // Conversión desde coordenadas de mundo a coordenadas hexagonales
    public static HexCoordinates FromWorldPosition(Vector3 position, float hexOuterRadius)
    {
        float width = hexOuterRadius * 2f;
        float height = Mathf.Sqrt(3f) * hexOuterRadius;

        float q = (position.x * 2f/3f) / hexOuterRadius;
        float r = (-position.x / 3f + Mathf.Sqrt(3f)/3f * position.y) / hexOuterRadius;

        return FromFractional(q, r);
    }

    // Ajuste de coordenadas flotantes a coordenadas hexagonales enteras (redondeo cúbico)
    public static HexCoordinates FromFractional(float q, float r)
    {
        float s = -q - r;

        int rq = Mathf.RoundToInt(q);
        int rr = Mathf.RoundToInt(r);
        int rs = Mathf.RoundToInt(s);

        float q_diff = Mathf.Abs(rq - q);
        float r_diff = Mathf.Abs(rr - r);
        float s_diff = Mathf.Abs(rs - s);

        if (q_diff > r_diff && q_diff > s_diff)
            rq = -rr - rs;
        else if (r_diff > s_diff)
            rr = -rq - rs;

        return new HexCoordinates(rq, rr);
    }

    public override string ToString()
    {
        return $"({Q}, {R}, {S})";
    }

    public Vector2Int ToVector2Int()
    {
        return new Vector2Int(Q, R);
    }

public static Vector3 ToWorldPosition(HexCoordinates coord, float outerRadius)
{
    float width = outerRadius * 2f;
    float height = outerRadius * Mathf.Sqrt(3f);

    float x = coord.Q * width * 0.75f;
    float y = coord.R * height;

    if (coord.Q % 2 != 0)
    {
        y += height / 2f;
    }

    return new Vector3(x, y, 0f);
}


    public static int Distance(HexCoordinates a, HexCoordinates b)
    {
        return (Mathf.Abs(a.Q - b.Q) + Mathf.Abs(a.R - b.R) + Mathf.Abs(a.S - b.S)) / 2;
    }

    public static HexCoordinates Zero => new HexCoordinates(0, 0);
}
