using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class HandleSelectable : NodeSelectable
{
    public PathPoint parentPoint;
    public bool isHandleIn; 
    public HandleSelectable oppositeHandle;

    public override void Move(Vector2 position)
    {
        base.Move(position); // move the GameObject

        // Update the parent offset
        Vector2 anchorPos = parentPoint.anchor.GetPosition();
        Vector2 offset = position - anchorPos;

        if (isHandleIn)
            parentPoint.handleInOffset = offset;
        else
            parentPoint.handleOutOffset = offset;

        // Move opposite handle in mirrored direction
        if (oppositeHandle != null)
        {
            Vector2 mirroredOffset = -offset;
            oppositeHandle.transform.position = anchorPos + mirroredOffset;

            if (isHandleIn)
                parentPoint.handleOutOffset = mirroredOffset;
            else
                parentPoint.handleInOffset = mirroredOffset;
        }
    }

    public override void SetSelected(bool selected)
    {
        node.color = selected ? Color.blue : Color.red;
    }
}
