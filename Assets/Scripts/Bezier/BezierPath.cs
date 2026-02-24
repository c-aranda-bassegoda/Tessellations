using System.Collections.Generic;
using UnityEngine;

public class BezierPath
{
    private List<PathPoint> points;
    private int resolutionPerSegment;

    public BezierPath(Vector2 start, Vector2 end, int resolution)
    {
        resolutionPerSegment = resolution;
        points = new List<PathPoint>();

        points.Add(new PathPoint(start));
        points.Add(new PathPoint(end));
    }

    public IReadOnlyList<PathPoint> Points => points;

    public void AddPoint(int segmentIndex, Vector2 position, bool smooth)
    {
        var p = new PathPoint(position);
        p.Smooth = smooth;

        if (smooth)
        {
            Vector2 a = points[segmentIndex].Position;
            Vector2 b = points[segmentIndex + 1].Position;
            Vector2 dir = (b - a).normalized;
            float len = Vector2.Distance(a, b) * 0.25f;

            p.HandleInOffset = -dir * len;
            p.HandleOutOffset = dir * len;
        }

        points.Insert(segmentIndex + 1, p);
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