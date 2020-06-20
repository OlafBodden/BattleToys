using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
public class UnitBase : NetworkBehaviour
{
    BTObject myUnit;
    Animator myAnimator;

    public GameObject myUnitPrefab;

    public Transform landingPosition;

    public float openRange=10f;

    OpenCloseState openCloseState=OpenCloseState.Nothing;

    public void Start()
    {
        myAnimator=this.GetComponent<Animator>();

        if (base.hasAuthority)
        {

            GameObject go=GameObject.Instantiate(myUnitPrefab, this.transform.position + landingPosition.localPosition, this.transform.rotation);

            NetworkServer.Spawn(go);

            myUnit=go.GetComponent<BTObject>();

            
        }

        Close();
    }

    public void Init()
    {
        
    }

    void Update()
    {
        if (CheckIfMyUnitIsInRange())
        {
            Open();
        } else
        {
            Close();
        }
    }

    bool CheckIfMyUnitIsInRange()
    {
        if (myUnit==null) return false;

        return  (Vector3.Distance(this.transform.position, myUnit.transform.position)<openRange);
    }

    void Open()
    {
        if (openCloseState==OpenCloseState.Open) return;
        myAnimator.SetTrigger("Open");
        openCloseState=OpenCloseState.Open;
    }

    void Close()
    {
        if (openCloseState==OpenCloseState.Close) return;
        myAnimator.SetTrigger("Close");
        openCloseState=OpenCloseState.Close;
    }
    
}

public enum OpenCloseState
{
    Nothing
    ,Open
    ,Close
}
