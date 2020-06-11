using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewCursor", menuName = "Cursor", order = 52)]
public class CursorSO : ScriptableObject
{
    public CursorType cursorType;

    public Texture2D cursorTexture;

    public Vector2 offset;
}
