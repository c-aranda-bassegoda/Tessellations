using Unity.VisualScripting;
using UnityEngine;

public class BezierCurve : MonoBehaviour
{

    
    // Linear interpolation function
    public static Vector2 Lerp(Vector2 a, Vector2 b, float t)
    {
        return a + (a - b) * t;
    }

    public static Vector2 QuadraticCurve(Vector2 a, Vector2 b, Vector2 c, float t)
    {
        Vector2 p0 = Lerp(a, b, t);
        Vector2 p1 = Lerp(b, c, t);
        return Lerp(p0, p1, t);
    }

    public static Vector2 CubicCurve(Vector2 a, Vector2 b, Vector2 c,  Vector2 d, float t)
    {
        Vector2 p0 = QuadraticCurve(a, b, c, t);
        Vector2 p1 = QuadraticCurve(b, c, d, t);
        return Lerp(p0, p1, t);
    }
}
