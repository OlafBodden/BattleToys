using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Shot : Mirror.NetworkBehaviour
{
    // Start is called before the first frame update        public float destroyAfter = 5;
    public Rigidbody rigidBody;
    //public float maxForce = 50000;

    public float destroyAfter = 12;

    public float initialVelocity=30;

    [SyncVar]
    Vector3 realPosition = Vector3.zero;
    [SyncVar]
    Quaternion realRotation;
    private float updateInterval;

    private static float GRAVITY = 9.8067f;

    public override void OnStartServer()
    {
        Invoke(nameof(DestroySelf), destroyAfter);
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

    // set velocity for server and client. this way we don't have to sync the
    // position, because both the server and the client simulate it.
    [Client]
    public void Fire(float velocity)
    {
        rigidBody=GetComponent<Rigidbody>();

        if (base.hasAuthority)
        {
            rigidBody.isKinematic=false;

            //Vector3 velocity=GetStartVelocity2(target,this.transform.rotation.x);

            //For Cannonshot only
            rigidBody.velocity=transform.forward * velocity;

        } else
        {
            rigidBody.isKinematic=true;
        }
    }

    // destroy for everyone on the server
    [Server]
    void DestroySelf()
    {
        NetworkServer.Destroy(gameObject);
    }

    // ServerCallback because we don't want a warning if OnTriggerEnter is
    // called on the client
    [ServerCallback]
    void OnTriggerEnter(Collider co)
    {
        //NetworkServer.Destroy(gameObject);
    }

    [Command]
    void CmdSync(Vector3 position, Quaternion rotation)
    {
        realPosition = position;
        realRotation = rotation;
    }

     
 
    //Calculate, how far the bullet will fly
    public static float GetHorizontalDistance(float velocity, float launchAngle, bool degrees = false)
    {
        // convert to radians if necessary
        if (degrees)
        {
            launchAngle *= Mathf.Deg2Rad;
        }
    
        return (velocity * velocity * Mathf.Sin(2.0f * launchAngle)) / GRAVITY; //=distance
    }

/*
    //Calculate the start-velocity, to shoot a given distance at a given shoot-angle
    public static float GetStartVelocity(float distance, float launchAngle, bool degrees=false)
    {
        launchAngle=-10f;
        // convert to radians if necessary
        if (degrees)
        {
            launchAngle *= Mathf.Deg2Rad;
        }

        launchAngle=Mathf.Abs(launchAngle);

        

        Debug.Log($"GetStartVelocity({distance},{launchAngle},{degrees} result: {Mathf.Sqrt(distance * GRAVITY / Mathf.Sin(2.0f * launchAngle))}");

        return Mathf.Sqrt(distance * GRAVITY / Mathf.Sin(2.0f * launchAngle)) * 10;
    }

    public  Vector3 GetStartVelocity2(Vector3 target, float launchAngleDegree)
    {
        Vector3 p = target;
 
        float gravity = Physics.gravity.magnitude;
        // Selected angle in radians
        float angle = launchAngleDegree * Mathf.Deg2Rad;
 
        // Positions of this object and the target on the same plane
        Vector3 planarTarget = new Vector3(p.x, 0, p.z);
        Vector3 planarPostion = new Vector3(this.transform.position.x, 0, transform.position.z);
 
        // Planar distance between objects
        float distance = Vector3.Distance(planarTarget, planarPostion);
        // Distance along the y axis between objects
        float yOffset = transform.position.y - p.y;
 
        float initialVelocity = (1 / Mathf.Cos(angle)) * Mathf.Sqrt((0.5f * gravity * Mathf.Pow(distance, 2)) / (distance * Mathf.Tan(angle) + yOffset));
 
        Vector3 velocity = new Vector3(0, initialVelocity * Mathf.Sin(angle), initialVelocity * Mathf.Cos(angle));
 
        // Rotate our velocity to match the direction between the two objects
        float angleBetweenObjects = Vector3.Angle(Vector3.forward, planarTarget - planarPostion) * (p.x > transform.position.x ? 1 : -1);
        Vector3 finalVelocity = Quaternion.AngleAxis(angleBetweenObjects, Vector3.up) * velocity;

        return finalVelocity;
 
    }

    public float GetStartVelocity3(float distance, float yOffset, float gravity, float angle)
    {
        float speed = (distance * Mathf.Sqrt(gravity) * Mathf.Sqrt(1 / Mathf.Cos(angle))) / Mathf.Sqrt(2 * distance * Mathf.Sin(angle) + 2 * yOffset * Mathf.Cos(angle));

        return speed;
    }

    public float  GetStartVelocity4(Vector3 point, float angle)
    {
        float currentAngle = angle;

        Vector3 direction = point - this.transform.position;
        float yOffset = -direction.y;
        direction = Math3d.ProjectVectorOnPlane(Vector3.up, direction);
        float distance = direction.magnitude;

        float currentSpeed = ProjectileMath.LaunchSpeed(distance, yOffset, Physics.gravity.magnitude, angle * Mathf.Deg2Rad);

        return currentSpeed;
        //projectileArc.UpdateArc(currentSpeed, distance, Physics.gravity.magnitude, currentAngle * Mathf.Deg2Rad, direction, true);
        //SetTurret(direction, currentAngle);

        //currentTimeOfFlight = ProjectileMath.TimeOfFlight(currentSpeed, currentAngle * Mathf.Deg2Rad, yOffset, Physics.gravity.magnitude);
    }


    public void SetTargetWithSpeed(Vector3 point, float speed, bool useLowAngle)
    {
        float currentSpeed = speed;
        float currentAngle;

        Vector3 direction = point - this.transform.position;
        float yOffset = direction.y;
        direction = Math3d.ProjectVectorOnPlane(Vector3.up, direction);
        float distance = direction.magnitude;     

        float angle0, angle1;
        bool targetInRange = ProjectileMath.LaunchAngle(speed, distance, yOffset, Physics.gravity.magnitude, out angle0, out angle1);

        if (targetInRange)
            currentAngle = useLowAngle ? angle1 : angle0;                     

        //SetTurret(direction, currentAngle * Mathf.Rad2Deg);

    }
    */

}
