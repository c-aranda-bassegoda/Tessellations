using UnityEngine;

public class EdgeSelectable : LineSelectable
{
    public DerivedPolygon Polygon { get; set; }
    public EdgeSelectable SymmEdge { get; set; }

    /// <summary>
    /// Removes the edge from the polygon and deregisters it from the selection manager.
    /// Also resets the line renderer to its default state.
    /// </summary>
    public override void Remove()
    {
        if (Polygon == null) Debug.LogError("edge selectable has null polygon");
        if (line == null) Debug.LogError("edge selectable has null line renderer");
        Polygon.ResetLine(line);
        if (SymmEdge != null)
        {
            SymmEdge.SymmEdge = null;
            SymmEdge.Remove();
        }
        else
        {
            Polygon.DrawnEdges--;
        }
        base.Remove();
    }

    public override void SetSelected(bool selected)
    {
        base.SetSelected(selected);
        if (SymmEdge != null)
        {
            SymmEdge.line.startColor = selected ? Color.blue : Color.black;
            SymmEdge.line.endColor = selected ? Color.blue : Color.black;
        }
    }
}
