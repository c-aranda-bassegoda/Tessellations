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
        Path symmetricEdge = null;

        if (enableSymmetry)
        {
            int edgeIndex = edges.IndexOf(edge);
            symmetry = symmetries[edgeIndex];
            int symmetricIndex = GetSymmetricEdgeIndex(edgeIndex);
            if (symmetricIndex >= 0 && symmetricIndex < edges.Count) 
            {
                symmetricEdge = edges[symmetricIndex];

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
        if (symmetricEdge == null) return null;

        // Create composite selectable
        TessPointSelectable composite;
        switch (symmetry)
        {
            case Symmetry.GlideReflection:
                var axis = GetReflectionAxis(edge, symmetricEdge);
                composite = new TessPointSelectable(pointA, pointB, symmetry, axis.Item2, axis.Item1);
                break;
            default:
                composite = new TessPointSelectable(pointA, pointB, symmetry);
                break;
        }

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

        Vector2 translatedPnt = TranslatePointOnSymEdge(edge, symEdge, pointA);

        (Vector2, Vector2) reflectionAxis = GetReflectionAxis(edge, symEdge);
        Vector2 midLinePoint = reflectionAxis.Item1;
        Vector2 axisDir = reflectionAxis.Item2;

        return SymmetryUtils.ReflectAcrossAxis(translatedPnt, midLinePoint, axisDir);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="edge"></param>
    /// <param name="symEdge"></param>
    /// <returns>(axisPoint, axisDir)</returns>
    private (Vector2, Vector2) GetReflectionAxis(Path edge, Path symEdge)
    {
        Vector2 midPnt = (edge.End + edge.Start) / 2;
        Vector2 midPntSym = (symEdge.End + symEdge.Start) / 2;
        Vector2 axisPoint = (midPnt + midPntSym) / 2;

        Vector2 axisDir = (edge.End + edge.Start).normalized;
        return (axisPoint, axisDir);
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
