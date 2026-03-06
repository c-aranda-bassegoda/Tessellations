using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Searcher.SearcherWindow.Alignment;

public class DerivedPolygon : NonConvexPolygon
{
    [SerializeField] public Polygon BasePolygon; 

    private readonly Dictionary<Vertex, Vertex> _baseToDerivedVertex = new();
    private readonly Dictionary<Edge, List<Edge>> _baseToDerivedEdges = new();

    private void Awake()
    {
        if (!BasePolygon.Initialized)
        {
            Debug.LogError("BasePolygon not initialized yet.");
        }
        _vertices = new List<Vertex>(BasePolygon.Vertices);
        BuildEdges();
    }

    public override void ReplaceEdge(GameObject line)
    {
        

        List<Vertex> newVertices = ToVertices(line);
        Vertex vtx0 = FindClosestVertex(newVertices[0].Position);
        Vertex vtxEnd = FindClosestVertex(newVertices[newVertices.Count - 1].Position);
        Debug.Log(vtx0.Position);
        Debug.Log(BasePolygon.Vertices.Count);
        if (vtx0 == null || vtxEnd == null)
        {
            Debug.LogError("Failed to snap to base vertices.");
            return;
        }
        newVertices.RemoveAt(0);
        newVertices.RemoveAt(newVertices.Count - 1);
        int idx0 = _vertices.IndexOf(vtx0);
        int idxEnd = _vertices.IndexOf(vtxEnd);
        ReplaceVerticesBetween(newVertices, idx0, idxEnd);
    }

    private List<Vertex> ToVertices(GameObject currentLine)
    {
        var vertices = new List<Vertex>();

        LineRenderer lineRenderer = currentLine.GetComponent<LineRenderer>();
        if (lineRenderer == null)
        {
            Debug.LogError("GameObject does not have a LineRenderer component.");
            return vertices;
        }

        int count = lineRenderer.positionCount;
        Vector3[] positions = new Vector3[count];
        lineRenderer.GetPositions(positions);

        foreach (var pos in positions)
        {
            vertices.Add(new Vertex(new Vector2(pos.x, pos.y)));
        }

        return vertices;
    }

    private void ReplaceVerticesBetween(List<Vertex> newVertices, int index1, int index2)
    {

        if (index1 < 0 || index2 < 0 || index1 >= _vertices.Count || index2 >= _vertices.Count)
        {
            Debug.LogError("Invalid vertex indices.");
            return;
        }

        // If traversal is reversed, swap indices and invert inserted vertices
        if ((index2 < index1 && !(index2==0 && index1==_vertices.Count-1)) || (index1 == 0 && index2 == _vertices.Count - 1))
        {
            (index1, index2) = (index2, index1);
            newVertices.Reverse();
        }

        int removeStart = index1 + 1;
        int removeCount = index2 - index1 - 1;

        if (removeCount > 0)
            _vertices.RemoveRange(removeStart, removeCount);

        _vertices.InsertRange(removeStart, newVertices);
    }

    private float snapDistance = 0.2f;

    private Vertex FindClosestVertex(Vector3 pos)
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