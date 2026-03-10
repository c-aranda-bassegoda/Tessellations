using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEditor.Searcher.SearcherWindow.Alignment;
using static UnityEngine.Rendering.VolumeComponent;

public class DerivedPolygon : NonConvexPolygon
{
    [SerializeField] public Polygon BasePolygon;
    public new IReadOnlyList<Vertex> SnapVertices => BasePolygon.Vertices;

    private readonly Dictionary<(Vertex, Vertex), List<Vertex> > _baseToDerivedVertex = new Dictionary<(Vertex, Vertex), List<Vertex>>();

    private void Awake()
    {
        _vertices = new List<Vertex>(BasePolygon.Vertices);
        BuildEdges();
        for (int i = 0; i < BasePolygon.Vertices.Count; i++)
        {
            List<Vertex> listvtx = new List<Vertex>();

            _baseToDerivedVertex[(BasePolygon.Vertices[i], BasePolygon.Vertices[(i + 1) % BasePolygon.Vertices.Count])] = listvtx;
            _baseToDerivedVertex[(BasePolygon.Vertices[(i + 1) % BasePolygon.Vertices.Count], BasePolygon.Vertices[i])] = listvtx;
        }
    }

    public override void ReplaceEdge(GameObject line)
    {

        LineRenderer lineRenderer = line.GetComponent<LineRenderer>();
        if (lineRenderer == null)
        {
            Debug.LogError("GameObject does not have a LineRenderer component.");
            return;
        }

        List<Vertex> newVertices = ToVertices(lineRenderer);
        Vertex vtx0 = BasePolygon.FindClosestVertex(newVertices[0].Position);
        Vertex vtxEnd = BasePolygon.FindClosestVertex(newVertices[newVertices.Count - 1].Position);
        Debug.Log(vtx0.Position);
        Debug.Log(vtxEnd.Position);
        if (vtx0 == null || vtxEnd == null)
        {
            Debug.LogError("Failed to snap to base vertices.");
            return;
        }
        newVertices.RemoveAt(0);
        newVertices.RemoveAt(newVertices.Count - 1);
        ReplaceVerticesBetween(newVertices, vtx0, vtxEnd);
    }

    public void ResetEdge(LineRenderer lineRenderer)
    {
        List<Vertex> newVertices = ToVertices(lineRenderer);
        Vertex vtx0 = BasePolygon.FindClosestVertex(newVertices[0].Position);
        Vertex vtxEnd = BasePolygon.FindClosestVertex(newVertices[newVertices.Count - 1].Position);
        if (vtx0 == null || vtxEnd == null)
        {
            Debug.LogError("Failed to snap to base vertices.");
            return;
        }
        ResetVerticesBetween(vtx0, vtxEnd);
    }


    private List<Vertex> ToVertices(LineRenderer lineRenderer)
    {

        var vertices = new List<Vertex>();

        int count = lineRenderer.positionCount;
        Vector3[] positions = new Vector3[count];
        lineRenderer.GetPositions(positions);

        foreach (var pos in positions)
        {
            vertices.Add(new Vertex(new Vector2(pos.x, pos.y)));
        }

        return vertices;
    }

    private List<Vector2> VerticesToPositions(List<Vertex> vertices)
    {
        if (vertices== null || vertices.Count == 0) return null;

        var list = new List<Vector2>();

        for (int i = 0; i < vertices.Count; i++)
        {
            list.Add(vertices[i].Position);
        }
        return list;
    }

    private void ReplaceVerticesBetween(List<Vertex> newVertices, Vertex vertex1, Vertex vertex2)
    {
        List <Vertex> oldVertices = _baseToDerivedVertex[(vertex1, vertex2)]; 
        ISelectable oldEdge = SelectionManager.Instance.FindBestFitSelectable(VerticesToPositions(oldVertices));
        oldEdge?.Remove();

        oldVertices = _baseToDerivedVertex[(vertex1, vertex2)];
        Vertex oldVtx1 = oldVertices.Count > 0 ? oldVertices[0] : vertex1;
        Vertex oldVtx2 = oldVertices.Count > 0 ? oldVertices[^1] : vertex2;
        int index1 = _vertices.IndexOf(oldVtx1);
        int index2 = _vertices.IndexOf(oldVtx2);

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

        var list = _baseToDerivedVertex[(vertex1, vertex2)];
        list.Clear();
        list.AddRange(newVertices);

        _vertices.InsertRange(removeStart, newVertices);
    }

    private void ResetVerticesBetween(Vertex vtx0, Vertex vtxEnd)
    {
        List<Vertex> oldVertices = _baseToDerivedVertex[(vtx0, vtxEnd)];


        int removeCount = oldVertices.Count;

        if (removeCount > 0)
        {
            int removeStart = _vertices.IndexOf(oldVertices[0]);
            _vertices.RemoveRange(removeStart, removeCount);
        }

        var list = _baseToDerivedVertex[(vtx0, vtxEnd)];
        list.Clear();
    }

}