using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/*
    Camera-Movement like in Siedler

    Client only!

*/
public class BTPlayerCameraMovement : MonoBehaviour
{
    [SerializeField] Transform cameraTransform;

    [SerializeField] float movementSpeed=5f; 

    [SerializeField] float movementTime;        

    Vector3 newPosition;

    [SerializeField] float rotationAmount;

    Quaternion newRotation;

    [SerializeField] Vector3 zoomAmount;

    Vector3 newZoom;

    [SerializeField]  float minZoomDistance;

    [SerializeField]  float maxZoomDistance;



    // Start is called before the first frame update
    void Start()
    {
        newPosition=transform.position;
        newRotation=transform.rotation;
        newZoom=cameraTransform.localPosition;


    }

    // Update is called once per frame
    void Update()
    {
        HandleMovementInput();
    }

    void HandleMovementInput()
    {
        #region move
        if (Input.GetKey(KeyCode.W))
        {
            newPosition += (transform.forward * movementSpeed);
        }

        if (Input.GetKey(KeyCode.S))
        {
            newPosition += (transform.forward * -movementSpeed);
        }

        if (Input.GetKey(KeyCode.A))
        {
            newPosition += (transform.right * -movementSpeed);
        }

        if (Input.GetKey(KeyCode.D))
        {
            newPosition += (transform.right * movementSpeed);
        }
        #endregion move

        #region rotate
        if (Input.GetKey(KeyCode.Q))
        {
            newRotation*=Quaternion.Euler(Vector3.up*rotationAmount);
        }

        if (Input.GetKey(KeyCode.E))
        {
            newRotation*=Quaternion.Euler(Vector3.up*-rotationAmount);
        }
        #endregion rotate

        #region zoom
        if (Input.GetAxis("Mouse ScrollWheel") < 0) //back
        {
            newZoom-=zoomAmount;
        }

        if (Input.GetAxis("Mouse ScrollWheel") > 0) //forward
        {
            newZoom+=zoomAmount;
        }

        if ((newZoom.y<minZoomDistance) || (newZoom.y>maxZoomDistance))
        {
            newZoom=cameraTransform.localPosition;
        }
        #endregion zoom

        transform.position=Vector3.Lerp(transform.position, newPosition, Time.deltaTime * movementTime);
        transform.rotation=Quaternion.Lerp(transform.rotation,newRotation,Time.deltaTime * movementTime);
        cameraTransform.localPosition=Vector3.Lerp(cameraTransform.localPosition, newZoom, Time.deltaTime * movementTime);



        // Vector3 lp=cameraTransform.localPosition;
        // if (lp.y>maxZoomDistance) lp.y=maxZoomDistance;
        // if (lp.y<minZoomDistance) lp.y=minZoomDistance;
        // cameraTransform.localPosition=lp;

        
    }

    public static Vector3 ClampMagnitude(Vector3 v, float max, float min)
    {
        double sm = v.sqrMagnitude;
        if(sm > (double)max * (double)max) return v.normalized * max;
        else if(sm < (double)min * (double)min) return v.normalized * min;
        return v;
    }


}
