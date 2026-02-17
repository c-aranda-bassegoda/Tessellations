using UnityEngine;

public class PathPointSelectionHandler : ISelectionHandler
{
    private PathPoint point;

    private GameObject handleInVisual;
    private GameObject handleOutVisual;

    public PathPointSelectionHandler(PathPoint point, GameObject handleIn, GameObject handleOut)
    {
        this.point = point;
        this.handleInVisual = handleIn;
        this.handleOutVisual = handleOut;
    }

    public void OnSelected()
    {
        UpdateHandlePositions();
        handleInVisual.SetActive(true);
        handleOutVisual.SetActive(true);
    }

    public void OnDeselected()
    {
        handleInVisual.SetActive(false);
        handleOutVisual.SetActive(false);
    }

    private void UpdateHandlePositions()
    {
        handleInVisual.transform.position = (Vector3)point.anchor.GetPosition() + point.handleInOffset;
        handleOutVisual.transform.position = (Vector3)point.anchor.GetPosition() + point.handleOutOffset;
    }
}
