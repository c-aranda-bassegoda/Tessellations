using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;


public class PathPoint : ISelectable
{
    public NodeSelectable anchor;
    public Vector3 handleInOffset;
    public Vector3 handleOutOffset;
    public HandleSelectable handleInSelectable;
    public HandleSelectable handleOutSelectable;

    private ISelectionHandler selectionHandler;

    public bool smooth;

    private bool isSelected;
    public bool IsSelected() => isSelected;

    private enum ActivePart
    {
        None,
        Anchor,
        HandleIn,
        HandleOut
    }

    private ActivePart activePart = ActivePart.None;

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
        isSelected = selected;

        if (selectionHandler != null)
        {
            if (selected) selectionHandler.OnSelected();
            else selectionHandler.OnDeselected();
        }

        switch (activePart)
        {
            case ActivePart.Anchor: anchor.SetSelected(selected); break;
            case ActivePart.HandleIn: handleInSelectable.SetSelected(selected); break;
            case ActivePart.HandleOut: handleOutSelectable.SetSelected(selected); break;
        }

        if (!selected)
            activePart = ActivePart.None;
    }

    public bool HitTest(Vector2 worldPoint)
    {
        // Always check the anchor
        if (anchor.HitTest(worldPoint))
        {
            activePart = ActivePart.Anchor;
            return true;
        }

        // Only check handles if the anchor is currently selected
        if (IsSelected() && selectionHandler != null)
        {
            float handleRadius = 0.2f; // tweak for your scale
            if ((HandleInPos - (Vector3)worldPoint).sqrMagnitude <= handleRadius * handleRadius)
            {
                activePart = ActivePart.HandleIn;
                return true;
            }

            if ((HandleOutPos - (Vector3)worldPoint).sqrMagnitude <= handleRadius * handleRadius)
            {
                activePart = ActivePart.HandleOut;
                return true;
            }
        }

        activePart = ActivePart.None;
        return false;
    }
}
