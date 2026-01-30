using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public class Polygon : MonoBehaviour
{
    [SerializeField] List<Vector2> vertices;
    List<Path> edges;
    [SerializeField] private GameObject linePrefab; // Prefab with LineRenderer
    [SerializeField] private int resolutionPerSegment = 20;

    

    internal NodeSelectable TryAddPoint(Vector3 pointerWorldPos)
    {
        NodeSelectable node = null;
        foreach (var edge in edges)
        {
            node = edge.TryAddPoint(pointerWorldPos);
            if (node != null) break;
        }

        return node;
    }

    private void Awake()
    {
        edges = new List<Path>();
    }

    void Start()
    {
        if (vertices == null || vertices.Count < 2) return;

        Vector2 prev = vertices[0];
        for(int i=1; i<vertices.Count; i++)
        {
            GameObject edgeObj = Instantiate(linePrefab, Vector3.zero, Quaternion.identity);
            edgeObj.transform.parent = transform;

            Path path = edgeObj.AddComponent<Path>();
            path.resolutionPerSegment = resolutionPerSegment;

            path.Initialize(prev, vertices[i]);
            edges.Add(path);
            

            prev = vertices[i];
        }
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
