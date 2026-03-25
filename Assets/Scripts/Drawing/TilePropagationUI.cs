using UnityEngine;
using UnityEngine.UI;

public class TilePropagationUI : MonoBehaviour
{
    [SerializeField] private Button propagateButton;
    [SerializeField] private TilePolygon tilePolygon;

    void Start()
    {
        propagateButton.onClick.AddListener(() => TilePropagator.Propagate(tilePolygon));
    }
}