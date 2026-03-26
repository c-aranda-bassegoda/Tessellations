using System.Collections.Generic;
using NUnit.Framework;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class PolygonSelectionManager : MonoBehaviour
{
    public static PolygonSelectionManager Instance { get; private set; }

    [SerializeField] public DrawingManager drawingManager;
    [SerializeField] public PathManager pathManager;
    [SerializeField] public GameObject polygonsPanel;
    [SerializeField] public List<GameObject> polygons;
    [SerializeField] public GameObject buttonsPanel;
    [SerializeField] public List<Button> buttons;
    public Color selectedColor = Color.gray;


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (drawingManager == null)
            Debug.LogWarning("DrawingManager reference not set in PolygonSelectionManager. Attempting to find one in the scene.");

        if (pathManager == null)
            Debug.LogWarning("PathManager reference not set in PolygonSelectionManager. Attempting to find one in the scene.");

        if (polygons == null)
            polygons = new List<GameObject>();
        
        if (polygons.Count == 0 && polygonsPanel != null)
        {
            for (int i = 0; i < polygonsPanel.transform.childCount; i++)
            {
                polygons.Add(polygonsPanel.transform.GetChild(i).gameObject);
            }
        }

        if (buttonsPanel != null)
        {
            for (int i = 0; i < buttonsPanel.transform.childCount; i++)
            {
                var button = buttonsPanel.transform.GetChild(i).GetComponent<Button>();
                var polyButton = button.GetComponent<PolygonButton>();
                if (polyButton == null)
                {
                    button.AddComponent<PolygonButton>();
                }
                buttons.Add(button);
            }
        }
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (polygons.Count == 0)
        {
            Debug.LogError("No polygons assigned to PolygonSelectionManager. Please assign at least one polygon in the inspector.");
            return;
        }

        for (int i = 0; i < buttonsPanel.transform.childCount; i++)
        {
            var button = buttonsPanel.transform.GetChild(i).GetComponent<Button>();
            var polyButton = button.GetComponent<PolygonButton>();
            button.onClick.AddListener(() => polyButton.OnClick());
            polyButton.selectedColor = this.selectedColor;
        }

        ResetPolygons();
        buttons[0].GetComponent<PolygonButton>().SetSelected(true);
        polygons[0].SetActive(true);
        drawingManager.baseShape = polygons[0].GetComponent<BezierPolygon>();
        pathManager.polygon = polygons[0].GetComponent<BezierPolygon>();
    }

    public void SetPolygon(int idx)
    {
        if (idx < 0 || idx >= polygons.Count)
        {
            Debug.LogError($"Index {idx} is out of bounds for the list of polygons.");
            return;
        }
        ResetPolygons();
        polygons[idx].SetActive(true);
        drawingManager.baseShape = polygons[idx].GetComponent<BezierPolygon>();
        pathManager.polygon = polygons[idx].GetComponent<BezierPolygon>();
    }

    private void ResetPolygons()
    {
        SelectionManager.Instance.ClearAll();
        foreach (var poly in polygons)
        {
            poly.SetActive(false);
        }
    }
    public void ResetButtons()
    {
        foreach (var button in buttons)
        {
            button.GetComponent<PolygonButton>().SetSelected(false);
        }
    }
}
