using UnityEngine;

public class PathManager : MonoBehaviour
{
    public BezierPolygon polygon;
    public GameObject handlePrefab;
    bool isEditing;

    private IPointSelectable currentNode;
    private GameObject handleInVisual;
    private GameObject handleOutVisual;


    void Awake()
    {
        // Instantiate shared handle visuals
        handleInVisual = Instantiate(handlePrefab);
        HandleSelectable handleInSelectable = handleInVisual.GetComponent<HandleSelectable>();
        handleOutVisual = Instantiate(handlePrefab);
        HandleSelectable handleOutSelectable = handleOutVisual.GetComponent<HandleSelectable>();


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
                var handler = new PathPointSelectionHandler(currentNode.SelectedNode, handleInVisual, handleOutVisual);
                currentNode.SetSelectionHandler(handler);
                isEditing = true;
            }
        }

        if (InputManager.Instance.PointerHeld && isEditing)
        {
            currentNode.Move(InputManager.Instance.PointerWorldPos);
        }

        if (InputManager.Instance.PointerUp && isEditing)
        {
            if (currentNode != null)
            {
                SelectionManager.Instance.Register(currentNode);   
            }
            isEditing = false;
        }
    }
}

