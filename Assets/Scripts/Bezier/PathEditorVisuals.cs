using UnityEngine;

public class PathEditorVisuals : MonoBehaviour
{
    public GameObject handleInVisual;
    public GameObject handleOutVisual;

    public void ShowHandles(PathPoint point)
    {
        handleInVisual.transform.position = point.HandleInPos;
        handleOutVisual.transform.position = point.HandleOutPos;

        handleInVisual.SetActive(true);
        handleOutVisual.SetActive(true);
    }

    public void HideHandles()
    {
        handleInVisual.SetActive(false);
        handleOutVisual.SetActive(false);
    }
}
