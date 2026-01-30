using System.Collections.Generic;
using UnityEngine;

internal class NodeSelectable : MonoBehaviour, ISelectable
{
    [SerializeField]Vector2 position;

    public float hitRadius = 0.1f;

    void Awake()
    {
        
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