using UnityEngine;

public class GlobalHandleController : MonoBehaviour
{
    public static GlobalHandleController Instance;

    [Header("Handle Prefab")]
    public HandleView handlePrefab;   // assign in inspector

    private HandleView handleInVisual;
    private HandleView handleOutVisual;

    private PathPointController selectedController;

    private void Awake()
    {
        Instance = this;

        // Spawn the global handles at runtime
        handleInVisual = Instantiate(handlePrefab, Vector3.zero, Quaternion.identity, transform);
        handleInVisual.Initialize(true);
        handleInVisual.gameObject.SetActive(false);

        handleOutVisual = Instantiate(handlePrefab, Vector3.zero, Quaternion.identity, transform);
        handleOutVisual.Initialize(false);
        handleOutVisual.gameObject.SetActive(false);
    }
    private void Start()
    {
        // **Register handles in the SelectionManager**
        if (SelectionManager.Instance != null)
        {
            SelectionManager.Instance.Register(handleInVisual);
            SelectionManager.Instance.Register(handleOutVisual);
        } else
        {
            Debug.LogError("Handles were not registered as selectables");
        }
    }

    public void Select(PathPointController controller)
    {
        selectedController = controller;

        if (controller == null)
        {
            handleInVisual.gameObject.SetActive(false);
            handleOutVisual.gameObject.SetActive(false);
            return;
        }

        handleInVisual.parentController = controller;
        handleOutVisual.parentController = controller;

        UpdateHandlePositions();

        handleInVisual.gameObject.SetActive(true);
        handleOutVisual.gameObject.SetActive(true);
    }

    public void UpdateHandlePositions()
    {
        if (selectedController == null || selectedController.point == null) return;

        Vector2 anchorPos = selectedController.point.Position;

        handleInVisual.transform.position = anchorPos + selectedController.point.HandleInOffset;
        handleOutVisual.transform.position = anchorPos + selectedController.point.HandleOutOffset;
    }

    public void MoveHandle(HandleView handle, Vector2 worldPos)
    {
        if (handle.parentController == null || handle.parentController.point == null) return;

        var point = handle.parentController.point;
        Vector2 anchorPos = point.Position;
        Vector2 offset = worldPos - anchorPos;

        if (handle.isHandleIn)
        {
            point.HandleInOffset = offset;
            if (point.Smooth) point.HandleOutOffset = -offset;
        }
        else
        {
            point.HandleOutOffset = offset;
            if (point.Smooth) point.HandleInOffset = -offset;
        }

        UpdateHandlePositions();
        handle.parentController.parentPathView?.UpdateView();
    }

    public void Deselect()
    {
        selectedController = null;
        handleInVisual.gameObject.SetActive(false);
        handleOutVisual.gameObject.SetActive(false);
    }
}