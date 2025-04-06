using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CanvasController : MonoBehaviour
{
    public Texture2D texture;
    [SerializeField] Material mat;
    public Color32[] resetColorArray;
    public Vector2 textureSize = new Vector2(2048, 2048);
    void Start()
    {
        var r = GetComponent<Renderer>();
        texture = new Texture2D((int)textureSize.x, (int)textureSize.y);
        Color32 baseColor = mat.GetColor("_Color"); // Get original color of canvas
        resetColorArray = texture.GetPixels32();
        for (int i = 0; i < resetColorArray.Length; i++)
        {
            resetColorArray[i] = baseColor;
        }
        texture.SetPixels32(resetColorArray); // Can use this line to clear the canvas back to its original state whenever we need to
        texture.Apply();
        mat.SetTexture("_Canvas_Texture", texture); // Sets texture to texture reference in shader graph
        r.material.mainTexture = texture;
    }
}
