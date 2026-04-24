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
                    break;
            }
            // translate the new tile to the correct position
            Translate(newTile, edge, symEdge);
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
        Matrix2x2 matrix2X2 = newTile.GetComponent<TessellationPolygon>().GetRotationMatrix(edge, symEdge);

        // Rotate line renderers
        LineRenderer[] lineRenderers = newTile.GetComponentsInChildren<LineRenderer>();
        foreach (var lr in lineRenderers)
        {
            Vector3[] positions = new Vector3[lr.positionCount];
            lr.GetPositions(positions);
            for (int i = 0; i < positions.Length; i++)
            {
                Vector2 rotatedPos = matrix2X2.Multiply((Vector2)positions[i]);
                positions[i] = rotatedPos;
            }
            lr.SetPositions(positions);
        }
    }

    public void GlideReflect(GameObject newTile, Path edge, Path symEdge)
    {
        var (axisPoint, axisDir) = tile.GetMidpointReflectionAxis(edge);

        Vector2 pos = newTile.transform.position;
        Vector2 reflected = SymmetryUtils.ReflectAcrossAxis(pos, axisPoint, axisDir);

        newTile.transform.position = reflected;

        // Reflect line renderers
        LineRenderer[] lineRenderers = newTile.GetComponentsInChildren<LineRenderer>();
        foreach (var lr in lineRenderers)
        {
            Vector3[] positions = new Vector3[lr.positionCount];
            lr.GetPositions(positions);
            for (int i = 0; i < positions.Length; i++)
            {
                Vector2 reflectedPos = SymmetryUtils.ReflectAcrossAxis(positions[i], axisPoint, axisDir);
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
