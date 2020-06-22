using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Projectile : NetworkBehaviour 
{

    public ProjectileType projectileType;

    
    BTPlayer player;
    public override void OnStartAuthority() 
    {
        player=BTLocalGameManager.Instance.localPlayer;

        player.PlayerAuthorizedProjectileWasSpawned(this.gameObject);
    }



}

public enum ProjectileType
{
    Cannonball
    ,Lasershot
    ,Laserbeam
}
