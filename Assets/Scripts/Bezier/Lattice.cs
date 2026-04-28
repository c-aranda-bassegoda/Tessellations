using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public class Lattice : MonoBehaviour
{
    public TessellationPolygon tile;

    public List<GameObject> gameObjects = new List<GameObject>();

    public void Tessellate()
    {
        SelectionManager.Instance.Deselect();

        for (int i = 0; i < tile.edges.Count; i++)
        {
            // Copy the polygon game obj
            GameObject newTile = Instantiate(tile.gameObject, tile.transform.parent);
            gameObjects.Add(newTile);
            // get rid of nodes 
            NodeSelectable[] nodes = newTile.GetComponentsInChildren<NodeSelectable>();
            foreach (var node in nodes)
            {
                Destroy(node.gameObject);
            }

            // get the symmetry of the ith edge
            TessPointSelectable.Symmetry symmetry = tile.GetSymmetryForEdge(i);

            // get the tile's edge and its symmetric edge
            Path edge = tile.edges[i];
            int symIndex = tile.GetSymmetricEdgeIndex(i);

            if (symIndex < 0 || symIndex >= tile.edges.Count)
                return;

            var symEdge = tile.edges[symIndex];


            switch (symmetry)
            {
                case TessPointSelectable.Symmetry.Rotation:
                    Rotate(newTile, edge, symEdge);
                    break;
                case TessPointSelectable.Symmetry.GlideReflection:
                    GlideReflect(newTile, edge, symEdge);
                    break;
                default:
                    // translate the new tile to the correct position
                    Translate(newTile, edge, symEdge);
                    break;
            }
        }
    }

    private void Translate(GameObject newTile, Path edge, Path symEdge)
    {

        Vector2 midA = (edge.Start + edge.End) / 2f;
        Vector2 midB = (symEdge.Start + symEdge.End) / 2f;

        // Base translation
        Vector2 translation = midB - midA;
        newTile.transform.position += (Vector3)translation;

        // Move line renderers because LineRenderer positions are in world space fml
        LineRenderer[] lineRenderers = newTile.GetComponentsInChildren<LineRenderer>();
        foreach (var lr in lineRenderers)
        {
            Vector3[] positions = new Vector3[lr.positionCount];
            lr.GetPositions(positions);
            for (int i = 0; i < positions.Length; i++)
            {
                positions[i] += (Vector3)translation;
            }
            lr.SetPositions(positions);
        }

    }

    public void Rotate(GameObject newTile, Path edge, Path symEdge)
    {
        TessellationPolygon tessellationPolygon = newTile.GetComponent<TessellationPolygon>();
        Vector2 pivot;
        bool success = tessellationPolygon.GetSharedVertex(edge, symEdge, out pivot);
        Matrix2x2 matrix2X2 = tessellationPolygon.GetRotationMatrix(edge, symEdge);

        // Rotate line renderers
        LineRenderer[] lineRenderers = newTile.GetComponentsInChildren<LineRenderer>();
        foreach (var lr in lineRenderers)
        {
            Vector3[] positions = new Vector3[lr.positionCount];
            lr.GetPositions(positions);
            for (int i = 0; i < positions.Length; i++)
            {

                // Translate point into edge local space
                Vector2 local = (Vector2)positions[i] - pivot;

                Vector2 rotatedLocal = matrix2X2.Multiply(local);

                // Translate back to symEdge
                Vector2 rotatedPos = rotatedLocal + pivot;

                positions[i] = rotatedPos;
            }
            lr.SetPositions(positions);
        }
    }

    public void GlideReflect(GameObject newTile, Path edge, Path symEdge)
    {
        TessellationPolygon tessellationPolygon = newTile.GetComponent<TessellationPolygon>();

        Vector2 pos = newTile.transform.position;
        Vector2 reflected = tessellationPolygon.GlideReflectPointOnSymEdge(edge, symEdge, pos);

        newTile.transform.position = reflected;

        // Reflect line renderers
        LineRenderer[] lineRenderers = newTile.GetComponentsInChildren<LineRenderer>();
        foreach (var lr in lineRenderers)
        {
            Vector3[] positions = new Vector3[lr.positionCount];
            lr.GetPositions(positions);
            for (int i = 0; i < positions.Length; i++)
            {
                Vector2 reflectedPos = tessellationPolygon.GlideReflectPointOnSymEdge(edge, symEdge, positions[i]);

                positions[i] = reflectedPos;
            }
            lr.SetPositions(positions);
        }
    }

    public void Reset()
    {
        foreach (var obj in gameObjects)
        {
            Destroy(obj);
        }
        gameObjects.Clear();
    }
}
