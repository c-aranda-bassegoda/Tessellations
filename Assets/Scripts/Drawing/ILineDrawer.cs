using UnityEngine;

public interface ILineDrawer
{
    GameObject StartDrawing(Vector3 startPos, Transform parent);
    void UpdateDrawing(Vector3 currentPos);
    bool EndDrawing(Vector3 endPos);

    void DeleteDrawing();
}

