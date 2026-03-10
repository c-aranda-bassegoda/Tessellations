using System;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Searcher.SearcherWindow.Alignment;

// polygon embeded in 2D - Otherwise ContainsPoint is a nonsensical method
public abstract class Polygon : MonoBehaviour 
{
    [SerializeField] protected List<Vertex> _vertices = new();
    public IReadOnlyList<Vertex> Vertices => _vertices;
    public IReadOnlyList<Vertex> SnapVertices => _vertices;
    protected List<Edge> _edges = new();
    public IReadOnlyList<Edge> Edges => _edges;
    public bool Initialized { get; protected set; }

    // Should indicate whether point is enclosed by the polygon
    public abstract bool ContainsPoint(Vector2 point);

    // Should indicade whether there is an adge in the Polygon with those vertices
    public abstract bool HasEdge(Vertex a, Vertex b);

    public abstract void ReplaceEdge(GameObject line);

    protected float snapDistance = 0.2f;
    public Vertex FindClosestVertex(Vector3 pos)
    {
        Vertex closest = null;
        float minDist = snapDistance;
        foreach (Vertex v in Vertices)
        {
            float dist = Vector3.Distance(pos, v.Position);
            if (dist < minDist)
            {
                closest = v;
                minDist = dist;
            }
        }
        return closest;
    }
}


public class Edge
{
    public Vertex A { get; }
    public Vertex B { get; }

    public Vector2 MidPoint => (A.Position + B.Position) / 2f;

    public Edge(Vertex a, Vertex b)
    {
        A = a;
        B = b;
    }
}

[System.Serializable]
public class Vertex
{
    [SerializeField] private Vector2 position;
    public Vector2 Position => position;

    public Vertex(Vector2 position)
    {
        this.position = position;
    }
}

