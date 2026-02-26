using UnityEngine;
// <sumary>
// Composite selectable that controls two symmetric path points
// </summary>
public class TessPointSelectable : IPointSelectable
{
    public PathPointSelectable mainPoint;
    public PathPointSelectable symPoint;
    public PathPointSelectable activePoint;

    private bool isSelected;
    private ISelectionHandler selectionHandler;

    public PathPointSelectable SelectedNode => mainPoint;

    public TessPointSelectable(PathPointSelectable a, PathPointSelectable b)
    {
        mainPoint = a;
        symPoint = b;
    }

    public bool IsSelected() => isSelected;

    public void SetSelected(bool selected)
    {
        isSelected = selected;

        mainPoint?.SetSelected(selected);
        symPoint?.SetSelected(selected);
    }

    public bool HitTest(Vector2 worldPoint)
    {
        if (mainPoint != null && mainPoint.HitTest(worldPoint))
        {
            activePoint = mainPoint;
            return true;
        }
        if (symPoint != null && symPoint.HitTest(worldPoint))
        {
            activePoint = symPoint;
            return true;
        }
        return false;
    }

    public void OnDrag(Vector2 worldPosition)
    {
        if (mainPoint == null || symPoint == null)
            return;
        if (activePoint == null)
            activePoint = mainPoint;

        PathPointSelectable other = (activePoint == mainPoint ? symPoint : mainPoint);

        // Compute delta from selected point
        Vector2 oldPos = activePoint.Position;
        Vector2 delta = worldPosition - oldPos;

        // Move first point normally
        activePoint.OnDrag(worldPosition);

        // Apply same delta to symmetric point
        Vector2 mirroredTarget = other.Position + delta;
        if (activePoint.SelectedPart == PathPointSelectable.ActivePart.Anchor)
        {
            other.Move(mirroredTarget);
        }
        else
        {
            Vector2 otherAnchor = other.Position;
            Vector2 mirroredOffset = mirroredTarget - otherAnchor;

            if (activePoint.SelectedPart == PathPointSelectable.ActivePart.HandleOut)
            {
                other.handleInOffset = mirroredOffset;
            }
            else if (activePoint.SelectedPart == PathPointSelectable.ActivePart.HandleIn)
            {
                other.handleOutOffset = mirroredOffset;
            }
        }
        //selectionHandler.OnSelected();
    }

    public void Remove()
    {
        mainPoint?.Remove();
        symPoint?.Remove();

        SelectionManager.Instance?.Deregister(this);
    }

    

    public void SetSelectionHandler(ISelectionHandler handler)
    {
        mainPoint?.SetSelectionHandler(handler);
        symPoint?.SetSelectionHandler(handler);
    }

    public void Move(Vector2 worldPosition)
    {
        if (mainPoint == null || symPoint == null)
            return;
        if (activePoint == null)
            activePoint = mainPoint;

        PathPointSelectable other =
        activePoint == mainPoint ? symPoint : mainPoint;

        // Compute delta from selected point
        Vector2 oldPos = activePoint.Position;
        Vector2 delta = worldPosition - oldPos;

        // Move first point normally
        activePoint.Move(worldPosition);

        // Apply same delta to symmetric point
        Vector2 mirroredTarget = other.Position + delta;
        other.Move(mirroredTarget);
    }
}