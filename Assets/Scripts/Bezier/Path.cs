using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.PlayerSettings;

public class PathPoint
{
    public NodeSelectable anchor;
    public NodeSelectable handleIn;
    public NodeSelectable handleOut;
    public bool smooth;

    public void MoveAnchor(Vector3 newPosition)
    {
        Vector3 delta = newPosition - (Vector3)anchor.GetPosition();

        anchor.Move(newPosition);

        if (handleIn != null)
            handleIn.Move((Vector3)handleIn.GetPosition() + delta);

        if (handleOut != null)
            handleOut.Move((Vector3)handleOut.GetPosition() + delta);
    }



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
        p1.handleIn = a;
        p1.handleOut = a;
        p1.smooth = false;

        NodeSelectable b = Instantiate(nodePrefab, end, Quaternion.identity)
                            .GetComponent<NodeSelectable>();
        PathPoint p2 = new PathPoint();
        p2.anchor = b;
        p2.handleIn = b;
        p2.handleOut = b;
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
            PathPoint p0 = points[i];
            PathPoint p1 = points[i + 1];

            Vector3 a = p0.anchor.GetPosition();
            Vector3 d = p1.anchor.GetPosition();

            for (int j = 0; j <= resolutionPerSegment; j++)
            {
                float t = j / (float)resolutionPerSegment;

                Vector3 position;

                Vector3 b = p0.handleOut.GetPosition();
                Vector3 c = p1.handleIn.GetPosition();

                position = BezierCurve.CubicCurve(a, b, c, d, t);
                line.SetPosition(index++, position);
            }
        }
    }


    public PathPoint TryAddPoint(Vector3 position, bool smooth)
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
            p1.smooth = smooth;
            if (smooth)
            {
                Vector3 a = points[segmentIndex].anchor.GetPosition();
                Vector3 b = points[segmentIndex + 1].anchor.GetPosition();
                Vector3 dir = (b - a).normalized;
                float handleLength = Vector3.Distance(a, b) * 0.25f;

                Vector3 handleInPos = closestPoint - dir * handleLength;
                Vector3 handleOutPos = closestPoint + dir * handleLength;

                p1.handleIn = Instantiate(nodePrefab, handleInPos, Quaternion.identity).GetComponent<NodeSelectable>();
                p1.handleOut = Instantiate(nodePrefab, handleOutPos, Quaternion.identity).GetComponent<NodeSelectable>();
            } else
            {

                p1.handleIn = Instantiate(nodePrefab, closestPoint, Quaternion.identity)
                .GetComponent<NodeSelectable>();

                p1.handleOut = Instantiate(nodePrefab, closestPoint, Quaternion.identity)
                                .GetComponent<NodeSelectable>();
            }

            points.Insert(segmentIndex + 1, p1);
            return p1;
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
            Vector3 b = points[i].handleOut.GetPosition();
            Vector3 c = points[i + 1].handleIn.GetPosition();
            Vector3 d = points[i + 1].anchor.GetPosition();

            Vector3 projected = BezierCurve.GetClosestPointOnCubic(a, b, c, d, clickPos, resolutionPerSegment);
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
