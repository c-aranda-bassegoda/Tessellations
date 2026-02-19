using System;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;

public class FreehandDrawingSystem : ILineDrawer
{
    private GameObject currentLine;
    private LineRenderer currentLineRenderer;
    private List<Vector3> points;
    private GameObject linePrefab;
    private float minDistance;

    public FreehandDrawingSystem(GameObject prefab, float minDistance = 0.05f)
    {
        linePrefab = prefab;
        this.minDistance = minDistance;
    }

    // Return the GameObject so the manager can register it
    public GameObject StartDrawing(Vector3 startPos)
    {
        currentLine = GameObject.Instantiate(linePrefab, Vector3.zero, Quaternion.identity);
        currentLineRenderer = currentLine.GetComponent<LineRenderer>();
        points = new List<Vector3>();
        AddPoint(startPos);
        return currentLine;
    }

    public void UpdateDrawing(Vector3 currentPos)
    {
        AddPoint(currentPos);
    }

    public void EndDrawing(Vector3 endPos)
    {
        AddPoint(endPos);
        currentLine = null;
        currentLineRenderer = null;
        points = null;
    }

    private void AddPoint(Vector3 pos)
    {
        if (currentLineRenderer == null) return;

        if (points.Count == 0 || Vector3.Distance(points[points.Count - 1], pos) >= minDistance)
        {
            points.Add(pos);
            currentLineRenderer.positionCount = points.Count;
            currentLineRenderer.SetPosition(points.Count - 1, pos);
        }
    }

    public void DeleteDrawing()
    {
        if (currentLine != null)
            GameObject.Destroy(currentLine);

        currentLine = null;
        currentLineRenderer = null;
        points = null;
    }

}
