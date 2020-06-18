using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewBTObject", menuName = "BTObject", order = 54)]
public class BTObjectSO : ScriptableObject
{
    public BTObjectCategory btObjectCategory;
    public BTObjectType btObjectType;

    public WeapontTypeSO weaponSystem;

    [Header("Selectable")]
    public bool isSelectable;
    public bool selectTargetWhenSelected;
    public bool selectBaseWhenSelected;
    public bool showMoveTargetPositionWhenSelected;
    public bool showStatsWhenSelected ;

    [Header("Moveable")]
    public bool canMove;
    public MoveType moveType;
    public bool stopWhenInShootRange;
    public bool autoReturnToBase;

    public bool hasStartLandingPhasis;


    [Header("Aimable")]
    public bool canAim;
    public bool needHorzontalAimingBevoreAttack;
    public bool needVerticalAimingBevoreAttack;
     
    public AimType aimType;


    [Header("Shootable/Attackable")]
    public bool canShoot;
    public TargetingMode targetingMode;
    public bool canShootToLand;
    public bool canShootToAir;
    public bool needToReload;
    public float reloadAfterSeconds;
    public bool reloadAfterEachShot;
    public float reloadTime;

    [Header("Capture")]
    bool canCaptureChest;
    bool canCaptureTheFlag;
    bool canCaptureEnemyUnits;
    bool canCaptureEnemyBuildings;



}



public enum BTObjectCategory
{
    Unit
    ,Building
    

}

public enum BTObjectType
{
    Cannon
    ,Catapult
    ,Trike
    ,Laserturm
    ,Tank
    ,Luftabwehrtank
    ,LaserRobot
    ,SawbladeRobot
    ,GunInfantrie
    ,Laserinfantrie
    ,GranateInfantrie
    ,Laserzaun
    ,Reperatureinheit
    ,GunfireHeli
    ,RocketHeli
    ,HeliPort
    ,Plane
    ,PlanePort
    ,RepairStation
    ,MainTower
    ,Chest
    ,TransportTruck
    ,TransformerTruck
    ,TransformerRobot
    ,Block
    ,Luftschiff
    ,Luftabwehrstellung
    ,ShieldGenerator
    ,Rocketthrower
    ,GranateThrower
    ,TransportPlane

}

public enum MoveType
{
    NoMovement
    ,NavMeshAgent
    ,FlyingLinear
    ,PhysicsMovement

}

public enum TargetingMode
{
    Nothing
    ,PlayerSelectsTarget
    ,SelectAutoTarget_Closest
    ,SelectAutoTarget_Weakest
    ,SelectAutoTarget_Random
}

public enum AimType
{
    Nothing
    ,AimDirectlyOntoTarget
    ,AimForCannonballshot
    ,AimForBombThrow
}


