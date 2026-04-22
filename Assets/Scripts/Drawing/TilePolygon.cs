using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework.Constraints;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEngine.GraphicsBuffer;

public class TilePolygon : DerivedPolygon
{
    public int Rotations { get; set; }
    public int StraightRotations { get; set; }
    public int AdjGlideReflections { get; set; }
    public int ParallelGlideReflections { get; set; }

    private void Awake()
    {
        base.Initialize();
        Rotations = 0;
        StraightRotations = 0;
        AdjGlideReflections = 0;
        ParallelGlideReflections = 0;
    }

    public bool EdgeIsDrawn(Vertex vertex1, Vertex vertex2)
    {
        //(Vertex vertex1, Vertex vertex2) = (Edges[edgeIdx].A, Edges[edgeIdx].B);
        ISelectable oldEdge = SelectionManager.Instance.FindSelectableWithEndpnts(vertex1.Position, vertex2.Position);
        if (oldEdge != null) return true;
        return false;
    }

    public bool EdgeIsHalfDrawn(Vertex vtx0, Vertex vtxEnd, Vertex midpoint = null)
    {
        if (midpoint == null)
        {
            Edge edge = BasePolygon.GetEdge(vtx0, vtxEnd);
            if (edge == null)
            {
                Debug.LogError("Edge not found between vertices: " + vtx0.Position + " and " + vtxEnd.Position);
                return false;
            }
            midpoint = edge.MidPoint;
        }
        ISelectable oldEdge = SelectionManager.Instance.FindSelectableWithEndpnts(vtx0.Position, midpoint.Position);
        if (oldEdge != null) return true;
        oldEdge = SelectionManager.Instance.FindSelectableWithEndpnts(midpoint.Position, vtxEnd.Position);
        if (oldEdge != null) return true;
        return false;
    }

    public override bool ReplaceEdge(GameObject line)
    {
        LineRenderer lineRenderer = line.GetComponent<LineRenderer>();
        if (lineRenderer == null)
        {
            Debug.LogError("GameObject does not have a LineRenderer component.");
            return false;
        }

        List<Vertex> newVertices = base.ToVertices(lineRenderer);
        (Vertex vtx0, Vertex vtxEnd, Vertex vtxM) = base.GetVerticesWhereLine(newVertices);

        Debug.Log("Drawn edges: " + DrawnEdges + "Drawn half edges: " + DrawnHalfEdges + "Drawable: " + TotalEdges / 2);

        if (((float)DrawnEdges + (float)(DrawnHalfEdges / 2)) >= (float)TotalEdges / 2)
            return false;

        if (!IsMidPoint(newVertices[^1]) && !IsMidPoint(newVertices[0]))
        {
            Debug.Log("midpoint");
            if (((float)DrawnEdges + 1 + (float)(DrawnHalfEdges / 2)) > (float)TotalEdges / 2)
                return false;
        }
        else
        {
            if (((float)DrawnEdges + (float)((DrawnHalfEdges + 1) / 2)) > ((float)TotalEdges / 2))
                return false;
        }

        if (!EdgeIsDrawn(vtx0,vtxEnd) && !EdgeIsHalfDrawn(vtx0, vtxEnd, vtxM) && ExistsSymmTransformation(line.GetComponent<EdgeSelectable>()))
            return base.ReplaceEdge(line);
        return false;
    }

    private bool ExistsSymmTransformation(EdgeSelectable line)
    {
        List<int> result = FindTranslationCompatibleEdges(line).Union(FindRotationCompatibleEdges(line).Union(FindGlideReflectionCompatibleEdges(line))).ToList();
        return result.Count > 0;
    }

    internal List<int> FindGlideReflectionCompatibleEdges(EdgeSelectable selectedLine)
    {
        // We can define glide reflection compatibility as the same criteria as translation compatibility (parallel and equal length),
        // since glide reflection is essentially a translation followed by a reflection.
        // A special case are glide reflections of type VI which reflect on adjacent lines.
        // half edges cannot be glide reflected.

        Debug.Log("Looking for compatible rot...");
        List<int> edges = new List<int>();
        if (selectedLine == null)
        {
            Debug.Log("null edge");
            return edges;
        }
        if (selectedLine.SymmEdge != null)
        {
            Debug.Log("Already has symm edge");
            return edges;
        }

        (Vector2 origA, Vector2 origB) = GetLineEndpoints(selectedLine);
        float selectedLength = GetEdgeLength(selectedLine);
        Vector2 selectedDir = GetEdgeDirection(selectedLine);

        float directionTolerance = 0.99f;

        float lengthTolerance = snapDistance;
        bool adjacency;

        for (int i = 0; i < BasePolygon.SnapVertices.Count; i++)
        {
            Vertex va = BasePolygon.SnapVertices[i];
            Vertex vb = BasePolygon.SnapVertices[(i + 1) % BasePolygon.SnapVertices.Count];
            if(EdgeIsDrawn(va, vb) || EdgeIsHalfDrawn(va, vb))
                continue;
            Vector2 a = va.Position;
            Vector2 b = vb.Position;

            Vector2 dir = (b - a).normalized;
            float length = Vector2.Distance(a, b);

            bool lengthMatch = Mathf.Abs(length - selectedLength) < lengthTolerance;

            adjacency = (a == origA || a == origB) || (b == origA || b == origB);

            // dot product checks orientation similarity
            bool directionMatch = Mathf.Abs(Vector2.Dot(dir, selectedDir)) > directionTolerance;

            bool isSelf = (a == origA && b == origB) || (a == origB && b == origA);

            if (lengthMatch && (adjacency || directionMatch) && !isSelf)
            {
                edges.Add(i);
            }
        }

        return edges;
    }

    /// <summary>
    /// Finds edges in the base polygon that are compatible for rotation with the selected line.
    /// An edge is considered compatible if it has the same length (up to floating pnt error) and shares at least one endpoint with the selected line.
    /// </summary>
    /// <param name="selectedLine"> </param> 
    /// <returns>  </returns>
    public List<int> FindRotationCompatibleEdges(EdgeSelectable selectedLine)
    {
        Debug.Log("Looking for compatible rot...");
        List<int> edges = new List<int>();
        if (selectedLine == null)
        {
            Debug.Log("null edge");
            return edges;
        }
        if (selectedLine.SymmEdge != null)
        {
            Debug.Log("Already has symm edge");
            return edges;
        }

        (Vector2 origA, Vector2 origB) = GetLineEndpoints(selectedLine);
        float selectedLength = GetEdgeLength(selectedLine);

        float lengthTolerance = snapDistance;
        bool adjacency;
        bool inEdge = false;

        for (int i = 0; i < BasePolygon.SnapVertices.Count; i++)
        {
            Vertex va = BasePolygon.SnapVertices[i];
            Vertex vb = BasePolygon.SnapVertices[(i + 1) % BasePolygon.SnapVertices.Count];
            if (EdgeIsDrawn(va, vb) || EdgeIsHalfDrawn(va, vb))
                continue;
            Vector2 a = BasePolygon.SnapVertices[i].Position;
            Vector2 b = BasePolygon.SnapVertices[(i + 1) % BasePolygon.SnapVertices.Count].Position;

            Vector2 dir = (b - a).normalized;
            float length = Vector2.Distance(a, b);

            bool lengthMatch = Mathf.Abs(length - selectedLength) < lengthTolerance;

            adjacency = (a==origA || a == origB) || (b == origA || b == origB);

            bool isSelf = (a == origA && b == origB) || (a == origB && b == origA);

            if (lengthMatch && adjacency && !isSelf)
            {
                edges.Add(i);
            }
        }

        for (int i = 0; i < BasePolygon.Midpoints.Count; i++)
        {
            Vector2 m = BasePolygon.Midpoints[i].Position;

            inEdge = (m == origA || m == origB);
            Debug.Log("Mid: "+ m + "Line ends " + origA + " " + origB);

            if (inEdge)
            {
                edges.Add(i);
            }
        }

        return edges;
    }

    /// <summary>
    /// Finds edges in the base polygon that are compatible for translation with the selected line.
    /// An edge is considered compatible if it has the same length and is parallel (up to floating pnt error) to the selected line.
    /// </summary>
    /// <param name="selectedLine"></param>
    /// <returns></returns>
    public List<int> FindTranslationCompatibleEdges(EdgeSelectable selectedLine)
    {
        Debug.Log("Looking for compatible...");
        List<int> edges = new List<int>();
        if (selectedLine == null)
        {
            Debug.Log("null edge");
            return edges;
        }

        if (selectedLine.SymmEdge != null)
        {
            Debug.Log("Already has symm edge");
            return edges;
        }

        (Vector2 origA, Vector2 origB) = GetLineEndpoints(selectedLine);
        Vector2 selectedDir = GetEdgeDirection(selectedLine);
        float selectedLength = GetEdgeLength(selectedLine);

        float lengthTolerance = snapDistance; 
        float directionTolerance = 0.99f;

        for (int i = 0; i < BasePolygon.SnapVertices.Count; i++)
        {
            Vertex va = BasePolygon.SnapVertices[i];
            Vertex vb = BasePolygon.SnapVertices[(i + 1) % BasePolygon.SnapVertices.Count];
            if (EdgeIsDrawn(va, vb) || EdgeIsHalfDrawn(va, vb))
                continue;
            Vector2 a = BasePolygon.SnapVertices[i].Position;
            Vector2 b = BasePolygon.SnapVertices[(i + 1) % BasePolygon.SnapVertices.Count].Position;

            Vector2 dir = (b - a).normalized;
            float length = Vector2.Distance(a, b);

            bool lengthMatch = Mathf.Abs(length - selectedLength) < lengthTolerance;

            // dot product checks orientation similarity
            bool directionMatch = Mathf.Abs(Vector2.Dot(dir, selectedDir)) > directionTolerance;

            bool isSelf = (a == origA && b == origB) || (a == origB && b == origA);
            
            if (lengthMatch && directionMatch && !isSelf)
            {
                edges.Add(i);
            }
        }

        return edges;
    }


    /// <summary>
    /// Checks if the given position is within snap distance of the edge at the specified index in the base polygon.
    /// </summary>
    /// <param name="pos"></param>
    /// <param name="idx"></param>
    /// <returns></returns>
    public bool IsInEdgeWithIdx(Vector2 pos, int idx)
    {
        if (idx < 0 || idx >= BasePolygon.Edges.Count)
        {
            Debug.LogError("Edge index out of range: " + idx);
            return false;
        }
        Edge edge = BasePolygon.Edges[idx];
        if (edge == null)
        {
            Debug.LogError("Edge not found");
            return false;
        }
        Vector2 a = edge.A.Position;
        Vector2 b = edge.B.Position;

        float distance = LineSelectable.DistancePointToSegment(pos, a, b);

        return distance <= snapDistance;
    }

    /// <summary>
    /// Returns the endpoints of the line as Vector2s. 
    /// Assumes the line renderer has at least 2 positions.
    /// </summary>
    /// <param name="line"></param>
    /// <returns></returns>
    (Vector2, Vector2) GetLineEndpoints(LineSelectable line)
    {
        var lr = line.GetComponent<LineRenderer>();

        Vector2 a = lr.GetPosition(0);
        Vector2 b = lr.GetPosition(lr.positionCount - 1);

        return (a, b);
    }

    /// <summary>
    /// Returns the normalized direction vector of the line. 
    /// Assumes the line renderer has at least 2 positions.
    /// </summary>
    /// <param name="line"></param>
    /// <returns></returns>
    Vector2 GetEdgeDirection(LineSelectable line)
    {
        (Vector2 a, Vector2 b) = GetLineEndpoints(line);

        return (b - a).normalized;
    }

    /// <summary>
    /// Returns the length of the line. Assumes the line renderer has at least 2 positions and is straight. 
    /// For more complex lines, this would need to be modified to sum segment lengths.
    /// </summary>
    /// <param name="line"></param>
    /// <returns></returns>
    float GetEdgeLength(LineSelectable line)
    {
        var lr = line.GetComponent<LineRenderer>();

        Vector2 a = lr.GetPosition(0);
        Vector2 b = lr.GetPosition(lr.positionCount-1);

        return Vector2.Distance(a, b);
    }

    // -------------------------------------------------------------------
    // Transformations
    // -------------------------------------------------------------------

    private void SetEdgeProps(GameObject newObj, GameObject lineObj)
    {
        EdgeSelectable newES = newObj.GetComponent<EdgeSelectable>();
        newES.Polygon = this;
        EdgeSelectable oldES = lineObj.GetComponent<EdgeSelectable>();
        oldES.SymmEdge = newES;
        newES.SymmEdge = oldES;
    }

    /// <summary>
    /// Translates the given line object to align with the edge at the specified index in the base polygon.
    /// </summary>
    /// <param name="lineObj"></param>
    /// <param name="selectedEdg"></param>
    /// <returns></returns>
    internal ISelectable Translate(GameObject lineObj, int selectedEdg)
    {
        Edge edge = BasePolygon.Edges[selectedEdg];

        LineRenderer src = lineObj.GetComponent<LineRenderer>();

        if (src == null)
        {
            Debug.LogError("No LineRenderer on source object");
            return null;
        }

        GameObject newObj = Instantiate(lineObj);
        SetEdgeProps(newObj, lineObj);

        LineSelectable ls = newObj.GetComponent<LineSelectable>();
        LineRenderer lr = newObj.GetComponent<LineRenderer>();

        Vector2 a = lr.GetPosition(0);
        Vector2 b = lr.GetPosition(lr.positionCount -1);
        Vector2 midpnt = (a + b)/2;

        Vector2 targetA = edge.A.Position;
        Vector2 targetB = edge.B.Position;
        Vector2 targetMidpnt = (targetA + targetB)/2;

        // Compute translation so first point aligns
        Vector2 delta = targetMidpnt - midpnt;

        Vector2 center = ls.Center;
        Debug.Log("Center: "+center + "New: "+ (center+delta));
        ls.OnTranslate(center + delta);

        Debug.Log("Translating edge");

        bool success = base.ReplaceEdge(newObj);

        if (!success)
        {
            Destroy(newObj);
            return null;
        }
        DrawnEdges--;
        return ls;
    }

    /// <summary>
    /// Rotates the given line object to align with the edge at the specified index in the base polygon.
    /// </summary>
    /// <param name="lineObj"></param>
    /// <param name="selectedEdg"></param>
    /// <returns></returns>
    internal ISelectable Rotate(GameObject lineObj, int selectedEdg)
    {
        bool success = false;

        Edge edge = BasePolygon.Edges[selectedEdg];

        LineRenderer src = lineObj.GetComponent<LineRenderer>();

        if (src == null)
        {
            Debug.LogError("No LineRenderer on source object");
            return null;
        }

        GameObject newObj = Instantiate(lineObj);
        SetEdgeProps(newObj, lineObj);

        LineSelectable ls = newObj.GetComponent<LineSelectable>();
        LineRenderer lr = newObj.GetComponent<LineRenderer>();

        // Source line
        Vector2 a = lr.GetPosition(0);
        Vector2 b = lr.GetPosition(lr.positionCount - 1);

        // Target edge
        Vector2 targetA = edge.A.Position;
        Vector2 targetB = edge.B.Position;
        Vector2 targetMid = edge.MidPoint.Position;

        if (Vector2.Distance(a, targetMid)<snapDistance || Vector2.Distance(b, targetMid) < snapDistance)
        {
            ls.OnRotate(180, targetMid);
            success = ExtendEdge(newObj);
            StraightRotations += (success ? 1 : 0);
        } 
        else
        {
            bool pivotIsA = Vector2.Distance(a, targetA) < snapDistance || Vector2.Distance(a, targetB) < snapDistance;

            Vector2 pivot = pivotIsA ? a : b;

            // The angles are computed with respect to the pivot point (i.e. The direction vectors are from pivot to the other endpoint)
            Vector2 sourceDir = pivotIsA ? (b - a) : (a - b);
            float sourceAngle = Mathf.Atan2(sourceDir.y, sourceDir.x) * Mathf.Rad2Deg;

            Vector2 targetDir = (Vector2.Distance(pivot, targetA) < snapDistance ? targetB - targetA : targetA - targetB);
            float targetAngle = Mathf.Atan2(targetDir.y, targetDir.x) * Mathf.Rad2Deg;

            // Rotation difference
            float deltaAngle = targetAngle - sourceAngle;

            ls.OnRotate(deltaAngle, pivot);
            Debug.Log("Rotating edge by " + deltaAngle);
            success = base.ReplaceEdge(newObj);
            Rotations += (success ? 1 : 0);
        }

        if (!success)
        {
            Destroy(newObj);
            return null;
        }

        DrawnEdges--;
        return ls;
    }

    internal ISelectable GlideReflect(GameObject lineObj, int selectedEdg)
    {
        Edge edge = BasePolygon.Edges[selectedEdg];

        LineRenderer src = lineObj.GetComponent<LineRenderer>();

        if (src == null)
        {
            Debug.LogError("No LineRenderer on source object");
            return null;
        }
        GameObject newObj = Instantiate(lineObj);
        SetEdgeProps(newObj, lineObj);

        LineSelectable ls = newObj.GetComponent<LineSelectable>();
        LineRenderer lr = newObj.GetComponent<LineRenderer>();

        Vector2 a = lr.GetPosition(0);
        Vector2 b = lr.GetPosition(lr.positionCount - 1);
        Vector2 midpnt = (a + b) / 2;
        Vector2 dir = (b - a).normalized;

        Vector2 targetA = edge.A.Position;
        Vector2 targetB = edge.B.Position;
        Vector2 targetMidpnt = (targetA + targetB) / 2;
        Vector2 targerDir = (targetB - targetA).normalized;


        // dot product checks orientation similarity
        bool parallel = Mathf.Abs(Vector2.Dot(dir, targerDir)) > 0.99f;

        // Compute translation so first point aligns
        Vector2 delta = targetMidpnt - midpnt;

        Vector2 center = ls.Center;
        Debug.Log("Center: " + center + "New: " + (center + delta));
        ls.OnReflect(center, dir);
        bool success = false;
        if (parallel)
        {
            ls.OnTranslate(center + delta);
            success = base.ReplaceEdge(newObj);
            ParallelGlideReflections += (success ? 1 : 0);
        }
        else
        {
            bool pivotIsA = Vector2.Distance(a, targetA) < snapDistance || Vector2.Distance(a, targetB) < snapDistance;

            Vector2 pivot = pivotIsA ? a : b;

            // The angles are computed with respect to the pivot point (i.e. The direction vectors are from pivot to the other endpoint)
            Vector2 sourceDir = pivotIsA ? (b - a) : (a - b);
            float sourceAngle = Mathf.Atan2(sourceDir.y, sourceDir.x) * Mathf.Rad2Deg;

            Vector2 targetDir = (Vector2.Distance(pivot, targetA) < snapDistance ? targetB - targetA : targetA - targetB);
            float targetAngle = Mathf.Atan2(targetDir.y, targetDir.x) * Mathf.Rad2Deg;

            // Rotation difference
            float deltaAngle = targetAngle - sourceAngle;

            ls.OnRotate(deltaAngle, pivot);
            Debug.Log("Glide reflecting edge");

            success = base.ReplaceEdge(newObj);
            AdjGlideReflections += (success ? 1 : 0);
        }

        if (!success)
        {
            Destroy(newObj);
            return null;
        }

        DrawnEdges--;
        return ls;
    }

    // ------------------------------------------
    // visuals
    // ------------------------------------------

    [SerializeField] private GameObject highlightPrefab; // transparent sprite in the future?

    /// <summary>
    /// Highlights the edges at the specified indices in the base polygon by changing their line renderer colors to the given highlight color.
    /// </summary>
    /// <param name="edgeIndices"></param>
    /// <param name="highlightColor"></param>
    public void HighlightEdges(List<int> edgeIndices, Color highlightColor)
    {
        if (BasePolygon.edgeRenderers == null)
            return;

        DehighlightEdges();

        foreach (int index in edgeIndices)
        {
            if (index < 0 || index >= BasePolygon.edgeRenderers.Count)
            {
                Debug.LogWarning("Invalid edge index: " + index);
                continue;
            }

            var lr = BasePolygon.edgeRenderers[index];
            lr.startColor = highlightColor;
            lr.endColor = highlightColor;
        }
    }

    /// <summary>
    /// Resets the colors of all edge line renderers in the base polygon to black.
    /// </summary>
    public void DehighlightEdges()
    {
        for (int i = 0; i < BasePolygon.edgeRenderers.Count; i++)
        {
            BasePolygon.edgeRenderers[i].startColor =
                BasePolygon.edgeRenderers[i].endColor = Color.black;
        }
    }

}
