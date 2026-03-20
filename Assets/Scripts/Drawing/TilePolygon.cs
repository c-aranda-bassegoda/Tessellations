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

    public bool EdgeIsHalfDrawn(Vertex vertex1, Vertex vertex2)
    {
        Edge edge = BasePolygon.GetEdge(vertex1, vertex2);
        ISelectable oldEdge = SelectionManager.Instance.FindSelectableWithEndpnts(vertex1.Position, edge.MidPoint.Position);
        if (oldEdge != null) return true;
        oldEdge = SelectionManager.Instance.FindSelectableWithEndpnts(edge.MidPoint.Position, vertex2.Position);
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

        if (!EdgeIsDrawn(vtx0,vtxEnd))
            return base.ReplaceEdge(line);
        return false;
    }

    internal List<int> FindGlideReflectionCompatibleEdges(LineSelectable line)
    {
        // We can define glide reflection compatibility as the same criteria as translation compatibility (parallel and equal length),
        // since glide reflection is essentially a translation followed by a reflection.
        // A special case are glide reflections of type VI which reflect on adjacent lines.
        // For these the compatibility criteria is the same as rotation compatibility (equal length and sharing an endpoint).

        List<int> result = FindTranslationCompatibleEdges(line).Union(FindRotationCompatibleEdges(line)).ToList();
        return result;
    }

    /// <summary>
    /// Finds edges in the base polygon that are compatible for rotation with the selected line.
    /// An edge is considered compatible if it has the same length (up to floating pnt error) and shares at least one endpoint with the selected line.
    /// </summary>
    /// <param name="selectedLine"> </param> 
    /// <returns>  </returns>
    public List<int> FindRotationCompatibleEdges(LineSelectable selectedLine)
    {
        Debug.Log("Looking for compatible rot...");
        List<int> edges = new List<int>();
        if (selectedLine == null)
        {
            Debug.Log("null edge");
            return edges;
        }
        (Vector2 origA, Vector2 origB) = GetLineEndpoints(selectedLine);
        float selectedLength = GetEdgeLength(selectedLine);

        float lengthTolerance = snapDistance;
        bool adjacency;
        bool inEdge = false;

        for (int i = 0; i < BasePolygon.SnapVertices.Count; i++)
        {
            Vector2 a = BasePolygon.SnapVertices[i].Position;
            Vector2 b = BasePolygon.SnapVertices[(i + 1) % BasePolygon.SnapVertices.Count].Position;

            Vector2 dir = (b - a).normalized;
            float length = Vector2.Distance(a, b);

            bool lengthMatch = Mathf.Abs(length - selectedLength) < lengthTolerance;

            adjacency = (a==origA || a == origB) || (b == origA || b == origB);

            if (lengthMatch && adjacency)
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
    public List<int> FindTranslationCompatibleEdges(LineSelectable selectedLine)
    {
        Debug.Log("Looking for compatible...");
        List<int> edges = new List<int>();
        if (selectedLine == null)
        {
            Debug.Log("null edge");
            return edges;
        }
        Vector2 selectedDir = GetEdgeDirection(selectedLine);
        float selectedLength = GetEdgeLength(selectedLine);

        float lengthTolerance = snapDistance; 
        float directionTolerance = 0.99f;

        for (int i = 0; i < BasePolygon.SnapVertices.Count; i++)
        {
            Vector2 a = BasePolygon.SnapVertices[i].Position;
            Vector2 b = BasePolygon.SnapVertices[(i + 1) % BasePolygon.SnapVertices.Count].Position;

            Vector2 dir = (b - a).normalized;
            float length = Vector2.Distance(a, b);

            bool lengthMatch = Mathf.Abs(length - selectedLength) < lengthTolerance;

            // dot product checks orientation similarity
            bool directionMatch = Mathf.Abs(Vector2.Dot(dir, selectedDir)) > directionTolerance;

            if (lengthMatch && directionMatch)
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
        newObj.GetComponent<EdgeSelectable>().Polygon = this;

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

        bool success = ReplaceEdge(newObj);

        if (!success)
        {
            Destroy(newObj);
            return null;
        }
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
        newObj.GetComponent<EdgeSelectable>().Polygon = this;

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
            success = ReplaceEdge(newObj);
            Rotations += (success ? 1 : 0);
        }

        if (!success)
        {
            Destroy(newObj);
            return null;
        }

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
        newObj.GetComponent<EdgeSelectable>().Polygon = this;

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
            success = ReplaceEdge(newObj);
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

            success = ReplaceEdge(newObj);
            AdjGlideReflections += (success ? 1 : 0);
        }

        if (!success)
        {
            Destroy(newObj);
            return null;
        }

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
