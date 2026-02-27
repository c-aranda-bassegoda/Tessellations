using System;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;
using static UnityEngine.RuleTile.TilingRuleOutput;

// A node or vertex with handles that define the curvature of an anchor point of a cubic bezier curve
public class PathPointSelectable : IPointSelectable
{
    public Path parentPath;

    public NodeSelectable anchor;
    public Vector2 Position => anchor.GetPosition();

    public Vector2 handleInOffset;
    public Vector2 handleOutOffset;
    public HandleSelectable handleInSelectable;
    public HandleSelectable handleOutSelectable;

    private ISelectionHandler selectionHandler;

    public bool smooth;

    private bool isSelected;
    public bool IsSelected() => isSelected;

    public enum ActivePart
    {
        None,
        Anchor,
        HandleIn,
        HandleOut
    }

    protected ActivePart activePart = ActivePart.None;
    public ActivePart SelectedPart => activePart;

    public Vector2 HandleInPos => anchor.GetPosition() + handleInOffset;
    public Vector2 HandleOutPos => anchor.GetPosition() + handleOutOffset;

    public PathPointSelectable SelectedNode => this;

    public void Move(Vector2 newPosition)
    {
        Vector2 delta = newPosition - anchor.GetPosition();

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

        anchor.SetSelected(selected);
        switch (activePart)
        {
            case ActivePart.HandleIn: handleInSelectable.SetSelected(selected); break;
            case ActivePart.HandleOut: handleOutSelectable.SetSelected(selected); break;
        }

        if (!selected)
            activePart = ActivePart.None;
    }

    public void UpdateHandlePosition(HandleSelectable handle, Vector2 worldPosition)
    {
        Vector2 anchorPos = anchor.GetPosition();
        Vector2 offset = worldPosition - anchorPos;

        bool isInHandle = (handle == handleInSelectable);

        if (isInHandle)
            handleInOffset = offset;
        else
            handleOutOffset = offset;

        // Smooth logic (mirror opposite handle)
        if (smooth)
        {
            Vector2 mirroredOffset = -offset;

            if (isInHandle)
                handleOutOffset = mirroredOffset;
            else
                handleInOffset = mirroredOffset;

            // Move opposite visual without recursion
            HandleSelectable opposite =
                isInHandle ? handleOutSelectable : handleInSelectable;

            if (opposite != null)
                opposite.MoveWithoutNotify(anchorPos + mirroredOffset);
        }
    }

    public bool HitTest(Vector2 worldPoint)
    {
        if (anchor == null) return false;

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
            if ((HandleInPos - worldPoint).sqrMagnitude <= handleRadius * handleRadius)
            {
                activePart = ActivePart.HandleIn;
                return true;
            }

            if ((HandleOutPos - worldPoint).sqrMagnitude <= handleRadius * handleRadius)
            {
                activePart = ActivePart.HandleOut;
                return true;
            }
        }

        activePart = ActivePart.None;
        return false;
    }

    public void OnDrag(Vector2 worldPosition)
    {
        if (activePart == ActivePart.Anchor)
        {
            Move(worldPosition);  
        }
        else
        {
            if (activePart == ActivePart.HandleIn)
                UpdateHandlePosition(handleInSelectable, worldPosition);
            if (activePart == ActivePart.HandleOut)
                UpdateHandlePosition(handleOutSelectable, worldPosition);
        }
        selectionHandler.OnSelected();
    }

    public void MoveHandle(Vector2 worldPosition)
    {
        if (activePart == ActivePart.HandleIn)
        {
            handleInSelectable.Move(worldPosition);
        }
        else
        {
            handleOutSelectable.Move(worldPosition);
        }
    }


    public void Remove()
    {
        parentPath?.DeletePoint(this);
        if (SelectionManager.Instance != null)
            SelectionManager.Instance.Deregister(this);
        DestroyVisuals();
    }

    public void DestroyVisuals()
    {
        if (anchor != null)
            UnityEngine.Object.Destroy(anchor.gameObject);
    }
}
