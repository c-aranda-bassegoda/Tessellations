using UnityEngine;

public class EdgeSelectable : LineSelectable
{
    public DerivedPolygon Polygon { get; set; }
    public override void Remove()
    {
        if (Polygon == null) Debug.LogError("edge selectable has null polygon");
        if (line == null) Debug.LogError("edge selectable has null line renderer");
        Polygon.ResetEdge(line);
        base.Remove();
    }
}
