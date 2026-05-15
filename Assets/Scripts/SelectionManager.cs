using UnityEngine;
using System.Collections.Generic;
using System;
using UnityEngine.EventSystems;

public class SelectionManager : MonoBehaviour
{
    public event Action<ISelectable> OnSelectingChanged;
    public event Action<ISelectable> OnSelectionChanged;
    public static SelectionManager Instance { get; private set; }

    [SerializeField] List<ISelectable> selectables = new List<ISelectable>();
    public ISelectable selected;
    public ISelectable lastSelected;

    IDraggable currentDraggable;
    bool isDragging;

    private void Awake()
    {
        Instance = this;
        ToolManager.Instance.OnToolChanged += () =>
        {
            if (ToolManager.Instance.CurrentTool != ToolType.Select && !ToolManager.Instance.CurrentToolRequiresSelection())
            {
                Debug.Log("Tool changed to non-select, deselecting");
                OnSelectingChanged?.Invoke(null);
                selected = null;
            }
        };
    }

    private void Instance_OnToolChanged()
    {
        throw new NotImplementedException();
    }

    public void Register(ISelectable selectable)
    {
        Debug.Log("Added Selectable");
        selectables.Add(selectable);
    }

    void Update()
    {
        //if (ToolManager.Instance.CurrentTool != ToolType.Select && !ToolManager.Instance.CurrentToolRequiresSelection())
        //{
        //    Debug.Log("Not in select mode, deselecting if needed");
        //    if (ToolManager.Instance.CurrentTool != ToolManager.Instance.PreviousTool)
        //    {
        //        OnSelectingChanged?.Invoke(null);
        //        ToolManager.Instance.PreviousTool = ToolManager.Instance.CurrentTool;
        //    }
        //    return;
        //}

        if (InputManager.Instance.PointerDown)
        {
            Debug.Log("Pointer down, trying to select");
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
        Debug.Log("DeleteSelected");
        if (lastSelected == null)
            return;
        Debug.Log("Removing " + lastSelected);
        ISelectable toRemove = lastSelected;
        Deselect();
        isDragging = false;
        currentDraggable = null;
        toRemove.Remove();
        OnSelectingChanged?.Invoke(null);
    }


    public void ClearAll()
    {
        Deselect();
        if (selectables == null || selectables.Count == 0)
            return;
        selectables[0].Remove(); // recursive bc some selectables remove others when removed 
        ClearAll();
    }

    public void ClearDrawings()
    {
        Deselect();
        if (selectables == null || selectables.Count == 0)
            return;
        List<ISelectable> toRemove = new List<ISelectable>();
        foreach (var s in selectables)
        {
            if (s is LineSelectable && s is not EdgeSelectable)
                toRemove.Add(s);
        }
        foreach (var s in toRemove)
            s.Remove();
        Update();
    }

    public void Undo()
    {
        Deselect();
        if (selectables == null || selectables.Count == 0)
            return;
        ISelectable s = selectables[selectables.Count - 1];
        if (s is LineSelectable && s is not EdgeSelectable)
            s.Remove();
    }

    public ISelectable FindSelectableWithEndpnts(Vector2 a, Vector2 b)
    {

        ISelectable match = null;

        float tolerance = 0.05f;

        foreach (ISelectable selectable in selectables)
        {
            var mb = selectable as MonoBehaviour;
            if (mb == null)
                continue;

            LineRenderer lr = mb.GetComponent<LineRenderer>();
            if (lr == null)
                continue;

            Vector2 p = lr.GetPosition(0);
            Vector2 q = lr.GetPosition(lr.positionCount-1);
            if ((Vector2.Distance(p, a) < tolerance && Vector2.Distance(q, b) < tolerance)
                || (Vector2.Distance(q, a) < tolerance && Vector2.Distance(p, b) < tolerance))
                match = selectable;
        }

        return match;
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
        Debug.Log("Trying to select at " + pointerWorldPos);
        if (InputManager.Instance.PointerOverUI)
        {
            Debug.Log("Pointer over UI, ignoring selection");
            if (selected != null && selected is not EdgeSelectable)
            {
                Deselect();
            }
            return;
        }

        //if (EventSystem.current != null &&
        //    EventSystem.current.IsPointerOverGameObject())
        //{
        //        Debug.Log("Pointer over UI (EventSystem), ignoring selection");
        //    if (selected != null && selected is not EdgeSelectable)
        //    {
        //        Deselect();
        //    }
        //    return;
        //}

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
                Debug.Log("selected");
                Select(s);
                return;
            }
        }

        Deselect();

    }

    public void Select(ISelectable s)
    {
        if (selected != null)
        {
            selected.SetSelected(false); // if sth is selected deselect it
        }
            lastSelected = selected;
        Debug.Log("Selecting " + s.ToString());
        selected = s;
        selected.SetSelected(true);

        OnSelectingChanged?.Invoke(selected);
    }

    public void Deselect()
    {
        if (selected != null)
        {
            selected.SetSelected(false); // if sth is selected deselect it
        }
        lastSelected = selected;
        Debug.Log("Deselecting");
        //OnSelectionChanged?.Invoke(selected);
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
