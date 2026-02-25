using UnityEngine;

public abstract class BezierPolygon : Polygon
{
    public virtual IPointSelectable TryAddPoint(Vector2 pointerWorldPos, bool smooth)
    {
        throw new System.NotImplementedException();
    }
}
