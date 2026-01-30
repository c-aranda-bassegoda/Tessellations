using UnityEngine;

public class PathManager : MonoBehaviour
{
    public Polygon polygon;
    bool isEditing;

    private GameObject currentNode;

    void Update()
    {
        if (ToolManager.Instance.CurrentTool != ToolType.Pencil)
            return;

        if (InputManager.Instance.PointerOverUI)
            return;

        if (InputManager.Instance.PointerDown)
        {
            currentNode = polygon.TryAddPoint(InputManager.Instance.PointerWorldPos);
            isEditing = true;
        }

        if (InputManager.Instance.PointerHeld && isEditing)
        {
            polygon.MovePoint(InputManager.Instance.PointerWorldPos);
        }

        if (InputManager.Instance.PointerUp && isEditing)
        {
            NodeSelectable nodeSelectable = currentNode?.GetComponent<NodeSelectable>();
            if (nodeSelectable != null)
                SelectionManager.Instance.Register(nodeSelectable);
            isEditing = false;
        }
    }
}

