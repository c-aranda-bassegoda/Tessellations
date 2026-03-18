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

    private readonly Dictionary<(Vertex, Vertex), (List<Vertex>, List<Vertex>)> _baseToDerivedVertex =
    new Dictionary<(Vertex, Vertex), (List<Vertex>, List<Vertex>)>(new VertexTupleComparer());

    private void Awake()
    {
        BasePolygon.Initialize();
        _vertices = new List<Vertex>(BasePolygon.Vertices);
        BuildEdges();
        for (int i = 0; i < BasePolygon.Vertices.Count; i++)
        {
            List<Vertex> listvtx1 = new List<Vertex>();
            List<Vertex> listvtx2 = new List<Vertex>();
            Vertex v1 = BasePolygon.Vertices[i];
            Vertex v2 = BasePolygon.Vertices[(i + 1) % BasePolygon.Vertices.Count];
            Vertex m = BasePolygon.Midpoints[i];

            _vertices.Insert(2*i+1, m);

            listvtx1.Add(v1);
            listvtx1.Add(BasePolygon.Midpoints[i]);
            listvtx2.Add(BasePolygon.Midpoints[i]);
            listvtx2.Add(v2);

            _baseToDerivedVertex[(v1, v2)] = (listvtx1, listvtx2);
            _baseToDerivedVertex[(v1, m)] = (listvtx1, listvtx2);
            _baseToDerivedVertex[(v2, m)] = (listvtx1, listvtx2);
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
        (Vertex vtx0, Vertex vtxEnd, Vertex vtxM) = GetVerticesWhereLine(newVertices);

        ReplaceVerticesBetween(newVertices, vtx0, vtxEnd);
        return true;
    }

    public bool ExtendEdge(GameObject line)
    {

        LineRenderer lineRenderer = line.GetComponent<LineRenderer>();
        if (lineRenderer == null)
        {
            Debug.LogError("GameObject does not have a LineRenderer component.");
            return false;
        }

        List<Vertex> newVertices = ToVertices(lineRenderer);
        (Vertex vtx0, Vertex vtxEnd, Vertex vtxMid) = GetVerticesWhereLine(newVertices);

        AddVerticesBetween(newVertices, vtx0, vtxEnd, vtxMid);
        return true;
    }

    private (Vertex vtx0, Vertex vtxEnd, Vertex vtxMid) GetVerticesWhereLine(List<Vertex> newVertices)
    {
        // Vertex vtx0, Vertex vtxEnd reflect the orientation of newVertices
        Vertex vtx0 = BasePolygon.FindClosestVertex(newVertices[0].Position);
        Vertex vtxEnd = BasePolygon.FindClosestVertex(newVertices[newVertices.Count - 1].Position);
        Vertex vtxMid;
        if (vtx0 == null && vtxEnd == null)
        {

            Debug.Log("Failed to snap to base vertices." + newVertices[0].Position + " " + newVertices[newVertices.Count - 1].Position);
            return (null, null, null); ;
        }
        if (vtx0 == null)
        {
            Edge edge = BasePolygon.FindEdgeThroughMidpoint(newVertices[0].Position);
            if (vtxEnd.Position == edge?.B.Position) 
                vtx0 = edge?.A; 
            else
                vtx0 = edge?.B;
        }
        if (vtxEnd == null)
        {
            Edge edge = BasePolygon.FindEdgeThroughMidpoint(newVertices[newVertices.Count - 1].Position);
            if (vtx0.Position == edge?.B.Position)
                vtxEnd = edge?.A;
            else
                vtxEnd = edge?.B;
        }

        if (vtx0 == null || vtxEnd == null)
        {
            Debug.Log("Failed to snap to base midpoints." + newVertices[0].Position + " " + newVertices[newVertices.Count - 1].Position);
            return (null,null, null);
        }
        vtxMid = FindEdgeThroughMidpoint((vtx0.Position + vtxEnd.Position) / 2).MidPoint;

        return (vtx0,vtxEnd, vtxMid);
    }

    public void ResetLine(LineRenderer lineRenderer)
    {
        List<Vertex> newVertices = ToVertices(lineRenderer);
        (Vertex vtx0, Vertex vtxEnd, Vertex vtxMid) = GetVerticesWhereLine(newVertices);
        (Vertex line0, Vertex lineEnd) = (newVertices[0], newVertices[^1]);
        //ResetVerticesBetween(vtxMid, vtxEnd);
        //ResetVerticesBetween(vtx0, vtxMid);
        ResetEdge(vtx0, vtxEnd);
    }

    //public void ResetEdge(Vertex vtx0, Vertex vtxEnd)
    //{
    //    Vertex mid = FindEdgeThroughMidpoint((vtx0.Position + vtxEnd.Position) / 2 ).MidPoint;
    //    ResetVerticesBetween(vtx0, mid);
    //    ResetVerticesBetween(mid, vtxEnd);
    //}


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
        Debug.Log("Replacing Vertices");
        ClearEdge(vertex1, vertex2);

        (List<Vertex> oldVertices, List<Vertex> oldVertices2) = _baseToDerivedVertex[(vertex1, vertex2)];
        Vertex oldVtx1 = vertex1;
        Vertex oldVtx2 = vertex2;
        int index1 = _vertices.IndexOf(oldVtx1);
        int index2 = _vertices.IndexOf(oldVtx2);

        if (index1 < 0 || index2 < 0 || index1 >= _vertices.Count || index2 >= _vertices.Count)
        {
            Debug.LogError("Invalid vertex indices.");
            return;
        }

        // If traversal is reversed, swap indices and invert inserted vertices
        if ((index2 < index1 && !(index2==0 && index1==_vertices.Count-2)) || (index1 == 0 && index2 == _vertices.Count - 2))
        {
            Debug.Log("Reversed vtices");
            (index1, index2) = (index2, index1);
            newVertices.Reverse();
        }

        int removeStart = index1+1;
        int removeCount = index2 - index1 - 2;
        if (index2 == 0 && index1 == _vertices.Count - 2)
        {
            removeCount = 0;
            Debug.Log("fix");
        }

        Wipe(oldVertices);
        Wipe(oldVertices2);
        if (IsMidPoint(newVertices[^1]))
        {
            Debug.Log("First half");
            newVertices.RemoveAt(0);
            newVertices.RemoveAt(newVertices.Count - 1);
            oldVertices.InsertRange(1,newVertices);
        }
        else if (IsMidPoint(newVertices[0]))
        {
            Debug.Log("2nd half");
            newVertices.RemoveAt(0);
            newVertices.RemoveAt(newVertices.Count - 1);
            oldVertices2.InsertRange(1, newVertices);
            removeStart++;
        } 
        else
        {
            Debug.Log("all");
            newVertices.RemoveAt(0);
            newVertices.RemoveAt(newVertices.Count - 1);
            oldVertices.InsertRange(1, newVertices);
            removeCount++;
        }

        if (removeCount > 0)
            _vertices.RemoveRange(removeStart, removeCount);

        _vertices.InsertRange(removeStart, newVertices);
    }

    private void Wipe(List<Vertex> oldVertices)
    {
        if (oldVertices.Count <= 1) return;

        Vertex start = oldVertices[0];
        Vertex end = oldVertices[^1];

        oldVertices.Clear();
        oldVertices.Add(start);
        oldVertices.Add(end);
    }

    private bool IsMidPoint(Vertex vertex)
    {
        Edge edge = FindEdgeThroughMidpoint(vertex.Position);
        if (edge != null) 
            return true;
        return false;
    }

    private void AddVerticesBetween(List<Vertex> newVertices, Vertex vertex0, Vertex vertexEnd, Vertex vertexM)
    {
        Debug.Log("Adding Vertices");
        (List<Vertex> oldVertices, List < Vertex > oldVertices2) = _baseToDerivedVertex[(vertex0, vertexEnd)];
        int index1 = _vertices.IndexOf(vertex0);
        int index2 = _vertices.IndexOf(vertexEnd);

        if (index1 < 0 || index2 < 0 || index1 >= _vertices.Count || index2 >= _vertices.Count)
        {
            Debug.LogError("Invalid vertex indices.");
            return;
        }

        // If traversal is reversed, swap indices and invert inserted vertices
        if ((index2 < index1 && !(index2 == 0 && index1 == _vertices.Count - 2)) || (index1 == 0 && index2 == _vertices.Count - 2))
        {
            Debug.Log("Reversed vtices");
            (index1, index2) = (index2, index1);
            newVertices.Reverse();
        }


        int addStart;
        if (IsMidPoint(newVertices[^1]))
        {
            Debug.Log("First half");
            newVertices.RemoveAt(0);
            newVertices.RemoveAt(newVertices.Count - 1);
            oldVertices.InsertRange(1, newVertices);
            addStart = _vertices.IndexOf(oldVertices[0]) + 1;
        }
        else if (IsMidPoint(newVertices[0]))
        {
            Debug.Log("2nd half");
            newVertices.RemoveAt(0);
            newVertices.RemoveAt(newVertices.Count - 1);
            oldVertices2.InsertRange(1, newVertices);
            addStart = _vertices.IndexOf(oldVertices2[0]) + 1; ;
        } else
        {
            newVertices.RemoveAt(0);
            newVertices.RemoveAt(newVertices.Count - 1); 
            addStart = _vertices.IndexOf(oldVertices[0]) + 1;
        }

        _vertices.InsertRange(addStart, newVertices);
    }

    private void ClearEdge(Vertex vertex1, Vertex vertex2)
    {
        Debug.Log("Clear Edge");
        ISelectable oldEdge = SelectionManager.Instance.FindSelectableWithEndpnts(vertex1.Position, vertex2.Position);
        oldEdge?.Remove();
        Edge edge = BasePolygon.GetEdge(vertex1, vertex2);
        oldEdge = SelectionManager.Instance.FindSelectableWithEndpnts(vertex1.Position, edge.MidPoint.Position);
        oldEdge?.Remove();
        oldEdge = SelectionManager.Instance.FindSelectableWithEndpnts(edge.MidPoint.Position, vertex2.Position);
        oldEdge?.Remove();
    }

    // I think it assumes vertices are endpoints (midpoint check is superfluous)
    private void ResetEdge(Vertex vtx0, Vertex vtxEnd)
    {
        Debug.Log("Reset Vertices");
        (List<Vertex> oldVertices, List<Vertex> oldVertices2) = _baseToDerivedVertex[(vtx0, vtxEnd)];


        int removeStart = _vertices.IndexOf(oldVertices[0]) + 1;
        int removeEnd = _vertices.IndexOf(oldVertices2[^1]);

        int removeCount = removeEnd - removeStart;
        if (removeEnd < removeStart) 
            removeCount = _vertices.Count - removeStart;


        // need to add midpoint back to _vertices
        Vertex mid = FindEdgeThroughMidpoint((vtx0.Position + vtxEnd.Position) / 2).MidPoint;

        if (removeCount > 0)
        {
            _vertices.RemoveRange(removeStart, removeCount);
        }
        _vertices.Insert(removeStart, mid);

        Wipe(oldVertices);
        Wipe(oldVertices2);

    }

    private (Vertex, Vertex) GetEndpntVertices(LineRenderer lineRenderer)
    {
        List<Vertex> newVertices = ToVertices(lineRenderer);
        Vertex vtx0 = BasePolygon.FindClosestVertex(newVertices[0].Position);
        Vertex vtxEnd = BasePolygon.FindClosestVertex(newVertices[newVertices.Count - 1].Position);

        return (vtx0, vtxEnd);
    }


    //internal LineSelectable TryPaste(GameObject lineObj, Vector2 position)
    //{
    //    LineRenderer lineRenderer = lineObj.GetComponent<LineRenderer>();
    //    if (lineRenderer == null)
    //    {
    //        Debug.LogError("GameObject does not have a LineRenderer component.");
    //        return null;
    //    }

    //    for (int i=0; i < BasePolygon.SnapVertices.Count; i++)
    //    {
    //        Vector2 midpoint = (BasePolygon.Vertices[i].Position + BasePolygon.Vertices[(i + 1) % BasePolygon.Vertices.Count].Position) / 2;
    //        if (Vector3.Distance(midpoint, position) <= snapDistance)
    //        {
    //            GameObject newObj = Instantiate(lineObj);
    //            newObj.GetComponent<EdgeSelectable>().Polygon = this;
    //            Debug.Log("replacing(pasting) edge");
    //            bool success = this.ReplaceEdge(newObj);
    //            if (!success)
    //            {
    //                Destroy(newObj);
    //                return null;
    //            }
    //            return newObj.GetComponent<LineSelectable>();
    //        }
    //    }
    //    return null;
    //}

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

class VertexTupleComparer : IEqualityComparer<(Vertex, Vertex)>
{
    public bool Equals((Vertex, Vertex) x, (Vertex, Vertex) y)
    {
        return
            (x.Item1.Equals(y.Item1) && x.Item2.Equals(y.Item2)) ||
            (x.Item1.Equals(y.Item2) && x.Item2.Equals(y.Item1));
    }

    public int GetHashCode((Vertex, Vertex) obj)
    {
        // Order-independent hash
        int h1 = obj.Item1.GetHashCode();
        int h2 = obj.Item2.GetHashCode();

        return h1 ^ h2; // XOR is symmetric
    }
}