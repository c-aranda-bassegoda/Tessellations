using System;
using System.Collections.Generic;
using UnityEngine;

public class NonConvexPolygon : Polygon
{

    public void Initialize()
    {
        if(Initialized) { return; }

        if (_vertices == null)
            _vertices = new List<Vertex>();
        base._vertices = _vertices;
        Initialized = true;

        if (_vertices.Count < 3)
        {
            _vertices.Clear();
            _vertices.Add(new Vertex(new Vector2(0, 0)));
            _vertices.Add(new Vertex(new Vector2(0, 1)));
            _vertices.Add(new Vertex(new Vector2(1, 1)));
            _vertices.Add(new Vertex(new Vector2(1, 0)));
        }

        BuildEdges();

        DrawEdges();
        DrawVertices();
    }

    protected void BuildEdges()
    {
        _edges = new List<Edge>();

        for (int i = 0; i < _vertices.Count; i++)
        {
            var a = _vertices[i];
            var b = _vertices[(i + 1) % _vertices.Count];
            _edges.Add(new Edge(a, b));
            _midpnts.Add(_edges[i].MidPoint);
        }
    }

    public override bool HasEdge(Vertex a, Vertex b)
    {
        foreach (var e in _edges)
        {
            if ((e.A == a && e.B == b) || (e.A == b && e.B == a))
                return true;
        }
        return false;
    }

    // Uses winding number algorithm
    // Assumes edges are ordered (counter) clockwise
    public override bool ContainsPoint(Vector2 point)
    {
        int windingNumber = 0;

        for (int i = 0; i < _vertices.Count; i++)
        { 
            Vector2 v1 = _vertices[i].Position;
            Vector2 v2 = _vertices[(i + 1) % _vertices.Count].Position;

            if (IsPointOnEdge(point, v1, v2))
                return true; // treats boundary as inside

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


    /*---------------------------------------------------------------------------------------------
     * Rendering
     * --------------------------------------------------------------------------------------------*/

    [SerializeField] private GameObject edgePrefab;
    [SerializeField] private GameObject vtxPrefab;
    [SerializeField] private int resolutionPerSegment = 3;
    public List<LineRenderer> edgeRenderers = new List<LineRenderer>();

    private void DrawVertices()
    {
        if (_vertices == null) return;

        for (int i = 0; i < _vertices.Count; i++)
        {
            GameObject vtxObj = Instantiate(vtxPrefab, (Vector2)_vertices[i].Position, Quaternion.identity, transform);
            Debug.Log("color: " + vtxObj.GetComponent<SpriteRenderer>().color);
        }
    }

    private void DrawEdges()
    {
        if (_edges == null) return;

        for (int i = 0; i < _edges.Count; i++)
        {
            GameObject edgeObj = Instantiate(edgePrefab, Vector2.zero, Quaternion.identity, transform);
            //edgeObj.transform.parent = transform;

            LineRenderer lr = edgeObj.GetComponent<LineRenderer>();
            if (lr == null) return;
            lr.positionCount = resolutionPerSegment + 1;

            Vector2 a = _edges[i].A.Position;
            Vector2 b = _edges[i].B.Position;

            for (int j = 0; j <= resolutionPerSegment; j++)
            {
                float t = j / (float)resolutionPerSegment;
                lr.SetPosition(j, Vector2.Lerp(a, b, t));
            }

            edgeRenderers?.Add(lr);

            GameObject midpntObj = Instantiate(vtxPrefab, (Vector2)_midpnts[i].Position, Quaternion.identity, transform);
            midpntObj.GetComponent<SpriteRenderer>().color = Color.red;
        }
    }

    public override bool ReplaceEdge(GameObject line)
    {
        throw new System.NotImplementedException();
    }

    internal Edge GetEdge(Vertex a, Vertex b)
    {
        foreach (var e in _edges)
        {
            if ((e.A == a && e.B == b) || (e.A == b && e.B == a))
                return e;
        }
        return null;
    }
}
