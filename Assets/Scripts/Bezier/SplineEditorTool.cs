using UnityEngine;

public class SplineEditorTool : MonoBehaviour
{
    public static SplineEditorTool Instance;

    [Header("Handle Prefab")]
    public HandleView handlePrefab;  

    private HandleView handleIn;
    private HandleView handleOut;

    public PathPoint SelectedPoint { get; private set; }

    public bool HasSelection => SelectedPoint != null;

    void Awake()
    {
        Instance = this;

        handleIn = Instantiate(handlePrefab, Vector3.zero, Quaternion.identity, transform);
        handleIn.Initialize(this, true);   // true = isHandleIn

        handleOut = Instantiate(handlePrefab, Vector3.zero, Quaternion.identity, transform);
        handleOut.Initialize(this, false); // false = isHandleOut

        HideHandles();
    }

    public void Select(PathPoint point)
    {
        SelectedPoint = point;

        handleIn.gameObject.SetActive(true);
        handleOut.gameObject.SetActive(true);

        SyncHandlesToModel();
    }
    public void DeselectIf(PathPoint point)
    {
        if (SelectedPoint == point)
        {
            SelectedPoint = null;
            HideHandles();
        }
    }

    void HideHandles()
    {
        handleIn.gameObject.SetActive(false);
        handleOut.gameObject.SetActive(false);
    }

    public void SyncHandlesToModel()
    {
        if (SelectedPoint == null) return;

        handleIn.transform.position = SelectedPoint.HandleInPos;
        handleOut.transform.position = SelectedPoint.HandleOutPos;
    }

    public void NotifyPointMoved()
    {
        SyncHandlesToModel();
    }
}