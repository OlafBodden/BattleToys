using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewWeaponType", menuName = "WeaponType", order = 52)]
public class WeapontTypeSO : ScriptableObject
{
    public WeaponType weapontType;

    public WeaponPhysicsType weaponPhysicsType;

    public ProjectileLivetimeType projectileLivetimeType;
 
}

public enum WeaponType
{
    Cannonball
    
    ,Rocket
    ,Laserbeam
    ,Pulselaser
    ,Bomb
    ,Gunshot
    ,Granade
    ,Sawblade

}

public enum WeaponPhysicsType
{
    Nothing
    ,CurveWithGravity
    ,LineNoGravity
    ,GravityOnly
    ,RotatingRigidbody
}

public enum ProjectileLivetimeType
{
    HitAndGone
    ,Permanent
    ,PulsedPermanent
}


