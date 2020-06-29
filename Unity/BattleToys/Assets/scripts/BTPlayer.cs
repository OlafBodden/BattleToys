using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using TMPro;




public class BTPlayer : NetworkBehaviour 
{

    public Camera myCam;

    BTTeam myTeam=BTTeam.nothing;
    public BTTeam Team {get { return myTeam;}}

    
    //ToDo: Move To ChatBehavior disalbed for now
    public Canvas chatCanvas;

    //Refernece to Shop
    Shop shop;

    //used for storing current player State. Due to the state, Player has different abilities (Shop. Place Objects, Attack...)
    PlayerState playerState=PlayerState.Born;

    //If we are currently placing objects out of the shop, a reference is stored here. NULL, if currently no object is being placed
    Placeable currentPlaceable=null;

    //Selectable: Which Object is currently selected by the player
    Selectable lastSeleectedUnit=null;  //Used to inicate, which Unit was selected in the last frame
    Selectable selectedUnit=null;       //Used to inicate, which Unit is selected in the current frame

    //Hovered: If the player hovers by mouse over an Object, it recognized
    GameObject hoveredGameObject;       //Currently hovered GameObject (if any. NULL, if not)                 
    Selectable hoveredSelectable;       //Selectable of the currently hovered GameObject (if any. NULL, if not)
    BTObject hoveredBTObject;           //BTObject of the currently hovered GameObject (if any. NULL, if not)
    GameObject lastHoveredGameObject;   //hovered GameObject of the last frame (if any. NULL, if not)         
    Selectable lastHoveredSelectable;   //Selectable of the last-frame-hovered GameObject (if any. NULL, if not)
    BTObject lastHoveredBTObject;       //BTObject of the last-frame-hovered GameObject (if any. NULL, if not)

    
    GameObject goToSpawn;               //ToDo: do we really need this?

    static int numberPlayersReadyForMatch; //Synchronized by Server. Holds the number of Players, that hit the "Ready-For-Match-Button"

    public int NumberPlayersReadyForMatch {get { return numberPlayersReadyForMatch;}}





    void Awake() 
    {
        //Switch the main camera off. As we are spawned by the Server, we have our own camera.
        if (Camera.main) Camera.main.gameObject.SetActive(false);

        //We start as new born player. 
        playerState=PlayerState.Born;

        //See "OnStartAuthority() for more initialization
    }

    /// <summary>
    /// Called, when we are a client and got authority by the server. Called after "OnStartClient", "OnStartLocalPlayer"
    /// </summary>
    public override void OnStartAuthority() 
    {
        //Double check: If we don't have authority over this player-object: nothing to do here
        if (!base.hasAuthority) return;

        //Register at BTLocalGameManager
        BTLocalGameManager.Instance.localPlayer=this;

        //Check, if we are blue or red team. Store the information in myTeam-Variable
        GetTeam();

        //Rename our player-object, so that we can identfiy it (just for readability)
        this.gameObject.name = "Player_ " + myTeam.ToString();

        //First thing to do: Open the Shop
        BTLocalGameManager.Instance.OpenShop();
        playerState=PlayerState.Shopping;   //We are now in shopping-mode
        
        //Activate players objects camera (Main camera was disabled allready)
        myCam.gameObject.SetActive(true);

        //initialize and activate Camera Movement 
        BTPlayerCameraMovement camMovement=GetComponent<BTPlayerCameraMovement>();
        camMovement.enabled=true;

        //Chat is deactivated for now. ToDo: Move to another behavior
        //chatCanvas=this.gameObject.GetComponentInChildren<Canvas>();
        //chatCanvas.gameObject.SetActive(true);

        //Refresh NetworkInfo: Show Team-Name, Number of players, number of match-ready-players
        BTLocalGameManager.Instance.RefreshNetworkInfo();

    }

    //Called, when we are the server and a client connects 
    public override void  OnStartServer()
    {
        base.OnStartServer();

        //Refresh NetworkInfo: Show Team-Name, Number of players, number of match-ready-players
        BTLocalGameManager.Instance.RefreshNetworkInfo();
    }

    // Update is called once per frame
    void Update()
    {
        if (!base.hasAuthority) return; //Wenn nicht localPlayer dann exit

        lastSeleectedUnit=selectedUnit; //remember the selected unit of the last frame



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

            Attacking(); //ToDo: For testing only. Remove this!
        }

        if (playerState==PlayerState.InMatch)
        {
            Selecting();
            Hovering();
            Moving();
            Attacking();

        }

    }

    /// <summary>
    /// called by Update(), when player is in State "PlacingObject"
    /// Allows the player to place objects, bought from the shop
    /// </summary>
    void UpdateInPlacingMode()
    {
        if (currentPlaceable!=null)             //if we have something to place
        {
            if (Input.GetMouseButtonDown(0))    //Wenn während dem Platzieren ein Mausklick...
            {
                GameObject go=currentPlaceable.PlaceObject();   //Try placing object
                
                if (go) //if placing was successfull
                {
                    //Get BTObject
                    BTObject currentBTObject=go?.GetComponent<BTObject>();

                    //Tell BTObject of PlacedObject, that it is now placed
                    currentBTObject?.PlacedByPlayer();

                    //Finish placing of that object 
                    ShopItemPlaced(go);

                }
            }

            else if (Input.GetMouseButtonDown(1))    //Cancel Placement
            {
                currentPlaceable.CanclePlacement(); //Delete Object
                currentPlaceable=null;              //We don't have a placeableObject anymore
                playerState=PlayerState.Shopping;   //Back to shopping
                EventManager.TriggerEvent(EventEnum.CancelPlacingBTObject);
            }
        }
    }

    /// <summary>
    /// Called by "Update()", when we are in Shopping or in Match-Mode
    /// Allows the player to select objects in game
    /// </summary>
    [Client]
    void Selecting()
    {
        if (Input.GetMouseButtonDown(0))    //"Select" is initialized by left-mouse-button
        {
            //Did we click on an unit?
            Ray ray = myCam.ScreenPointToRay (Input.mousePosition);
            RaycastHit hit;
            int layerMaskObstacle=LayerMask.GetMask(new string[]{"Units","Buildings"});
            if (Physics.Raycast(ray, out hit,  100, layerMaskObstacle))
            {
                //We clicked on a unit/object--> Store reference to it
                Selectable newSelection= hit.rigidbody.gameObject.GetComponent<Selectable>();

                //Check, if it is a new selection, or player selected the same object than before
                if (newSelection!=selectedUnit)
                {
                    //Deselect previous selected unit 
                    if (selectedUnit) 
                    {
                        selectedUnit.DeSelect(); //Hide marking-ring
                        selectedUnit =null;
                    }

                    //Store currently selected unit
                    selectedUnit=newSelection;

                    if (selectedUnit) 
                    {
                        selectedUnit.Select(this); //sohw marking ring
                    } 
                }
            } else
            {
                //If we didn't select an object, but clicked somewhere else --> deselect currently selected unit (if any)
                if (selectedUnit) 
                {
                    selectedUnit.DeSelect();
                    selectedUnit=null;
                }
            }
        }
    }

    /// <summary>
    /// Called by Update() when we are in Shopping-Mode or in Match-Mode
    /// Allows the player to move the previous selected unit 
    /// </summary>
    [Client]
    void Moving()
    {
        
        if (Input.GetMouseButtonDown(1)) //Moving is initialized by right mouse button
        {
            //Check, if we hit a non attackable-object (the floor-plane/terrain/what so ever)
            Ray ray = myCam.ScreenPointToRay (Input.mousePosition);
            RaycastHit hit;
            int layerMaskFloor=LayerMask.GetMask(new string[]{"Floor"});
            if (Physics.Raycast(ray, out hit,  100, layerMaskFloor))
            {
                if (selectedUnit)   //Moving is only possivle, if we allready selected a unit (see Selecting())
                {
                    //Double check: Did we defently not click on a unit 
                    Hitable target=hit.rigidbody?.GetComponent<Hitable>();
                    if (target==null)
                    {
                        //Cancel MoveAndAttack-Mode, if needed
                        selectedUnit.GetComponent<BTObject>()?.CancelMoveAndAttack();

                        //Get Moveable-Behavior of currently selected unit (if any)
                        Moveable moveable=selectedUnit.gameObject.GetComponent<Moveable>();

                        //Move
                        moveable?.MoveToDestination(hit.point);

                        //As we may had a enemy-target before, tell the attacking-behavior to stop attacking
                        Shootable shootable=selectedUnit.gameObject.GetComponent<Shootable>();
                        shootable?.CancelAttack();


                    }
                }
            }
        }      
    }

    /// <summary>
    /// Called by Update(), when we are in Match-Mode
    /// Allows the player to attack with a previous selected unit 
    /// ToDo: Should also contain defence-attacking. Not implemented now
    /// </summary>
    [Client]
    void Attacking()
    {
        if (selectedUnit==null) return; //Can only attack, when one of my units is selected
                                        //ToDo: Defence: Units can shoot, whenever enemy-hitable in range

        if (Input.GetMouseButtonDown(1))    //Offence-Attack is initialized by righ mouse button
        {
            //Check, if we hit another object
            Ray ray = myCam.ScreenPointToRay (Input.mousePosition);
            RaycastHit hit;
            int layerMaskFloor=LayerMask.GetMask(new string[]{"Buildings","Units"});
            if (Physics.Raycast(ray, out hit,  100, layerMaskFloor))
            {
                //Attack is only possible, when we have a (previous) selected unit
                if (selectedUnit)
                {
                    //Check, if enemy-object is attackable
                    Hitable hitable=hit.rigidbody.GetComponent<Hitable>();

                    if (hitable)
                    {
                        //Get BTOjbect of our currently selected unit
                        BTObject myObject = selectedUnit.gameObject.GetComponent<BTObject>();

                        //Tell our selected unit to move towars the enemy object and attack it,
                        //  as soon as we are in fire range
                        myObject?.MoveAndAttack(hitable);

                        //ToDo: For stationary objects (turrets), no movement is required/possible
                    }
                }
            }
        } 
    }

    /// <summary>
    /// Called by Update() when we are in Shopping or Match-Mode
    /// Stores currently and previous hoveres object
    /// Responsible for changing cursor-sprite
    /// </summary>
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
        
            if (hoveredSelectable!=null && selectedUnit!=null && !hoveredSelectable.IsMe(this) && selectedUnit.IsMe(this))  EventManager.TriggerEvent(EventEnum.CursorAttack);
  
            
        }

        //Do Hovered-Object-Projector-Stuff
        if (hoveredSelectable != lastHoveredSelectable)
        {
            lastHoveredSelectable?.DeSelectHovered();
            hoveredSelectable?.SelectAsHovered(this);
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


    /// <summary>
    /// Called by ShopItemSlot, if Player clicked on item
    /// 
    /// </summary>
    /// <param name="prefab"></param>
    [Client]
    public void InstantiateShopItem(GameObject prefab)
    {
        //we will not instantiate the prefab from the shopItemSlot directly
        //we need the server to spawn the prefab, so we need to tell him the object-type

        //Get the BTObject-type
        BTObjectType type=prefab.GetComponent<BTObject>().btObjectSO.btObjectType;

        //Tell the server to spawn it. First thing it will do after spawning is calling the PlayerAuthorizedBTObjectWasSpawned()-method
        CmdInstantiateBTObject(type, this.transform.position, this.transform.rotation);
    }

    /// <summary>
    /// A new Player-Authorized BTObject was spawned by the server. 
    /// First thing it does is, calling this method
    /// </summary>
    /// <param name="go"></param>
    [Client]
    public void PlayerAuthorizedBTObjectWasSpawned(GameObject go)
    { 
        //As we are in Shop-Mode, when a new object is spawned, we need to initialize it.
        //After that, we are in Placing-Mode

        //Add the Placeable-Behavior and initialize it
        currentPlaceable=go.AddComponent<Placeable>();
        currentPlaceable.Init(this, go.GetComponent<BTObject>());

        //We are now in Placing-Mode
        playerState=PlayerState.PlacingShopObject;

    }

    //Not used yet.
    [Client]
    public void PlayerAuthorizedProjectileWasSpawned(GameObject go)
    {


    }

    /// <summary>
    /// Called by Update(), when we finally have placed an object
    /// </summary>
    /// <param name="placedObject"></param>
    [Client]
    public void ShopItemPlaced(GameObject placedObject)
    {
        //We don't have a placeable object any more
        currentPlaceable=null;

        //Go on shopping
        playerState=PlayerState.Shopping;

        EventManager.TriggerEvent(EventEnum.PlacedBTObject);
 
    }

    /// <summary>
    /// Server-Command to spawn an BTObject of a specific type 
    /// Called by Player.InstantiateShopItem
    /// </summary>
    /// <param name="type"></param>
    /// <param name="pos"></param>
    /// <param name="rot"></param>
    [Command]
    void CmdInstantiateBTObject(BTObjectType type, Vector3 pos, Quaternion rot)
    {
        //Loop through the List of spawnable prefabs to find the one with the correct type. 
        //ToDo: Can be optimized, if neccessary
        foreach (GameObject go in NetworkManager.singleton.spawnPrefabs)
        {
            if (go.GetComponent<BTObject>()?.btObjectSO?.btObjectType==type)
            {
                //We found the correct prefab. Instantiate it... 
                goToSpawn=GameObject.Instantiate(go, pos, rot);

                //and spawn it. Set Authrority to player
                NetworkServer.Spawn(goToSpawn,base.connectionToClient);
            
            }

        }
    }


    /// <summary>
    /// Currently not used. Could be used by Shootable to spawn Projectiles
    /// ToDo: Change to Cached List
    /// </summary>
    /// <param name="type"></param>
    /// <param name="pos"></param>
    /// <param name="rot"></param>
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


    /// <summary>
    /// Returns true, if player is currently in shopping-mode. 
    /// Otherwise false
    /// </summary>
    /// <returns></returns>
    public bool IsShopping()
    {
        return (playerState==PlayerState.Shopping);
    }

    /// <summary>
    /// Called by shop, if player hits "ReadyForMatch"-Button
    /// Sets the state of the player to "ReadyForMatch", so that he cannot buy or move objects any more
    /// Instead he is waiting for all other players to be "ReadyForMatch"
    /// </summary>
    [Client]
    public void FinishedShopping()
    {
        if (!base.hasAuthority) return;     //if we do not have authority, nothing to do here

        //Check, if we are not allready in ReadyForMatch-State. We are not allowed to call this twice
        if (this.playerState!=PlayerState.ReadyForMatch)
        {
            BTLocalGameManager.Instance.HideShop();
            BTLocalGameManager.Instance.LocalPlayerIsWaitingForMatch();

            this.playerState=PlayerState.ReadyForMatch;
            CmdPlayerIsReadyForMatch();     //Tell the server, that one more player is readyForMatch. Server is syncing this info with all clients
            
        }



    }

    /// <summary>
    /// Server-Command to tell the Server, that one more player is ready for match
    /// Server is synchronizing this information with all clients
    /// If all Players are readyForMatch, Server is starting the match
    /// </summary>
    [Command]
    void CmdPlayerIsReadyForMatch()
    {
        numberPlayersReadyForMatch++;

        Debug.Log("Server: PlayerReady: " + numberPlayersReadyForMatch + "/" + NetworkServer.connections.Count);
        RpcOneMorePlayerIsReadyForMatch(numberPlayersReadyForMatch);

        if (numberPlayersReadyForMatch>=NetworkServer.connections.Count)
        {
            //Start Match       
            RpcStartMatch();
            if (base.isServerOnly) CmdRaiseCourtainOnServerOnly(); //Courtain-GameObjekt auf Server-Instanz löschen
        }
    }

    [Command]
    void CmdRaiseCourtainOnServerOnly()
    {
        //Courtain-GameObjekt auf Server-Instanz löschen
        BTLocalGameManager.Instance.courtain.RaiseCourtain();
    }

    /// <summary>
    /// Client-Procedure, used by the server to synchronize the number of player, that are ready for match
    /// </summary>
    /// <param name="value"></param>
    [ClientRpc]
    void RpcOneMorePlayerIsReadyForMatch(int value)
    {
        numberPlayersReadyForMatch=value;

        //Show the number of match-ready players in UI
        BTLocalGameManager.Instance.RefreshNetworkInfo();
    }

    /// <summary>
    /// Client-Procedure, used by the Server to start the match
    /// </summary>
    [ClientRpc]
    void RpcStartMatch()
    {
        //Tell the localGameManager to start the match
        BTLocalGameManager.Instance.StartMatchCountdown();
    }

    /// <summary>
    /// Called by BTLocalGameManager to tell the player, that we are now in match-mode
    /// </summary>
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


