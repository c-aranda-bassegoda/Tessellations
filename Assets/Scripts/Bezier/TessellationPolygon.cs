using System.Collections.Generic;
using UnityEngine;

public class TessellationPolygon : PiecewisePolygon
{
    [SerializeField] private bool enableSymmetry = true;

    // Map each edge index to its symmetric edge index
    // e.g., edges[0] is symmetric to edges[3], etc.
    [SerializeField] private List<int> symmetricEdgeMap = new List<int>();

    // Center point for mirroring (can also be an axis line)
    [SerializeField] private Vector2 symmetryCenter = Vector2.zero;

    /// <summary>
    /// Adds a node, and if symmetry is enabled, automatically adds mirrored node
    /// </summary>
    internal override PathPointSelectable TryAddPoint(Vector2 pointerWorldPos, bool smooth)
    {
        Debug.Log("Adding point");
        // Add the original node
        PathPointSelectable node = null;
        Path edge = null;
        foreach (var e in edges)
        {
            node = e.TryAddPoint(pointerWorldPos, smooth);
            if (node != null)
            {
                edge = e;
                break;
            }
        }

        if (enableSymmetry && node != null)
        {
            int symmetricIndex = GetSymmetricEdgeIndex(edges.IndexOf(edge));
            Debug.Log("Sym idx: " +  symmetricIndex);
            if (symmetricIndex >= 0 && symmetricIndex < edges.Count)
            {
                Path symmetricEdge = edges[symmetricIndex];

                // Mirror the position across the symmetry center
                Vector2 mirroredPos = MirrorAcrossCenter(node.Position);

                // Add mirrored node to symmetric edge
                var symPnt = symmetricEdge.TryAddPoint(mirroredPos, smooth);
                Debug.Log(symPnt);
            }
                
        }

        return node;
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
    /// Mirrors a point across the symmetry center
    /// </summary>
    private Vector2 MirrorAcrossCenter(Vector2 point)
    {
        Vector2 offset = point - symmetryCenter;
        return symmetryCenter - offset;
    }

    /// <summary>
    /// Optionally, automatically generate symmetric map if polygon is regular
    /// </summary>
    public void AutoGenerateSymmetryMap()
    {
        symmetricEdgeMap.Clear();
        int n = edges.Count;
        for (int i = 0; i < n; i++)
        {
            symmetricEdgeMap.Add((n - i) % n); // Simple rotational symmetry
        }
    }
}
