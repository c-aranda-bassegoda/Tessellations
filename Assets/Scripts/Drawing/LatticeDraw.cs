using UnityEngine;

public class LatticeDraw : MonoBehaviour
{
    public TilePolygon tile;

    public void Tessellate()
    {
        SelectionManager.Instance.Deselect();

        for (int i = 0; i < tile.BasePolygon.Edges.Count; i++)
        {
            // Copy the polygon game obj
            GameObject newTile = Instantiate(tile.gameObject, tile.transform.parent);

            // get the symmetry of the ith edge
            TessPointSelectable.Symmetry symmetry = tile.GetSymmetryForEdge(i);

            // get the tile's edge and its symmetric edge
            Edge edge = tile.BasePolygon.Edges[i];
            int symIndex = tile.GetSymmetricEdgeIndex(i);

            if (symIndex < 0 || symIndex >= tile.Edges.Count)
                return;

            var symEdge = tile.Edges[symIndex];

            // translate the new tile to the correct position
            Translate(newTile, edge, symEdge);

            switch (symmetry)
            {
                default:
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
    }
}