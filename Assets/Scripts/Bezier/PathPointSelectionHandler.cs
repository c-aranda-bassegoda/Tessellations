using UnityEngine;

/// <summary>
/// Handles selection of a PathPointController and shows global handles.
/// </summary>
public class PathPointSelectionHandler : ISelectionHandler
{
    private PathPointController controller;

    public PathPointSelectionHandler(PathPointController controller)
    {
        this.controller = controller;
    }

    public void OnSelected()
    {
        if (controller == null) return;

        // Select this controller in the global handle system
        GlobalHandleController.Instance.Select(controller);

        // Also visually select the anchor
        controller.anchorView.SetSelected(true);
    }

    public void OnDeselected()
    {
        if (controller == null) return;

        // Hide global handles
        GlobalHandleController.Instance.Deselect();

        // Deselect the anchor visually
        controller.anchorView.SetSelected(false);
    }
}