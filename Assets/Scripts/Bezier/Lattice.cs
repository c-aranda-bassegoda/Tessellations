using UnityEngine;

public class Lattice : MonoBehaviour
{
    public TessellationPolygon tile;

    void Start()
    {
        tile = GetComponent<TessellationPolygon>();
    }
}
