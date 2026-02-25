using UnityEngine;
// <sumary>
// Composite selectable that controls two symmetric path points
// </summary>
public class TessPointSelectable : IPointSelectable
{
    public PathPointSelectable pointA;
    public PathPointSelectable pointB;

    private bool isSelected;
    private ISelectionHandler selectionHandler;

    public PathPointSelectable SelectedNode => pointA;

    public TessPointSelectable(PathPointSelectable a, PathPointSelectable b)
    {
        pointA = a;
        pointB = b;
    }

    public bool IsSelected() => isSelected;

    public void SetSelected(bool selected)
    {
        isSelected = selected;

        pointA?.SetSelected(selected);
        pointB?.SetSelected(selected);
    }

    public bool HitTest(Vector2 worldPoint)
    {
        // Delegate hit test to either point
        return (pointA != null && pointA.HitTest(worldPoint)) ||
               (pointB != null && pointB.HitTest(worldPoint));
    }

    public void OnDrag(Vector2 worldPosition)
    {
        Move(worldPosition);
    }

    public void Remove()
    {
        pointA?.Remove();
        pointB?.Remove();

        if (SelectionManager.Instance != null)
            SelectionManager.Instance.Deregister(this);
    }

    public void DestroyVisuals()
    {
        pointA?.DestroyVisuals();
        pointB?.DestroyVisuals();
    }

    public void SetSelectionHandler(ISelectionHandler handler)
    {
        selectionHandler = handler;
    }

    public void Move(Vector2 worldPosition)
    {
        if (pointA == null || pointB == null)
            return;

        // Compute delta from A
        Vector2 oldPos = pointA.Position;
        Vector2 delta = worldPosition - oldPos;

        // Move first point normally
        pointA.Move(worldPosition);

        // Apply same delta to symmetric point
        Vector2 mirroredTarget = pointB.Position + delta;
        pointB.Move(mirroredTarget);
    }
}