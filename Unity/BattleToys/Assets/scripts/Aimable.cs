using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Aimable :  MonoBehavior 
{
    public Transform horizontalTurningTransform;

    //z.B. Kanonenrohr, bewegliche Laserturmspitze usw.
    public Transform verticalTurningTransform;

    public Transform shootPositionTransform;

    public float horizontalTurnSpeed;

    public float verticalTurnSpeed;

    public Vector3 xQuaternion;

    public AimMode aimMode=AimMode.Nothing;

    //return true, if we are facing the target
    //targetPosition: transform.position of target (hitable)
    //fixedVerticalAngle: Set to true, if cannonshot (schiefer Wurf), otherwise false
    //initialShotSpeed: only neccessary, if fixedVerticalAngle==true. InitialSpeed of cannonball/Schiefer Wurf
    public bool Aim(Vector3 targetPosition, bool fixedVerticalAngle=false, float initialShotSpeed=0)
    {

        if (aimMode==AimMode.Nothing) return true; //Nothing to do. We aimed and are ready to fire

        

        if (aimMode==AimMode.Horizontal)
        {
            return RotateHorizontal(targetPosition);        
        }

        if (aimMode==AimMode.Vertical)
        {
            return fixedVerticalAngle==false ? RotateVertical(targetPosition) : RotateVerticalForCannonballshot(targetPosition,initialShotSpeed);
        }

        if (aimMode==AimMode.HorizontalAndVertical)
        {
            bool h= RotateHorizontal(targetPosition);
            bool v= fixedVerticalAngle==false ? RotateVertical(targetPosition) : RotateVerticalForCannonballshot(targetPosition,initialShotSpeed);

            return (h==v==true);


        }

        return false;
    }

    bool RotateHorizontal(Vector3 targetPosition)
    {
        targetPosition.y=horizontalTurningTransform.position.y;
        Quaternion desiredRotation=Quaternion.LookRotation(targetPosition-horizontalTurningTransform.position);
        horizontalTurningTransform.rotation=Quaternion.RotateTowards(horizontalTurningTransform.rotation,desiredRotation,horizontalTurnSpeed*Time.deltaTime);
        if (Quaternion.Angle(horizontalTurningTransform.rotation,desiredRotation)<0.2f) return true;   

        return false;
    }

    bool RotateVertical(Vector3 targetPosition)
    {
        Quaternion desiredRotation=Quaternion.LookRotation(targetPosition-horizontalTurningTransform.position);
        
        Vector3 from=horizontalTurningTransform.localEulerAngles;
        Vector3 to=desiredRotation.eulerAngles;

        to.y=from.y;
        to.z=from.z;

        verticalTurningTransform.localEulerAngles=Vector3.Lerp(from, to, verticalTurnSpeed*Time.deltaTime);

        if (Mathf.Abs(from.x-to.x)<0.2f) return true;

        return false;
    }

    bool RotateVerticalForCannonballshot(Vector3 targetPosition,float initialSpeed)
    {
        Vector3 from= horizontalTurningTransform.eulerAngles;

        //Vector3 to;
        Vector3 direction;
        
        float to=SetTargetWithSpeed(shootPositionTransform.position,targetPosition,initialSpeed,false, out direction);
 

        SetTurret(direction, to * Mathf.Rad2Deg);



        // from.x=Mathf.Lerp(from.x,to,verticalTurnSpeed*Time.deltaTime);

        // //horizontalTurningTransform.localEulerAngles=from;

        // verticalTurningTransform.eulerAngles=Vector3.Lerp(from, to, verticalTurnSpeed*Time.deltaTime);

        // if (Mathf.Abs(from.x-to.x)<0.2f) return true;

        // return false;

        return true;

    }

    public float SetTargetWithSpeed(Vector3 shootpoint, Vector3 point, float speed, bool useLowAngle, out Vector3 direction)
    {
        float currentSpeed = speed;
        float currentAngle;

        direction = point - shootpoint;
        float yOffset = direction.y;
        direction = Math3d.ProjectVectorOnPlane(Vector3.up, direction);
        float distance = direction.magnitude;     

        float angle0, angle1;
        bool targetInRange = ProjectileMath.LaunchAngle(speed, distance, yOffset, Physics.gravity.magnitude, out angle0, out angle1);

        if (targetInRange)
            currentAngle = useLowAngle ? angle1 : angle0; 

        Debug.Log($"SetTargetWithSpeed({point},{speed},{useLowAngle})  targetInRange: {targetInRange} angle0: {angle0} angle1: {angle1}");
        
        

        return angle1;

                            

        

    }

    private void SetTurret(Vector3 planarDirection, float turretAngle)
    {
        verticalTurningTransform.localRotation = Quaternion.AngleAxis(1-turretAngle, Vector3.right);//*Quaternion.Euler(turretCorrector.x,turretCorrector.y,turretCorrector.z);         
        //verticalTurningTransform.localRotation = xQuaternion * Quaternion.AngleAxis(turretAngle, Vector3.left);        
    }
    
    
    
}

public enum AimMode
{
    Nothing, 

    Horizontal,

    Vertical,

    HorizontalAndVertical
}




