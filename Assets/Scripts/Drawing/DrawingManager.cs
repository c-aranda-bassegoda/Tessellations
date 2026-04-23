using UnityEngine;

public class DrawingManager : MonoBehaviour
{
    [SerializeField] public Polygon baseShape;
    [SerializeField] private GameObject linePrefab;
    [SerializeField] private GameObject edgePrefab;

    private ILineDrawer activeDrawer;
    private bool isDrawing;

    private GameObject currentLine;

    private ToolType lastTool;

    void Update()
    {
        if (ToolManager.Instance.CurrentTool != lastTool)
        {
            SetupDrawer(ToolManager.Instance.CurrentTool);
            lastTool = ToolManager.Instance.CurrentTool;
        }

        if (ToolManager.Instance.CurrentTool != ToolType.Pencil &&
            ToolManager.Instance.CurrentTool != ToolType.SnappingPencil)
            return;

        if (InputManager.Instance.PointerOverUI)
            return;

        Vector3 pointerPos = InputManager.Instance.PointerWorldPos;

        if (InputManager.Instance.PointerDown)
        {
            currentLine = activeDrawer.StartDrawing(pointerPos, baseShape.transform);
            isDrawing = true;
        }

        if (InputManager.Instance.PointerHeld && isDrawing)
        {
            activeDrawer.UpdateDrawing(pointerPos);
        }

        if (InputManager.Instance.PointerUp && isDrawing)
        {
            if (activeDrawer.EndDrawing(pointerPos) && currentLine != null)
            {
                bool success = false;
                if (ToolType.SnappingPencil == lastTool)
                {
                    success = baseShape.ReplaceEdge(currentLine);
                    if (!success)
                    {
                        Destroy(currentLine);
                        currentLine = null;
                    }
                }
                ISelectable selectable = currentLine?.GetComponent<ISelectable>();
                if (selectable != null)
                    SelectionManager.Instance.Register(selectable);
            }
            currentLine = null;
            isDrawing = false;
        }
    }

    private void SetupDrawer(ToolType tool)
    {
        FreehandDrawingSystem freehand;
        switch (tool)
        {
            case ToolType.Pencil:
                freehand = new FreehandDrawingSystem(linePrefab);
                activeDrawer = new InsidePolygonDrawingSystem(freehand, baseShape);
                break;

            case ToolType.SnappingPencil:
                freehand = new FreehandDrawingSystem(edgePrefab);
                activeDrawer = new SnapDrawingSystem(freehand, (DerivedPolygon) baseShape);
                break;

            default:
                activeDrawer = null;
                break;
        }
    }
}
