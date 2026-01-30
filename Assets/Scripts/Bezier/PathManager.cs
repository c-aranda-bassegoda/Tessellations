using UnityEngine;

public class PathManager : MonoBehaviour
{
    public Polygon polygon;
    bool isEditing;

    private NodeSelectable currentNode;

    void Update()
    {

        if (InputManager.Instance.PointerOverUI)
            return;

        if (InputManager.Instance.PointerDown)
        {
            currentNode = polygon.TryAddPoint(InputManager.Instance.PointerWorldPos);
            isEditing = true;
        }

        if (InputManager.Instance.PointerHeld && isEditing)
        {
            currentNode.move(InputManager.Instance.PointerWorldPos);
        }

        if (InputManager.Instance.PointerUp && isEditing)
        {
            NodeSelectable nodeSelectable = currentNode;
            //if (nodeSelectable != null)
            //    SelectionManager.Instance.Register(nodeSelectable);
            isEditing = false;
        }
    }
}

