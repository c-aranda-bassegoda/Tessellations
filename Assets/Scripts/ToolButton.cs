using UnityEngine;
using UnityEngine.UI;

public class ToolButton : MonoBehaviour
{
    public ToolType toolType;
    public Image icon;
    public Color selectedColor = Color.blue;
    public Color normalColor = Color.white;
    private bool selected = false;

    void Update()
    {
        selected = ToolManager.Instance.CurrentTool == toolType;
        icon.color = selected ? selectedColor : normalColor;
    }

    public void OnClick()
    {
        if (selected)
        {
            selected = false;
            ToolManager.Instance.SetTool(ToolType.None);
        } else
        {
            selected = true;
            ToolManager.Instance.SetTool(toolType);
        }
    }
}
