using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;


//BTObject can shoot.
// If you want to see Hitable --> go there!
public class Shootable : NetworkBehaviour 
{
    public GameObject shotPrefab;

    public Transform shootPosition;

    public float reloadTime=2f;

    public float shotVelocity=20;
    float lastReloadTime=0f;

    public float attackRange=10f;

    public float initialShootSpeed=10f;
    Hitable enemyToShootAt;
    bool isAttacking;

    public delegate void EnemyIsGone();

    EnemyIsGone EnemyIsGoneDelegate;

    void Update()
    {
        if(isAttacking==true)
        {
            if (CheckIfTargetStillAttackable())
            {
                //Attack it!
                Shoot();
            } else
            {
                //We have lost our tartet.

                EnemyIsGoneDelegate(); //Inform BTObject, that we have lost our target
                isAttacking=false;
            }
        }

        //DrawRangeGizmo();
    }

    void Shoot()
    {
        if ((lastReloadTime + reloadTime) < Time.time)
        {
            GameObject go=GameObject.Instantiate(shotPrefab, shootPosition.position, shootPosition.rotation );
            //GameObject go = BTLocalGameManager.Instance.localPlayer.GetCannonball();

            NetworkServer.Spawn(go, connectionToClient);

            go.transform.position=shootPosition.position;
            go.transform.rotation=shootPosition.rotation;
            Shot shot=go.GetComponent<Shot>();

            shot.Fire(initialShootSpeed);//enemyToShootAt.transform.position);

            lastReloadTime=Time.time;

        }

    }

    void OnDrawGizmosSelected()
    {
        if (isAttacking)
        {
            if (CheckIfTargetStillAttackable())
            {
                Gizmos.color = new Color(255,0,0,0.25f);    //red, when active and target is in range
            } else
            {
                
                Gizmos.color = new Color(0,0,255,0.25f);    //blue, when active and target out of range
            }
            
        } else
        {
            Gizmos.color = new Color(0,255,0,0.25f); //green, when we are idle
        }
        Gizmos.DrawSphere(transform.position, attackRange);

        if (enemyToShootAt)
        {
            Gizmos.DrawSphere(transform.position, Vector3.Distance(this.transform.position,enemyToShootAt.transform.position));
        }
    }

    public bool CheckIfTargetStillAttackable()
    {
        //Check, if we have an enemy-targe
        if (enemyToShootAt==null)
        {
            //isAttacking=false;
            return false;
        }


        //Check if we have an enemy-Target
        if (!enemyToShootAt.IsAlive())
        {
            //isAttacking=false;
            return false;
        }

        //Check, if enemy target is inside range
        if (Vector3.Distance(this.transform.position,enemyToShootAt.transform.position)>attackRange)
        {
            //isAttacking=false;
            return false;
        }

        return true;
    }

    public void SetTarget(Hitable enemyToShootAt)
    {
        this.enemyToShootAt=enemyToShootAt;
        isAttacking=false;
    }

    public void ReSetTarget()
    {
        this.enemyToShootAt=null;
        isAttacking=false;
    }

    public void Attack(Hitable enemyToShootAt, EnemyIsGone enemyIsGoneFunc)
    {
        this.enemyToShootAt=enemyToShootAt;
        this.EnemyIsGoneDelegate=enemyIsGoneFunc;
        isAttacking=CheckIfTargetStillAttackable();
    }
    
    public void CancelAttack()
    {
        this.enemyToShootAt=null;
        isAttacking=false;

    }

    public bool IsEnemeyInsideRange(Hitable enemyToShootAt)
    {
        if (enemyToShootAt==null) return false;

        if (Vector3.Distance(this.transform.position,enemyToShootAt.transform.position)>attackRange)
        {
            return false;
        } else
        {
            return true;
        }
    }

    public Hitable GetTarget()
    {
        return enemyToShootAt;
    }




}
