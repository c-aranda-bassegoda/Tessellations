using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class HandleSelectable : NodeSelectable
{
    public PathPointSelectable parentPoint;
    public bool isHandleIn; 
    public HandleSelectable oppositeHandle;

    public override void Move(Vector2 position)
    {
        MoveInternal(position, true);
    }

    public void MoveWithoutMirror(Vector2 position)
    {
        MoveInternal(position, false);
    }

    private void MoveInternal(Vector2 position, bool mirror)
    {
        base.Move(position);

        Vector2 anchorPos = parentPoint.anchor.GetPosition();
        Vector2 offset = position - anchorPos;

        if (isHandleIn)
            parentPoint.handleInOffset = offset;
        else
            parentPoint.handleOutOffset = offset;

        if (mirror && oppositeHandle != null)
        {
            Vector2 mirroredOffset = -offset;

            if (isHandleIn)
                parentPoint.handleOutOffset = mirroredOffset;
            else
                parentPoint.handleInOffset = mirroredOffset;

            oppositeHandle.MoveWithoutMirror(anchorPos + mirroredOffset);
        }

    }


    public override void SetSelected(bool selected)
    {
        node.color = selected ? Color.blue : Color.red;
    }
}
