using UnityEngine;
public enum ToolType
{
    None,
    Pencil,
    Select
}
public class ToolManager : MonoBehaviour
{
    public static ToolManager Instance { get; private set; }

    public ToolType CurrentTool { get; private set; } = ToolType.None;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Instance = this;
    }

    public void SetTool(ToolType tool)
    {
        if (CurrentTool == tool) return;

        CurrentTool = tool;
        Debug.Log("Selected tool: " + tool);
    }
}
