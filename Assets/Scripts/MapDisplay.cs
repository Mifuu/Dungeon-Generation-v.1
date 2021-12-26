using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapDisplay : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;

    public void DrawPixelMap(int width, int height, Color[] colorMap) {
        Texture2D texture = new Texture2D(width, height);

        //color map flip vertical
        colorMap = FlipVertical(width, height, colorMap);

        texture.SetPixels (colorMap);
        texture.filterMode = FilterMode.Point;
        texture.Apply ();

        spriteRenderer.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0,0), width);
    }

    private Color[] FlipVertical(int width, int height, Color[] colorMap) {
        List<Color> outputList = new List<Color>();
        for (int y=height-1;y>=0;y--) {
            for (int x=0;x<width;x++) {
                outputList.Add(colorMap[y*width+x]);
            }
        }
        return outputList.ToArray();
    }
}