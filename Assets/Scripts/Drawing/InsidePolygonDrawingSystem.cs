using System.Collections.Generic;
using UnityEngine;

public class InsidePolygonDrawingSystem :  ILineDrawer
{
    private FreehandDrawingSystem baseDrawer;
    private ConvexPolygon baseShape;

    private bool isDrawingValid = true;
    private Vector3 lastPoint;

    public InsidePolygonDrawingSystem(FreehandDrawingSystem drawer, ConvexPolygon shape)
    {
        baseDrawer = drawer;
        baseShape = shape;
    }

    public GameObject StartDrawing(Vector3 startPos)
    {
        if (!baseShape.ContainsPoint(startPos))
            return null;

        isDrawingValid = true;
        return baseDrawer.StartDrawing(startPos);
    }

    public void UpdateDrawing(Vector3 currentPos)
    {
        if (!isDrawingValid)
            return;

        if (!baseShape.ContainsPoint(currentPos))
        {
            isDrawingValid = false;
            return;
        }

        lastPoint = currentPos;
        baseDrawer.UpdateDrawing(currentPos);
    }

    public void EndDrawing(Vector3 endPos)
    {
        if (!isDrawingValid)
        {
            endPos = lastPoint;
        }

        baseDrawer.EndDrawing(endPos);
    }

    public void DeleteDrawing()
    {
        baseDrawer.DeleteDrawing();
        isDrawingValid = false;
    }
}

