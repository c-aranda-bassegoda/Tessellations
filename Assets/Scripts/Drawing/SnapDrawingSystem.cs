using System;
using UnityEngine;

public class SnapDrawingSystem : ILineDrawer
{
    private FreehandDrawingSystem baseDrawer;
    private DerivedPolygon baseShape;
    private Vertex startVertex;
    private GameObject currentLine;
    private float snapDistance;

    public SnapDrawingSystem(FreehandDrawingSystem drawer, DerivedPolygon shape, float snapDistance = 0.2f)
    {
        baseDrawer = drawer;
        baseShape = shape;
        this.snapDistance = snapDistance;
    }

    public GameObject StartDrawing(Vector3 startPos)
    {
        startVertex = FindClosestVertex(startPos);
        if (startVertex == null) return null;

        currentLine = baseDrawer.StartDrawing(startVertex.Position);
        EdgeSelectable selectable = currentLine.GetComponent<EdgeSelectable>();
        if (selectable != null)
        {
            selectable.Polygon = baseShape;
        }
        return currentLine;
    }

    public void UpdateDrawing(Vector3 currentPos)
    {
        if (startVertex == null) return;
        baseDrawer.UpdateDrawing(currentPos);
    }

    public bool EndDrawing(Vector3 endPos)
    {
        Vertex endVertex = FindClosestVertex(endPos);

        bool valid =
            startVertex != null &&
            endVertex != null &&
            baseShape.HasEdge(endVertex, startVertex);

        if (valid)
        {
            baseDrawer.EndDrawing(endVertex.Position);
        }
        else
        {
            DeleteDrawing(); // destroy the line
        }

        startVertex = null;
        currentLine = null;

        return valid;
    }

    private Vertex FindClosestVertex(Vector3 pos)
    {
        Vertex closest = null;
        float minDist = snapDistance;
        foreach (Vertex v in baseShape.Vertices)
        {
            float dist = Vector3.Distance(pos, v.Position);
            if (dist < minDist)
            {
                closest = v;
                minDist = dist;
            }
        }
        return closest;
    }

    public void DeleteDrawing()
    {
        baseDrawer.DeleteDrawing();
    }
}
