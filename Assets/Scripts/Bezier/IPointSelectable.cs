using UnityEngine;

public interface IPointSelectable : ISelectable, IDraggable
{
    PathPointSelectable SelectedNode { get; }

    void Move(Vector2 pointerWorldPos);
    public void SetSelectionHandler(ISelectionHandler handler);
}