using UnityEngine;
using UnityEngine.UI;

public class PolygonButton : MonoBehaviour
{
    Button button;
    public Image icon;
    public Color selectedColor = Color.blue;
    public Color normalColor = Color.white;
    private bool selected = false;

    void Awake()
    {
        button = GetComponent<Button>();
        if (icon == null)
            icon = GetComponent<Image>();
    }

    void Start()
    {

    }

    void Update()
    {
        icon.color = selected ? selectedColor : normalColor;
    }

    public void SetSelected(bool isSelected)
    {
        selected = isSelected;
    }

    public void OnClick()
    {
        Debug.Log("Polygon button clicked: " + transform.name);
        PolygonSelectionManager.Instance.ResetButtons();
        selected = true;
        PolygonSelectionManager.Instance.SetPolygon(transform.GetSiblingIndex());
    }
}

