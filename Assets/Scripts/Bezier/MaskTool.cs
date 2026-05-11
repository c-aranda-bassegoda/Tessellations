using System.Collections.Generic;
using UnityEngine;

public class MaskTool : MonoBehaviour
{
    [SerializeField] public Polygon polygon;

    // Texture to mask
    [SerializeField] private Sprite sourceSprite;

    private GameObject maskObject;

    private bool isMaskActive = false;

    public void ToggleMaskObj()
    {
        if (isMaskActive)
        {
            ActivateMaskObj(false);
        }
        else
        {
            ActivateMaskObj(true);
        }
    }
    public void ActivateMaskObj(bool active)
    {
        if (maskObject != null)
        {
            Destroy(maskObject);
            if (active)
            {
                CreateMask();
            }
        }
        else if (active)
        {
            CreateMask();
        }

        isMaskActive = active;
    }

    public void CreateMask()
    {
        if (polygon == null || sourceSprite == null)
        {
            Debug.LogError("Missing polygon or sprite.");
            return;
        }

        // Create a game object for the sprite
        maskObject = new GameObject("MaskedSprite");
        SpriteRenderer sr = maskObject.AddComponent<SpriteRenderer>();
        // Set parent to polygon for organization
        maskObject.transform.SetParent(polygon.transform);

        // set the sprite to the source sprite
        sr.sprite = sourceSprite;

        // resize the sprite to fit the bounding box of the polygon
        (float width, float height, Vector2 bottomCorner, Vector2 topCorner) = polygon.GetBoundingBox();
        Debug.Log($"Bounding box: width: {width}, height: {height}");
        Debug.Log($"Source sprite size: {sourceSprite.bounds.size}");

        //spriteObj.transform.localScale = new Vector3(0.3f, 0.3f, 1);
        maskObject.transform.localScale = new Vector3(width / (float)sourceSprite.bounds.size.x, height / (float)sourceSprite.bounds.size.y, 1);

        // Offset the sprite by bounding box center
        maskObject.transform.position = new Vector3((bottomCorner.x + topCorner.x) / 2, (bottomCorner.y + topCorner.y) / 2, 0);
    }
}