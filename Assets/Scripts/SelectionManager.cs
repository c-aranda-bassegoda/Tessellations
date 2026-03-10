using UnityEngine;
using System.Collections.Generic;
using System;

public class SelectionManager : MonoBehaviour
{
    public event Action<ISelectable> OnSelectionChanged;
    public static SelectionManager Instance { get; private set; }

    [SerializeField] List<ISelectable> selectables = new List<ISelectable>();
    public ISelectable selected;

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

    public ISelectable FindBestFitSelectable(List<Vector2> positions)
    {
        if (positions == null || positions.Count == 0)
            return null;

        ISelectable bestMatch = null;
        int bestScore = 0;

        float tolerance = 0.05f;

        foreach (ISelectable selectable in selectables)
        {
            var mb = selectable as MonoBehaviour;
            if (mb == null)
                continue;

            LineRenderer lr = mb.GetComponent<LineRenderer>();
            if (lr == null)
                continue;

            int score = 0;

            for (int i = 0; i < lr.positionCount; i++)
            {
                Vector2 p = lr.GetPosition(i);

                foreach (var pos in positions)
                {
                    if (Vector2.Distance(p, pos) < tolerance)
                    {
                        score++;
                        break;
                    }
                }
            }

            if (score > bestScore)
            {
                bestScore = score;
                bestMatch = selectable;
            }
        }

        return bestMatch;
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

    public void Select(ISelectable s)
    {
        if (selected != null)
            selected.SetSelected(false); // if sth is selected deselect it
        selected = s;
        selected.SetSelected(true);

        OnSelectionChanged?.Invoke(selected);
    }

    public void Deselect()
    {
        if (selected != null)
            selected.SetSelected(false); // if sth is selected deselect it
        selected = null;
        OnSelectionChanged?.Invoke(null);
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
