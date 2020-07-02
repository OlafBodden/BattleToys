using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewBTObject", menuName = "BTObject", order = 54)]
public class BTObjectSO : ScriptableObject
{
    public BTObjectCategory btObjectCategory;
    public BTObjectType btObjectType;

    public WeapontTypeSO weaponSystem;

    public SelectableStats selectableStats;
    public MoveableStats moveableStats;
    public AimableStats aimableStats;
    public ShootableStats shootableStats;
    public CaptureStats captureStats;




}

[Serializable]
public class MoveableStats
{
    public bool canMove;
    public MoveType moveType;
    public bool stopWhenInShootRange;
    public bool autoReturnToBase;

    public bool hasStartLandingPhasis;
}

[Serializable]
public class SelectableStats
{
    public bool isSelectable;
    public bool selectTargetWhenSelected;
    public bool selectBaseWhenSelected;
    public bool showMoveTargetPositionWhenSelected;
    public bool showStatsWhenSelected;
}

[Serializable]
public class AimableStats
{
    
    public bool canAim;
    public bool needHorzontalAimingBevoreAttack;
    public bool needVerticalAimingBevoreAttack;
     
    public AimType aimType;
}

[Serializable]
public class ShootableStats
{
    public bool canShoot;
    public TargetingMode targetingMode;
    public bool canShootToLand;
    public bool canShootToAir;
    public bool needToReload;
    public float reloadAfterSeconds;
    public bool reloadAfterEachShot;
    public float reloadTime;

    public int ammoAmount=1;

    public bool reloadsAtItsBase;
}

[Serializable]
public class CaptureStats
{
    public bool canCaptureChest;
    public bool canCaptureTheFlag;
    public bool canCaptureEnemyUnits;
    public bool canCaptureEnemyBuildings;
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
    ,AimForCatapultshot
    ,AimForRocketShot
    ,AimForBombThrow
}


