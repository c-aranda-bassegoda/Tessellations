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

        float lengthTolerance = snapDistance * 2;
        float directionTolerance = snapDistance * 2;

        for (int i = 0; i < BasePolygon.SnapVertices.Count; i++)
        {
            Vector2 a = BasePolygon.SnapVertices[i].Position;
            Vector2 b = BasePolygon.SnapVertices[(i + 1) % BasePolygon.SnapVertices.Count].Position;

            Vector2 dir = (b - a).normalized;
            float length = Vector2.Distance(a, b);

            bool lengthMatch = Mathf.Abs(length - selectedLength) < lengthTolerance;

            // dot product checks orientation similarity
            bool directionMatch = Mathf.Abs(Vector2.Dot(dir, selectedDir)) > directionTolerance;

            Debug.Log("Length: "+ length +"vs"+selectedLength + "Direction: " + dir + "vs" + selectedDir);

            if (lengthMatch && directionMatch)
            {
                edges.Add(i);
                Debug.Log("Compatible" + i);
            }
        }

        return edges;
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
