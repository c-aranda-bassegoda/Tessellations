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
        // Delegate hit test to either point
        return (mainPoint != null && mainPoint.HitTest(worldPoint)) ||
               (symPoint != null && symPoint.HitTest(worldPoint));
    }

    public void OnDrag(Vector2 worldPosition)
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
        activePoint.OnDrag(worldPosition);

        // Apply same delta to symmetric point
        Vector2 mirroredTarget = other.Position + delta;
        other.Move(mirroredTarget);
    }

    public void Remove()
    {
        mainPoint?.Remove();
        symPoint?.Remove();

        if (SelectionManager.Instance != null)
            SelectionManager.Instance.Deregister(this);
    }

    public void DestroyVisuals()
    {
        mainPoint?.DestroyVisuals();
        symPoint?.DestroyVisuals();
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