using System.Collections.Generic;
using UnityEngine;

public class Path : MonoBehaviour
{
    private List<NodeSelectable> points;
    public int resolutionPerSegment = 20;
    private LineRenderer line;
    public GameObject nodePrefab;

    void Awake()
    {
        points = new List<NodeSelectable>();

        line = GetComponent<LineRenderer>();
        if (line == null)
            Debug.LogError("No LineRenderer on Path");
    }

    public void Initialize(Vector2 start, Vector2 end)
    {
        points.Clear();

        NodeSelectable a = Instantiate(nodePrefab, start, Quaternion.identity)
                        .GetComponent<NodeSelectable>();

        NodeSelectable b = Instantiate(nodePrefab, end, Quaternion.identity)
                            .GetComponent<NodeSelectable>();

        points.Add(a);
        points.Add(b);
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
            Vector3 a = points[i].GetPosition();
            Vector3 b = points[i + 1].GetPosition();

            for (int j = 0; j <= resolutionPerSegment; j++)
            {
                float t = j / (float)resolutionPerSegment;
                line.SetPosition(index++, Vector3.Lerp(a, b, t)); // can deal with cubic curves later
            }
        }
    }


    public NodeSelectable TryAddPoint(Vector3 position)
    {
        if (points.Count < 2) return null;

        Vector3 clickPos = position;

        Vector3 closestPoint;
        int segmentIndex = FindClosestSegment(clickPos, out closestPoint);

        float clickThreshold = 0.2f; // tweak for your scale

        if (segmentIndex != -1 &&
            Vector3.Distance(closestPoint, clickPos) < clickThreshold)
        {
            NodeSelectable node = Instantiate(nodePrefab, closestPoint, Quaternion.identity)
                        .GetComponent<NodeSelectable>();
            points.Insert(segmentIndex + 1, node);
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
            Vector3 a = points[i].GetPosition();
            Vector3 b = points[i + 1].GetPosition();

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
        return points;
    } 

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        foreach (var p in points)
            Gizmos.DrawSphere(p.GetPosition(), 0.05f);
    }

}
