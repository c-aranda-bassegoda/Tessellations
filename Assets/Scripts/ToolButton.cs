using System;
using UnityEngine;
using UnityEngine.UI;

public class ToolButton : MonoBehaviour
{
    Button button;
    public ToolType toolType;
    public Image icon;
    public Color selectedColor = Color.blue;
    public Color normalColor = Color.white;
    private bool selected = false; 

    // is interactable only if there is a selected obj
    public bool requiresSelection;

    void Awake()
    {
        button = GetComponent<Button>();
    }

    void Start()
    {
        if (requiresSelection)
        {
            SelectionManager.Instance.OnSelectionChanged += HandleSelectionChanged;
            button.interactable = SelectionManager.Instance.selected != null;
        }
    }

    void OnDestroy()
    {
        if (requiresSelection && SelectionManager.Instance != null)
            SelectionManager.Instance.OnSelectionChanged -= HandleSelectionChanged;
    }
    private void HandleSelectionChanged(ISelectable selection)
    {
        button.interactable = selection != null;
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
