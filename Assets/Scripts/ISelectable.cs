using UnityEngine;

public interface ISelectable
{
    bool HitTest(Vector2 worldPoint);
    void SetSelected(bool selected);
}

