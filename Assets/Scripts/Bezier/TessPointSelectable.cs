using UnityEngine;
using UnityEngine.SocialPlatforms;
using static PathPointSelectable;

// <sumary>
// Composite selectable that controls two symmetric path points
// </summary>
public class TessPointSelectable : IPointSelectable
{
    public PathPointSelectable mainPoint;
    public PathPointSelectable symPoint;
    public PathPointSelectable activePoint;
    public Vector2 axisDir;
    public Vector2 axisPivot;
    public Matrix2x2 rotMtx;

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

    public TessPointSelectable(
    PathPointSelectable a,
    PathPointSelectable b,
    Symmetry symmetry = Symmetry.Translation)
    : this(a, b, symmetry, Matrix2x2.Identity, default, default)
    {
    }
    public TessPointSelectable(
    PathPointSelectable a,
    PathPointSelectable b, Symmetry symmetry, Matrix2x2 rot)
    : this(a, b, symmetry, rot, default, default)
    {
    }

    public TessPointSelectable(PathPointSelectable a, PathPointSelectable b, Symmetry symmetry, Matrix2x2 rot, Vector2 axisDir, Vector2 axisPoint)
    {
        Transformation = symmetry;
        mainPoint = a;
        symPoint = b;
        this.axisDir = axisDir;
        this.axisPivot = axisPoint;
        this.rotMtx = rot;
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

        Vector2 oldActiveAnchorPos = activePoint.Position;

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
                TranslationOntoSymAxis(other, oldActiveAnchorPos, activePoint);
                break;
            case Symmetry.Rotation:

                break;
            case Symmetry.GlideReflection:
                GlideReflectionOntoSymAxis(other, oldActiveAnchorPos, activePoint);
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
        switch (Transformation)
        {
            case Symmetry.Translation:
                TranslationOntoSymAxis(other, oldPos, activePoint);
                break;
            case Symmetry.Rotation:
                
                break;
            case Symmetry.GlideReflection:
                GlideReflectionOntoSymAxis(other, oldPos, activePoint);
                break;
        }
    }


    /// -----------------------------------------------------
    /// Transformations
    /// ------------------------------------------------------

    /// <summary>
    /// Moves other by the same offset in the same direction as active 
    /// (Resulting in a transformation with translational symmetry)
    /// </summary>
    private void TranslationOntoSymAxis(PathPointSelectable other, Vector2 oldActiveAnchorPos, PathPointSelectable active)
    {
        switch (active.SelectedPart)
        {
            case PathPointSelectable.ActivePart.HandleIn:
            {
                Vector2 offset =
                    active.HandleInPos - active.Position;

                Vector2 mirroredOffset = -offset;

                other.UpdateHandlePosition(
                    other.handleInSelectable,
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
                    other.handleOutSelectable,
                    other.Position + mirroredOffset
                );
                break;

            }
            default:
            {
                Vector2 anchorDelta = active.Position - oldActiveAnchorPos;
                other.Move(other.Position + anchorDelta);
                break;
            }
        }
    }

    /// <summary>
    /// Moves other by the same offset in the same direction as active and reflects it.
    /// (Resulting in a transformation with glide-reflection symmetry)
    /// </summary>
    private void GlideReflectionOntoSymAxis(PathPointSelectable other, Vector2 oldActiveAnchorPos, PathPointSelectable active)
    {
        // Reflect anchor position
        Vector2 activeAnchorReflected = SymmetryUtils.ReflectAcrossAxis(
            active.Position,
            axisPivot,
            axisDir
        );
        Vector2 activeAnchorTransformed = rotMtx.Multiply(activeAnchorReflected - axisPivot) + axisPivot;

        switch (active.SelectedPart)
        {
            case PathPointSelectable.ActivePart.HandleIn:
                {
                    Vector2 reflectedHandle = SymmetryUtils.ReflectAcrossAxis(
                        active.HandleInPos,
                        axisPivot,
                        axisDir
                    );
                    Vector2 transformedHandle = rotMtx.Multiply(reflectedHandle - axisPivot) + axisPivot;

                    Vector2 offset = transformedHandle - activeAnchorTransformed;

                    Vector2 mirroredOffset = offset;

                    other.UpdateHandlePosition(
                        other.handleInSelectable,
                        other.Position + mirroredOffset
                    );
                    break;
                }

            case PathPointSelectable.ActivePart.HandleOut:
                {
                    Vector2 reflectedHandle = SymmetryUtils.ReflectAcrossAxis(
                        active.HandleOutPos,
                        axisPivot,
                        axisDir
                    );

                    Vector2 transformedHandle = rotMtx.Multiply(reflectedHandle - axisPivot) + axisPivot;

                    Vector2 offset = transformedHandle - activeAnchorTransformed;

                    Vector2 mirroredOffset = -offset;

                    other.UpdateHandlePosition(
                        other.handleOutSelectable,
                        other.Position + mirroredOffset
                    );
                    break;
                }
            default:
                {
                    // Reflect old anchor positions of active point
                    Vector2 oldReflected = SymmetryUtils.ReflectAcrossAxis(
                        oldActiveAnchorPos,
                        axisPivot,
                        axisDir
                    );
                    Vector2 oldTransformed = rotMtx.Multiply(oldReflected - axisPivot) + axisPivot;


                    // Compute delta in reflected space
                    Vector2 reflectedDelta = activeAnchorTransformed - oldTransformed;

                    // Apply that delta to the other point
                    other.Move(other.Position + reflectedDelta);
                    break;
                }
        }
    }

    
}

public static class SymmetryUtils
{
    public static Vector2 ReflectAcrossAxis(Vector2 point, Vector2 axisPoint, Vector2 axisDir)
    {
        axisDir.Normalize();

        Vector2 relative = point - axisPoint;

        // perpendicular normal
        Vector2 normal = new Vector2(-axisDir.y, axisDir.x);

        Vector2 reflected = Vector2.Reflect(relative, normal);

        return reflected + axisPoint;
    }
}