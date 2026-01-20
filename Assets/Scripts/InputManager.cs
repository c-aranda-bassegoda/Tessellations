using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class InputManager : MonoBehaviour
{
    public DrawingSystem drawingSystem;
    public Camera mainCamera;

    public bool IsPencil { get; set; }

    private bool isDrawing = false;

    void Update()
    {
        switch (ToolManager.Instance.CurrentTool)
        {
            case ToolType.Pencil:
                HandlePencil();
                break;

            case ToolType.Select:
                HandleSelect();
                break;
        }
    }

    private void HandleSelect()
    {
        if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
        {

        }
    }

    private void HandlePencil()
    {
        // For mouse input
        if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
        {
            Debug.Log("Mouse down");
            Vector3 worldPos = GetWorldPosition(Input.mousePosition);
            drawingSystem.StartLine(worldPos);
            isDrawing = true;
        }

        if (Input.GetMouseButton(0) && isDrawing)
        {
            Vector3 worldPos = GetWorldPosition(Input.mousePosition);
            drawingSystem.AddPoint(worldPos);
        }

        if (Input.GetMouseButtonUp(0) && isDrawing)
        {
            drawingSystem.EndLine();
            isDrawing = false;
        }

        // Touch input
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            Vector3 worldPos = GetWorldPosition(touch.position);

            if (touch.phase == TouchPhase.Began && !EventSystem.current.IsPointerOverGameObject())
            {
                drawingSystem.StartLine(worldPos);
            }
            else if (touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary)
            {
                drawingSystem.AddPoint(worldPos);
            }
            else if (touch.phase == TouchPhase.Ended)
            {
                drawingSystem.EndLine();
            }
        }
    }

    Vector3 GetWorldPosition(Vector3 screenPosition)
    {
        screenPosition.z = 10f; // Distance from camera
        return mainCamera.ScreenToWorldPoint(screenPosition);
    }
}
