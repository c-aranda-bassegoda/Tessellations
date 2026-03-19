using System.Collections.Generic;
using UnityEngine;

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

    /// <summary>
    /// Replaces the vertices of the edge whose andpoints correspond to the lineRenderer with the vertices corresponding to the lineRenderer.
    /// </summary>
    /// <param name="line"></param>
    /// <returns></returns>
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

    /// <summary>
    /// Adds vertices to the edge corresponding to the lineRenderer. It does not remove any vertices that were there already, 
    /// so it can be used to extend a half edge to a full edge or to add more vertices to an already fully extended edge.
    /// HOwever, the latter will result in a double edge which will cause some weird behavior (e.g. The ContainsPoint() method is not well defined for such polygons).
    /// </summary>
    /// <param name="line"></param>
    /// <returns></returns>
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

    /// <summary>
    /// Given a list of vertices corresponding (position-wise) to an edge or to half of the edge, it returns the endpoints and midpoint of said edge.
    /// (vtx0, vtxEnd) are the endpoints of the edge corresponding to the lineRenderer. They reflect the orientation of newVertices 
    /// (i.e. if newVertices goes from b to a, then a is the second item in the tuple and b is the first).
    /// </summary>
    /// <param name="newVertices"></param>
    /// <returns></returns>
    private (Vertex vtx0, Vertex vtxEnd, Vertex vtxMid) GetVerticesWhereLine(List<Vertex> newVertices)
    {
        // Vertex vtx0, Vertex vtxEnd reflect the orientation of newVertices
        Vertex vtx0 = BasePolygon.FindClosestVertex(newVertices[0].Position);
        Vertex vtxEnd = BasePolygon.FindClosestVertex(newVertices[newVertices.Count - 1].Position);
        Vertex vtxMid;
        if (vtx0 == null && vtxEnd == null)
        {
            Debug.LogError("Failed to snap to base vertices." + newVertices[0].Position + " " + newVertices[newVertices.Count - 1].Position);
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

    /// <summary>
    /// Resets the vertices of the edge corresponding to the lineRenderer to be a straight line between vtx0 and vtxEnd.
    /// </summary>
    /// <param name="lineRenderer"></param>
    public void ResetLine(LineRenderer lineRenderer)
    {
        List<Vertex> newVertices = ToVertices(lineRenderer);
        (Vertex vtx0, Vertex vtxEnd, Vertex vtxMid) = GetVerticesWhereLine(newVertices);
        (Vertex line0, Vertex lineEnd) = (newVertices[0], newVertices[^1]);

        ResetEdge(vtx0, vtxEnd);
    }

    /// <summary>
    /// Converts a LineRenderer to a list of vertices. 
    /// </summary>
    /// <param name="lineRenderer"></param>
    /// <returns></returns>
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

    /// <summary>
    /// Replaces vertices of an edge. 
    /// newVertices must correspond to a list of vertices from one end of an edge (vertex1, vertex2) to the opposite end (of the edge or half edge)
    /// It removes any vertices that might've previously been on the edge (vertex1, vertex2).
    /// </summary>
    /// <param name="newVertices"></param>
    /// <param name="vertex1"></param>
    /// <param name="vertex2"></param>
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
            (index1, index2) = (index2, index1);
            newVertices.Reverse();
        }

        int removeStart = index1+1;
        int removeCount = index2 - index1 - 2;
        if (index2 == 0 && index1 == _vertices.Count - 2)
        {
            removeCount = 0;
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

    /// <summary>
    /// Removes all vertices from the list except the first and the last one.
    /// </summary>
    /// <param name="oldVertices"></param>
    private void Wipe(List<Vertex> oldVertices)
    {
        if (oldVertices.Count <= 1) return;

        Vertex start = oldVertices[0];
        Vertex end = oldVertices[^1];

        oldVertices.Clear();
        oldVertices.Add(start);
        oldVertices.Add(end);
    }

    /// <summary>
    /// Checks if the vertex position corresponds to a midpoint of an edge in the base polygon.
    /// </summary>
    /// <param name="vertex"></param>
    /// <returns></returns>
    private bool IsMidPoint(Vertex vertex)
    {
        Edge edge = FindEdgeThroughMidpoint(vertex.Position);
        if (edge != null) 
            return true;
        return false;
    }

    /// <summary>
    /// Adds vertices to an edge (without removing vertices that were there already!)
    /// </summary>
    /// <param name="newVertices"></param>
    /// <param name="vertex0"></param>
    /// <param name="vertexEnd"></param>
    /// <param name="vertexM"></param>
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

    /// <summary>
    /// Removes all vertices corresponding to the edge (vertex1, vertex2) and deregisters any selectables corresponding to that edge from the selection manager.
    /// </summary>
    /// <param name="vertex1"></param>
    /// <param name="vertex2"></param>
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


    /// <summary>
    /// Resets the _vertices such that the edge corresponding to the base edge (vtx0, vtxEnd) 
    /// is a stright line between vtx0 and vtxEnd with only the midpoint in between.
    /// It assumes the input vertices are endpoints. 
    /// </summary>
    /// <param name="vtx0"></param>
    /// <param name="vtxEnd"></param>
    private void ResetEdge(Vertex vtx0, Vertex vtxEnd)
    {
        Debug.Log("Reset Vertices");
        (List<Vertex> oldVertices, List<Vertex> oldVertices2) = _baseToDerivedVertex[(vtx0, vtxEnd)];


        int removeStart = _vertices.IndexOf(oldVertices[0]) + 1;
        int removeEnd = _vertices.IndexOf(oldVertices2[^1]);

        int removeCount = removeEnd - removeStart;
        if (removeEnd < removeStart) // _vertices[0] is always the beginning of the first edge and the end of the last edge.
                                     // "removeEnd < removeStart" is only true if removeEnd = 0 (i.e. we're resetting the last edge)
            removeCount = _vertices.Count - removeStart;

        if (removeCount > 0)
        {
            _vertices.RemoveRange(removeStart, removeCount);
        }

        // We add midpoint back to _vertices 
        Vertex mid = FindEdgeThroughMidpoint((vtx0.Position + vtxEnd.Position) / 2).MidPoint;
        _vertices.Insert(removeStart, mid);

        Wipe(oldVertices);
        Wipe(oldVertices2);
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

/// <summary>
/// Custom comparer. The vertex tuple (a, b) should be the same as (b,a)
/// </summary>
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