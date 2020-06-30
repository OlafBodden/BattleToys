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

    private WeapontTypeSO weaponTypeStats;
    public GameObject shotPrefab;

    public LineRenderer lineRenderer;

    public Transform shootPosition;

    float lastReloadTime=0f;

    float lastLaserApplyDamageTime=0f;





    public float attackRange=10f;

    public float initialShootSpeed=10f;

    public LayerMask layerMaskToShootAt;


    Hitable enemyToShootAt;
    bool isAttacking;

    public delegate void EnemyIsGone();

    EnemyIsGone EnemyIsGoneDelegate;

    float updateIntervall;  //For Syncing Vars with Server/Clients: LineRenderer

    [Client]
    public void Init(BTObject btObject, ShootableStats shootableStats, WeapontTypeSO weaponTypeStats)
    {
        this.btObject=btObject;
        this.shootableStats = shootableStats;
        this.weaponTypeStats=weaponTypeStats;
        this.lineRenderer=GetComponent<LineRenderer>();

        if (lineRenderer) 
        {
            lineRenderer.enabled=false;
            CmdDisableLineRenderer();
        }
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
        if (!shootableStats.canShoot) return;

        if (weaponTypeStats.weapontType==WeaponType.Cannonball) HandleCannonball();

        if (weaponTypeStats.weapontType==WeaponType.Laserbeam) HandleLaserBeam();

    }

    void HandleCannonball()
    {
        if (shootableStats.needToReload)
        {
            if ((lastReloadTime + shootableStats.reloadTime) < Time.time)
            {
                CmdCreateShot(shootPosition.position, shootPosition.rotation, initialShootSpeed);

                lastReloadTime=Time.time;
            }
        }

        
    }

    void HandleLaserBeam()
    {
        if (lineRenderer.enabled==false) 
        {
            lineRenderer.enabled=true;
            CmdEnableLineRenderer();
        }

        //Set lineRenderer Positions
        lineRenderer.SetPosition(0,this.shootPosition.position);
        lineRenderer.SetPosition(1,this.enemyToShootAt.GetPreferedHitPosition());

        //Sync Line Renderer
        updateIntervall+=Time.deltaTime;
        if (updateIntervall> 0.11f) // 9 times per second)
        {
            CmdSetLineRenderer(this.shootPosition.position,this.enemyToShootAt.GetPreferedHitPosition());
        }

        //Apply Damage
        if (lastLaserApplyDamageTime + 0.5f < Time.time)
        {
            enemyToShootAt.CmdTakeDamage(
                5f,
                enemyToShootAt.GetPreferedHitPosition(),
                (enemyToShootAt.GetPreferedHitPosition()-shootPosition.position).normalized,EffectType.SparkleSmall
            );

            lastLaserApplyDamageTime=Time.time;
        }
    }

    [Command]
    void CmdSetLineRenderer(Vector3 pos1, Vector3 pos2)
    {
        RpcSetLineRenderer(pos1,pos2);
    }



    [Command]
    void CmdDisableLineRenderer()
    {
        RpcDisableLineRenderer();
    }

    [Command]
    void CmdEnableLineRenderer()
    {
        RpcEnableLineRenderer();
    }

    [ClientRpc(excludeOwner=true)]
    void RpcSetLineRenderer(Vector3 pos1, Vector3 pos2)
    {
        if (lineRenderer)
        {
            lineRenderer.SetPosition(0,pos1);
            lineRenderer.SetPosition(1,pos2);
        }
    }

    [ClientRpc(excludeOwner=true)]
    void RpcEnableLineRenderer()
    {
        if (lineRenderer) lineRenderer.enabled=true;
    }

    [ClientRpc(excludeOwner=true)]
    void RpcDisableLineRenderer()
    {
        if (lineRenderer) lineRenderer.enabled=false;
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

    public Hitable GetAutoTarget(TargetingMode targetingMode)
    {
        Collider[] colliders=Physics.OverlapSphere(shootPosition.position, attackRange,layerMaskToShootAt);

        if (colliders==null) return null;
        if (colliders.Length<=0) return null;

        if (targetingMode==TargetingMode.SelectAutoTarget_Closest)
        {
            Hitable closest=null;
            Hitable current=null;
            float minDistance=float.MaxValue;
            float currentDistance=0;

            for (int i=0; i<colliders.Length; i++)
            {
                current=colliders[i].transform.GetComponentInParent<Hitable>();

                if (current)
                {
                    //is current an enemy?
                    if (current.connectionToClient!=this.connectionToClient)
                    {
                        //Is this closer than the closest we got allready?
                        if (closest==null)
                        {
                            closest=current;
                            minDistance=Vector3.Distance(shootPosition.position, current.preferedHitPositionTransform.position);
                        } else
                        {
                            currentDistance=Vector3.Distance(shootPosition.position, current.preferedHitPositionTransform.position);
                            if (currentDistance<minDistance)
                            {
                                minDistance=currentDistance;
                                closest=current;

                            }
                        }
                    }
                }
            }

            return closest;
        }

        return null;
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

        if (lineRenderer) lineRenderer.enabled=false;
        CmdDisableLineRenderer();
    }

    public void Attack(Hitable enemyToShootAt, EnemyIsGone enemyIsGoneFunc)
    {
        if (enemyToShootAt==this.enemyToShootAt) return;

        this.enemyToShootAt=enemyToShootAt;
        this.EnemyIsGoneDelegate=enemyIsGoneFunc;
        isAttacking=CheckIfTargetStillAttackable();

        Debug.Log($"Attack - hitable: {enemyToShootAt.gameObject.name} isAttacking: {isAttacking}" );
    }
    
    public void CancelAttack()
    {
        this.enemyToShootAt=null;
        isAttacking=false;

        if (lineRenderer) lineRenderer.enabled=false;
        CmdDisableLineRenderer();

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
