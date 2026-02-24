using UnityEngine;

/// <summary>
/// Controls a single point in a BezierPath.
/// </summary>
public class PathPointController : MonoBehaviour, ISelectable, IDraggable
{
    public BezierPath parentPath;
    public PathPoint point;
    public PathView parentPathView;

    public NodeSelectable anchorView;

    private ISelectionHandler selectionHandler;
    private bool isSelected;

    public void Initialize(BezierPath path, PathPoint modelPoint, PathView pathView)
    {
        parentPath = path;
        point = modelPoint;
        parentPathView = pathView;

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

    public void OnDrag(Vector2 worldPos)
    {
        // Only moves the anchor
        point.Position = worldPos;

        GlobalHandleController.Instance.UpdateHandlePositions();
        parentPathView?.UpdateView();
    }

    public void SetSelected(bool selected)
    {
        isSelected = selected;

        anchorView.SetSelected(selected);
        selectionHandler?.OnSelected();
        if (!selected) selectionHandler?.OnDeselected();

        if (selected)
            GlobalHandleController.Instance.Select(this);
    }

    public void Remove()
    {
        parentPath.RemovePoint(point);
        SelectionManager.Instance?.Deregister(this);

        if (anchorView != null)
            Destroy(anchorView.gameObject);

        parentPathView?.UpdateView();
    }
}