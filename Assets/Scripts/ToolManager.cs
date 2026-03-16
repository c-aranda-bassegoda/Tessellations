using System.Collections.Generic;
using UnityEngine;
public enum ToolType
{
    None,
    Pencil,
    SnappingPencil,
    Select,
    Node,
    SharpNode,
    Copy,
    Translate,
    Delete
}
public class ToolManager : MonoBehaviour
{
    public static ToolManager Instance { get; private set; }

    public ToolType CurrentTool { get; private set; } = ToolType.None;

    public static readonly HashSet<ToolType> toolsRequiringSelection =
    new HashSet<ToolType>
    {
        ToolType.Copy,
        ToolType.Translate,
        ToolType.Delete
    };

    public bool CurrentToolRequiresSelection()
    {
        return toolsRequiringSelection.Contains(CurrentTool);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        Instance = this;
    }

    public void SetTool(ToolType tool)
    {
        if (CurrentTool == tool) return;

        CurrentTool = tool;
        //Debug.Log("Selected tool: " + tool);
    }
}
