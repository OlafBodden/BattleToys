using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using TMPro;




public class BTPlayer : NetworkBehaviour 
{

    public Camera myCam;

    //public GameObject cannonPrefab;

    [SerializeField]
    BTTeam myTeam=BTTeam.nothing;

    public BTTeam Team {get { return myTeam;}}

    

    public Canvas chatCanvas;

    Shop shop;

    [SerializeField] PlayerState playerState=PlayerState.Born;

    [SerializeField] Placeable currentPlaceable=null;

    Selectable selectedUnit;

    GameObject goToSpawn;

    static int numberPlayersReadyForMatch;

    public int NumberPlayersReadyForMatch {get { return numberPlayersReadyForMatch;}}





    void Awake() 
    {
        if (Camera.main) Camera.main.gameObject.SetActive(false);
        playerState=PlayerState.Born;
    }

    public override void OnStartAuthority() 
    {
        if (!base.hasAuthority) return;

        BTLocalGameManager.Instance.localPlayer=this;

        GetTeam();

        BTLocalGameManager.Instance.OpenShop();
        playerState=PlayerState.Shopping;
        
        this.gameObject.name="Player_ " + myTeam.ToString();

        myCam.gameObject.SetActive(true);

        BTPlayerCameraMovement camMovement=GetComponent<BTPlayerCameraMovement>();
        camMovement.enabled=true;

        //chatCanvas=this.gameObject.GetComponentInChildren<Canvas>();

        chatCanvas.gameObject.SetActive(true);

        BTLocalGameManager.Instance.RefreshNetworkInfo();

        //debugText=GameObject.Find("TextDebug").GetComponent<TextMeshProUGUI>();

        //debugText.text="Ich bin " + myTeam.ToString();
        //debugText.text+="\n meine Camera: " + myCam.name;

        
    }

    public override void  OnStartServer()
    {
        base.OnStartServer();
        
        //if (BTLocalGameManager.Instance.localPlayer==null) BTLocalGameManager.Instance.localPlayer=this;
        BTLocalGameManager.Instance.RefreshNetworkInfo();


        // if (base.isServer)
        // {
        //     debugText=GameObject.Find("TextDebug").GetComponent<TextMeshProUGUI>();

        //     debugText.text="Ich bin Server. Team: " + myTeam.ToString();
        //     debugText.text+="\n Players Connected: " + NetworkServer.connections.Count;
        // } else
        // {
        //     debugText=GameObject.Find("TextDebug").GetComponent<TextMeshProUGUI>();
        //     debugText.text="Ich bin Client. Team: " + myTeam.ToString();

        // }

    }

    // Update is called once per frame
    void Update()
    {
        if (!base.hasAuthority) return; //Wenn nicht localPlayer dann exit

        //Wenn Shop Objekt bereits ausgwählt und am plazieren
        if (playerState==PlayerState.PlacingShopObject)
        {
            if (currentPlaceable!=null)
            {
                if (Input.GetMouseButtonDown(0))    //Wenn während dem Platzieren ein Mausklick...
                {
                    GameObject go=currentPlaceable.PlaceObject();   //Plaziere Objekt
                    if (go)
                    {
                        ShopItemPlaced(go);
                    }
                }

                if (Input.GetMouseButtonDown(1))    //Cancel Placement
                {
                    currentPlaceable.CanclePlacement();
                    currentPlaceable=null;
                    playerState=PlayerState.Shopping;   //Back to shopping
                }
            }
        }

        else if (playerState==PlayerState.Shopping)
        {
            if (Input.GetMouseButtonDown(0))    
            {
                //Did we click on an unit?
                Ray ray = myCam.ScreenPointToRay (Input.mousePosition);
                RaycastHit hit;
                int layerMaskObstacle=LayerMask.GetMask(new string[]{"Units","Buildings"});
                if (Physics.Raycast(ray, out hit,  100, layerMaskObstacle))
                {
                    Selectable newSelection= hit.rigidbody.gameObject.GetComponent<Selectable>();

                    if (newSelection!=selectedUnit)
                    {
                        if (selectedUnit) selectedUnit.DeSelect();

                        selectedUnit=newSelection;

                        if (selectedUnit) selectedUnit.Select();
                    }
                } else
                {
                    if (selectedUnit) selectedUnit.DeSelect();
                }
            }

            if (Input.GetMouseButtonDown(1))
            {
                Ray ray = myCam.ScreenPointToRay (Input.mousePosition);
                RaycastHit hit;
                int layerMaskFloor=LayerMask.GetMask(new string[]{"Floor"});
                if (Physics.Raycast(ray, out hit,  100, layerMaskFloor))
                {
                    if (selectedUnit)
                    {
                        Moveable moveable=selectedUnit.gameObject.GetComponent<Moveable>();

                        if (moveable)
                        {
                            moveable.MoveToDestination(hit.point);
                        }
                    }
                }
            }
        }
    }


    [Client]
    void GetTeam()
    {
        Transform spawnPointParent;

        spawnPointParent=GameObject.Find("SpawnPoints").transform;

        for (int i=0; i<spawnPointParent.childCount;i++)
        {
            if (this.transform.position==spawnPointParent.GetChild(i).position)
            {
                if (spawnPointParent.GetChild(i).name.Equals("SpawnpointBlue")) myTeam=BTTeam.blue;
                if (spawnPointParent.GetChild(i).name.Equals("SpawnpointRed")) myTeam=BTTeam.red;
            }
        }
    }

    ///Called by ShopItemSlot, if Player clicked on item
    [Client]
    public void InstantiateShopItem(GameObject prefab)
    {
        //CmdInstantiateCannon(this.transform.position, this.transform.rotation);
        BTObjectType type=prefab.GetComponent<BTObject>().btObjectType;
        CmdInstantiateBTObject(type, this.transform.position, this.transform.rotation);
    }

    public void PlayerAuthorizedBTObjectWasSpawned(GameObject go)
    {
        currentPlaceable=go.AddComponent<Placeable>();
        currentPlaceable.Init(this);

        //We are now in Placing-Mode
        playerState=PlayerState.PlacingShopObject;

    }

    [Client]
    public void ShopItemPlaced(GameObject placedObject)
    {
        Debug.Log("ShopItemPlaced1 " + placedObject.name);

        currentPlaceable=null;

        //Go on shopping
        playerState=PlayerState.Shopping;
        Debug.Log("ShopItemPlaced2 " + placedObject.name);
    }

    [Command]
    void CmdInstantiateBTObject(BTObjectType type, Vector3 pos, Quaternion rot)
    {

        foreach (GameObject go in NetworkManager.singleton.spawnPrefabs)
        {
            if (go.GetComponent<BTObject>()?.btObjectType==type)
            {
                goToSpawn=GameObject.Instantiate(go, pos, rot);

                NetworkServer.Spawn(goToSpawn,base.connectionToClient);
            
            }

        }



    }



    public bool IsShopping()
    {
        return (playerState==PlayerState.Shopping);
    }

    public void FinishedShopping()
    {
        if (!base.hasAuthority) return; 


        if (this.playerState!=PlayerState.ReadyForMatch)
        {
            this.playerState=PlayerState.ReadyForMatch;
            CmdPlayerIsReadyForMatch();
            
        }



    }

    [Command]
    void CmdPlayerIsReadyForMatch()
    {
        numberPlayersReadyForMatch++;

        Debug.Log("Server: PlayerReady: " + numberPlayersReadyForMatch + "/" + NetworkServer.connections.Count);
        RpcOneMorePlayerIsReadyForMatch(numberPlayersReadyForMatch);

        if (numberPlayersReadyForMatch>=NetworkServer.connections.Count)
        {
            //Start Match       
            //ToDo: Hide Vorhang

            RpcStartMatch();
        }
    }

    [ClientRpc]
    void RpcOneMorePlayerIsReadyForMatch(int value)
    {

        numberPlayersReadyForMatch=value;
        BTLocalGameManager.Instance.RefreshNetworkInfo();

    }

    [ClientRpc]
    void RpcStartMatch()
    {
        BTLocalGameManager.Instance.StartMatchCountdown();
    }

    [Client]
    public void StartMatch()
    {
        this.playerState=PlayerState.InMatch;
    }


    public int GetNetworkConnectionCount()
    {
        return NetworkServer.connections.Count;
    }

    public string IsClientOrServer()
    {
        if (base.isServer) { return "Server"; }
        else if (base.isClient) { return "Client"; }
        else { return "Nothing"; }
    }
}

public enum BTTeam
{
    red,
    blue    

    ,nothing
}

public enum PlayerState
{
    Born,

    Shopping,

    PlacingShopObject,

    ReadyForMatch,

    InMatch
}


