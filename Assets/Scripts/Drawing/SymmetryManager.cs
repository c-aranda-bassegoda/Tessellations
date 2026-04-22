using System.Collections.Generic;
using UnityEngine;

public class SymmetryManager : MonoBehaviour
{
    [SerializeField] public TilePolygon baseShape;

    public static SymmetryManager Instance { get; private set; }

    GameObject clipboard;
    List<int> compatibleEdgeIdxs = new List<int>();

    ITransformable previewDraggable;

    private ToolType lastTool;

    void Awake()
    {
        Instance = this;
    }

    void Update()
    {
        ToolType currentTool = ToolManager.Instance.CurrentTool;

        Vector3 pointerPos = InputManager.Instance.PointerWorldPos;

        // tool changed
        if (currentTool != lastTool)
        {
            if (!ToolManager.Instance.CurrentToolIsTransformationTool())
                baseShape.DehighlightEdges();
            else
            {
                CopySelected();
                EdgeSelectable line = clipboard?.GetComponent<EdgeSelectable>();
                compatibleEdgeIdxs.Clear();
                switch (currentTool)
                {
                    case ToolType.Translate:
                        compatibleEdgeIdxs = baseShape.FindTranslationCompatibleEdges(line);
                        break;
                    case ToolType.Rotate:
                        compatibleEdgeIdxs = baseShape.FindRotationCompatibleEdges(line);
                        break;
                    case ToolType.Glide:
                        compatibleEdgeIdxs = baseShape.FindGlideReflectionCompatibleEdges(line);
                        break;
                }
                baseShape.HighlightEdges(compatibleEdgeIdxs, Color.red);
            }

            lastTool = currentTool;
        } 

        if (!ToolManager.Instance.CurrentToolIsTransformationTool())
            return;

        if (InputManager.Instance.PointerOverUI)
            return;

        if (InputManager.Instance.PointerDown)
        {
            Paste(pointerPos);
            ToolManager.Instance.SetTool(ToolType.None);
        }
    }

    /// <summary>
    /// Copies the currently selected object to the clipboard. Only works if the selected object is a MonoBehaviour (i.e. a GameObject in the scene).
    /// </summary>
    public void CopySelected()
    {
        var selected = SelectionManager.Instance.selected;

        if (selected == null)
        {
            Debug.Log("null selected");
            return;
        }

        MonoBehaviour mb = selected as MonoBehaviour;

        if (mb == null)
        {
            Debug.Log("null mb");
            return;
        }


        Debug.Log("copySelected");
        clipboard = mb.gameObject;
    }

    /// <summary>
    /// Pastes the object in the clipboard to the position given if it is position lies on any edge of the base shape. 
    /// If successful, the pasted object is registered to the selection manager and selected. 
    /// If there is no compatible edge, it deselects all objects.
    /// </summary>
    /// <param name="position"></param>
    public void Paste(Vector3 position)
    {
        if (clipboard == null)
            return;

        ISelectable selectable = null;
        foreach(var idx in compatibleEdgeIdxs)
        {
            if (baseShape.IsInEdgeWithIdx(position, idx))
            {
                switch (ToolManager.Instance.CurrentTool)
                {
                    case ToolType.Translate:
                        selectable = baseShape.Translate(clipboard, idx);
                        break;
                    case ToolType.Rotate:
                        selectable = baseShape.Rotate(clipboard, idx);
                        break;
                    case ToolType.Glide:
                        selectable = baseShape.GlideReflect(clipboard, idx);
                        break;
                }
                break;
            }
        }

        if (selectable != null)
        {
            SelectionManager.Instance.Register(selectable);
            SelectionManager.Instance.Deselect();
            SelectionManager.Instance.Select(selectable);
        }
        else
        {
            SelectionManager.Instance.Deselect();
        }
        clipboard = null;
    }

    public bool CanDraw()
    {
        Debug.Log("CanDraw: " + baseShape.DrawnEdges + " " + baseShape.DrawnHalfEdges + " " + baseShape.TotalEdges);
        if (((float)baseShape.DrawnEdges + (float)baseShape.DrawnHalfEdges/2) >= (float)baseShape.TotalEdges / 2)
        {
            Debug.Log("Cannot draw: all edges drawn");
            return false;
        }
        return true;
    }

}
