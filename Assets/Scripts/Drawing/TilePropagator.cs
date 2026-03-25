using System.Collections.Generic;
using TMPro;
using UnityEngine;

public static class TilePropagator
{
    public static void Propagate(TilePolygon tile)
    {
        if (tile == null || tile.edgeMappings.Count == 0)
        {
            Debug.LogWarning("Tile or edge mappings not set.");
            return;
        }

        Camera cam = Camera.main;
        if (cam == null)
        {
            Debug.LogError("No main camera found.");
            return;
        }

        // Camera bounds
        Vector2 bottomLeft = cam.ViewportToWorldPoint(new Vector3(0, 0, 0));
        Vector2 topRight = cam.ViewportToWorldPoint(new Vector3(1, 1, 0));

        List<TilePolygon> spawnedTiles = new List<TilePolygon>();

        foreach (var mapping in tile.edgeMappings)
        {
            // Instantiate a new tile
            TilePolygon newTile = tile.DeepCopy(tile.transform);

            // Compute the transformation
            Matrix4x4 transformMatrix = mapping.Transform;

            Edge targetEdge = tile.Edges[mapping.TargetEdgeIndex];
            Edge sourceEdge = tile.Edges[mapping.SourceEdgeIndex];
            Vector2 targetEdgeMidpoint = targetEdge.MidPoint.Position;
            // Apply translation + rotation from matrix to the new tile
            Vector2 midOriginal = sourceEdge.MidPoint.Position;
            Vector2 midNew = transformMatrix.MultiplyPoint3x4(midOriginal);
            Vector2 delta = midOriginal - midNew;
            newTile.OnTranslate(newTile.GetCenter() + delta);

            //// Only keep tiles in camera bounds
            //Vector3 tilePos = newTile.transform.position;
            //if (tilePos.x < bottomLeft.x || tilePos.x > topRight.x || tilePos.y < bottomLeft.y || tilePos.y > topRight.y)
            //{
            //    Object.Destroy(newTile.gameObject);
            //}
            //else
            //{
                spawnedTiles.Add(newTile);
            //}
        }

        Debug.Log($"Tile propagation completed. Spawned {spawnedTiles.Count} new tiles.");
    }
}