using System.Collections.Generic;
using UnityEngine;

public class DerivedPolygon : NonConvexPolygon
{
    public Polygon BasePolygon { get; }

    private readonly Dictionary<Vertex, Vertex> _baseToDerivedVertex = new();
    private readonly Dictionary<Edge, List<Edge>> _baseToDerivedEdges = new();

    public DerivedPolygon(Polygon basePolygon)
    {
        BasePolygon = basePolygon;
    }

    public override void ReplaceEdge(GameObject line)
    {
        throw new System.NotImplementedException();
    }

    private float snapDistance = 0.2f;

    private Vertex FindClosestVertex(Vector3 pos)
    {
        Vertex closest = null;
        float minDist = snapDistance;
        foreach (Vertex v in BasePolygon.Vertices)
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