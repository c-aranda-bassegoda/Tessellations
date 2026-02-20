using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public class PiecewisePolygon : Polygon
{
    [SerializeField] List<Vector2> vertices;
    List<Path> edges;
    [SerializeField] private GameObject linePrefab; // Prefab with LineRenderer
    [SerializeField] private GameObject nodePrefab;
    [SerializeField] private int resolutionPerSegment = 20;

    SpriteRenderer nodeRenderer;

    // Assumes edges are ordered (counter) clockwise
    public override bool ContainsPoint(Vector3 point)
    {
        Vector2 p = new Vector2(point.x, point.y);
        int windingNumber = 0;

        foreach (var edge in edges)
        {
            var nodes = edge.GetNodes();
            if (nodes.Count < 2) continue;

            for (int i = 0; i < nodes.Count - 1; i++)
            {
                PathPointSelectable p0 = edge.GetPoint(i);
                PathPointSelectable p1 = edge.GetPoint(i + 1);

                Vector3 a = p0.Position;
                Vector3 b = p0.HandleOutPos;
                Vector3 c = p1.HandleInPos;
                Vector3 d = p1.Position;

                Vector3 prev = a;

                for (int j = 1; j <= resolutionPerSegment; j++)
                {
                    float t = j / (float)resolutionPerSegment;
                    Vector3 current = BezierCurve.CubicCurve(a, b, c, d, t);

                    Vector2 v1 = new Vector2(prev.x, prev.y);
                    Vector2 v2 = new Vector2(current.x, current.y);

                    if (IsPointOnEdge(p, v1, v2))
                        return true;

                    if (v1.y <= p.y)
                    {
                        if (v2.y > p.y && IsLeft(v1, v2, p) > 0)
                            windingNumber++;
                    }
                    else
                    {
                        if (v2.y <= p.y && IsLeft(v1, v2, p) < 0)
                            windingNumber--;
                    }
                    prev = current;
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


    // for completeness of interface implementation not really needed
    public override bool HasEdge(Vertex a, Vertex b)
    {
        foreach (var edge in edges)
        {
            if(edge.HasEdge(a.Position,b.Position))
                return true;
        }
        return false;
    }

    internal PathPointSelectable TryAddPoint(Vector3 pointerWorldPos, bool smooth)
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

        Vector2 prev = vertices[vertices.Count-1];
        for(int i=0; i<vertices.Count; i++)
        {
            GameObject edgeObj = Instantiate(linePrefab, Vector3.zero, Quaternion.identity);
            edgeObj.transform.parent = transform;

            Path path = edgeObj.AddComponent<Path>();
            path.resolutionPerSegment = resolutionPerSegment;
            path.nodePrefab = nodePrefab;
            path.Initialize(prev, vertices[i]);
            edges.Add(path);
            

            prev = vertices[i];
        }
        
    }
}
