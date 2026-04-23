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
    //public bool StartedOverUI { get; private set; }
    //public bool BlockWorldInput { get; private set; }



    Camera mainCamera;

    void Awake()
    {
        Instance = this;
        mainCamera = Camera.main;
    }

    void Update()
    {
        ResetFrameState();

        //HandleMouse();
        HandleTouch();

        //if (PointerDown)
        //{
        //    PointerOverUI = EventSystem.current.IsPointerOverGameObject(Input.touchCount > 0 ? Input.GetTouch(0).fingerId : -1);

        //    StartedOverUI = PointerOverUI;
        //    BlockWorldInput = PointerOverUI;
        //    Debug.Log("BlockWorldInput: " + BlockWorldInput);
        //}

        //if (PointerUp)
        //{
        //    StartedOverUI = false;
        //    BlockWorldInput = false;
        //}
    }

    void ResetFrameState()
    {
        PointerDown = false;
        PointerHeld = false;
        PointerUp = false;
    }

    void HandleMouse()
    {
        PointerDown = Input.GetMouseButtonDown(0);
        PointerHeld = Input.GetMouseButton(0);
        PointerUp = Input.GetMouseButtonUp(0);

        PointerOverUI = EventSystem.current.IsPointerOverGameObject();

        PointerWorldPos = GetWorldPosition(Input.mousePosition);
    }

    void HandleTouch()
    {
        if (Input.touchCount == 0)
            return;

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
