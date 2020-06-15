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

    private Moveable moveable;
    private Shootable shootable;

    private Selectable selectable;

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
    
    }

    void Update()
    {
        if (moveAndAttackState==MoveAndAttackState.Move)
        {
            if (shootable.IsEnemeyInsideRange(enemyToShootAt))
            {
                moveable.StopMoving();

                moveAndAttackState=MoveAndAttackState.Attack;
                shootable.Attack(enemyToShootAt, AttackLostHitable);
            } else if (moveable.HasReachedDestination())
            {
                //CancelMoveAndAttack();
                              
            }
        }

        if (moveAndAttackState==MoveAndAttackState.Attack)
        {

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

public enum BTObjectType
{
    Cannon
    ,Catapult
    ,Laserturm
    ,Trke
}



public enum MoveAndAttackState
{
    Nothing,

    Move,

    Attack
}
