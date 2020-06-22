using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using TMPro;
//using UnityEngine.Events;
using System;

public class BTLocalGameManager : MonoBehaviour
{
    static private BTLocalGameManager _instance;

    public BTPlayer localPlayer;

    public GameObject PanelBackgroundNetworkHUD;

    public TextMeshProUGUI debugText;

    public GameObject shop;

    public GameObject prefabCountdown;
    public Transform spawnPointCountdownRed;
    public Transform spawnPointCountdownBlue;

    public Courtain courtain;

    public Material projectorMaterialSelectedUnit;
    public Material projectorMaterialSelectedTarget;

    public Material projectorMaterialHoveredUnit;

    public Material projectorMaterialHoveredTarget;

    public List<BTObject> myBTObjects;


    static public BTLocalGameManager Instance { get { return _instance;}}

    

    private void Awake() {
        _instance=this;

        ShowNetworkUI();

        debugText=GameObject.Find("TextDebug").GetComponent<TextMeshProUGUI>();

        myBTObjects=new List<BTObject>();
    }

    public void ShowNetworkUI()
    {
        Transform.FindObjectOfType<NetworkManagerHUD>().enabled=true;
        PanelBackgroundNetworkHUD.SetActive(true);

    }

    public void HideNetworkUI()
    {
        Transform.FindObjectOfType<NetworkManagerHUD>().enabled=false;
        PanelBackgroundNetworkHUD.SetActive(false);
    }

    private void ShowShop()
    {
        shop.SetActive(true);
    }

    private void HideShop()
    {
        shop.SetActive(false);
    }

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

    public void StartMatchCountdown()
    {
        HideShop();

        HideNetworkUI();

        ResetPlayerPosition();

        localPlayer.GetComponent<BTPlayerCameraMovement>().IsActive=false;

        Transform t = localPlayer.Team==BTTeam.red ? spawnPointCountdownRed : spawnPointCountdownBlue;

        //GameObject.Instantiate(prefabCountdown, t.localPosition,t.localRotation);

        Invoke("StartMatch",1f);

        EventManager.TriggerEvent(EventEnum.MatchCountdownStarted);
        
    }

    void ResetPlayerPosition()
    {
        localPlayer.GetComponent<BTPlayerCameraMovement>().ResetPositionAndRotation();
    }

    void StartMatch()
    {
        //Tell all units to to their Match-Prepare-Action, if any
        foreach(BTObject o in myBTObjects)
        {
            UnitBase unitBase=o.transform.GetComponent<UnitBase>();
            unitBase?.PrepareForMatch();
        }


        courtain.RaiseCourtain();
        localPlayer.GetComponent<BTPlayerCameraMovement>().IsActive=true;
        localPlayer.StartMatch();
        EventManager.TriggerEvent(EventEnum.MatchStarted);

    }
    public void OpenShop()
    {
        ShowShop();

        shop.GetComponent<Shop>().OpenShop(localPlayer);

        EventManager.TriggerEvent(EventEnum.StartShopping);
    }

    public void RegisterAsObject(BTObject o)
    {
        myBTObjects.Add(o);
    }

    public void DeRegisterAsObject(BTObject o)
    {
        myBTObjects.Remove(o);
    }



}

