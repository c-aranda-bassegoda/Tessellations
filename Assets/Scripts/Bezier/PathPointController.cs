using UnityEngine;
using static UnityEngine.GraphicsBuffer;

using UnityEngine;

public class PathPointController : MonoBehaviour, ISelectable, IDraggable
{
    public BezierPath parentPath;
    public PathPoint point;

    public NodeSelectable anchorView;

    private ISelectionHandler selectionHandler;
    private bool isSelected;

    public void Initialize(BezierPath path, PathPoint modelPoint)
    {
        parentPath = path;
        point = modelPoint;

        // Assign anchorView automatically if not assigned
        if (anchorView == null)
            anchorView = GetComponent<NodeSelectable>();

        if (anchorView != null)
            anchorView.transform.position = point.Position;
        else
            Debug.LogError("PathPointController requires a NodeSelectable on the same GameObject.");

    }

    void Update()
    {
        if (anchorView != null && point != null)
            anchorView.transform.position = point.Position;
    }

    public void SetSelectionHandler(ISelectionHandler handler)
    {
        selectionHandler = handler;
    }

    public bool HitTest(Vector2 worldPoint)
    {
        return anchorView.HitTest(worldPoint);
    }

    public void OnDrag(Vector2 worldPosition)
    {
        point.Position = worldPosition;

        // Notify editor so handles move
        SplineEditorTool.Instance.NotifyPointMoved();
    }

    public void SetSelected(bool selected)
    {
        isSelected = selected;

        anchorView.SetSelected(selected);

        if (selectionHandler != null)
        {
            if (selected)
                selectionHandler.OnSelected();
            else
                selectionHandler.OnDeselected();
        }

        if (selected)
            SplineEditorTool.Instance.Select(point);
    }

    public void Remove()
    {
        parentPath.RemovePoint(point);
        SelectionManager.Instance?.Deregister(this);

        Destroy(anchorView.gameObject);

        SplineEditorTool.Instance.DeselectIf(point);
    }
}