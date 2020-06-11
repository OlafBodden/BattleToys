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

    static public BTLocalGameManager Instance { get { return _instance;}}

    

    private void Awake() {
        _instance=this;

        ShowNetworkUI();

        debugText=GameObject.Find("TextDebug").GetComponent<TextMeshProUGUI>();
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

    public void ShowShop()
    {
        shop.SetActive(true);
    }

    public void HideShop()
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

        GameObject.Instantiate(prefabCountdown, t.localPosition,t.localRotation);

        Invoke("StartMatch",6f);


        
    }

    void ResetPlayerPosition()
    {
        localPlayer.GetComponent<BTPlayerCameraMovement>().ResetPositionAndRotation();
    }

    void StartMatch()
    {
        courtain.RaiseCourtain();
        localPlayer.GetComponent<BTPlayerCameraMovement>().IsActive=true;
        localPlayer.StartMatch();

    }
    public void OpenShop()
    {
        ShowShop();

        shop.GetComponent<Shop>().OpenShop(localPlayer);
    }

}

