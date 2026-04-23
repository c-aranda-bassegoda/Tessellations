using UnityEngine;
using System.Collections.Generic;
using System;
using static UnityEditor.PlayerSettings;
using System.Collections;
using UnityEngine.EventSystems;

public class SelectionManager : MonoBehaviour
{
    public event Action<ISelectable> OnSelectionChanged;
    public static SelectionManager Instance { get; private set; }

    [SerializeField] List<ISelectable> selectables = new List<ISelectable>();
    public ISelectable selected;

    IDraggable currentDraggable;
    bool isDragging;

    Coroutine selectRoutine;

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
        //if (InputManager.Instance.BlockWorldInput)
        //{
        //    return;
        //}

        //if (InputManager.Instance.StartedOverUI)
        //{
        //    Debug.Log("Pointer started over UI, ignoring selection input");
        //    return;
        //}

        if (ToolManager.Instance.CurrentTool != ToolType.Select && !ToolManager.Instance.CurrentToolRequiresSelection())
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

    IEnumerator DelayedSelect(Vector2 pos)
    {
        yield return null; // wait one frame

        Debug.Log("Delayed select, trying to select");
        // Now UI system has updated correctly
        if (EventSystem.current.IsPointerOverGameObject(
            Input.touchCount > 0 ? Input.GetTouch(0).fingerId : -1))
        {
            yield break; 
        }

        TrySelect(pos);

        if (selected is IDraggable draggable)
        {
            currentDraggable = draggable;
            isDragging = true;
        }
    }

    public void DeleteSelected()
    {
        Debug.Log("DELETE CALLED");

        if (selectRoutine != null)
        {
            StopCoroutine(selectRoutine);
            selectRoutine = null;
        }

        if (selected == null)
        {
            Debug.Log("BUT SELECTED IS NULL");
            return;
        }

        ISelectable toRemove = selected;
        Deselect();
        toRemove.Remove();
    }


    public void ClearAll()
    {
        Deselect();
        if (selectables == null || selectables.Count == 0)
            return;
        selectables[0].Remove(); // recursive bc some selectables remove others when removed 
        ClearAll();
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
        if (InputManager.Instance.PointerOverUI)
            return;

        if (EventSystem.current != null &&
            EventSystem.current.IsPointerOverGameObject())
            return;

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

        //if (!InputManager.Instance.PointerOverUI)
        //{
            Deselect();
        //}
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
