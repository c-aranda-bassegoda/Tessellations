using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;

public class PathPoint : ISelectable, IDraggable
{
    public NodeSelectable anchor;
    public Vector3 handleInOffset;
    public Vector3 handleOutOffset;

    private ISelectionHandler selectionHandler;

    public bool smooth;


    public Vector3 HandleInPos => (Vector3)anchor.GetPosition() + handleInOffset;
    public Vector3 HandleOutPos => (Vector3)anchor.GetPosition() + handleOutOffset;

    public void MoveAnchor(Vector3 newPosition)
    {
        Vector3 delta = newPosition - (Vector3)anchor.GetPosition();

        anchor.Move(newPosition);
    }

    public void SetSelectionHandler(ISelectionHandler handler)
    {
        selectionHandler = handler;
    }

    public void SetSelected(bool selected)
    {
        anchor.SetSelected(selected); // highlight anchor

        if (selectionHandler != null)
        {
            if (selected) selectionHandler.OnSelected();
            else selectionHandler.OnDeselected();
        }
    }

    public bool HitTest(Vector2 worldPoint)
    {
        return anchor.HitTest(worldPoint);
    }

    public void OnDrag(Vector2 worldPosition)
    {
        MoveAnchor(worldPosition);
        selectionHandler.OnSelected();
    }

}
