using UnityEngine;

public class DrawingManager : MonoBehaviour
{
    [SerializeField] private BaseShape baseShape;
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
            LineSelectable lineSelectable = currentLine?.GetComponent<LineSelectable>();
            if (lineSelectable != null) 
                SelectionManager.Instance.Register(lineSelectable);
            isDrawing = false;
        }
    }

    private void SetupDrawer(ToolType tool)
    {
        switch (tool)
        {
            case ToolType.Pencil:
                activeDrawer = new FreehandDrawingSystem(linePrefab);
                break;

            case ToolType.SnappingPencil:
                var freehand = new FreehandDrawingSystem(linePrefab);
                activeDrawer = new SnapDrawingSystem(freehand, baseShape);
                break;

            default:
                activeDrawer = null;
                break;
        }
    }
}
