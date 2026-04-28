using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using static UnityEditor.PlayerSettings;

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
        TessellationPolygon tessellationPolygon = newTile.GetComponent<TessellationPolygon>();

        newTile.transform.position = tessellationPolygon.TranslatePointOnSymEdge(edge, symEdge, newTile.transform.position);

        // Move line renderers because LineRenderer positions are in world space 
        LineRenderer[] lineRenderers = newTile.GetComponentsInChildren<LineRenderer>();
        foreach (var lr in lineRenderers)
        {
            Vector3[] positions = new Vector3[lr.positionCount];
            lr.GetPositions(positions);
            for (int i = 0; i < positions.Length; i++)
            {
                positions[i] = tessellationPolygon.TranslatePointOnSymEdge(edge, symEdge, positions[i]);
            }
            lr.SetPositions(positions);
        }

    }

    public void Rotate(GameObject newTile, Path edge, Path symEdge)
    {
        TessellationPolygon tessellationPolygon = newTile.GetComponent<TessellationPolygon>();

        newTile.transform.position = tessellationPolygon.RotatePointOnSymEdge(edge, symEdge, newTile.transform.position);

        // Rotate line renderers
        LineRenderer[] lineRenderers = newTile.GetComponentsInChildren<LineRenderer>();
        foreach (var lr in lineRenderers)
        {
            Vector3[] positions = new Vector3[lr.positionCount];
            lr.GetPositions(positions);
            for (int i = 0; i < positions.Length; i++)
            {
                positions[i] = tessellationPolygon.RotatePointOnSymEdge(edge, symEdge, positions[i]);
            }
            lr.SetPositions(positions);
        }
    }

    public void GlideReflect(GameObject newTile, Path edge, Path symEdge)
    {
        TessellationPolygon tessellationPolygon = newTile.GetComponent<TessellationPolygon>();

        newTile.transform.position = tessellationPolygon.GlideReflectPointOnSymEdge(edge, symEdge, newTile.transform.position);

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
