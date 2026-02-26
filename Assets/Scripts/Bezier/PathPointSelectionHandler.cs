using UnityEngine;

public class PathPointSelectionHandler : ISelectionHandler
{
    private PathPointSelectable point;

    private GameObject handleInVisual;
    private GameObject handleOutVisual;

    public PathPointSelectionHandler(PathPointSelectable point, GameObject handleIn, GameObject handleOut)
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

        var handleIn = handleInVisual.GetComponent<HandleSelectable>();
        var handleOut = handleOutVisual.GetComponent<HandleSelectable>();
        point.handleInSelectable = handleIn;
        point.handleOutSelectable = handleOut;

        // Assign parent
        handleIn.parentPoint = point;
        handleOut.parentPoint = point;

    }

    public void OnDeselected()
    {

        handleInVisual.SetActive(false);
        handleOutVisual.SetActive(false);

        // Deselect the anchor
        point.anchor.SetSelected(false);
    }

    private void UpdateHandlePositions()
    {
        handleInVisual.transform.position = point.anchor.GetPosition() + point.handleInOffset;
        handleOutVisual.transform.position = point.anchor.GetPosition() + point.handleOutOffset;
    }
}
