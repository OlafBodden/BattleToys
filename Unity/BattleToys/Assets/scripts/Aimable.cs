using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Aimable :  NetworkBehaviour 
{
    public Transform turnableBaseTransform;

    //z.B. Kanonenrohr, bewegliche Laserturmspitze usw.
    public Transform nozzleTransform;

    public float baseTurnSpeed;

    public float aimingSpeed;

    public AimMode aimMode=AimMode.Nothing;

    //return true, if we are facing the target
    public bool Aim(Vector3 targetPosition)
    {

        if (aimMode==AimMode.Nothing) return true; //Nothing to do. We aimed and are ready to fire

        

        if (aimMode==AimMode.LeftRight)
        {
            return RotateHorizontal(targetPosition);
            
        }

        

/*
        if (aimMode==AimMode.LeftRight)
        {
            if (desiredNozzleRotation==null) return true; 

            Vector3 vX=nozzleTransform.localEulerAngles;

            vX.x=Mathf.Lerp(1,2,3);

            nozzleTransform.rotation=Quaternion.Lerp(nozzleTransform.rotation, (Quaternion)desiredNozzleRotation, aimingSpeed*Time.deltaTime);
            if (Quaternion.Angle(nozzleTransform.rotation,(Quaternion)desiredNozzleRotation)<0.2f) return true;
        }
        */

        return false;
    }

    bool RotateHorizontal(Vector3 targetPosition)
    {
        targetPosition.y=turnableBaseTransform.position.y;
        //Vector3 y=turnableBaseTransform.rotation.eulerAngles;

        //float desiredY=Quaternion.LookRotation(targetPosition-turnableBaseTransform.position).eulerAngles.y;

        //y.y=Mathf.Lerp(y.y,desiredY, aimingSpeed*Time.deltaTime);

        Quaternion desiredRotation=Quaternion.LookRotation(targetPosition-turnableBaseTransform.position);
        turnableBaseTransform.rotation=Quaternion.RotateTowards(turnableBaseTransform.rotation,desiredRotation,baseTurnSpeed*Time.deltaTime);
        //turnableBaseTransform.rotation(//.SetEulerAngles(y.x,y.y,y.z);

        //Debug.Log("")

        if (Quaternion.Angle(turnableBaseTransform.rotation,desiredRotation)<0.2f) return true;   

        return false;
    }
    
}

public enum AimMode
{
    Nothing, 

    LeftRight,

    UpDown,

    LeftRightUpDown
}


