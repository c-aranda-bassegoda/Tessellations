using UnityEngine;

public class PathManager : MonoBehaviour
{
    public PiecewisePolygonView  polygon;

    private PathPointController currentController;
    private bool isEditing;

    void Update()
    {
        // Only run if we are in node editing mode
        if (ToolManager.Instance.CurrentTool != ToolType.Node &&
            ToolManager.Instance.CurrentTool != ToolType.SharpNode)
            return;

        if (InputManager.Instance.PointerOverUI)
            return;

        Vector2 pointerPos = InputManager.Instance.PointerWorldPos;

        // Try adding a new point
        if (InputManager.Instance.PointerDown)
        {
            currentController = polygon.TryAddPoint(pointerPos,
                ToolManager.Instance.CurrentTool == ToolType.Node);

            if (currentController != null)
            {
                // Tell SplineEditorTool to select this point
                currentController.SetSelected(true);
                isEditing = true;
            }
        }

        // Drag the point
        if (InputManager.Instance.PointerHeld && isEditing && currentController != null)
        {
            currentController.OnDrag(pointerPos);
        }

        // Finish editing
        if (InputManager.Instance.PointerUp && isEditing && currentController != null)
        {
            SelectionManager.Instance.Register(currentController);
            isEditing = false;
            currentController = null;
        }
    }
}

