using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;


//BTObject can shoot. ToDo: Raname to Attackable
// If you want to see Hitable --> go there!
//ToDo: Make it a MonoBehavior instead. Let BTObject spawn shots

public class Shootable : NetworkBehaviour 
{
    
    private BTObject btObject;              //Reference to the BTObject, containing this instance
    private ShootableStats shootableStats;  //Reference to the object's ShootableStats


    public GameObject shotPrefab;

    public Transform shootPosition;

    float lastReloadTime=0f;

    public float attackRange=10f;

    public float initialShootSpeed=10f;


    Hitable enemyToShootAt;
    bool isAttacking;

    public delegate void EnemyIsGone();

    EnemyIsGone EnemyIsGoneDelegate;

    [Client]
    public void Init(BTObject btObject, ShootableStats shootableStats)
    {
        this.btObject=btObject;
        this.shootableStats = shootableStats;
    }

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
    }

    /// <summary>
    /// Shoot a bullet/cannonball/rocket/laser
    /// </summary>
    
    void Shoot()
    {
        if ((lastReloadTime + shootableStats.reloadTime) < Time.time)
        {
            CmdCreateShot(shootPosition.position, shootPosition.rotation, initialShootSpeed);
            //ToDo: Use a more performant way, e.g. Pooling
            //GameObject go=GameObject.Instantiate(shotPrefab, shootPosition.position, shootPosition.rotation );

            //NetworkServer.Spawn(go, base.connectionToClient);

            //go.transform.position=shootPosition.position;
            //
            //go.transform.rotation=shootPosition.rotation;


            lastReloadTime=Time.time;

        }
    }

    [Command] 
    void CmdCreateShot(Vector3 pos, Quaternion rot, float initialShootSpeed)
    {
        GameObject go=GameObject.Instantiate(shotPrefab, pos, rot);

        NetworkServer.Spawn(go, base.connectionToClient);

        Shot shot=go.GetComponent<Shot>();

        shot.RpcFire(initialShootSpeed);

    }

    /// <summary>
    /// for debug only
    /// </summary>
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
            //Debug.Log("CheckIfTargetStillAttackable - enemyToShootAt==null");
            //isAttacking=false;
            return false;
        }


        //Check if we have an enemy-Target
        if (enemyToShootAt.IsAlive()==false)
        {
            //Debug.Log("CheckIfTargetStillAttackable - enemyToShootAt.IsAlive()==false");
            //isAttacking=false;
            return false;
        }

        //Check, if enemy target is inside range
        if (Vector3.Distance(this.transform.position,enemyToShootAt.transform.position)>attackRange)
        {
            //Debug.Log($"CheckIfTargetStillAttackable - Distance ({Vector3.Distance(this.transform.position,enemyToShootAt.transform.position)}) > attackRange ({attackRange})");
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

        Debug.Log($"Attack - hitable: {enemyToShootAt.gameObject.name} isAttacking: {isAttacking}" );
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
