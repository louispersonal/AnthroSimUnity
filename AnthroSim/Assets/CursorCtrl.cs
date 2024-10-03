using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CursorCtrl : MonoBehaviour
{
    [SerializeField]
    Texture2D _cursorTexture;

    public Vector2 hotSpot = Vector2.zero;  // The "click point" on the cursor image
    public CursorMode cursorMode = CursorMode.Auto;

    // Start is called before the first frame update
    void Start()
    {
        Cursor.SetCursor(_cursorTexture, hotSpot, cursorMode);
    }
}
