using UnityEngine;

/// <summary>
/// Visual for a global handle. Implements ISelectable and IDraggable.
/// </summary>
public class HandleView : NodeSelectable, ISelectable, IDraggable
{
    public bool isHandleIn;
    public PathPointController parentController;

    public void Initialize(bool isIn)
    {
        isHandleIn = isIn;
    }

    public void OnDrag(Vector2 worldPos)
    {
        if (parentController == null) return;

        GlobalHandleController.Instance.MoveHandle(this, worldPos);
    }

    public override bool HitTest(Vector2 worldPoint)
    {
        return base.HitTest(worldPoint);
    }

    public override void SetSelected(bool selected)
    {
        node.color = selected ? Color.blue : Color.red;
    }
}