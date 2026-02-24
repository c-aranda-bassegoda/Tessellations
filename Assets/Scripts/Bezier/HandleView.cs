using UnityEngine;

public class HandleView : NodeSelectable
{
    public bool isHandleIn;

    private SplineEditorTool editor;

    public void Initialize(SplineEditorTool editor, bool isIn)
    {
        this.editor = editor;
        this.isHandleIn = isIn;
    }

    public override void Move(Vector2 position)
    {
        if (!editor.HasSelection)
            return;

        MoveInternal(position, true);
    }

    public void MoveWithoutMirror(Vector2 position)
    {
        MoveInternal(position, false);
    }

    private void MoveInternal(Vector2 position, bool mirror)
    {
        var point = editor.SelectedPoint;
        if (point == null) return;

        Vector2 anchorPos = point.Position;
        Vector2 offset = position - anchorPos;

        if (isHandleIn)
            point.HandleInOffset = offset;
        else
            point.HandleOutOffset = offset;

        if (mirror && point.Smooth)
        {
            Vector2 mirrored = -offset;

            if (isHandleIn)
                point.HandleOutOffset = mirrored;
            else
                point.HandleInOffset = mirrored;
        }

        editor.NotifyPointMoved();
    }

    public override void SetSelected(bool selected)
    {
        node.color = selected ? Color.blue : Color.red;
    }
}
