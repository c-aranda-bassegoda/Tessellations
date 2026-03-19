using UnityEngine;

public class EdgeSelectable : LineSelectable
{
    public DerivedPolygon Polygon { get; set; }

    /// <summary>
    /// Removes the edge from the polygon and deregisters it from the selection manager.
    /// Also resets the line renderer to its default state.
    /// </summary>
    public override void Remove()
    {
        if (Polygon == null) Debug.LogError("edge selectable has null polygon");
        if (line == null) Debug.LogError("edge selectable has null line renderer");
        Polygon.ResetLine(line);
        base.Remove();
    }
}
