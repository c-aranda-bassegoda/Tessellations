using System.Collections.Generic;
using UnityEngine;

public abstract class Polygon : MonoBehaviour 
{
    public IReadOnlyList<Vertex> Vertices { get; }
    IReadOnlyList<Edge> Edges { get; }

    public abstract bool ContainsPoint(Vector3 point);
    public abstract bool HasEdge(Vertex a, Vertex b);

}


public class Edge
{
    public Vertex A { get; }
    public Vertex B { get; }

    public Vector3 MidPoint => (A.Position + B.Position) / 2f;

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

