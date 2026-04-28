using System.Collections.Generic;
using UnityEngine;

public class LatticeDraw : MonoBehaviour
{
    public TilePolygon tile;

    public List<GameObject> gameObjects = new List<GameObject>();

    public void Tessellate()
    {
        SelectionManager.Instance.Deselect();

        for (int i = 0; i < tile.Edges.Count; i++)
        {
            // Copy the polygon game obj
            GameObject newTile = Instantiate(tile.gameObject, tile.transform.parent);
            gameObjects.Add(newTile);

            // get the symmetry of the ith edge
            TessPointSelectable.Symmetry symmetry = tile.GetSymmetryForEdge(i);

            // get the tile's edge and its symmetric edge
            Edge edge = tile.Edges[i];
            int symIndex = tile.GetSymmetricEdgeIndex(i);

            if (symIndex < 0 || symIndex >= tile.Edges.Count)
                return;

            var symEdge = tile.Edges[symIndex];

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

    private void Translate(GameObject newTile, Edge edge, Edge symEdge)
    {

        Vector2 midA = (edge.A.Position + edge.B.Position) / 2f;
        Vector2 midB = (symEdge.A.Position + symEdge.B.Position) / 2f;

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

    public void Rotate(GameObject newTile, Edge edge, Edge symEdge)
    {

        // TODO: if symEdge = edge, then we rotate 180 degrees around the midpoint of the edge.
        LineRenderer[] lineRenderers = newTile.GetComponentsInChildren<LineRenderer>();

        if (edge == symEdge)
        {
            Vector2 mid = (edge.A.Position + edge.B.Position) / 2f;

            // Rotate line renderers
            foreach (var lr in lineRenderers)
            {
                Vector3[] positions = new Vector3[lr.positionCount];
                lr.GetPositions(positions);
                for (int i = 0; i < positions.Length; i++)
                {
                    // Rotate 180 degrees around midpoint
                    Vector2 local = (Vector2)positions[i] - mid;
                    Vector2 rotatedLocal = -local;
                    Vector2 rotatedPos = rotatedLocal + mid;
                    positions[i] = rotatedPos;
                }
                lr.SetPositions(positions);
            }
            return;
        }

        TilePolygon tilePolygon = newTile.GetComponent<TilePolygon>();
        tilePolygon.GetSharedVertex(edge, symEdge, out Vector2 pivot);
        Matrix2x2 matrix2X2 = tilePolygon.GetRotationMatrix(edge, symEdge);

        // Rotate line renderers
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

    public void GlideReflect(GameObject newTile, Edge edge, Edge symEdge)
    {
        TilePolygon tilePolygon = newTile.GetComponent<TilePolygon>();
        var (axisPoint, axisDir) = tilePolygon.GetMidpointReflectionAxis(edge);
         
        Vector2 midA = (edge.A.Position + edge.B.Position) / 2f;
        Vector2 midB = (symEdge.A.Position + symEdge.B.Position) / 2f;
        // Base translation
        Vector2 translation = midB - midA;

        tilePolygon.GetSharedVertex(edge, symEdge, out Vector2 pivot);
        Matrix2x2 matrix2X2 = tilePolygon.GetRotationMatrix(edge, symEdge);

        // Reflect line renderers
        LineRenderer[] lineRenderers = newTile.GetComponentsInChildren<LineRenderer>();
        foreach (var lr in lineRenderers)
        {
            Vector3[] positions = new Vector3[lr.positionCount];
            lr.GetPositions(positions);
            for (int i = 0; i < positions.Length; i++)
            {
                Vector2 reflectedPos = SymmetryUtils.ReflectAcrossAxis(positions[i], axisPoint, axisDir);

                if (TilePolygon.AreParallel(edge, symEdge))
                {
                    positions[i] = (reflectedPos + translation);
                }
                else
                {
                    // Translate point into edge local space
                    Vector2 local = reflectedPos - pivot;

                    Vector2 rotatedLocal = matrix2X2.Multiply(local);

                    // Translate back to symEdge
                    Vector2 rotatedPos = rotatedLocal + pivot;

                    positions[i] = rotatedPos;
                }
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
