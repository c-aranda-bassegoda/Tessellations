using UnityEngine;

public class Lattice : MonoBehaviour
{
    public TessellationPolygon tile;

    //void Start()
    //{
    //    tile = GetComponent<TessellationPolygon>();
    //}

    public void Tessellate()
    {
        SelectionManager.Instance.Deselect();

        for (int i = 0; i < tile.edges.Count; i++)
        {
            // Copy the polygon game obj
            GameObject newTile = Instantiate(tile.gameObject, tile.transform.parent);

            // get the symmetry of the ith edge
            TessPointSelectable.Symmetry symmetry = tile.GetSymmetryForEdge(i);

            // get the tile's edge and its symmetric edge
            Path edge = tile.edges[i];
            int symIndex = tile.GetSymmetricEdgeIndex(i);

            if (symIndex < 0 || symIndex >= tile.edges.Count)
                return;

            var symEdge = tile.edges[symIndex];

            // translate the new tile to the correct position
            Translate(newTile, edge, symEdge);

            switch (symmetry)
            {
                default:
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
    }
}
