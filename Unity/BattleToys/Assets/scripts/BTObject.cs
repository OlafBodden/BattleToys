using System.Collections;
using System.Collections.Generic;
using Mirror;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;
using UnityEngine.UIElements;

public class BTObject : NetworkBehaviour
{
    public BTObjectType btObjectType;

    BTPlayer player;
    public override void OnStartAuthority() 
    {
        player=BTLocalGameManager.Instance.localPlayer;

        player.PlayerAuthorizedBTObjectWasSpawned(this.gameObject);
    
    }

    public void Destroy()
    {
       CmdDestroy();     

    }

    [Command]
    void CmdDestroy()
    {
        NetworkServer.Destroy(this.gameObject);
    }

    void Update() 
    {
        
    }

}

public enum BTObjectType
{
    Cannon
    ,Catapult
    ,Laserturm
    ,Trke
        
    

}
