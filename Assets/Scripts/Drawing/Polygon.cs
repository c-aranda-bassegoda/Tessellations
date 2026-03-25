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

    protected List<Vertex> _midpnts = new();
    public IReadOnlyList<Vertex> Midpoints => _midpnts;
    protected List<Edge> _edges = new();
    public IReadOnlyList<Edge> Edges => _edges;
    public bool Initialized { get; protected set; }

    // Should indicate whether point is enclosed by the polygon
    public abstract bool ContainsPoint(Vector2 point);

    // Should indicade whether there is an edge in the Polygon with those vertices
    public abstract bool HasEdge(Vertex a, Vertex b);

    public abstract bool ReplaceEdge(GameObject line);

    protected float snapDistance = 0.2f;

    /// <summary>
    /// Finds the vertex closest to the given position, but only if it's within snapDistance.
    /// </summary>
    /// <param name="pos"></param>
    /// <returns></returns>
    public Vertex FindClosestVertex(Vector2 pos)
    {
        Vertex closest = null;
        float minDist = snapDistance;
        foreach (Vertex v in SnapVertices)
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

    /// <summary>
    /// Finds the edge whose midpoint is closest to the given position, but only if it's within snapDistance.
    /// </summary>
    /// <param name="pos"></param>
    /// <returns></returns>
    public Edge FindEdgeThroughMidpoint(Vector2 pos)
    {
        Edge closest = null;
        float minDist = snapDistance;
        foreach (Edge e in Edges)
        {
            float dist = Vector3.Distance(pos, e.MidPoint.Position);
            if (dist < minDist)
            {
                closest = e;
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

    public Vertex MidPoint { get; }

    public Edge(Vertex a, Vertex b)
    {
        A = a;
        B = b;
        MidPoint = new Vertex( (A.Position + B.Position) / 2f);
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

    /// <summary>
    /// A Vertex Equals another if they have the same position
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public override bool Equals(object obj)
    {
        if (obj is not Vertex other) return false;
        return Position == other.Position; // Or use Vector2.Distance(Position, other.Position) < tolerance
    }

    public override int GetHashCode()
    {
        return Position.GetHashCode();
    }
}

