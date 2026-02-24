using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class PathView : MonoBehaviour
{
    public BezierPath PathData { get; private set; }
    public int Resolution { get; private set; } = 20;

    private LineRenderer lineRenderer;

    public void Initialize(BezierPath path, int resolution = 20)
    {
        PathData = path;
        Resolution = resolution;

        lineRenderer = GetComponent<LineRenderer>();
        if (lineRenderer == null)
            lineRenderer = gameObject.AddComponent<LineRenderer>();

        UpdateView();
    }

    public void UpdateView()
    {
        if (PathData == null || PathData.Points.Count < 2)
            return;

        // Count all points for the line renderer
        int totalSegments = (PathData.Points.Count - 1) * (Resolution + 1);
        lineRenderer.positionCount = totalSegments;

        int idx = 0;
        for (int i = 0; i < PathData.Points.Count - 1; i++)
        {
            for (int j = 0; j <= Resolution; j++)
            {
                float t = j / (float)Resolution;
                Vector2 pos = PathData.GetCurveAtT(i, t);
                lineRenderer.SetPosition(idx++, new Vector3(pos.x, pos.y, 0));
            }
        }
    }
}