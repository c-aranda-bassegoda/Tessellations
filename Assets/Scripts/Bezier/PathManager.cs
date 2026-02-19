using UnityEngine;

public class PathManager : MonoBehaviour
{
    public Polygon polygon;
    public GameObject handlePrefab;
    bool isEditing;

    private PathPointSelectable currentNode;
    private GameObject handleInVisual;
    private GameObject handleOutVisual;


    void Awake()
    {
        // Instantiate shared handle visuals
        handleInVisual = Instantiate(handlePrefab);
        HandleSelectable handleInSelectable = handleInVisual.GetComponent<HandleSelectable>();
        handleInSelectable.isHandleIn = true;
        handleOutVisual = Instantiate(handlePrefab);
        HandleSelectable handleOutSelectable = handleOutVisual.GetComponent<HandleSelectable>();
        handleOutSelectable.isHandleIn = false;

        handleInSelectable.oppositeHandle = handleOutSelectable;
        handleOutSelectable.oppositeHandle = handleInSelectable;

        handleInVisual.SetActive(false);
        handleOutVisual.SetActive(false);
    }

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
            {
                var handler = new PathPointSelectionHandler(currentNode, handleInVisual, handleOutVisual);
                currentNode.SetSelectionHandler(handler);
                isEditing = true;
            }
        }

        if (InputManager.Instance.PointerHeld && isEditing)
        {
            currentNode.MoveAnchor(InputManager.Instance.PointerWorldPos);
        }

        if (InputManager.Instance.PointerUp && isEditing)
        {
            //NodeSelectable nodeSelectable = currentNode?.anchor.GetComponent<NodeSelectable>();
            if (currentNode != null)
            {
                SelectionManager.Instance.Register(currentNode);   
            }
            isEditing = false;
        }
    }
}

