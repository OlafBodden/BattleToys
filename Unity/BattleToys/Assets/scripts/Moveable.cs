using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.AI;

public class Moveable : NetworkBehaviour
{

    BTPlayer player;

    Rigidbody _rigidbody;
    NavMeshAgent agent;
 
    [SyncVar]
    Vector3 realPosition = Vector3.zero;
    [SyncVar]
    Quaternion realRotation;

     
    private float updateInterval;
 
    public void Init(BTPlayer player)
    {
        this.player=player;
        agent = GetComponent<NavMeshAgent>();
        _rigidbody=GetComponent<Rigidbody>();
        _rigidbody.isKinematic=true;

        Debug.Log(this.gameObject.name + "has authority: " + base.hasAuthority.ToString());
    }
 
    void Update()
    {
        if (base.hasAuthority)
        {
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
            transform.position = Vector3.Lerp(transform.position, realPosition, 0.1f);
            transform.rotation = Quaternion.Lerp(transform.rotation, realRotation, 0.1f);
        }
    }

    public void MoveToDestination(Vector3 destination)
    {
        if (agent.isStopped) agent.isStopped=false;
        agent.destination = destination;
        Debug.Log("MoveToDestination: " + destination.ToString());

        GameObject go=GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.transform.position=destination;
   
    }

    public bool HasReachedDestination()
    {
        return agent.pathStatus==NavMeshPathStatus.PathComplete;
    }

    public bool IsInsideDestinationRange(float range)
    {
        if (agent==null) return true;
        if (agent.pathStatus==NavMeshPathStatus.PathComplete) return true;
        if (Vector3.Distance(agent.destination,this.transform.position)<=range) return true;

        return false;
    }

    public void StopMoving()
    {
        agent.isStopped=true;
        agent.destination=this.transform.position;
        

    }
 
    [Command]
    void CmdSync(Vector3 position, Quaternion rotation)
    {
        realPosition = position;
        realRotation = rotation;
    }
}
