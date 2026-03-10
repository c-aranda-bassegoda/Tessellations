using UnityEngine;

public class CopyPasteManager : MonoBehaviour
{
    [SerializeField] private DerivedPolygon baseShape;

    public static CopyPasteManager Instance { get; private set; }

    GameObject clipboard;
    GameObject previewObject;

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

        UpdatePreview(pointerPos);

        // tool changed
        if (currentTool != lastTool)
        {
            if (currentTool != ToolType.Copy)
                DestroyPreview();

            else
            {
                CopySelected();
                CreatePreview(pointerPos);
            }

            lastTool = currentTool;
        }

        if (ToolManager.Instance.CurrentTool != ToolType.Copy)
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

    void CreatePreview(Vector3 pointerPos)
    {
        if (clipboard == null)
            return;


        DestroyPreview();

        previewObject = Instantiate(clipboard);

        previewDraggable = previewObject.GetComponent<ITransformable>();

        previewDraggable?.OnTransform(pointerPos);

        SetPreviewVisual(previewObject);
    }

    void UpdatePreview(Vector3 position)
    {
        previewDraggable?.OnTransform(position);
    }

    void DestroyPreview()
    {
        if (previewObject != null)
            Destroy(previewObject);

        previewDraggable = null;
    }

    void SetPreviewVisual(GameObject obj)
    {
        // make line renderers transparent
        LineRenderer[] lines = obj.GetComponentsInChildren<LineRenderer>();

        foreach (var lr in lines)
        {
            Color c = lr.startColor;
            c.a = 0.3f;

            lr.startColor = c;
            lr.endColor = c;
        }
    }
}
