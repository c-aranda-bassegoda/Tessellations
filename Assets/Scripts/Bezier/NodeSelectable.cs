using System.Collections.Generic;
using UnityEngine;

public class NodeSelectable : MonoBehaviour, ISelectable
{
    protected SpriteRenderer node;

    public float hitRadius = 1f;

    void Awake()
    {
        node = GetComponent<SpriteRenderer>();
    }

    public virtual void Move(Vector2 position)
    {
        transform.position = position;
    }

    public Vector2 GetPosition()
    {
        return transform.position;
    }
 
    public bool HitTest(Vector2 worldPoint)
    {
        Vector2 nodePos = new Vector2(transform.position.x, transform.position.y);
        Debug.Log(nodePos + " " + worldPoint + " " + Vector2.Distance(worldPoint, nodePos) + " " + hitRadius);
        return Vector2.Distance(worldPoint, nodePos) <= hitRadius;
    }

    public virtual void SetSelected(bool selected)
    {
        node.color = selected ? Color.blue : Color.white;
    }

    public void Remove()
    {
        Destroy(node);

        if (SelectionManager.Instance != null)
            SelectionManager.Instance.Deregister(this);
    }
}

