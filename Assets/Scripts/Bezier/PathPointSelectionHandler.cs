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

        point.handleInSelectable = handleInVisual.GetComponent<HandleSelectable>();
        point.handleOutSelectable = handleOutVisual.GetComponent<HandleSelectable>();
        //var handleIn = handleInVisual.GetComponent<HandleSelectable>();
        //var handleOut = handleOutVisual.GetComponent<HandleSelectable>();

        //// Assign parent
        //handleIn.parentPoint = point;
        //handleOut.parentPoint = point;

        //// Make handles aware of each other for mirroring
        //handleIn.oppositeHandle = handleOut;
        //handleOut.oppositeHandle = handleIn;

        //// Register handles so they can be selected and dragged
        //SelectionManager.Instance.Register(handleIn);
        //SelectionManager.Instance.Register(handleOut);
    }

    public void OnDeselected()
    {

        handleInVisual.SetActive(false);
        handleOutVisual.SetActive(false);

        //var handleIn = handleInVisual.GetComponent<HandleSelectable>();
        //var handleOut = handleOutVisual.GetComponent<HandleSelectable>();

        //SelectionManager.Instance.Deregister(handleIn);
        //SelectionManager.Instance.Deregister(handleOut);

        //// Deselect the anchor
        //point.anchor.SetSelected(false);
    }

    private void UpdateHandlePositions()
    {
        handleInVisual.transform.position = (Vector3)point.anchor.GetPosition() + point.handleInOffset;
        handleOutVisual.transform.position = (Vector3)point.anchor.GetPosition() + point.handleOutOffset;
    }
}
