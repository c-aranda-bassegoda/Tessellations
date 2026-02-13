using UnityEngine;
using System.Collections.Generic;
using System;

public class SelectionManager : MonoBehaviour
{
    public static SelectionManager Instance { get; private set; }

    List<ISelectable> selectables = new List<ISelectable>();
    ISelectable selected;

    private void Start()
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
        }
    }

    private void TrySelect(Vector2 pointerWorldPos)
    {
        Vector2 mouse = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        foreach (var s in selectables)
        {
            Debug.Log("Looking for a hit");
            if (s.HitTest(mouse))
            {
                Debug.Log("Got a hit");
                Select(s);
                return;
            }
        }

        Deselect();
    }

    void Select(ISelectable s)
    {
        selected?.SetSelected(false); // if sth is selected deselect it
        selected = s;
        selected.SetSelected(true);
    }

    void Deselect()
    {
        selected?.SetSelected(false); // if sth is selected deselect it
        selected = null;
    }
}
