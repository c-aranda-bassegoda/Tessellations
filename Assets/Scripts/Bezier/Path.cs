using System;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.PlayerSettings;

public class Path : MonoBehaviour
{
    private List<PathPointSelectable> points;
    public int resolutionPerSegment = 20;
    private LineRenderer line;
    public GameObject nodePrefab;
    public float clickThreshold = 0.4f;
    public Vector2 Start {  get; private set; }
    public Vector2 End { get; private set; }

    void Awake()
    {
        points = new List<PathPointSelectable>();

        line = GetComponent<LineRenderer>();
        if (line == null)
            Debug.LogError("No LineRenderer on Path");
    }

    public void Initialize(Vector2 start, Vector2 end)
    {
        this.Start = start;
        this.End = end;
        points.Clear();

        NodeSelectable a = Instantiate(nodePrefab, start, Quaternion.identity)
                        .GetComponent<NodeSelectable>();
        a.transform.SetParent(transform, true);
        PathPointSelectable p1 = new PathPointSelectable();
        p1.parentPath = this;
        p1.anchor = a;
        p1.handleInOffset = Vector3.zero;
        p1.handleOutOffset = Vector3.zero;
        p1.smooth = false;

        NodeSelectable b = Instantiate(nodePrefab, end, Quaternion.identity)
                            .GetComponent<NodeSelectable>();
        b.transform.SetParent(transform, true);
        PathPointSelectable p2 = new PathPointSelectable();
        p2.parentPath = this;
        p2.anchor = b;
        p2.handleInOffset = Vector3.zero;
        p2.handleOutOffset = Vector3.zero;
        p2.smooth = false;

        points.Add(p1);
        points.Add(p2);
    }

    void Update()
    {
        DrawPath();
    }

    public void DeletePoint(PathPointSelectable point)
    {
        if (points.Count <= 2)
            return;

        int index = points.IndexOf(point);
        if (index <= 0 || index >= points.Count - 1)
            return;

        points.RemoveAt(index);
    }


    public PathPointSelectable TryAddPoint(Vector3 position, bool smooth)
    {
        if (points.Count < 2) return null;

        Vector3 clickPos = position;

        Vector3 closestPoint;
        int segmentIndex = FindClosestSegment(clickPos, out closestPoint);

        if (segmentIndex != -1 && Vector3.Distance(closestPoint, clickPos) < clickThreshold)
        {
            NodeSelectable node = Instantiate(nodePrefab, closestPoint, Quaternion.identity)
                        .GetComponent<NodeSelectable>();
            PathPointSelectable p1 = new PathPointSelectable();
            p1.parentPath = this;
            p1.anchor = node;
            p1.smooth = smooth;
            if (smooth)
            {
                Vector3 a = points[segmentIndex].anchor.GetPosition();
                Vector3 b = points[segmentIndex + 1].anchor.GetPosition();
                Vector3 dir = (b - a).normalized;
                float handleLength = Vector3.Distance(a, b) * 0.25f;

                p1.handleInOffset = -dir * handleLength;
                p1.handleOutOffset = dir * handleLength;
            } else
            {

                p1.handleInOffset = Vector3.zero;
                p1.handleOutOffset = Vector3.zero;
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
            Vector3 b = points[i].HandleOutPos;
            Vector3 c = points[i + 1].HandleInPos;
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


    public bool HasEdge(Vector2 a, Vector2 b)
    {
        for(int i = 0; i < points.Count; i++)
        {
            var p1 = points[i];
            var p2 = points[(i + 1) % points.Count];
            if ((p1.Position == a && p2.Position == b) || (p1.Position == b && p2.Position == a))
                return true;
        }
        return false;
    }

    public PathPointSelectable GetPoint(int index)
    {
        return points[index];
    }

    public Vector2 GetCurveAtT(PathPointSelectable p0, PathPointSelectable p1, float t)
    {
        Vector2 a = p0.Position;
        Vector2 b = p0.HandleOutPos;
        Vector2 c = p1.HandleInPos;
        Vector2 d = p1.Position;

        return BezierCurve.CubicCurve(a, b, c, d, t);
    }

    public void InvertPath()
    {
        Vector2 temp = Start;
        Start = End;
        End = temp;
        List<PathPointSelectable> list = new List<PathPointSelectable>();
        for (int i = points.Count-1; i>=0; i--)
        {
            list.Add(points[i]);
        }
        points.Clear();

        foreach (var p in list)
        {
            points.Add(p);
        }
    }

    /*
     * -------------------------------------------------------------------------------------------
     * Rendering
     * -------------------------------------------------------------------------------------------
     */

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        foreach (var p in points)
            Gizmos.DrawSphere(p.anchor.GetPosition(), 0.05f);
    }
    void DrawPath()
    {
        if (line == null || points == null || points.Count < 2) return;

        int totalPoints = (points.Count - 1) * (resolutionPerSegment + 1);
        line.positionCount = totalPoints;

        int index = 0;

        for (int i = 0; i < points.Count - 1; i++)
        {
            PathPointSelectable p0 = points[i];
            PathPointSelectable p1 = points[i + 1];

            Vector3 a = p0.anchor.GetPosition();
            Vector3 d = p1.anchor.GetPosition();

            for (int j = 0; j <= resolutionPerSegment; j++)
            {
                float t = j / (float)resolutionPerSegment;

                Vector3 position;

                Vector3 b = p0.HandleOutPos;
                Vector3 c = p1.HandleInPos;

                position = BezierCurve.CubicCurve(a, b, c, d, t);
                line.SetPosition(index++, position);
            }
        }
    }


    /// <summary>
    /// Returns true if there is a node close to postion
    /// </summary>
    internal bool isNodeAt(Vector2 position)
    {
        foreach (var p in points)
        {
            if (Vector2.Distance(p.Position, position) < clickThreshold)
                return true;
        }
        return false;
    }
}
