using System.Collections.Generic;
using UnityEngine;

public class NonConvexPolygon : Polygon
{
    private List<Edge> edges;
    [SerializeField] List<Vertex> vertices = new List<Vertex>();

    public IReadOnlyList<Edge> Edges => edges;
    public IReadOnlyList<Vertex> Vertices => vertices;

    private void Start()
    {
        if (vertices == null)
            vertices = new List<Vertex>();

        if (vertices.Count < 3)
        {
            vertices.Clear();
            vertices.Add(new Vertex(new Vector3(0, 0, 0)));
            vertices.Add(new Vertex(new Vector3(0, 1, 0)));
            vertices.Add(new Vertex(new Vector3(1, 1, 0)));
            vertices.Add(new Vertex(new Vector3(1, 0, 0)));
        }

        BuildEdges();

        DrawEdges();
        DrawVertices();
    }

    private void BuildEdges()
    {
        edges = new List<Edge>();

        for (int i = 0; i < vertices.Count; i++)
        {
            var a = vertices[i];
            var b = vertices[(i + 1) % vertices.Count];
            edges.Add(new Edge(a, b));
        }
    }

    public override bool HasEdge(Vertex a, Vertex b)
    {
        foreach (var e in edges)
        {
            if ((e.A == a && e.B == b) || (e.A == b && e.B == a))
                return true;
        }
        return false;
    }

    // Assumes edges are ordered (counter) clockwise
    public override bool ContainsPoint(Vector3 point)
    {
        Vector2 p = new Vector2(point.x, point.y);
        int windingNumber = 0;

        for (int i = 0; i < vertices.Count; i++)
        {
            Vector2 v1 = To2D(vertices[i].Position);
            Vector2 v2 = To2D(vertices[(i + 1) % vertices.Count].Position);

            if (IsPointOnEdge(p, v1, v2))
                return true; // treat boundary as inside

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

    private Vector2 To2D(Vector3 v)
    {
        return new Vector2(v.x, v.y);
    }


    /*---------------------------------------------------------------------------------------------
     * Rendering
     * --------------------------------------------------------------------------------------------*/

    [SerializeField] private GameObject edgePrefab;
    [SerializeField] private GameObject vtxPrefab;
    [SerializeField] private int resolutionPerSegment = 3;
    private List<LineRenderer> edgeRenderers = new List<LineRenderer>();

    private void DrawVertices()
    {
        if (vertices == null) return;

        for (int i = 0; i < vertices.Count; i++)
        {
            GameObject vtxObj = Instantiate(vtxPrefab, (Vector3)vertices[i].Position, Quaternion.identity);
            Debug.Log("color: " + vtxObj.GetComponent<SpriteRenderer>().color);
        }
    }

    private void DrawEdges()
    {
        if (edges == null) return;

        for (int i = 0; i < edges.Count; i++)
        {
            GameObject edgeObj = Instantiate(edgePrefab, Vector3.zero, Quaternion.identity);
            edgeObj.transform.parent = transform;

            LineRenderer lr = edgeObj.GetComponent<LineRenderer>();
            if (lr == null) return;
            lr.positionCount = resolutionPerSegment + 1;

            Vector3 a = edges[i].A.Position;
            Vector3 b = edges[i].B.Position;

            for (int j = 0; j <= resolutionPerSegment; j++)
            {
                float t = j / (float)resolutionPerSegment;
                lr.SetPosition(j, Vector3.Lerp(a, b, t));
            }

            edgeRenderers.Add(lr);
        }
    }
}
