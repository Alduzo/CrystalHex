using UnityEngine;

public static class Bezier
{
    public static Vector3 GetPoint(Vector3 a, Vector3 b, Vector3 c, float t)
    {
        t = Mathf.Clamp01(t);
        float oneMinusT = 1f - t;
        return
            oneMinusT * oneMinusT * a +
            2f * oneMinusT * t * b +
            t * t * c;
    }

    public static Vector3 GetFirstDerivative(Vector3 a, Vector3 b, Vector3 c, float t)
    {
        t = Mathf.Clamp01(t);
        return
            2f * (1f - t) * (b - a) +
            2f * t * (c - b);
    }
}
