using System.Collections.Generic;
using UnityEngine;

public class Path : MonoBehaviour
{
    private List<Vector2> points;
    public int resolutionPerSegment = 20;
    private LineRenderer line;

    void Awake()
    {
        points = new List<Vector2>();

        line = GetComponent<LineRenderer>();
        if (line == null)
            Debug.LogError("No LineRenderer on Path");
    }

    public void Initialize(Vector2 start, Vector2 end)
    {
        points.Clear();
        points.Add(start);
        points.Add(end);
    }

    void Update()
    {
        DrawPath();

        if (Input.GetMouseButtonDown(0))
        {
            TryAddPoint();
        }
    }

    void DrawPath()
    {
        if (line == null || points == null || points.Count < 2) return;

        int totalPoints = (points.Count - 1) * (resolutionPerSegment + 1);
        line.positionCount = totalPoints;

        int index = 0;

        for (int i = 0; i < points.Count - 1; i++)
        {
            Vector3 a = points[i];
            Vector3 b = points[i + 1];

            for (int j = 0; j <= resolutionPerSegment; j++)
            {
                float t = j / (float)resolutionPerSegment;
                line.SetPosition(index++, Vector3.Lerp(a, b, t)); // can deal with cubic curves later
            }
        }
    }


    void TryAddPoint()
    {
        if (points.Count < 2) return;

        Vector3 clickPos = InputManager.Instance.PointerWorldPos;

        Vector3 closestPoint;
        int segmentIndex = FindClosestSegment(clickPos, out closestPoint);

        float clickThreshold = 0.2f; // tweak for your scale

        if (segmentIndex != -1 &&
            Vector3.Distance(closestPoint, clickPos) < clickThreshold)
        {
            points.Insert(segmentIndex + 1, closestPoint);

        }
    }


    int FindClosestSegment(Vector3 clickPos, out Vector3 closestPoint)
    {
        float minDistance = float.MaxValue;
        int closestSegment = -1;
        closestPoint = Vector3.zero;

        for (int i = 0; i < points.Count - 1; i++)
        {
            Vector3 a = points[i];
            Vector3 b = points[i + 1];

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

    public List<Vector2> GetNodes()
    {
        return points;
    } 

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        foreach (var p in points)
            Gizmos.DrawSphere(p, 0.05f);
    }

}
