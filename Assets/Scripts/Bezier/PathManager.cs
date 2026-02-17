using UnityEngine;

public class PathManager : MonoBehaviour
{
    public Polygon polygon;
    bool isEditing;

    private GameObject currentNode;

    void Update()
    {
        if (ToolManager.Instance.CurrentTool != ToolType.Node)
            return;

        if (InputManager.Instance.PointerOverUI)
            return;

        if (InputManager.Instance.PointerDown)
        {
            currentNode = polygon.TryAddPoint(InputManager.Instance.PointerWorldPos, false);
            isEditing = true;
        }

        if (InputManager.Instance.PointerHeld && isEditing)
        {
            currentNode?.GetComponent<NodeSelectable>().Move(InputManager.Instance.PointerWorldPos);
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

