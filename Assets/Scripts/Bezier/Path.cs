using System.Collections.Generic;
using UnityEngine;

public class PathPoint
{
    public NodeSelectable anchor;
    public NodeSelectable handleIn;
    public NodeSelectable handleOut;
    public bool smooth;
}

public class Path : MonoBehaviour
{
    private List<PathPoint> points;
    public int resolutionPerSegment = 20;
    private LineRenderer line;
    public GameObject nodePrefab;

    void Awake()
    {
        points = new List<PathPoint>();

        line = GetComponent<LineRenderer>();
        if (line == null)
            Debug.LogError("No LineRenderer on Path");
    }

    public void Initialize(Vector2 start, Vector2 end)
    {
        points.Clear();

        NodeSelectable a = Instantiate(nodePrefab, start, Quaternion.identity)
                        .GetComponent<NodeSelectable>();
        PathPoint p1 = new PathPoint();
        p1.anchor = a;
        p1.smooth = false;

        NodeSelectable b = Instantiate(nodePrefab, end, Quaternion.identity)
                            .GetComponent<NodeSelectable>();
        PathPoint p2 = new PathPoint();
        p2.anchor = b;
        p2.smooth = false;

        points.Add(p1);
        points.Add(p2);
    }

    void Update()
    {
        DrawPath();
    }

    void DrawPath()
    {
        if (line == null || points == null || points.Count < 2) return;

        int totalPoints = (points.Count - 1) * (resolutionPerSegment + 1);
        line.positionCount = totalPoints;

        int index = 0;

        for (int i = 0; i < points.Count - 1; i++)
        {
            Vector3 a = points[i].anchor.GetPosition();
            Vector3 b = points[i + 1].anchor.GetPosition();

            for (int j = 0; j <= resolutionPerSegment; j++)
            {
                float t = j / (float)resolutionPerSegment;
                line.SetPosition(index++, Vector3.Lerp(a, b, t)); // can deal with cubic curves later
            }
        }
    }


    public NodeSelectable TryAddPoint(Vector3 position, bool smooth)
    {
        if (points.Count < 2) return null;

        Vector3 clickPos = position;

        Vector3 closestPoint;
        int segmentIndex = FindClosestSegment(clickPos, out closestPoint);

        float clickThreshold = 0.2f; // tweak for scale

        if (segmentIndex != -1 && Vector3.Distance(closestPoint, clickPos) < clickThreshold)
        {
            NodeSelectable node = Instantiate(nodePrefab, closestPoint, Quaternion.identity)
                        .GetComponent<NodeSelectable>();
            PathPoint p1 = new PathPoint();
            p1.anchor = node;
            p1.smooth = false;

            points.Insert(segmentIndex + 1, p1);
            return node;
        }
        return null;
    }


    int FindClosestSegment(Vector3 clickPos, out Vector3 closestPoint)
    {
        float minDistance = float.MaxValue;
        int closestSegment = -1;
        closestPoint = Vector3.zero;

        for (int i = 0; i < points.Count - 1; i++)
        {
            Vector3 a = points[i].anchor.GetPosition();
            Vector3 b = points[i + 1].anchor.GetPosition();

            Vector3 projected = ClosestPointOnLineSegment(a, b, clickPos);
            float dist = Vector3.Distance(projected, clickPos);

            if (dist < minDistance)
            {
                minDistance = dist;
                closestSegment = i;
                closestPoint = projected;
            }
        }

        return closestSegment;
    }

    // Projection function
    Vector3 ClosestPointOnLineSegment(Vector3 a, Vector3 b, Vector3 p)
    {
        Vector3 ab = b - a;
        float t = Vector3.Dot(p - a, ab) / ab.sqrMagnitude;
        t = Mathf.Clamp01(t);
        return a + ab * t;
    }

    public List<NodeSelectable> GetNodes()
    {
        List<NodeSelectable> anchors = new List<NodeSelectable> ();
        foreach (var p  in points)
        {
            anchors.Add(p.anchor);
        }
        return anchors;
    } 

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        foreach (var p in points)
            Gizmos.DrawSphere(p.anchor.GetPosition(), 0.05f);
    }

}
