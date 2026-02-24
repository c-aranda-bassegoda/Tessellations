using System.Collections.Generic;
using UnityEngine;

public class BezierPath
{
    private List<PathPoint> points;
    private int resolutionPerSegment;
    private float clickThreshold = 0.2f;

    public BezierPath(Vector2 start, Vector2 end, int resolution)
    {
        resolutionPerSegment = resolution;
        points = new List<PathPoint>();

        points.Add(new PathPoint(start));
        points.Add(new PathPoint(end));
    }

    public IReadOnlyList<PathPoint> Points => points;

    /// <summary>
    /// Adds a point if the world position is near a segment. Returns the new PathPoint or null.
    /// </summary>
    public PathPoint AddPointIfClose(Vector2 worldPos, bool smooth)
    {
        for (int i = 0; i < points.Count - 1; i++)
        {
            Vector2 a = points[i].Position;
            Vector2 b = points[i + 1].Position;

            Vector2 closest = ClosestPointOnSegment(a, b, worldPos);
            if (Vector2.Distance(worldPos, closest) <= clickThreshold)
            {
                var p = new PathPoint(closest) { Smooth = smooth };

                if (smooth)
                {
                    Vector2 dir = (b - a).normalized;
                    float len = Vector2.Distance(a, b) * 0.25f;
                    p.HandleInOffset = -dir * len;
                    p.HandleOutOffset = dir * len;
                }

                points.Insert(i + 1, p);
                return p;
            }
        }

        return null;
    }

    private Vector2 ClosestPointOnSegment(Vector2 a, Vector2 b, Vector2 p)
    {
        Vector2 ab = b - a;
        float t = Vector2.Dot(p - a, ab) / ab.sqrMagnitude;
        t = Mathf.Clamp01(t);
        return a + ab * t;
    }

    public Vector2 GetCurveAtT(int i, float t)
    {
        var p0 = points[i];
        var p1 = points[i + 1];

        return BezierCurve.CubicCurve(
            p0.Position,
            p0.HandleOutPos,
            p1.HandleInPos,
            p1.Position,
            t);
    }

    public void RemovePoint(PathPoint point)
    {
        if (points.Count <= 2)
            return; // keep minimum valid path

        int index = points.IndexOf(point);
        if (index == -1)
            return;

        points.RemoveAt(index);
    }
}