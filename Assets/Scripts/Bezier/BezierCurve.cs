using Unity.VisualScripting;
using UnityEngine;


// Bezier curve mathematics, serves as utilities library
public static class BezierCurve
{

    public static Vector2 QuadraticCurve(Vector2 a, Vector2 b, Vector2 c, float t)
    {
        Vector2 p0 = Vector2.Lerp(a, b, t);
        Vector2 p1 = Vector2.Lerp(b, c, t);
        return Vector2.Lerp(p0, p1, t);
    }

    public static Vector2 CubicCurve(Vector2 a, Vector2 b, Vector2 c,  Vector2 d, float t)
    {
        Vector2 p0 = QuadraticCurve(a, b, c, t);
        Vector2 p1 = QuadraticCurve(b, c, d, t);
        return Vector2.Lerp(p0, p1, t);
    }

    public static Vector3 GetClosestPointOnCubic(Vector3 a, Vector3 b, Vector3 c, Vector3 d, Vector3 target, int samples = 20)
    {
        float bestT = 0f;
        float minDist = float.MaxValue;

        for (int i = 0; i <= samples; i++)
        {
            float t = i / (float)samples;
            Vector3 point = BezierCurve.CubicCurve(a, b, c, d, t);

            float dist = (point - target).sqrMagnitude;

            if (dist < minDist)
            {
                minDist = dist;
                bestT = t;
            }
        }

        return BezierCurve.CubicCurve(a, b, c, d, bestT);
    }

}
