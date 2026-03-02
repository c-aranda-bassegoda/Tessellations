using System.Collections.Generic;
using System.Drawing;
using NUnit.Framework.Internal;
using UnityEngine;
using static TessPointSelectable;

public class TessellationPolygon : PiecewisePolygon
{
    [SerializeField] private bool enableSymmetry = true;

    // Map each edge index to its symmetric edge index
    // e.g., edges[0] is symmetric to edges[3], etc.
    [SerializeField] private List<int> symmetricEdgeMap = new List<int>();
    [SerializeField] private List<Symmetry> symmetries = new List<Symmetry>();

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
        Symmetry symmetry = Symmetry.Translation;

        if (enableSymmetry)
        {
            int edgeIndex = edges.IndexOf(edge);
            symmetry = symmetries[edgeIndex];
            int symmetricIndex = GetSymmetricEdgeIndex(edgeIndex);
            if (symmetricIndex >= 0 && symmetricIndex < edges.Count) 
            {
                Path symmetricEdge = edges[symmetricIndex];

                Vector2 transformedPos;
                switch (symmetry)
                {
                    case Symmetry.GlideReflection: transformedPos = GlideReflectPointOnSymEdge(edge, symmetricEdge, pointA);
                        break;
                    default: transformedPos = TranslatePointOnSymEdge(edge, symmetricEdge, pointA);
                        break;
                }

                pointB = symmetricEdge.TryAddPoint(transformedPos, smooth) as PathPointSelectable;
            }
        }
        if (pointB == null) return null;

        // Create composite selectable
        TessPointSelectable composite = new TessPointSelectable(pointA, pointB, symmetry);

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
    /// Translates the point into symmetric edge. Returns the position of pointA when translated onto symEdge
    /// Assumes pointA is a point of edge and edge and symEdge are paralel
    /// </summary>
    private Vector2 TranslatePointOnSymEdge(Path edge, Path symEdge, PathPointSelectable pointA)
    {
        Vector2 midPnt = (edge.End + edge.Start)/2;
        Vector2 midPntSym = (symEdge.End + symEdge.Start) / 2;
        return pointA.Position + (midPntSym - midPnt);
    }

    /// <summary>
    /// Glide-reflects the point into symmetric edge. Returns the position of pointA when glide-reflected onto symEdge
    /// Assumes pointA is a point of edge and edge and symEdge are paralel
    /// </summary>
    private Vector2 GlideReflectPointOnSymEdge(Path edge, Path symEdge, PathPointSelectable pointA)
    {
        Vector2 midPnt = (edge.End + edge.Start) / 2;
        Vector2 midPntSym = (symEdge.End + symEdge.Start) / 2;
        Vector2 midLinePoint = (midPnt + midPntSym)/2;

        Vector2 translatedPnt = pointA.Position + (midPntSym - midPnt);

        Vector2 axisDir = (edge.End + edge.Start).normalized;
        Vector2 relative = translatedPnt - midLinePoint;
        Vector2 projection = Vector2.Dot(relative, axisDir) * axisDir;
        Vector2 reflected = 2 * projection - relative;

        return reflected + midLinePoint;
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
}
