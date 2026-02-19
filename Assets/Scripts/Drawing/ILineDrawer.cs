using UnityEngine;

public interface ILineDrawer
{
    GameObject StartLine(Vector3 startPos);
    void UpdateDrawing(Vector3 currentPos);
    void EndLine();
}
