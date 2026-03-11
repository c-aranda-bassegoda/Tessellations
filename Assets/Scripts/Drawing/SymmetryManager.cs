using UnityEngine;

public class SymmetryManager : MonoBehaviour
{
    [SerializeField] private TilePolygon baseShape;

    public static SymmetryManager Instance { get; private set; }

    GameObject clipboard;

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
            if (currentTool != ToolType.Translate)
                baseShape.DehighlightEdges();

            else
            {
                CopySelected();
                LineSelectable line = clipboard.GetComponent<LineSelectable>();
                baseShape.HighlightEdges(baseShape.FindCompatibleEdges(line), Color.red);
            }

            lastTool = currentTool;
        }

        if (ToolManager.Instance.CurrentTool != ToolType.Translate)
            return;

        if (InputManager.Instance.PointerOverUI)
            return;

        if (InputManager.Instance.PointerDown)
        {
            Paste(pointerPos);
            ToolManager.Instance.SetTool(ToolType.None);
        }
    }

    public void CopySelected()
    {
        var selected = SelectionManager.Instance.selected;

        if (selected == null)
            return;

        MonoBehaviour mb = selected as MonoBehaviour;

        if (mb == null)
            return;

        clipboard = mb.gameObject;
    }

    public void Paste(Vector3 position)
    {
        if (clipboard == null)
            return;

        ISelectable selectable = baseShape.TryPaste(clipboard, position);

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

}
