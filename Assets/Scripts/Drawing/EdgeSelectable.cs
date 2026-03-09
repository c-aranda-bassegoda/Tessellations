using UnityEngine;

public class EdgeSelectable : LineSelectable
{
    public DerivedPolygon Polygon { get; set; }
    public override void Remove()
    {
        Polygon.ResetEdge(line);
        base.Remove();
    }
}
