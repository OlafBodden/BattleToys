using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class UnitBase : NetworkBehaviour
{
    BTObject btObject;
    BTObject myUnit;
    Animator myAnimator;

    public GameObject myUnitPrefab;           //Prefab of unit (heli, plane,...)

    public Transform landingPosition;         //Each Base has to have a dedicated landing position

    [SerializeField] private float openRange=10f;               //If our unit is insied this range, base is opened, otherwise closed

    [SerializeField] private int numberUnitsInStock=1;          //IF >1, units can be recreated after they died --> ToDo: need some UI

    [SerializeField] private float unitConstructionTime = 5f;   //Time that the base needs to create a new unit

    [SerializeField] private float reloadTime = 2f;             //Time that the base needs to reload the unit (with rockets/bombs...)

    OpenCloseState openCloseState=OpenCloseState.Nothing;

    bool inConstruction=false;                                  //Is true, while base is construction a new unit

    bool inReloading = false;

    
    /// <summary>
    /// Called by BTOject to initialize the base
    /// </summary>
    /// <param name="baseBTObject"></param>
    public void Init(BTObject baseBTObject)
    {
        this.btObject = btObject;
    }

    
    public void Start()
    {
        myAnimator=this.GetComponent<Animator>();

        //Close the base 
        if (base.hasAuthority) CmdClose();
    }

    void Update()
    {
        //If we are not the owner of the base, there is nothing to do
        if (!base.hasAuthority) return;

        if (inConstruction) return; //if base is creating a new unit, we have nothing to do here.

        //Check, if we have an unit and if it is still alive
        if (HasUnit())
        {
            if (inReloading)
            {
                //do nothing
            } 
            else if (CheckIfMyUnitIsAtBase())        //If Unit is at base 
            {
                if (myUnit.NeedsToReload())          //If unit is at base and needs to be reloaded
                {
                    //Tell the server to Close base
                    if (openCloseState == OpenCloseState.Open) CmdClose();

                    //Tell the server to reload our unit and reopen base again (after reload-time)
                    Reload();
                }
            }
            else if (CheckIfMyUnitIsInRange())  //If Unit is in range but not Landed --> Open base if not allready done
            {
                if (openCloseState == OpenCloseState.Closed) CmdOpen();
            }
            else                                //If unit is not in range --> Close base if not allready closed
            {
                if (openCloseState == OpenCloseState.Open) CmdClose();
            }

        } else if (numberUnitsInStock>1)    //If we are here, our unit has died. Can we create a new one?
        {
            //Tell server to create a new unit, if we still have one in stock
            CreateUnit(unitConstructionTime);
        }
    }

    /// <summary>
    /// Called by BTLocalPlayer, when all clients are ready for match
    /// Creates unit
    /// </summary>
    public void PrepareForMatch()
    {
        CreateUnit();
    }

    /// <summary>
    /// Creates a new unit, if not allready exists
    /// Plays close-Animation before, open-Animation after creating object
    /// </summary>
    /// <param name="createTime">Time to get passed before new unit is created</param>
    public void CreateUnit(float createTime)
    {
        inConstruction = true;  //We are now in constriction mode.
        

        if (base.hasAuthority)  //If we are the owner of this base: Tell the server to close us and create a new unit after time
        {
            CmdClose();

            Invoke(nameof(CmdCreateUnit), createTime); //Will call callback RpcOnNewUnitCreated

        }


    }

    /// <summary>
    /// Creates unit immediatly, without any open- or close-actions
    /// </summary>
    private void CreateUnit()
    {
        if (base.hasAuthority)
        {
            CmdCreateUnit();
        }
    }

    /// <summary>
    /// Server creates a new unit and spwans it at our landing-position
    /// Server calls Open-Animation on each client, after that.
    /// </summary>
    [Command]
    void CmdCreateUnit()
    {
        
        GameObject go = GameObject.Instantiate(myUnitPrefab, this.transform.position + landingPosition.localPosition, this.transform.rotation);

        NetworkServer.Spawn(go,base.connectionToClient);

        myUnit = go.GetComponent<BTObject>();

        myUnit.CmdPlaced(this.landingPosition.position, this.landingPosition.rotation);

        RpcOnNewUnitCreated(go);

        RpcOpen();
        
    }

    /// <summary>
    /// Callback, when new unit was created by server
    /// </summary>
    /// <param name="newUnit"></param>
    [ClientRpc]
    void RpcOnNewUnitCreated(GameObject newUnit)
    {
        myUnit = newUnit.GetComponent<BTObject>();
 

        inConstruction = false;

        numberUnitsInStock--;

    }

    /// <summary>
    /// Reloads our unit with ammo
    /// Tells the server to reload after a specific time (reloadTime)
    /// </summary>
    [Client]
    void Reload()
    {
        if (!base.hasAuthority) return;

        if (inReloading == true) return;

        Invoke(nameof(CmdReload),reloadTime);
    }

    /// <summary>
    /// Server Command to reload our unit with ammo 
    /// Opens base after Reloading
    /// Calls callback-method RpcOnUnitReloaded()
    /// </summary>
    [Command]
    void CmdReload()
    {
        Shootable shootable;

        shootable=myUnit.GetComponent<Shootable>();
        if (shootable) shootable.CmdReload();     //Reload unit

        inReloading = false;    //Finished reloading

        RpcOnUnitReloaded();    //Tell all clients that reloading is finished

        RpcOpen();              //ReOpen base on all clients
    }

    /// <summary>
    /// Callback, when server reloaded unit (with ammo)
    /// </summary>
    [ClientRpc]
    void RpcOnUnitReloaded()
    {
        inReloading = false;

    }

    /// <summary>
    /// returns true, if we have a unit and it is still alive
    /// </summary>
    /// <returns></returns>
    bool HasUnit()
    {
        if (myUnit == null) return false;

        return myUnit.IsAlive();

    }

    /// <summary>
    /// Returns true, if our unit is at landing position (or really close to it)
    /// </summary>
    /// <returns></returns>
    bool CheckIfMyUnitIsAtBase()
    {
        if (myUnit==null) return false;

        return (Vector3.Distance(myUnit.transform.position,landingPosition.position)<0.1f);
    }

    /// <summary>
    /// Returns true, if our unit is in range of this base
    /// </summary>
    /// <returns></returns>
    bool CheckIfMyUnitIsInRange()
    {
        if (myUnit==null) return false;

        return  (Vector3.Distance(this.transform.position, myUnit.transform.position)<openRange);
    }

    /// <summary>
    /// Tells all clients to open base
    /// </summary>
    [Command] 
    void CmdOpen()
    {
        RpcOpen();
    }

    /// <summary>
    /// Client method, called by server, to open our base
    /// </summary>
    [ClientRpc]
    void RpcOpen()
    {
        if (openCloseState == OpenCloseState.Open) return;
        myAnimator.SetTrigger("Open");
        openCloseState = OpenCloseState.Open;
    }

    /// <summary>
    /// Tells all clients to close base
    /// </summary>
    [Command]
    void CmdClose()
    {
        RpcClose();
    }

    /// <summary>
    /// Client method, called by server, to close our base
    /// </summary>
    [ClientRpc]
    void RpcClose()
    {
        if (openCloseState == OpenCloseState.Closed) return;
        myAnimator.SetTrigger("Close");
        openCloseState = OpenCloseState.Closed;
    }

    /// <summary>
    /// Called by our unit to inform us, that it is dying
    /// </summary>
    public void UnitDied()
    {
        if (base.hasAuthority) CmdUnitDied();
    }
   
    /// <summary>
    /// Server-Commant to tell all clients, that our unit has died
    /// </summary>
    [Command]
    void CmdUnitDied()
    {
        RpcUnitDied();
    }

    /// <summary>
    /// Client-method called by server, to inform all clients, that our unit has died
    /// </summary>
    [ClientRpc]
    void RpcUnitDied()
    {
        this.myUnit = null;
    }
}

public enum OpenCloseState
{
    Nothing
    ,Open
    ,Closed
}
