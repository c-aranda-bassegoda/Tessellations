using UnityEngine;

public class PathPoint
{
    public Vector2 Position;
    public Vector2 HandleInOffset;
    public Vector2 HandleOutOffset;
    public bool Smooth;

    public Vector2 HandleInPos => Position + HandleInOffset;
    public Vector2 HandleOutPos => Position + HandleOutOffset;

    public PathPoint(Vector2 position)
    {
        Position = position;
        HandleInOffset = Vector2.zero;
        HandleOutOffset = Vector2.zero;
    }
}