using System.Collections.Generic;
using UnityEngine;

// polygon embeded in 2D - Otherwise ContainsPoint is a nonsensical method
public abstract class Polygon : MonoBehaviour 
{
    public IReadOnlyList<Vertex> Vertices { get; }
    IReadOnlyList<Edge> Edges { get; }

    // Should indicate whether point is enclosed by the polygon
    public abstract bool ContainsPoint(Vector2 point);

    // Should indicade whether there is an adge in the Polygon with those vertices
    public abstract bool HasEdge(Vertex a, Vertex b);

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

