using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class HandleSelectable : NodeSelectable
{
    public PathPointSelectable parentPoint;

    public override void Move(Vector2 position)
    {
        base.Move(position);
        parentPoint.UpdateHandlePosition(this, position);
    }

    // Used internally so we don't re-trigger offset logic
    public void MoveWithoutNotify(Vector2 position)
    {
        base.Move(position);
    }



    public override void SetSelected(bool selected)
    {
        node.color = selected ? Color.blue : Color.red;
    }
}
