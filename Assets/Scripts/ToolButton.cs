using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ToolButton : MonoBehaviour
{
    protected Button button;
    public ToolType toolType = ToolType.None;
    public Image icon;
    public Color selectedColor = Color.blue;
    public Color normalColor = Color.white;
    private bool selected = false; 

    // is interactable only if there is a selected obj
    private bool requiresSelection;

    void Awake()
    {
        button = GetComponent<Button>();
        requiresSelection = ToolManager.toolsRequiringSelection.Contains(toolType);
        Debug.Log($"ToolButton Awake: {toolType}, requiresSelection = {requiresSelection}");
    }

    void Start()
    {
        if (requiresSelection)
        {
            SelectionManager.Instance.OnSelectionChanged += HandleSelectionChanged;
            //button.interactable = SelectionManager.Instance.selected != null;
        }
    }

    void OnDestroy()
    {
        if (requiresSelection && SelectionManager.Instance != null)
            SelectionManager.Instance.OnSelectionChanged -= HandleSelectionChanged;
    }
    private void HandleSelectionChanged(ISelectable selection)
    {
        StartCoroutine(SetInteractableNextFrame(selection != null));
    }

    IEnumerator SetInteractableNextFrame(bool state)
    {
        yield return null;
        //button.interactable = state;
    }


    void Update()
    {
        selected = ToolManager.Instance.CurrentTool == toolType;
        if (toolType == ToolType.None) return;
        icon.color = selected ? selectedColor : normalColor;
    }

    public void OnClick()
    {
        if (selected)
        {
            selected = false;
            ToolManager.Instance.SetTool(ToolType.None);
        } else
        {
            selected = true;
            ToolManager.Instance.SetTool(toolType);
        }
    }
}
