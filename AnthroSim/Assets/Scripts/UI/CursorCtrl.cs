using UnityEngine;

public class CursorCtrl : MonoBehaviour
{
    public Texture2D cursorTexture;  // Assign this in the Inspector
    public Vector2 hotspot = Vector2.zero;  // Adjust if needed

    void Start()
    {
        Cursor.SetCursor(cursorTexture, hotspot, CursorMode.Auto);
    }
}