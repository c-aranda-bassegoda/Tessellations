using System.Collections.Generic;
using UnityEngine;

public class PiecewisePolygonView : MonoBehaviour
{
    [Header("Polygon Vertices")]
    [SerializeField] private List<Vector2> vertices = new List<Vector2>();

    [Header("Prefabs")]
    [SerializeField] private GameObject linePrefab;  // Holds LineRenderer
    [SerializeField] private GameObject nodePrefab;  // Anchor prefab

    [Header("Settings")]
    [SerializeField] private int resolutionPerSegment = 20;

    private PiecewisePolygonData polygon;
    private List<PathView> pathViews = new List<PathView>();

    private void Start()
    {
        BuildPolygonFromVertices();
    }

    public void BuildPolygonFromVertices()
    {
        if (vertices == null || vertices.Count < 2)
        {
            Debug.LogWarning("Need at least 2 vertices to create a polygon.");
            return;
        }

        polygon = new PiecewisePolygonData();

        Vector2 prev = vertices[^1]; // last vertex for looping

        foreach (var vertex in vertices)
        {
            // Create data
            var path = new BezierPath(prev, vertex, resolutionPerSegment);
            polygon.AddEdge(path);

            // Create view
            CreatePathView(path);

            prev = vertex;
        }
    }

    private void CreatePathView(BezierPath path)
    {
        GameObject edgeObj = Instantiate(linePrefab, transform);
        var view = edgeObj.GetComponent<PathView>();
        if (view == null)
            view = edgeObj.AddComponent<PathView>();

        view.Initialize(path, resolutionPerSegment);
        pathViews.Add(view);

        // Create anchors
        foreach (var point in path.Points)
        {
            GameObject nodeObj = Instantiate(nodePrefab, point.Position, Quaternion.identity, transform);
            var controller = nodeObj.AddComponent<PathPointController>();
            controller.Initialize(path, point);
        }
    }

    public PiecewisePolygonData PolygonData => polygon;
}