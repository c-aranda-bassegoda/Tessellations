using System;
using System.Collections.Generic;
using UnityEngine;

public class BaseShape : MonoBehaviour
{
    private List<Edge> edges;
    public List<Edge> Edges => edges;

    [SerializeField] List<Vertex> vertices = new List<Vertex>();
    public List<Vertex> Vertices => vertices;

    [SerializeField] private GameObject edgePrefab;
    [SerializeField] private GameObject vtxPrefab;
    [SerializeField] private int resolutionPerSegment = 3;
    private List<LineRenderer> edgeRenderers = new List<LineRenderer>();

    private void Start()
    {
        if (vertices == null)
            vertices = new List<Vertex>();
        edges = new List<Edge>();

        if (vertices.Count < 3)
        {
            vertices.Clear();
            vertices.Add(new Vertex(new Vector3(0, 0, 0)));
            vertices.Add(new Vertex(new Vector3(0, 1, 0)));
            vertices.Add(new Vertex(new Vector3(1, 1, 0)));
            vertices.Add(new Vertex(new Vector3(1, 0, 0)));
        }

        int prev = vertices.Count - 1;
        for (int i = 0; i < vertices.Count; i++)
        {
            edges.Add(new Edge(vertices[i], vertices[prev]));
            prev = i;
        }

        DrawEdges();
        DrawVertices();
    }

    private void DrawVertices()
    {
        if (vertices == null) return;
        
        for (int i = 0;i < vertices.Count;i++)
        {
            GameObject vtxObj = Instantiate(vtxPrefab, (Vector3)vertices[i].Position, Quaternion.identity);
            Debug.Log("color: "+vtxObj.GetComponent<SpriteRenderer>().color);
        }
    }

    private void DrawEdges()
    {
        if (edges == null) return;

        for (int i = 0; i < edges.Count; i++)
        {
            GameObject edgeObj = Instantiate(edgePrefab, Vector3.zero, Quaternion.identity);
            edgeObj.transform.parent = transform;

            LineRenderer lr = edgeObj.GetComponent<LineRenderer>();
            if (lr == null) return;
            lr.positionCount = resolutionPerSegment + 1;

            Vector3 a = edges[i].A.Position;
            Vector3 b = edges[i].B.Position;

            for (int j = 0; j <= resolutionPerSegment; j++)
            {
                float t = j / (float)resolutionPerSegment;
                lr.SetPosition(j, Vector3.Lerp(a, b, t));
            }

            edgeRenderers.Add(lr);
        }
    }


}

public class Edge
{
    public Vertex A { get; }
    public Vertex B { get; }

    public Vector3 MidPoint => (A.Position + B.Position) / 2f;

    public Edge(Vertex a, Vertex b)
    {
        A = a;
        B = b;
    }
}

[System.Serializable]
public class Vertex
{
    [SerializeField] private Vector2 position;
    public Vector2 Position => position;

    public Vertex(Vector2 position)
    {
        this.position = position;
    }
}
