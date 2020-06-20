using System.Collections;
using System.Collections.Generic;
using Mirror;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;
using UnityEngine.UIElements;

public class BTObject : NetworkBehaviour
{
    public BTObjectSO btObjectSO;

    [Header("Stats")]
    public float maxHealth=100f;
    public float costs=100f;

    //public BTObjectType btObjectType;

    private Moveable moveable;
    private Shootable shootable;

    private Selectable selectable;

    private Aimable aimable;

    private Hitable hitable;


    MoveAndAttackState moveAndAttackState=MoveAndAttackState.Nothing;

    Hitable enemyToShootAt;



    BTPlayer player;
    public override void OnStartAuthority() 
    {
        player=BTLocalGameManager.Instance.localPlayer;

        player.PlayerAuthorizedBTObjectWasSpawned(this.gameObject);

        moveable=this.transform.GetComponent<Moveable>();
        shootable=this.transform.GetComponent<Shootable>();
        selectable=this.transform.GetComponent<Selectable>();
        aimable=this.transform.GetComponent<Aimable>();
        hitable=this.transform.GetComponent<Hitable>();

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

    //Used as Delegate-Function in shootable.Attack. Is invoked, when shootable lost its hitable
    //    (out of range, or destroyed)
    public void AttackLostHitable()
    {
        
        enemyToShootAt.DeSelectAsTarget();

        enemyToShootAt=null;
        moveAndAttackState=MoveAndAttackState.Nothing;
    }

    public void Destroy()
    {
       CmdDestroy();     

    }

    public void MoveAndAttack(Hitable enemyToShootAt)
    {

        enemyToShootAt?.SelectAsTarget();
        moveAndAttackState=MoveAndAttackState.Move;
    
        moveable.MoveToDestination(enemyToShootAt.transform.position);
        shootable?.SetTarget(enemyToShootAt);


        
        this.enemyToShootAt=enemyToShootAt;
        
    }

    public void CancelMoveAndAttack()
    {
        moveAndAttackState=MoveAndAttackState.Nothing;

        moveable?.StopMoving();

        shootable.GetTarget().transform.GetComponent<Selectable>()?.DeSelectAsTarget();
        shootable.CancelAttack();
        
        //this.enemyToShootAt=null;
    }

    public void Move()
    {

    }

    public void StopMoving()
    {

    }

    bool Aim()
    {
        Debug.Log($"Aim() enemyToShootAt==null? {(enemyToShootAt==null ? 1 : 0) }");
        if (enemyToShootAt==null) return false;

        // to do: smooth things
        this.transform.LookAt(enemyToShootAt.transform.position);

        return true;
    }

    public void Attack()
    {

    }

    public void StopAttacking()
    {

    }

    public void SelectObjectAsTarget()
    {
        if (selectable)
        {
            selectable.SelectAsTarget();
        }
    }

    public void DeSelectObjectAsTarget()
    {
        if (selectable)
        {
            selectable.DeSelectAsTarget();
        }
    }

    [Command]
    void CmdDestroy()
    {
        NetworkServer.Destroy(this.gameObject);
    }


}





public enum MoveAndAttackState
{
    Nothing,

    Move,

    Aiming,

    Attack
}
