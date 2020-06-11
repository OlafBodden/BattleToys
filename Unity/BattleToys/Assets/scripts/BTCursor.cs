using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class BTCursor : MonoBehaviour
{
    [SerializeField] List<CursorSO> cursorSO;

    static BTCursor _instance;

    public static BTCursor Instance {get { return _instance;}}

    void Awake()
    {
        _instance=this;
    }

    // Start is called before the first frame update
    void Start()
    {
        Cursor.SetCursor(cursorSO[0].cursorTexture, new Vector2(50, 50), CursorMode.Auto);
    }

    public void SetCursorType(CursorType ct)
    {
        
    }


}

public enum CursorType
{
    Default
    ,Select
    ,Move
    ,Attack
    ,Grab

}