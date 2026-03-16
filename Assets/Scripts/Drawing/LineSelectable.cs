using System.Collections.Generic;
using UnityEngine;

public class LineSelectable : MonoBehaviour, ISelectable, ITransformable
{

    [SerializeField] protected LineRenderer line;
    List<Vector3> points = new List<Vector3>();
    public Vector2 Center => (points[0] + points[^1]) / 2f;

    public float hitRadius = 0.1f;

    void Awake()
    {
        line = GetComponent<LineRenderer>();

        if (line == null)
            Debug.LogError("No line renderer");

        CachePoints();
    }

    public void CachePoints()
    {
        points.Clear();
        for (int i = 0; i < line.positionCount; i++)
            points.Add(line.GetPosition(i));
    }

    public bool HitTest(Vector2 worldPoint)
    {
        CachePoints();
        for (int i = 0; i < points.Count - 1; i++)
        {
            //Debug.Log("line from "+ points[i] + " to " + points[i + 1] + " is at " + DistancePointToSegment(worldPoint, points[i], points[i + 1]) +" distance");
            if (DistancePointToSegment(worldPoint, points[i], points[i + 1]) <= hitRadius)
                return true;
        }
        return false;
    }

    public void SetSelected(bool selected)
    {
        line.startColor = selected ? Color.blue : Color.black;
        line.endColor = selected ? Color.blue : Color.black;
    }

    public static float DistancePointToSegment(Vector2 p, Vector2 a, Vector2 b)
    {
        Vector2 ab = b - a;
        float t = Vector2.Dot(p - a, ab) / ab.sqrMagnitude;
        t = Mathf.Clamp01(t);
        Vector2 closest = a + t * ab;
        return Vector2.Distance(p, closest);
    }

    public virtual void Remove()
    {
        if (SelectionManager.Instance != null)
            SelectionManager.Instance.Deregister(this);

        Destroy(gameObject);
    }

    public void OnTranslate(Vector2 worldPosition)
    {
        Vector2 delta = worldPosition - Center;

        for (int i = 0; i < points.Count; i++)
        {
            points[i] = new Vector3(
                points[i].x + delta.x,
                points[i].y + delta.y,
                points[i].z
            );
            line.SetPosition(i, points[i]);
        }
    }

    public void OnRotate(float angleDeg, Vector2 pivot)
    {
        Quaternion rot = Quaternion.Euler(0, 0, angleDeg);

        for (int i = 0; i < points.Count; i++)
        {
            Vector2 dir = (Vector2)points[i] - pivot;
            dir = rot * dir;

            points[i] = new Vector3(
                pivot.x + dir.x,
                pivot.y + dir.y,
                points[i].z
            );

            line.SetPosition(i, points[i]);
        }
    }

}
