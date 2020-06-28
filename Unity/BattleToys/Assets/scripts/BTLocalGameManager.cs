using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using TMPro;
//using UnityEngine.Events;
using System;

/// <summary>
/// Represents the game manager of each local instance
/// Is not a Network-Behavior!
/// Manages Game-States, objects
/// Is a singleton, as it exits only once per scene/client
/// </summary>
public class BTLocalGameManager : MonoBehaviour
{
    //Singleton
    static private BTLocalGameManager _instance;
    static public BTLocalGameManager Instance { get { return _instance; } }

    //Reference to the BTPlayer-Object of the local player. Set, after it is spawned and got authority
    public BTPlayer localPlayer;

    //UI-Panel and Text for Network-Info
    public GameObject PanelBackgroundNetworkHUD;
    public TextMeshProUGUI debugText;

    //Reference to the shop. Set in inspector
    public GameObject shop;

    public GameObject prefabCountdown;          //Reference to the countdown-prefab, used right before the match starts. Set in the inspector
    
    //References to spawn points. Needed to find out the team (red, blue)
    public Transform spawnPointCountdownRed;    
    public Transform spawnPointCountdownBlue;

    //Courtain-Reference is responsible for playing Animation "Vorhang öffnet sich"
    public Courtain courtain;

    //Projector-Materials for Selected and hovered Objects
    public Material projectorMaterialSelectedUnit;
    public Material projectorMaterialSelectedTarget;
    public Material projectorMaterialHoveredUnit;
    public Material projectorMaterialHoveredTarget;

    //List, that contains all BTObjects of the player
    public List<BTObject> myBTObjects;

    public LocalEffectPool localEffectPool;



    private void Awake() 
    {
        //Sigleton-Stuff
        _instance=this;

        localEffectPool=GetComponentInChildren<LocalEffectPool>();

        //Enable and initialize Network-UI-Panel
        ShowNetworkUI();
        debugText=GameObject.Find("TextDebug").GetComponent<TextMeshProUGUI>();

        //Initialize BT-Object-List (that will cotain each BTObject, that is spawned by the local player)
        myBTObjects=new List<BTObject>();
    }

    void Update()
    {
        //Handle Exit-Game-Input
        if (Input.GetKeyDown(KeyCode.Escape)) Application.Quit();   //ToDo: Go back to main menue
    }

    /// <summary>
    /// Enables Network-UI-Panel
    /// </summary>
    public void ShowNetworkUI()
    {
        Transform.FindObjectOfType<NetworkManagerHUD>().enabled=true;
        PanelBackgroundNetworkHUD.SetActive(true);

    }

    /// <summary>
    /// Disables/hides Network-UI-Panel
    /// </summary>
    public void HideNetworkUI()
    {
        Transform.FindObjectOfType<NetworkManagerHUD>().enabled=false;
        PanelBackgroundNetworkHUD.SetActive(false);
    }

    /// <summary>
    /// shows Shop
    /// Called by OpenShop()
    /// </summary>
    private void ShowShop()
    {
        shop.SetActive(true);
    }

    /// <summary>
    /// Hides Shop
    /// </summary>
    private void HideShop()
    {
        shop.SetActive(false);
    }

    /// <summary>
    /// Refreshes Network-UI-Panel 
    /// Displays Team- Player and ConnectedClient-Information
    /// </summary>
    public void RefreshNetworkInfo()
    {
        if (localPlayer)
        {

            debugText.text="My Team: " + localPlayer.Team.ToString();
            debugText.text+="\n I am " + localPlayer.IsClientOrServer();
            debugText.text+="\n Players Connected: " + localPlayer.GetNetworkConnectionCount();
            debugText.text+="\n Players Ready For Match: " + localPlayer.NumberPlayersReadyForMatch;
        }

    }

    /// <summary>
    /// Starts the Match-Countdown
    /// Hides the Shop
    /// Sets Player back to its initial position
    /// Freezes Camera
    /// Invokes Method to Start the match
    /// </summary>
    public void StartMatchCountdown()
    {
        HideShop();

        HideNetworkUI();

        ResetPlayerPosition();

        localPlayer.GetComponent<BTPlayerCameraMovement>().IsActive=false;

        Transform t = localPlayer.Team==BTTeam.red ? spawnPointCountdownRed : spawnPointCountdownBlue;

        //ToDo: Disabled for Testing-speed-up
        //GameObject.Instantiate(prefabCountdown, t.localPosition,t.localRotation);

        Invoke("StartMatch",1f);    //ToDo: If Countdown is enabled again, this should be set to 6f

        //Fire MatchCountdownStarted Event
        EventManager.TriggerEvent(EventEnum.MatchCountdownStarted);
        
    }

    /// <summary>
    /// Sets Player back to its spawn-point
    /// </summary>
    void ResetPlayerPosition()
    {
        localPlayer.GetComponent<BTPlayerCameraMovement>().ResetPositionAndRotation();
    }

    /// <summary>
    /// Starts the match
    /// Tells every necessary object to prepare for Match (e.g. UnitBase should Instantiate their units)
    /// Raises the courtain
    /// Un-Freezes Camera
    /// Tells the local player, that match has started
    /// Triggers MatchStarted-Event
    /// </summary>
    void StartMatch()
    {
        //Tell all units to to their Match-Prepare-Action, if any
        foreach(BTObject o in myBTObjects)
        {
            UnitBase unitBase=o.transform.GetComponent<UnitBase>();
            unitBase?.PrepareForMatch();
        }


        courtain.RaiseCourtain();   //ToDo: Change to courtain listens to MatchStarted-Event
        localPlayer.GetComponent<BTPlayerCameraMovement>().IsActive=true;
        localPlayer.StartMatch();   //ToDo: Change to player listens to MatchStarted-Event
        EventManager.TriggerEvent(EventEnum.MatchStarted);

    }

    /// <summary>
    /// Opens and shows the Shop.
    /// Triggers StartShopping-Event
    /// </summary>
    public void OpenShop()
    {
        ShowShop();

        shop.GetComponent<Shop>().OpenShop(localPlayer);

        EventManager.TriggerEvent(EventEnum.StartShopping);
    }

    /// <summary>
    /// Here, each BTObject can register itself to be a local-player-owned object (right after Spawning)
    /// </summary>
    /// <param name="o"></param>
    public void RegisterAsObject(BTObject o)
    {
        myBTObjects.Add(o);
    }


    /// <summary>
    /// Here, each BTObject can deregister itself to be a local-player-owned object. e.g. right before Destroy
    /// </summary>
    /// <param name="o"></param>
    public void DeRegisterAsObject(BTObject o)
    {
        myBTObjects.Remove(o);
    }

    public void PlayLocalEffect(EffectType effectType, Vector3 position, Quaternion rot)
    {
        GameObject go=localEffectPool.GetObjectFromPool(effectType);

        go.transform.position=position;
        go.transform.rotation=rot;
 
    }

    public void ReturnLocalEffectToStock(LocalEffect le)
    {
        localEffectPool.ReturnGameObjectToPool(le);
    }





}

