using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using TMPro;




public class BTPlayer : NetworkBehaviour 
{

    public Camera myCam;

    public GameObject cannonPrefab;

    [SerializeField]
    BTTeam myTeam=BTTeam.nothing;

    [SerializeField]
    Cannon myCannon;

    public TextMeshProUGUI debugText;

    public Canvas chatCanvas;


    void Awake() 
    {
        if (Camera.main) Camera.main.gameObject.SetActive(false);
    }

    public override void OnStartAuthority() 
    {
        if (!base.hasAuthority) return;

        

        GetTeam();
        CreateCannon();
        
        this.gameObject.name="Player_ " + myTeam.ToString();

        myCam.gameObject.SetActive(true);

        //chatCanvas=this.gameObject.GetComponentInChildren<Canvas>();

        chatCanvas.gameObject.SetActive(true);

        debugText=GameObject.Find("TextDebug").GetComponent<TextMeshProUGUI>();

        debugText.text="Ich bin " + myTeam.ToString();
        debugText.text+="\n meine Camera: " + myCam.name;

        
    }

    // Update is called once per frame
    void Update()
    {
        if (!base.hasAuthority) return;

        RotateCannonPipe();

        RotateCannon();

        if (Input.GetKeyDown(KeyCode.Space))
        {
            myCannon.ResetPower();
        }

        if (Input.GetKey(KeyCode.Space))
        {
            myCannon.IncreasePower();
        }

        


        if (Input.GetKeyUp(KeyCode.Space))
        {
            myCannon.Fire();
            //CmdFireCannon(myCannon.currentPower/100f);
        }

    }

    [Client]
    void RotateCannonPipe()
    {
        int rotX=(Input.GetKey(KeyCode.UpArrow)==true ? 1 : 0)  - (Input.GetKey(KeyCode.DownArrow)==true? 1 : 0) ;

        myCannon.RotateCannonPipe(rotX);
    }

    [Client]
    void RotateCannon()
    {
        int rotY=(Input.GetKey(KeyCode.RightArrow)==true ? 1 : 0)  - (Input.GetKey(KeyCode.LeftArrow)==true? 1 : 0) ;

        myCannon.RotateCannon(rotY);
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

    [Client]
    void CreateCannon()
    {
        Transform t=null; 
        if (myTeam==BTTeam.red)
        {
            t=GameObject.Find("CannonSpawnPointRed").transform;
        } else if (myTeam==BTTeam.blue)
        {
            t=GameObject.Find("CannonSpawnPointBlue").transform;
        }

        CmdSpawnCannon(t.position,t.rotation);

        //go.name="Cannon_" + myTeam.ToString();

        //myCannon=go.GetComponent<Cannon>();

        // foreach(Cannon c in Transform.FindObjectsOfType<Cannon>())
        // {
        //     if (c.hasAuthority)
        //     {
        //         myCannon=c;
        //     }
        // }
    }

    [Command]
    void CmdSpawnCannon(Vector3 pos, Quaternion rot)
    {

        //Respawn to get Authority
        //GameObject newCannon=GameObject.Instantiate(cannonPrefab,pos,rot);
        //newCannon.name="Cannon_" + myTeam.ToString();

        GameObject newCannon=GameObject.Instantiate(cannonPrefab,pos,rot);
        newCannon.name="Cannon_" + myTeam.ToString();

        NetworkServer.Spawn(newCannon,base.connectionToClient);

 
        // if (myCannon!=null)
        // {
        //     myCannon.GetComponent<NetworkIdentity>().AssignClientAuthority(base.connectionToClient);
        // }

    }

    public void SetCannon(Cannon cannon)
    {
        myCannon=cannon;
    }

    public void InstantiateShopItem(GameObject prefab)
    {
        GameObject.Instantiate(prefab, this.transform.position, this.transform.rotation);
    }
}

public enum BTTeam
{
    red,
    blue    

    ,nothing
}
