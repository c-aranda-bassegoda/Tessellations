using System;
using System.Collections.Generic;
using System.Drawing;
using NUnit.Framework.Internal;
using Unity.VisualScripting;
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
                    case Symmetry.GlideReflection: transformedPos = GlideReflectPointOnSymEdge(edge, symmetricEdge, pointA.Position);
                        Debug.Log("Glide Refl");
                        break;
                    case Symmetry.Rotation: transformedPos = RotatePointOnSymEdge(edge, symmetricEdge, pointA.Position);
                        Debug.Log("Rotation");
                        break;
                    default: transformedPos = TranslatePointOnSymEdge(edge, symmetricEdge, pointA.Position);
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
                var axis = GetMidpointReflectionAxis(edge);
                var mtx = GetRotationMatrix(edge, symmetricEdge);
                composite = new TessPointSelectable(pointA, pointB, symmetry, mtx, axis.Item2, axis.Item1);
                break;
            case Symmetry.Rotation:
                mtx = GetRotationMatrix(edge, symmetricEdge);
                composite = new TessPointSelectable(pointA, pointB, symmetry, mtx);
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
    public int GetSymmetricEdgeIndex(int edgeIndex)
    {
        if (symmetricEdgeMap != null && edgeIndex < symmetricEdgeMap.Count)
            return symmetricEdgeMap[edgeIndex];
        return -1;
    }

    /// <summary>
    /// Returns the symmetry type for the given edge index. Defaults to translation if not set.
    /// </summary>
    /// <param name="edgeIndex"></param>
    /// <returns></returns>
    public Symmetry GetSymmetryForEdge(int edgeIndex)
    {
        if (symmetries != null && edgeIndex < symmetries.Count)
            return symmetries[edgeIndex];
        return Symmetry.Translation; // default symmetry
    }

    /// <summary>
    /// Translates the point into symmetric edge. Returns the position of pointA when translated onto symEdge
    /// Assumes pointA is a point of edge and edge and symEdge are paralel
    /// </summary>
    private Vector2 TranslatePointOnSymEdge(Path edge, Path symEdge, Vector2 pointA)
    {
        Vector2 midPnt = (edge.End + edge.Start)/2;
        Vector2 midPntSym = (symEdge.End + symEdge.Start) / 2;
        return pointA + (midPntSym - midPnt);
    }

    /// <summary>
    /// Glide-reflects the point into symmetric edge. Returns the position of pointA when glide-reflected onto symEdge
    /// 
    /// </summary>
    private Vector2 GlideReflectPointOnSymEdge(Path edge, Path symEdge, Vector2 pointA)
    {
        (Vector2 axisPoint, Vector2 axisDir) = GetMidpointReflectionAxis(edge);
        Vector2 mirrored = SymmetryUtils.ReflectAcrossAxis(
            pointA,
            axisPoint,
            axisDir
        );
        if (AreParallel(edge, symEdge))
        { 
            return TranslatePointOnSymEdge(edge, symEdge, mirrored);
        }
        else
        {
            return RotatePointOnSymEdge(edge, symEdge, mirrored);
        }

    }

    /// <summary>
    /// Rotates the point into symmetric edge. Returns the position of pointA when glide-reflected onto symEdge
    /// </summary>
    public Vector2 RotatePointOnSymEdge(Path edge, Path symEdge, Vector2 pointA)
    {
        // Get shared pivot
        Vector2 pivot;
        bool success = GetSharedVertex(edge, symEdge, out pivot);
        Debug.Log("Pivot: " + pivot);

        Matrix2x2 rotMtx = GetRotationMatrix(edge, symEdge);

        // Translate point into edge local space
        Vector2 local = pointA - pivot;

        // Rotate
        Vector2 rotatedLocal = rotMtx.Multiply(local);

        // Translate back to symEdge
        Vector2 rotatedPoint = rotatedLocal + pivot;

        return rotatedPoint;
    }

    private bool GetSharedVertex(Path edge, Path symEdge, out Vector2 vertex)
    {
        if (edge.Start == symEdge.End || edge.Start == symEdge.Start) 
        { 
            vertex = edge.Start;
            return true;
        }

        if (edge.End == symEdge.Start || edge.End == symEdge.End)
        {
            vertex = edge.End;
            return true;
        }

        vertex = Vector2.zero;
        return false;
    }
    private Vector2 GetDirectionFromPivot(Path path, Vector2 pivot)
    {
        if (path.Start == pivot)
            return (path.End - path.Start).normalized;

        if (path.End == pivot)
            return (path.Start - path.End).normalized;

        throw new Exception("Pivot is not on path.");
    }

    private Vector2 FindIntersection(Path edge, Path symEdge)
    {
        Vector2 p = edge.Start;
        Vector2 r = edge.End - edge.Start;
        Vector2 q = symEdge.Start;
        Vector2 s = symEdge.End - symEdge.Start;

        float rxs = r.x * s.y - r.y * s.x; // 2D cross product
        Vector2 q_p = q - p;
        float qpxr = q_p.x * r.y - q_p.y * r.x;

        // Check if lines are parallel
        if (Mathf.Abs(rxs) < 1e-6f)
        {
            if (Mathf.Abs(qpxr) < 1e-6f)
            {
                if (edge.Start == symEdge.Start || edge.Start == symEdge.End)
                    return edge.Start;
                if (edge.End == symEdge.End || edge.End == symEdge.Start)
                    return edge.End;
                throw new System.Exception("Paths are colinear (overlapping)");
            }
            else
            {
                throw new System.Exception("Paths are parallel but do not intersect");
            }
        }

        float t = (q_p.x * s.y - q_p.y * s.x) / rxs;
        float u = qpxr / rxs;

        
        return p + t * r;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="edge"></param>
    /// <param name="symEdge"></param>
    /// <returns>(axisPoint, axisDir)</returns>
    private (Vector2, Vector2) GetReflectionAxis(Path edge, Path symEdge)
    {
        //if(!AreParallel(edge, symEdge))
        //{
        //    return GetAngleBisector(edge, symEdge);
        //}
        Vector2 midPnt = (edge.End + edge.Start) / 2;
        Vector2 midPntSym = (symEdge.End + symEdge.Start) / 2;
        Vector2 axisPoint = (midPnt + midPntSym) / 2;

        Vector2 axisDir = (edge.End + edge.Start).normalized;
        return (axisPoint, axisDir);
    }

    public (Vector2 pivot, Vector2 axisDir) GetMidpointReflectionAxis(Path path)
    {
        // Pivot = midpoint of endpoints
        Vector2 pivot = (path.Start + path.End) / 2f;

        // Reflection perpendicular to the path
        Vector2 axisDir = new Vector2(
            -(path.End - path.Start).y,
             (path.End - path.Start).x
        ).normalized;

        return (pivot, axisDir);
    }

    /// <summary>
    /// Reflection axis for non paralel lines
    /// </summary>
    /// <param name="edge"></param>
    /// <param name="symEdge"></param>
    /// <returns>(pivot, bisector)</returns>
    private (Vector2, Vector2) GetAngleBisector(Path edge, Path symEdge)
    {
        bool success = GetSharedVertex(edge, symEdge, out Vector2 pivot);

        Vector2 dirA = GetDirectionFromPivot(edge, pivot);
        Vector2 dirB = GetDirectionFromPivot(symEdge, pivot);

        Vector2 bisector = (dirA + dirB).normalized;

        return (pivot, bisector);
    }

    /// <summary>
    /// R = | cos(), -sin() |
    ///     | sin(),  cos() |
    /// </summary>
    /// <param name="edge"></param>
    /// <param name="symEdge"></param>
    public Matrix2x2 GetRotationMatrix(Path edge, Path symEdge)
    {
        if (!GetSharedVertex(edge, symEdge, out Vector2 pivot))
        {
            return Matrix2x2.Identity;
        }

        // Get directions from pivot
        Vector2 dirA = GetDirectionFromPivot(edge, pivot);
        Vector2 dirB = GetDirectionFromPivot(symEdge, pivot);

        float cos = Vector2.Dot(dirA, dirB);
        float sin = dirA.x * dirB.y - dirA.y * dirB.x; // 2D cross product 

        return new Matrix2x2(cos, sin);
    }

    private Vector2 MirrorPointAroundPathMidpoint(Path path, Vector2 point)
    {
        Vector2 midpoint = (path.Start + path.End) / 2;

        return 2 * midpoint - point;
    }

    /// <summary>
    /// Returns true if pathA and pathB are parallel
    /// </summary>
    public static bool AreParallel(Path pathA, Path pathB, float epsilon = 0.0001f)
    {
        Vector2 dirA = pathA.End - pathA.Start;
        Vector2 dirB = pathB.End - pathB.Start;

        float cross = dirA.x * dirB.y - dirA.y * dirB.x;

        return Mathf.Abs(cross) < epsilon;
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

public struct Matrix2x2
{
    public float m00, m01;
    public float m10, m11;

    public static Matrix2x2 Identity = new Matrix2x2
    {
        m00 = 1f,
        m01 = 0f,
        m10 = 0f,
        m11 = 1f
    };
    public Matrix2x2(bool identity)
    {
        m00 = 1f; m01 = 0f;
        m10 = 0f; m11 = 1f;
    }
    public Matrix2x2(float angleRad)
    {
        float cos = Mathf.Cos(angleRad);
        float sin = Mathf.Sin(angleRad);

        m00 = cos; m01 = -sin;
        m10 = sin; m11 = cos;
    }
    public Matrix2x2(float cos, float sin)
    {
        m00 = cos; m01 = -sin;
        m10 = sin; m11 = cos;
    }
    public Matrix2x2(Vector2 x, Vector2 y)
    {
        m00 = x.x; m01 = y.x;
        m10 = x.y; m11 = y.y;
    }

    public Vector2 Multiply(Vector2 v)
    {
        return new Vector2(
            m00 * v.x + m01 * v.y,
            m10 * v.x + m11 * v.y
        );
    }
}
