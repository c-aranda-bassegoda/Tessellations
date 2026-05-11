using System.Collections.Generic;
using UnityEngine;

public class MaskTool : MonoBehaviour
{
    [SerializeField] public Polygon polygon;

    // Texture to mask
    [SerializeField] private Sprite sourceSprite;

    public void CreateMask()
    {
        if (polygon == null || sourceSprite == null)
        {
            Debug.LogError("Missing polygon or sprite.");
            return;
        }

        // Create a game object for the sprite
        GameObject spriteObj = new GameObject("MaskedSprite");
        SpriteRenderer sr = spriteObj.AddComponent<SpriteRenderer>();
        // Set parent to polygon for organization
        spriteObj.transform.SetParent(polygon.transform);

        // set the sprite to the source sprite
        sr.sprite = sourceSprite;

        // resize to match the polygon using min and max of vertices
        float minX = float.MaxValue;
        float maxX = float.MinValue;
        float minY = float.MaxValue;
        float maxY = float.MinValue;
        for (int i = 0; i < polygon.Vertices.Count; i++)
        {
            Vector2 pos = polygon.Vertices[i].Position;
            if (pos.x < minX) minX = pos.x;
            if (pos.x > maxX) maxX = pos.x;
            if (pos.y < minY) minY = pos.y;
            if (pos.y > maxY) maxY = pos.y;
        }
        // resize the sprite to fit the bounding box of the polygon
        float width = maxX - minX;
        float height = maxY - minY;
        //Debug.Log($"Bounding box: ({minX}, {minY}) to ({maxX}, {maxY}), width: {width}, height: {height}");
        //Debug.Log($"Source sprite size: {sourceSprite.bounds.size}");

        //spriteObj.transform.localScale = new Vector3(0.3f, 0.3f, 1);
        spriteObj.transform.localScale = new Vector3(width / (float)sourceSprite.bounds.size.x, height / (float)sourceSprite.bounds.size.y, 1);


    }
}