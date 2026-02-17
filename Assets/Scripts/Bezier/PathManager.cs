using UnityEngine;

public class PathManager : MonoBehaviour
{
    public Polygon polygon;
    bool isEditing;

    private PathPoint currentNode;

    void Update()
    {
        if (ToolManager.Instance.CurrentTool != ToolType.Node && ToolManager.Instance.CurrentTool != ToolType.SharpNode)
            return;

        if (InputManager.Instance.PointerOverUI)
            return;

        if (InputManager.Instance.PointerDown)
        {
            currentNode = polygon.TryAddPoint(InputManager.Instance.PointerWorldPos, (ToolManager.Instance.CurrentTool == ToolType.Node));
            if (currentNode != null)
                isEditing = true;
        }

        if (InputManager.Instance.PointerHeld && isEditing)
        {
            currentNode.MoveAnchor(InputManager.Instance.PointerWorldPos);
        }

        if (InputManager.Instance.PointerUp && isEditing)
        {
            NodeSelectable nodeSelectable = currentNode?.anchor.GetComponent<NodeSelectable>();
            if (nodeSelectable != null)
                SelectionManager.Instance.Register(nodeSelectable);
            isEditing = false;
        }
    }
}

