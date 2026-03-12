using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEditor.Searcher.SearcherWindow.Alignment;
using static UnityEngine.Rendering.VolumeComponent;

public class DerivedPolygon : NonConvexPolygon
{
    [SerializeField] public NonConvexPolygon BasePolygon;

    public new IReadOnlyList<Vertex> SnapVertices => BasePolygon.Vertices;

    private readonly Dictionary<(Vertex, Vertex), List<Vertex> > _baseToDerivedVertex = new Dictionary<(Vertex, Vertex), List<Vertex>>();

    private void Awake()
    {
        _vertices = new List<Vertex>(BasePolygon.Vertices);
        BuildEdges();
        for (int i = 0; i < BasePolygon.Vertices.Count; i++)
        {
            List<Vertex> listvtx = new List<Vertex>();
            Vertex v1 = BasePolygon.Vertices[i];
            Vertex v2 = BasePolygon.Vertices[(i + 1) % BasePolygon.Vertices.Count];
            listvtx.Add(v1);
            listvtx.Add(BasePolygon.Midpoints[i]);
            listvtx.Add(v2);

            _baseToDerivedVertex[(v1, v2)] = listvtx;
            _baseToDerivedVertex[(v1, v2)] = listvtx;
        }
    }

    public override bool ReplaceEdge(GameObject line)
    {

        LineRenderer lineRenderer = line.GetComponent<LineRenderer>();
        if (lineRenderer == null)
        {
            Debug.LogError("GameObject does not have a LineRenderer component.");
            return false;
        }

        List<Vertex> newVertices = ToVertices(lineRenderer);
        (Vertex vtx0, Vertex vtxEnd) = GetEndpntVerticesWhereLine(newVertices);

        newVertices.RemoveAt(0);
        newVertices.RemoveAt(newVertices.Count - 1);
        ReplaceVerticesBetween(newVertices, vtx0, vtxEnd);
        return true;
    }

    private (Vertex vtx0, Vertex vtxEnd) GetEndpntVerticesWhereLine(List<Vertex> newVertices)
    {
        Vertex vtx0 = BasePolygon.FindClosestVertex(newVertices[0].Position);
        Vertex vtxEnd = BasePolygon.FindClosestVertex(newVertices[newVertices.Count - 1].Position);
        if (vtx0 == null && vtxEnd == null)
        {

            Debug.Log("Failed to snap to base vertices." + newVertices[0].Position + " " + newVertices[newVertices.Count - 1].Position);
            return (null, null); ;
        }
        if (vtx0 == null)
        {
            Edge edge = BasePolygon.FindClosestMidpoint(newVertices[0].Position);
            vtx0 = edge?.A; vtxEnd = edge?.B;
        }
        if (vtxEnd == null)
        {
            Edge edge = BasePolygon.FindClosestMidpoint(newVertices[newVertices.Count - 1].Position);
            vtx0 = edge?.A; vtxEnd = edge?.B;
        }

        if (vtx0 == null || vtxEnd == null)
        {
            Debug.Log("Failed to snap to base midpoints." + newVertices[0].Position + " " + newVertices[newVertices.Count - 1].Position);
            return (null,null);
        }

        return (vtx0,vtxEnd);
    }

    public void ResetEdge(LineRenderer lineRenderer)
    {
        List<Vertex> newVertices = ToVertices(lineRenderer);
        (Vertex vtx0, Vertex vtxEnd) = GetEndpntVerticesWhereLine(newVertices);
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
        ClearEdge(vertex1, vertex2);

        List<Vertex> oldVertices = _baseToDerivedVertex[(vertex1, vertex2)];
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

    private void ClearEdge(Vertex vertex1, Vertex vertex2)
    {
        ISelectable oldEdge = SelectionManager.Instance.FindSelectableWithEndpnts(vertex1.Position, vertex2.Position);
        oldEdge?.Remove();
        Edge edge = BasePolygon.GetEdge(vertex1, vertex2);
        oldEdge = SelectionManager.Instance.FindSelectableWithEndpnts(vertex1.Position, edge.MidPoint.Position);
        oldEdge?.Remove();
        oldEdge = SelectionManager.Instance.FindSelectableWithEndpnts(edge.MidPoint.Position, vertex2.Position);
        oldEdge?.Remove();
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
        list.Add(vtx0);
        list.Add(BasePolygon.GetEdge(vtx0, vtxEnd).MidPoint);
        list.Add(vtxEnd);
    }

    private (Vertex, Vertex) GetEndpntVertices(LineRenderer lineRenderer)
    {
        List<Vertex> newVertices = ToVertices(lineRenderer);
        Vertex vtx0 = BasePolygon.FindClosestVertex(newVertices[0].Position);
        Vertex vtxEnd = BasePolygon.FindClosestVertex(newVertices[newVertices.Count - 1].Position);

        return (vtx0, vtxEnd);
    }


    internal LineSelectable TryPaste(GameObject lineObj, Vector2 position)
    {
        LineRenderer lineRenderer = lineObj.GetComponent<LineRenderer>();
        if (lineRenderer == null)
        {
            Debug.LogError("GameObject does not have a LineRenderer component.");
            return null;
        }

        for (int i=0; i < BasePolygon.SnapVertices.Count; i++)
        {
            Vector2 midpoint = (BasePolygon.Vertices[i].Position + BasePolygon.Vertices[(i + 1) % BasePolygon.Vertices.Count].Position) / 2;
            if (Vector3.Distance(midpoint, position) <= snapDistance)
            {
                GameObject newObj = Instantiate(lineObj);
                newObj.GetComponent<EdgeSelectable>().Polygon = this;
                Debug.Log("replacing(pasting) edge");
                bool success = this.ReplaceEdge(newObj);
                if (!success)
                {
                    Destroy(newObj);
                    return null;
                }
                return newObj.GetComponent<LineSelectable>();
            }
        }
        return null;
    }

    internal bool HasHalfEdge(Vertex a, Vertex b)
    {

        foreach (var e in _edges)
        {
            if ((e.A == a && e.MidPoint == b) || (e.A == b && e.MidPoint == a)
                || (e.MidPoint == a && e.B == b) || (e.MidPoint == b && e.B == a))
                return true;
        }
        return false;
    }
}