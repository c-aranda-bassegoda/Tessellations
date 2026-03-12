using System;
using System.Collections.Generic;
using UnityEngine;

public class TilePolygon : DerivedPolygon
{
    public List<int> FindCompatibleEdges(LineSelectable selectedLine)
    {
        Debug.Log("Looking for compatible...");
        List<int> edges = new List<int>();
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

        float distance = DistancePointToSegment(pos, a, b);

        return distance <= snapDistance;
    }
    float DistancePointToSegment(Vector2 p, Vector2 a, Vector2 b)
    {
        Vector2 ab = b - a;
        float t = Vector2.Dot(p - a, ab) / ab.sqrMagnitude;
        t = Mathf.Clamp01(t);

        Vector2 closest = a + t * ab;
        return Vector2.Distance(p, closest);
    }

    Vector2 GetEdgeDirection(LineSelectable line)
    {
        var lr = line.GetComponent<LineRenderer>();

        Vector2 a = lr.GetPosition(0);
        Vector2 b = lr.GetPosition(lr.positionCount - 1);

        return (b - a).normalized;
    }

    float GetEdgeLength(LineSelectable line)
    {
        var lr = line.GetComponent<LineRenderer>();

        Vector2 a = lr.GetPosition(0);
        Vector2 b = lr.GetPosition(lr.positionCount-1);

        return Vector2.Distance(a, b);
    }

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
        ls.OnTransform(center + delta);

        Debug.Log("Translating edge");

        bool success = ReplaceEdge(newObj);

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
    
    public void DehighlightEdges()
    {
        for (int i = 0; i < BasePolygon.edgeRenderers.Count; i++)
        {
            BasePolygon.edgeRenderers[i].startColor =
                BasePolygon.edgeRenderers[i].endColor = Color.black;
        }
    }

}
