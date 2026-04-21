using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;

public class TransfToolButton : ToolButton
{

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        SelectionManager.Instance.OnSelectionChanged += HandleSelectionChanged;
        //button.interactable = SelectionManager.Instance.selected != null;
        button.gameObject.SetActive(SelectionManager.Instance.selected != null);
    }

    private void HandleSelectionChanged(ISelectable selection)
    {
        var selected = SelectionManager.Instance.selected;

        MonoBehaviour mb = selected as MonoBehaviour;

        if (mb == null)
        {
            Debug.Log("null mb");
            return;
        }
        EdgeSelectable line = mb.gameObject?.GetComponent<EdgeSelectable>();

        bool active = false;
        switch (toolType)
        {
            case ToolType.Translate:
                active = SymmetryManager.Instance.baseShape.FindTranslationCompatibleEdges(line).Count > 0;
                break;
            case ToolType.Rotate:
                active = SymmetryManager.Instance.baseShape.FindRotationCompatibleEdges(line).Count > 0;
                break;
            case ToolType.Glide:
                active = SymmetryManager.Instance.baseShape.FindGlideReflectionCompatibleEdges(line).Count > 0;
                break;
        }

        button.gameObject.SetActive((selection != null) && active);
        button.interactable = (selection != null) && active;
    }

    void OnDestroy()
    {
        if (SelectionManager.Instance != null)
            SelectionManager.Instance.OnSelectionChanged -= HandleSelectionChanged;
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
