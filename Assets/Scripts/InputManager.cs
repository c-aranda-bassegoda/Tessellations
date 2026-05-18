using System;
using UnityEngine;
using UnityEngine.EventSystems;

// Deals with input only, knows nothong about workings of the application
public class InputManager : MonoBehaviour
{
    public static InputManager Instance;

    public bool PointerDown { get; private set; }
    public bool PointerHeld { get; private set; }
    public bool PointerUp { get; private set; }
    public Vector2 PointerWorldPos { get; private set; }

    public bool PointerOverUI { get; private set; }

    public float ZoomDelta { get; private set; }
    public bool UsingTouch { get; private set; }


    Camera mainCamera;

    void Awake()
    {
        Instance = this;
        mainCamera = Camera.main; 
        Input.simulateMouseWithTouches = false;
    }

    void Update()
    {
        ResetFrameState();

        if (Input.touchCount > 0)
        {
            HandleTouch();
        }
        else
        {
            HandleMouse();
        }
    }

    void ResetFrameState()
    {
        PointerDown = false;
        PointerHeld = false;
        PointerUp = false;
        PointerOverUI = false;

        ZoomDelta = 0f;
    }

    void HandleMouse()
    {
        PointerDown = Input.GetMouseButtonDown(0);
        PointerHeld = Input.GetMouseButton(0);
        PointerUp = Input.GetMouseButtonUp(0);

        PointerOverUI = EventSystem.current.IsPointerOverGameObject();

        PointerWorldPos = GetWorldPosition(Input.mousePosition);

        ZoomDelta = Input.mouseScrollDelta.y;
    }

    void HandleTouch()
    {
        if (Input.touchCount == 0)
            return;

        if (Input.touchCount >= 2)
        {
            Touch touch0 = Input.GetTouch(0);
            Touch touch1 = Input.GetTouch(1);

            Vector2 prevPos0 = touch0.position - touch0.deltaPosition;
            Vector2 prevPos1 = touch1.position - touch1.deltaPosition;

            float prevDistance = Vector2.Distance(prevPos0, prevPos1);
            float currentDistance = Vector2.Distance(touch0.position, touch1.position);

            ZoomDelta = currentDistance - prevDistance;

            return; // Don't process further touches if we're zooming
        }

        Touch touch = Input.GetTouch(0);

        PointerDown = touch.phase == TouchPhase.Began;
        PointerHeld = touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary;
        PointerUp = touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled;

        PointerOverUI = EventSystem.current.IsPointerOverGameObject(touch.fingerId);

        PointerWorldPos = GetWorldPosition(touch.position);
    }
    Vector3 GetWorldPosition(Vector3 screenPosition)
    {
        screenPosition.z = 10f; // Distance from camera
        return mainCamera.ScreenToWorldPoint(screenPosition);
    }
}
