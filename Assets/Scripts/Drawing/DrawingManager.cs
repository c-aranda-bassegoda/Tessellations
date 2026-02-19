using UnityEngine;

public class DrawingManager : MonoBehaviour
{
    [SerializeField] private abstractPolygon baseShape;
    [SerializeField] private GameObject linePrefab;

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
            currentLine = activeDrawer.StartDrawing(pointerPos);
            isDrawing = true;
        }

        if (InputManager.Instance.PointerHeld && isDrawing)
        {
            activeDrawer.UpdateDrawing(pointerPos);
        }

        if (InputManager.Instance.PointerUp && isDrawing)
        {
            activeDrawer.EndDrawing(pointerPos);
            if (currentLine != null)
            {
                LineSelectable lineSelectable = currentLine?.GetComponent<LineSelectable>();
                if (lineSelectable != null)
                    SelectionManager.Instance.Register(lineSelectable);
            }
            currentLine = null;
            isDrawing = false;
        }
    }

    private void SetupDrawer(ToolType tool)
    {
        var freehand = new FreehandDrawingSystem(linePrefab);
        switch (tool)
        {
            case ToolType.Pencil:
                activeDrawer = new InsidePolygonDrawingSystem(freehand, baseShape);
                break;

            case ToolType.SnappingPencil:
                activeDrawer = new SnapDrawingSystem(freehand, baseShape);
                break;

            default:
                activeDrawer = null;
                break;
        }
    }
}
