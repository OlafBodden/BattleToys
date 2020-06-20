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

    Selectable lastSeleectedUnit=null;
    Selectable selectedUnit=null;
    

    GameObject hoveredGameObject;
    Selectable hoveredSelectable;
    BTObject hoveredBTObject;

    GameObject lastHoveredGameObject;
    Selectable lastHoveredSelectable;
    BTObject lastHoveredBTObject;

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

        //chatCanvas.gameObject.SetActive(true);

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

        lastSeleectedUnit=selectedUnit;

        if (Input.GetKey(KeyCode.Escape)) Application.Quit();

        //Wenn Shop Objekt bereits ausgwählt und am plazieren
        if (playerState==PlayerState.PlacingShopObject) 
        {
            UpdateInPlacingMode();
        }
        else if (playerState==PlayerState.Shopping) 
        {
            Selecting();
            Hovering();
            Moving();

            Attacking();
        }

        if (playerState==PlayerState.InMatch)
        {
            Selecting();
            Hovering();
            Moving();
            Attacking();

        }


        

        //Handle Hover
        //GetHoveredObject();
    }

    void UpdateInPlacingMode()
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
                EventManager.TriggerEvent(EventEnum.CancelPlacingBTObject);
            }
        }
    }

    void Selecting()
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
                    if (selectedUnit) 
                    {
                        selectedUnit.DeSelect();
                        selectedUnit=null;
                    }

                    selectedUnit=newSelection;

                    if (selectedUnit) 
                    {
                        selectedUnit.Select(this);
                    } 
                }
            } else
            {
                if (selectedUnit) 
                {
                    selectedUnit.DeSelect();
                    selectedUnit=null;
                }
            }
        }
    }

    void Moving()
    {

        if (Input.GetMouseButtonDown(1))
        {
            Ray ray = myCam.ScreenPointToRay (Input.mousePosition);
            RaycastHit hit;
            int layerMaskFloor=LayerMask.GetMask(new string[]{"Floor"});
            if (Physics.Raycast(ray, out hit,  100, layerMaskFloor))
            {
                if (selectedUnit)
                {
                    Hitable target=hit.rigidbody?.GetComponent<Hitable>();
                    if (target==null)
                    {
                        //Move
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

    void Attacking()
    {
        if (selectedUnit==null) return; //Can only attack, when one of my units is selected
                                        //ToDo: Defence: Units can shoot, whenever enemy-hitable in range

        if (Input.GetMouseButtonDown(1))
        {
            Ray ray = myCam.ScreenPointToRay (Input.mousePosition);
            RaycastHit hit;
            int layerMaskFloor=LayerMask.GetMask(new string[]{"Buildings","Units"});
            if (Physics.Raycast(ray, out hit,  100, layerMaskFloor))
            {
                if (selectedUnit)
                {
                    Hitable hitable=hit.rigidbody.GetComponent<Hitable>();
                    BTObject myObject=selectedUnit.gameObject.GetComponent<BTObject>();

                    myObject?.MoveAndAttack(hitable);
                }
            }
        } 
    }

    [Client]
    void Hovering()
    {
        Ray ray = myCam.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        lastHoveredBTObject=hoveredBTObject;
        lastHoveredGameObject=hoveredGameObject;
        lastHoveredSelectable=hoveredSelectable;

        if(Physics.Raycast(ray, out hit))
        {
            hoveredGameObject=hit.collider.gameObject;
            
            hoveredBTObject=hoveredGameObject.GetComponent<BTObject>();
            if (hoveredBTObject==null) hoveredBTObject=hoveredGameObject.GetComponentInParent<BTObject>();

            hoveredSelectable=hoveredGameObject.GetComponent<Selectable>();
            if (hoveredSelectable==null) hoveredSelectable=hoveredGameObject.GetComponentInParent<Selectable>();

        } else
        {
            hoveredBTObject=null;
            hoveredGameObject=null;
            hoveredSelectable=null;
        }

        //Do Cursor-Stuff
        if ((hoveredGameObject!=lastHoveredGameObject) || (lastSeleectedUnit!=selectedUnit))
        {
            if (hoveredSelectable==null && selectedUnit==null ) EventManager.TriggerEvent(EventEnum.CursorDefault);

            if (hoveredSelectable==null && selectedUnit!=null && selectedUnit.IsMe(this)) EventManager.TriggerEvent(EventEnum.CursorMove);

            if (hoveredSelectable!=null && selectedUnit==null && hoveredSelectable.IsMe(this)) EventManager.TriggerEvent(EventEnum.CursorSelect);
        
            if (hoveredSelectable!=null && selectedUnit!=null && !hoveredSelectable.IsMe(this) && selectedUnit.IsMe(this)) 
            {
                EventManager.TriggerEvent(EventEnum.CursorAttack);
                Debug.Log("Attack!");
            }
        }

        //Do Hovered-Object-Projector-Stuff
        if (hoveredSelectable != lastHoveredSelectable)
        {
            lastHoveredSelectable?.DeSelectHovered();
            hoveredSelectable?.SelectAsHovered(this);
        }

    }

    [Client]
    public GameObject GetCannonball()
    {
        return null;
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
        BTObjectType type=prefab.GetComponent<BTObject>().btObjectSO.btObjectType;
        CmdInstantiateBTObject(type, this.transform.position, this.transform.rotation);
    }

    public void PlayerAuthorizedBTObjectWasSpawned(GameObject go)
    {
        currentPlaceable=go.AddComponent<Placeable>();
        currentPlaceable.Init(this);

        //We are now in Placing-Mode
        playerState=PlayerState.PlacingShopObject;

    }

    public void PlayerAuthorizedProjectileWasSpawned(GameObject go)
    {


    }

    [Client]
    public void ShopItemPlaced(GameObject placedObject)
    {
        

        currentPlaceable=null;

        //Go on shopping
        playerState=PlayerState.Shopping;

        EventManager.TriggerEvent(EventEnum.PlacedBTObject);
        
    }

    [Command]
    void CmdInstantiateBTObject(BTObjectType type, Vector3 pos, Quaternion rot)
    {

        foreach (GameObject go in NetworkManager.singleton.spawnPrefabs)
        {
            if (go.GetComponent<BTObject>()?.btObjectSO?.btObjectType==type)
            {
                goToSpawn=GameObject.Instantiate(go, pos, rot);

                NetworkServer.Spawn(goToSpawn,base.connectionToClient);
            
            }

        }



    }



    [Command]
    void CmdInstantiateProjectile(ProjectileType type, Vector3 pos, Quaternion rot)
    {

        foreach (GameObject go in NetworkManager.singleton.spawnPrefabs)
        {
            if (go.GetComponent<Projectile>()?.projectileType==type)
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


