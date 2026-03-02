using System.Collections.Generic;
using UnityEngine;

public class TessellationPolygon : PiecewisePolygon
{
    [SerializeField] private bool enableSymmetry = true;

    // Map each edge index to its symmetric edge index
    // e.g., edges[0] is symmetric to edges[3], etc.
    [SerializeField] private List<int> symmetricEdgeMap = new List<int>();

    [SerializeField] private List<Vector2> edgeTranslationOffsets = new List<Vector2>();

    /// <summary>
    /// Adds a node, and if symmetry is enabled, automatically adds mirrored node
    /// </summary>
    public override IPointSelectable TryAddPoint(Vector2 pointerWorldPos, bool smooth)
    {
        PathPointSelectable pointA = null;
        Path edge = null;

        foreach (var e in edges)
        {
            if (e.isNodeAt(pointerWorldPos)) break; // avoid making points too close together (causes floating point error upon transformation)

            var newPoint = e.TryAddPoint(pointerWorldPos, smooth) as PathPointSelectable;
            if (newPoint != null)
            {
                pointA = newPoint;
                edge = e;
                break;
            }
        }

        if (pointA == null)
            return null;

        PathPointSelectable pointB = null;

        if (enableSymmetry)
        {
            int edgeIndex = edges.IndexOf(edge);
            int symmetricIndex = GetSymmetricEdgeIndex(edgeIndex);
            if (symmetricIndex >= 0 && symmetricIndex < edges.Count) 
            {
                Path symmetricEdge = edges[symmetricIndex];

                Vector2 transformedPos = TranslatePointOnSymEdge(edgeIndex, pointA);

                pointB = symmetricEdge.TryAddPoint(transformedPos, smooth) as PathPointSelectable;
            }
        }
        if (pointB == null) return null;

        // Create composite selectable
        TessPointSelectable composite = new TessPointSelectable(pointA, pointB);

        Debug.Log("Adding point");

        return composite;
    }

    /// <summary>
    /// Returns the symmetric edge index, or -1 if none
    /// </summary>
    private int GetSymmetricEdgeIndex(int edgeIndex)
    {
        if (symmetricEdgeMap != null && edgeIndex < symmetricEdgeMap.Count)
            return symmetricEdgeMap[edgeIndex];
        return -1;
    }

    /// <summary>
    /// Translates the point into symmetric edge. Returns the position of pointA when translated onto edge with edgeIndex
    /// </summary>
    private Vector2 TranslatePointOnSymEdge(int edgeIndex, PathPointSelectable pointA)
    {
        if (edgeTranslationOffsets != null && edgeIndex < edgeTranslationOffsets.Count)
            return pointA.Position + edgeTranslationOffsets[edgeIndex]; 

        return Vector2.zero;
    }

    /// <summary>
    /// Automatically generate symmetric map if polygon is regular
    /// </summary>
    public void AutoGenerateSymmetryMap()
    {
        symmetricEdgeMap.Clear();
        int n = edges.Count;

        for (int i = 0; i < n; i++)
        {
            int opposite = (i + n / 2) % n;
            symmetricEdgeMap.Add(opposite);
        }
    }

    // TODO?: automatically generate edge symmetries

}
