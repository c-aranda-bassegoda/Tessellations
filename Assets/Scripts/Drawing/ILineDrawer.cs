using UnityEngine;

public interface ILineDrawer
{
    GameObject StartDrawing(Vector3 startPos);
    void UpdateDrawing(Vector3 currentPos);
    bool EndDrawing(Vector3 endPos);

    void DeleteDrawing();
}

