using UnityEngine;

public class DrawingManager : MonoBehaviour
{
    public FreehandDrawingSystem drawingSystem;
    bool isDrawing;

    private GameObject currentLine;

    void Update()
    {
        if (ToolManager.Instance.CurrentTool != ToolType.Pencil)
            return;

        if (InputManager.Instance.PointerOverUI)
            return;

        if (InputManager.Instance.PointerDown)
        {
            currentLine = drawingSystem.StartLine(InputManager.Instance.PointerWorldPos);

            isDrawing = true;
        }

        if (InputManager.Instance.PointerHeld && isDrawing)
        {
            drawingSystem.AddPoint(InputManager.Instance.PointerWorldPos);
        }

        if (InputManager.Instance.PointerUp && isDrawing)
        {
            drawingSystem.EndLine();
            LineSelectable lineSelectable = currentLine?.GetComponent<LineSelectable>();
            if (lineSelectable != null) 
                SelectionManager.Instance.Register(lineSelectable);
            isDrawing = false;
        }
    }
}
