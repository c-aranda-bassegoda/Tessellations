using UnityEngine;
using System.Collections.Generic;
using System;

public class SelectionManager : MonoBehaviour
{
    public static SelectionManager Instance { get; private set; }

    List<ISelectable> selectables = new List<ISelectable>();
    ISelectable selected;

    IDraggable currentDraggable;
    bool isDragging;

    private void Awake()
    {
        Instance = this;
    }

    public void Register(ISelectable selectable)
    {
        Debug.Log("Added Selectable");
        selectables.Add(selectable);
    }

    void Update()
    {
        if (ToolManager.Instance.CurrentTool != ToolType.Select)
        {
            Deselect();
            return;
        }

        if (InputManager.Instance.PointerOverUI)
            return;

        if (InputManager.Instance.PointerDown)
        {
            TrySelect(InputManager.Instance.PointerWorldPos);

            if (selected is IDraggable draggable)
            {
                currentDraggable = draggable;
                isDragging = true;
            }
        }

        if (isDragging && InputManager.Instance.PointerHeld)
        {
            currentDraggable?.OnDrag(InputManager.Instance.PointerWorldPos);
        }

        if (isDragging && InputManager.Instance.PointerUp)
        {
            isDragging = false;
            currentDraggable = null;
        }
    }

    public void DeleteSelected()
    {
        if (selected == null)
            return;

        ISelectable toRemove = selected;
        Deselect();
        toRemove.Remove();
    }

    private void TrySelect(Vector2 pointerWorldPos)
    {

        for (int i = selectables.Count - 1; i >= 0; i--)
        {
            Debug.Log("Looking for a hit");
            var s = selectables[i];

            if (s == null)
            {
                selectables.RemoveAt(i);
                continue;
            }

            if (s.HitTest(pointerWorldPos))
            {
                if (s == selected) return; // already selected, do nothing

                Select(s);
                return;
            }
        }

        Deselect();
        
    }

    void Select(ISelectable s)
    {
        if (selected != null)
            selected.SetSelected(false); // if sth is selected deselect it
        selected = s;
        selected.SetSelected(true);
    }

    void Deselect()
    {
            if (selected != null)
                selected.SetSelected(false); // if sth is selected deselect it
            selected = null;
    }

    internal void Deregister(ISelectable selectable)
    {
        if (selectables.Contains(selectable))
            selectables.Remove(selectable);

        // Also deselect if it was the currently selected object
        if (selected == selectable)
            Deselect(); 
    }
}
