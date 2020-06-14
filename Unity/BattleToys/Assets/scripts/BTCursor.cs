using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class BTCursor : MonoBehaviour
{
    [SerializeField] List<CursorSO> cursorSO;

    Dictionary<CursorType, CursorSO> myCursors;

    //static BTCursor _instance;

    //public static BTCursor Instance {get { return _instance;}}

    void Awake()
    {
        //_instance=this;
        InitMyCursorDictionary();
    }

    void InitMyCursorDictionary()
    {
        myCursors=new Dictionary<CursorType, CursorSO>();

        foreach (CursorSO c in cursorSO)
        {
            myCursors.Add(c.cursorType, c);
        }

    }

    // Start is called before the first frame update
    void Start()
    {
        SetCursor(CursorType.Default);
    }

    void OnEnable()
    {
        EventManager.StartListening(EventEnum.CursorDefault, OnCursorDefault);
        EventManager.StartListening(EventEnum.CursorSelect, OnCursorSelect);
        EventManager.StartListening(EventEnum.CursorMove, OnCursorMove);
        EventManager.StartListening(EventEnum.CursorAttack, OnCursorAttack);
    }


    void OnDisable() 
    {
        EventManager.StopListening(EventEnum.CursorDefault, OnCursorDefault);
        EventManager.StopListening(EventEnum.CursorSelect, OnCursorSelect);
        EventManager.StopListening(EventEnum.CursorMove, OnCursorMove);
        EventManager.StopListening(EventEnum.CursorAttack, OnCursorAttack);      
    }

    void OnCursorDefault()
    {
        SetCursor(CursorType.Default);
    }

    void OnCursorSelect()
    {
        SetCursor(CursorType.Select);
    }

    void OnCursorMove()
    {
        SetCursor(CursorType.Move);
    }

    void OnCursorAttack()
    {
        SetCursor(CursorType.Attack);
    }


    void SetCursor(CursorType type )
    {
        CursorSO c;
        
        if (myCursors.TryGetValue(type, out c))
        {
            Cursor.SetCursor(c.cursorTexture, c.offset, CursorMode.Auto);
        }
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