using System.Collections.Generic;
using UnityEngine;

public class NodeSelectable :  ISelectable
{
    [SerializeField]Vector2 position;

    public float hitRadius = 0.1f;

    public NodeSelectable(Vector2 position)
    {
        this.position = position;
    }

    public void move(Vector2 position)
    {
        this.position = position;
    }

    public Vector2 GetPosition()
    {
        return this.position;
    }
 
    public bool HitTest(Vector2 worldPoint)
    {
        throw new System.NotImplementedException();
    }

    public void SetSelected(bool selected)
    {
        throw new System.NotImplementedException();
    }
}

