﻿using System.Collections;
using System.Collections.Generic;
using Mirror;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;
using UnityEngine.UIElements;

public class BTObject : NetworkBehaviour
{
    //contains all object-type related stats
    public BTObjectSO btObjectSO;

    //prefab-related stats
    [Header("Stats")]
    public float maxHealth=100f;
    public float costs=100f;

    //instances of our Behaviors
    private Moveable moveable;
    private Shootable shootable;
    private Selectable selectable;
    private Aimable aimable;
    private Hitable hitable;

    //enum for MoveAimAttack-StateMachine-State
    MoveAndAttackState moveAndAttackState=MoveAndAttackState.Nothing;

    //Reference to Hitable-Behavior of a selected target
    Hitable enemyToShootAt;

    //Reference to our player
    BTPlayer player;

   
    bool isKinematic_original = false; //needed to enable/disable rigidbody

    public override void OnStartAuthority() 
    {
        //get our player-reference
        player=BTLocalGameManager.Instance.localPlayer;

        //Tell the player, that we are now part of his armee
        player.PlayerAuthorizedBTObjectWasSpawned(this.gameObject);

        //Get references to our behaviors
        moveable=this.transform.GetComponent<Moveable>();
        shootable=this.transform.GetComponent<Shootable>();
        selectable=this.transform.GetComponent<Selectable>();
        aimable=this.transform.GetComponent<Aimable>();
        hitable=this.transform.GetComponent<Hitable>();

        /* --> now done in StartPlacing()*/
        //Disable our behaviors. On start, we are in placing-Mode. Nothing to do for all other behaviors
        if (moveable) moveable.enabled = false;
        if (shootable) shootable.enabled = false;
        if (selectable) selectable.enabled = false;
        if (aimable) aimable.enabled = false;
        if (hitable) hitable.enabled = false;
        

        //Initialize our behaviors
        // moveable?.Init(this);
        hitable?.Init(this, maxHealth);


    
    }

    void Update()
    {
        if (moveAndAttackState==MoveAndAttackState.Move)
        {
            if (shootable.IsEnemeyInsideRange(enemyToShootAt))
            {
                moveable.StopMoving();

                moveAndAttackState=MoveAndAttackState.Aiming;
            }
        } else if (moveAndAttackState==MoveAndAttackState.Aiming)
        {
            if (aimable==null)
            {
                //nothing to aim
                moveAndAttackState=MoveAndAttackState.Attack;
                shootable.Attack(enemyToShootAt, AttackLostHitable);
            }
            else if (aimable.Aim(enemyToShootAt.transform.position,true, shootable.initialShootSpeed)==true) 
            {
                //Ziel ist anvisiert
               
                shootable.Attack(enemyToShootAt, AttackLostHitable);

            } 
        } else if ( moveAndAttackState==MoveAndAttackState.Attack)
        {
            if (aimable.Aim(enemyToShootAt.transform.position,true,shootable.initialShootSpeed)==false)
            {
                shootable.CancelAttack();
                moveAndAttackState=MoveAndAttackState.Aiming;
            }
        }
        


    }

    //Called by BTPlayer, when a new BTObject is beeing placed (--> attached to mouse)
    public void OnStartPlacing()
    {
        //Disable our behaviors. Later, this Behaviors are enabled, when matchStarts
        if (moveable) moveable.enabled = false;
        if (shootable) shootable.enabled = false;
        if (selectable) selectable.enabled = false;
        if (aimable) aimable.enabled = false;
        if (hitable) hitable.enabled = false;

        //Disable rigidbody (will be enabled after placing is finished
        if (GetComponent<Rigidbody>() != null)
        {
            isKinematic_original = GetComponent<Rigidbody>().isKinematic;
            GetComponent<Rigidbody>().detectCollisions = false;
            GetComponent<Rigidbody>().isKinematic = true;
            GetComponent<Rigidbody>().Sleep();
        }
    }

    //Called by BTPlayer, when a new BTObject is placed
    public void OnPlaced()
    {
        //Enbale our behaviors. 
        if (moveable)
        {
            moveable.enabled = true;
            moveable.Init(this.player, this);
        }
        if (shootable)
        {
            shootable.enabled = true;
            shootable.Init(this.player, this);
        }
        if (selectable)
        {
            selectable.enabled = true;
            selectable.Init(this.player);
        }
        if (aimable)
        {
            aimable.enabled = true;
            aimable.Init(this.player, this);
        }
        if (hitable)
        {
            hitable.enabled = true;
            hitable.Init(this, this.maxHealth);
        }

        //re-enable rigidbody
        if (GetComponent<Rigidbody>() != null)
        { 
            GetComponent<Rigidbody>().isKinematic = isKinematic_original;
            GetComponent<Rigidbody>().detectCollisions = true;
            GetComponent<Rigidbody>().WakeUp();
        }

        BTLocalGameManager.Instance.RegisterAsObject(this);
    }

    //Used as Delegate-Function in shootable.Attack. Is invoked, when shootable lost its hitable
    //    (out of range, or destroyed)
    public void AttackLostHitable()
    {
        
        enemyToShootAt.DeSelectAsTarget();

        enemyToShootAt=null;
        moveAndAttackState=MoveAndAttackState.Nothing;
    }

    //Called by Hitable, when we got killed
    //Called by Placeable, when placing is canceled by user
    public void Destroy()
    {
        BTLocalGameManager.Instance.DeRegisterAsObject(this);

        CmdDestroy();   

    }

    //Called by BTPlayer.
    // a) this BTObject is allready selected
    // AND b) another BTOject is selected as target. It is allready checken, that this is a BTObject of another player
    [Client]
    public void MoveAndAttack(Hitable enemyToShootAt)
    {
        if (!base.hasAuthority) return;

        //In case we are still doing a previous action: cancel it!
        CancelMoveAndAttack();

        //ask target-object to show selection-ring
        enemyToShootAt?.SelectAsTarget();

        //Step 1: We are now in Move-Mode (Move,Aim,Attack) (=Reset MoveAimAttack-StateMachine)
        moveAndAttackState=MoveAndAttackState.Move;

        //tell our BTObject to move toward the new target
        moveable.MoveToDestination(enemyToShootAt.transform.position);

        //tell our shootable allready the new Target (it does nothing else then registering it)
        shootable?.SetTarget(enemyToShootAt);
               
        //register target inside our BTObject
        this.enemyToShootAt=enemyToShootAt;
        
    }

    //Cancel Move/Aim/Attack: Reset MoveAimAttack-StateMachine
    [Client]
    public void CancelMoveAndAttack()
    {
        //Stop MoveAimAttack-StateMachine
        moveAndAttackState=MoveAndAttackState.Nothing;

        //Stop Moving. Stay where you curently are
        moveable?.StopMoving();

        //To do: In case of plane: return to base

        //Deselect target (hide selecting-ring)
        shootable.GetTarget().transform.GetComponent<Selectable>()?.DeSelectAsTarget();

        //Stop Attacking
        shootable.CancelAttack();

        //No Need to stop aiming...
        
        //we no longer have a target. Deregister it in our BTObject
        this.enemyToShootAt=null;
    }

    //Called by hitable-behavior (delegate)
    [Client]
    public void SelectObjectAsTarget()
    {
        if (!base.hasAuthority) return;

        selectable?.SelectAsTarget();
        
    }

    //Called by hitable-behavior (delegate)
    [Client]
    public void DeSelectObjectAsTarget()
    {
        if (!base.hasAuthority) return;

        selectable?.DeSelectAsTarget();
        
    }

    //Server-Call. Private. Only called by this.Destroy()
    [Command]
    void CmdDestroy()
    {
        NetworkServer.Destroy(this.gameObject);
    }

    //called by 
    public bool IsAlive()
    {
        if (hitable==null) return true;

        return hitable.IsAlive();
    }


}




//Enums for the MoveAimAttack-StateMachine
public enum MoveAndAttackState
{
    Nothing,

    Move,

    Aiming,

    Attack
}
