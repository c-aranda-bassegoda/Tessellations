using System.Collections.Generic;
using UnityEngine;

public class PiecewisePolygonData : Polygon
{
    private List<BezierPath> edges = new();

    public IReadOnlyList<BezierPath> Edges => edges;

    public void AddEdge(BezierPath path)
    {
        edges.Add(path);
    }

    /// <summary>
    /// Tries to add a point to any edge in the polygon. Returns the PathPoint if added.
    /// </summary>
    public PathPoint AddPointIfClose(Vector2 worldPos, bool smooth)
    {
        foreach (var path in edges)
        {
            var p = path.AddPointIfClose(worldPos, smooth);
            if (p != null)
                return p;
        }
        return null;
    }

    public override bool ContainsPoint(Vector2 point)
    {
        int windingNumber = 0;

        foreach (var edge in edges)
        {
            var pts = edge.Points;

            for (int i = 0; i < pts.Count - 1; i++)
            {
                Vector2 v1 = pts[i].Position;

                for (int j = 1; j <= 20; j++)
                {
                    float t = j / 20f;
                    Vector2 v2 = edge.GetCurveAtT(i, t);

                    if (IsPointOnEdge(point, v1, v2))
                        return true;

                    if (v1.y <= point.y)
                    {
                        if (v2.y > point.y && IsLeft(v1, v2, point) > 0)
                            windingNumber++;
                    }
                    else
                    {
                        if (v2.y <= point.y && IsLeft(v1, v2, point) < 0)
                            windingNumber--;
                    }

                    v1 = v2;
                }
            }
        }

        return windingNumber != 0;
    }

    private float IsLeft(Vector2 a, Vector2 b, Vector2 p)
    {
        return (b.x - a.x) * (p.y - a.y) - (p.x - a.x) * (b.y - a.y);
    }

    private bool IsPointOnEdge(Vector2 p, Vector2 a, Vector2 b)
    {
        float cross = IsLeft(a, b, p);
        if (Mathf.Abs(cross) > 0.0001f) return false;

        float dot = Vector2.Dot(p - a, b - a);
        if (dot < 0) return false;
        if (dot > (b - a).sqrMagnitude) return false;

        return true;
    }

    public override bool HasEdge(Vertex a, Vertex b)
    {
        foreach (var edge in edges)
        {
            var pts = edge.Points;

            for (int i = 0; i < pts.Count - 1; i++)
            {
                if ((pts[i].Position == a.Position &&
                     pts[i + 1].Position == b.Position) ||
                    (pts[i].Position == b.Position &&
                     pts[i + 1].Position == a.Position))
                    return true;
            }
        }

        return false;
    }

    public int IndexOf(BezierPath path)
    {
        if (path == null) return -1;
        return edges.IndexOf(path);
    }
}
