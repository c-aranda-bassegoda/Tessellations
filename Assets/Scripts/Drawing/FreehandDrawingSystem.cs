using System;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;

public class FreehandDrawingSystem : MonoBehaviour, ILineDrawer
{
    public bool OnDrawing = true;

    [Header("Line Settings")]
    [SerializeField] private GameObject linePrefab; // Prefab with LineRenderer
    [SerializeField] private float minDistance = 0.05f; // How close can two adjacent points in a line be

    private LineRenderer currentLine;
    private List<Vector3> points;

    // Creates line game object with given starting point
    public GameObject StartLine(Vector3 worldPos)
    {
        GameObject lineObj = GameObject.Instantiate(linePrefab, Vector3.zero, Quaternion.identity);
        currentLine = lineObj.GetComponent<LineRenderer>();
        points = new List<Vector3>();
        AddPoint(worldPos);
        return lineObj;
    }

    public void UpdateDrawing(Vector3 currentPos)
    {
        AddPoint(currentPos);
    }

    // Adds a point to the current line
    internal void AddPoint(Vector3 worldPos)
    {
        if (currentLine == null) return;

        if (points.Count == 0 || Vector3.Distance(points[points.Count - 1], worldPos) >= minDistance)
        {
            points.Add(worldPos);
            currentLine.positionCount = points.Count;
            currentLine.SetPosition(points.Count - 1, worldPos);
        }
    }

    public void EndLine()
    {
        currentLine = null;
        points = null;
    }
}
