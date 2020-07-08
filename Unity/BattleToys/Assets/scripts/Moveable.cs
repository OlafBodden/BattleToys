using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.AI;


/// <summary>
/// Responsible for Moving a BTObject
/// Is a Network Behavior. It syncs it's position and rotation over network
/// </summary>
public class Moveable : NetworkBehaviour
{

    //BTPlayer player;
    BTObject btObject;
    MoveableStats moveableStats;


    Rigidbody _rigidbody;
    NavMeshAgent agent;

    UnitBase myUnitBase;            //For planes and helis only. Stores reference to their base.

    [SyncVar]
    Vector3 realPosition = Vector3.zero;
    [SyncVar]
    Quaternion realRotation;

    Transform flyingDestinationTarget;
    FlyingStates flyingStates=FlyingStates.Landed;

    float flyStartLandSpeed=1f;
    float flySpeed=2f;
    float flyDesiredAltitude=15f;
    float flyStartLandCurrentLerpValue=0;
    float flyFlyLerpValue=0;


     
    private float updateInterval;   //Timer for synchronizing position and rotation
 


    public void Init(BTObject btObject, MoveableStats moveableStats)
    {
        Init(btObject, moveableStats,null);
    }

    public void Init(BTObject btObject, MoveableStats moveableStats, UnitBase unitBase=null)
    {
        this.btObject = btObject;
        this.moveableStats = moveableStats;
        if (unitBase!=null) this.myUnitBase = unitBase;

        _rigidbody = GetComponent<Rigidbody>();
        agent = GetComponent<NavMeshAgent>();

        if (moveableStats.canMove)
        {
            if (moveableStats.moveType == MoveType.NavMeshAgent)
            {
                InitNavMeshAgent();
            }
            else if (moveableStats.moveType == MoveType.PhysicsMovement)
            {
                InitPhysics();
            }
            else if (moveableStats.moveType==MoveType.FlyingLinear)
            {
                //no NavMesh-Agent needed, no Physics needed
                if (_rigidbody) _rigidbody.isKinematic = true;
                

            }
        }
    }

    public void SetBase(UnitBase unitBase)
    {
        this.myUnitBase=unitBase;
    }

    void InitNavMeshAgent()
    {
        _rigidbody.isKinematic = true;
    }

    void InitPhysics()
    {
        _rigidbody.isKinematic = false;
    }
 
    void Update()
    {
        if (base.hasAuthority)
        {
            //if we are an object owned by local player: Do our actions

            //ToDo: Currently only NavMesh-Movement is implemented. Impplement Physics and straight movement as well.
            if (moveableStats.moveType==MoveType.FlyingLinear)
            {
                switch (flyingStates)
                {
                    case FlyingStates.Starting:
                        FlyStarting();
                        break;

                    case FlyingStates.FlyingTowardsTarget:
                        FlyFlyingTowardsTarget();
                        break;
                }
            } 


            // update the server with position/rotation
            updateInterval += Time.deltaTime;
            if (updateInterval > 0.11f) // 9 times per second
            {
                updateInterval = 0;
                CmdSync(transform.position, transform.rotation);
            }
        }
        else if (realPosition!=Vector3.zero)
        {
            //If we are not an object owned by local player: Sync Position
            transform.position = Vector3.Lerp(transform.position, realPosition, 0.1f);
            transform.rotation = Quaternion.Lerp(transform.rotation, realRotation, 0.1f);
        }
    }

    /// <summary>
    /// Called by Player to Move this object to a certain position
    /// Currently only NavMesh-Movement is implemented
    /// ToDo: Implement Phyiscs and straigt movement
    /// </summary>
    /// <param name="destination"></param>
    public void MoveToDestination(Vector3 destination)
    {
        if (moveableStats.moveType==MoveType.NavMeshAgent)
        {
            if (agent.isStopped) agent.isStopped=false;
            agent.destination = destination;

        } 

        //GameObject go=GameObject.CreatePrimitive(PrimitiveType.Cube);
        //go.transform.position=destination;
   
    }

    public void FlyToTarget(Transform target)
    {
        flyingDestinationTarget=target;
        flyingStates=FlyingStates.Starting;
        flyStartLandCurrentLerpValue=0;
    }

    void FlyStarting()
    {       
        //Move to flyDesiredAltitude
        flyStartLandCurrentLerpValue+=flyStartLandSpeed * Time.deltaTime;
        this.transform.position=Vector3.Lerp(
            myUnitBase.GetLandingPosition(), 
            myUnitBase.GetLandingPosition() + Vector3.up*flyDesiredAltitude,
            flyStartLandCurrentLerpValue);

        //Rotate towards target
        this.transform.rotation=Quaternion.Lerp(
            myUnitBase.GetLandingRotation(),
            Quaternion.LookRotation(flyingDestinationTarget.position - myUnitBase.GetLandingPosition()),
            flyStartLandCurrentLerpValue
            );
        

        if (flyStartLandCurrentLerpValue>=1)
        {
            flyingStates=FlyingStates.FlyingTowardsTarget;
            flyFlyLerpValue=0;
        }
    }

    void FlyFlyingTowardsTarget()
    {
        Vector3 startPosition=myUnitBase.GetLandingPosition();
        Vector3 targetPosInAir=flyingDestinationTarget.position;
        startPosition.y=this.transform.position.y;
        targetPosInAir.y=this.transform.position.y;

        flyFlyLerpValue+=flySpeed*Time.deltaTime;

        this.transform.position=Vector3.Lerp(startPosition,targetPosInAir,flyFlyLerpValue);
    }

    void FlyFlyingBackToBase()
    {

    }

    void FlyLanding()
    {

    }

    /// <summary>
    /// Checks, if we have reached our destination
    /// Currently only NavMesh-Movement is implemented
    /// ToDo: Implement Phyiscs and straigt movement
    /// </summary>
    /// <returns></returns>
    public bool HasReachedDestination()
    {
        return agent.pathStatus==NavMeshPathStatus.PathComplete;
    }

    /// <summary>
    /// Checks, if we are close to our target
    /// Currently only NavMesh-Movement is implemented
    /// ToDo: Implement Phyiscs and straigt movement
    /// </summary>
    /// <param name="range"></param>
    /// <returns></returns>
    public bool IsInsideDestinationRange(float range)
    {
        if (agent==null) return true;
        if (agent.pathStatus==NavMeshPathStatus.PathComplete) return true;
        if (Vector3.Distance(agent.destination,this.transform.position)<=range) return true;

        return false;
    }

    /// <summary>
    /// Immediatly stops movement
    /// Currently only NavMesh-Movement is implemented
    /// ToDo: Implement Phyiscs and straigt movement
    /// </summary>
    public void StopMoving()
    {
        if (moveableStats.moveType==MoveType.NavMeshAgent)
        {
            agent.isStopped=true;
            agent.destination=this.transform.position;
        }

    }
 
    /// <summary>
    /// Server-Command to sync position and rotation of this object
    /// </summary>
    /// <param name="position"></param>
    /// <param name="rotation"></param>
    [Command]
    void CmdSync(Vector3 position, Quaternion rotation)
    {
        realPosition = position;
        realRotation = rotation;
    }
}

public enum FlyingStates
{
    Starting,
    FlyingTowardsTarget,
    Attacking,
    FlyingBackToBase,

    Landing,

    Landed
}
