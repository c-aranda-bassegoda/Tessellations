using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

// polygon with special vertices which are game objects rather than just data
public class PiecewisePolygon : BezierPolygon
{
    [SerializeField] List<Vector2> vertices;
    public List<Path> edges;
    [SerializeField] private GameObject linePrefab; // Prefab with LineRenderer
    [SerializeField] private GameObject nodePrefab;
    [SerializeField] private int resolutionPerSegment = 20;
    [SerializeField] private float clickThreshold = 0.4f;

    SpriteRenderer nodeRenderer;

    // Uses winding number algorithm
    // Assumes edges are ordered (counter) clockwise
    public override bool ContainsPoint(Vector2 point)
    {
        int windingNumber = 0;

        foreach (var edge in edges)
        {
            var nodes = edge.GetNodes();
            for (int i = 0; i < nodes.Count - 1; i++)
            {
                PathPointSelectable p0 = edge.GetPoint(i);
                PathPointSelectable p1 = edge.GetPoint(i + 1);

                Vector2 v1 = p0.Position;

                for (int j = 1; j <= resolutionPerSegment; j++)
                {
                    float t = j / (float)resolutionPerSegment;
                    Vector2 v2 = edge.GetCurveAtT(p0, p1, t);

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
        if (Mathf.Abs(cross) > 0.0001f)
            return false;

        float dot = Vector2.Dot(p - a, b - a);
        if (dot < 0)
            return false;

        if (dot > (b - a).sqrMagnitude)
            return false;

        return true;
    }


    // for completeness of interface implementation not really needed or used so might be buggy
    public override bool HasEdge(Vertex a, Vertex b)
    {
        foreach (var edge in edges)
        {
            if (edge.HasEdge(a.Position, b.Position))
                return true;
        }
        return false;
    }

    public override IPointSelectable TryAddPoint(Vector2 pointerWorldPos, bool smooth)
    {

        PathPointSelectable node = null;
        foreach (var edge in edges)
        {
            node = edge.TryAddPoint(pointerWorldPos, smooth);
            if (node != null) break;
        }

        return node;
    }

    private void Awake()
    {
        edges = new List<Path>();
    }

    void Start()
    {
        if (vertices == null || vertices.Count < 2) return;
        if (edges.Count > 0) return;

        // Makes polygon out of vertex list
        Vector2 prev = vertices[vertices.Count - 1];
        for (int i = 0; i < vertices.Count; i++)
        {
            GameObject edgeObj = Instantiate(linePrefab, Vector2.zero, Quaternion.identity);
            //edgeObj.transform.parent = transform;
            edgeObj.transform.SetParent(transform, true);

            Path path = edgeObj.AddComponent<Path>();
            path.resolutionPerSegment = resolutionPerSegment;
            path.nodePrefab = nodePrefab;
            path.clickThreshold = clickThreshold;
            path.Initialize(prev, vertices[i]);
            edges.Add(path);


            prev = vertices[i];
        }

    }

    public override bool ReplaceEdge(GameObject line)
    {
        throw new NotImplementedException();
    }
}
