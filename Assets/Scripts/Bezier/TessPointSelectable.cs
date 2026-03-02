using UnityEngine;
using static PathPointSelectable;

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
    public enum Symmetry
    {
        Translation,
        Rotation,
        GlideReflection
    }
    public Symmetry Transformation { get; set; }

    public PathPointSelectable SelectedNode => mainPoint;

    public TessPointSelectable(PathPointSelectable a, PathPointSelectable b, Symmetry symmetry = Symmetry.Translation)
    {
        Transformation = symmetry;
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

        PathPointSelectable other =
            activePoint == mainPoint ? symPoint : mainPoint;

        Vector2 oldAnchorPos = activePoint.Position;

        if (other == mainPoint) 
        { // symPoint has no handle visuals (handles should not be draggable/selectable)
            if (activePoint.SelectedPart != ActivePart.Anchor)
            {
                SelectionManager.Instance.Deselect();
                return;
            }
        }
        // Let active point process normally
        activePoint.OnDrag(worldPosition);

        switch (Transformation)
        {
            case Symmetry.Translation:
                TranslationOntoSymAxis(other, oldAnchorPos, activePoint);
                break;
            case Symmetry.Rotation:
                break;
            case Symmetry.GlideReflection:
                break;
        }
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
        TranslationOntoSymAxis(other, oldPos, activePoint);
    }


    /// -----------------------------------------------------
    /// Transformations
    /// ------------------------------------------------------

    /// <summary>
    /// Moves other by the same offset in the same direction as active 
    /// (Resulting in a transformation with translational symmetry)
    /// </summary>
    private void TranslationOntoSymAxis(PathPointSelectable other, Vector2 oldAnchorPos, PathPointSelectable active)
    {
        switch (active.SelectedPart)
        {
            case PathPointSelectable.ActivePart.HandleIn:
                {
                    Vector2 offset =
                        active.HandleInPos - active.Position;

                    Vector2 mirroredOffset = -offset;

                    other.UpdateHandlePosition(
                        other.handleOutSelectable,
                        other.Position + mirroredOffset
                    );
                    break;
                }

            case PathPointSelectable.ActivePart.HandleOut:
                {
                    Vector2 offset =
                        active.HandleOutPos - active.Position;

                    Vector2 mirroredOffset = offset;

                    other.UpdateHandlePosition(
                        other.handleInSelectable,
                        other.Position + mirroredOffset
                    );
                    break;
                }
            default:
                {
                    Vector2 anchorDelta = active.Position - oldAnchorPos;
                    other.Move(other.Position + anchorDelta);
                    break;
                }
        }
    }
}