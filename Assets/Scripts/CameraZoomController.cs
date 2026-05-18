using UnityEngine;

public class CameraZoomController : MonoBehaviour
{
    [SerializeField] Camera cam;

    [SerializeField] float zoomSpeed = 0.01f;
    [SerializeField] float minZoom = 2f;
    [SerializeField] float maxZoom = 20f; 
    
    [SerializeField] float mouseZoomSpeed = 2f;
    [SerializeField] float pinchZoomSpeed = 0.02f;

    void Update()
    {
        float speed = InputManager.Instance.UsingTouch? pinchZoomSpeed : mouseZoomSpeed;
        float zoom = InputManager.Instance.ZoomDelta;

        if (Mathf.Abs(zoom) < 0.01f)
            return;

        cam.orthographicSize -= zoom * zoomSpeed;

        cam.orthographicSize = Mathf.Clamp(
            cam.orthographicSize,
            minZoom,
            maxZoom
        );
    }
}