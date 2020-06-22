using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class Placeable : MonoBehaviour
{


    BTPlayer player; 
    Camera cam;

    Rigidbody rigidbody;

    Transform markingCircle;

    Projector markingProjector;
    float collisionCheckRadius=0f;

    bool isKinematic_original=false; 

    int layerOriginal;

    Selectable selectable;
    Moveable moveable;

    Shootable shootable;


    public void Init(BTPlayer player)
    {
        this.player=player;
        cam=player.GetComponentInChildren<Camera>();
        layerOriginal=this.gameObject.layer;
        SetLayerRecursivly(LayerMask.NameToLayer("Placeable"));

        rigidbody=GetComponent<Rigidbody>();

        if (rigidbody!=null)
        {
            isKinematic_original=rigidbody.isKinematic;
            rigidbody.detectCollisions=false;
            rigidbody.isKinematic=true;
            rigidbody.Sleep();
        }

        markingProjector=GetComponentInChildren<Projector>();
        if (markingProjector==null) Debug.LogError("No Projector found");

        markingProjector.enabled=true;
        collisionCheckRadius=markingProjector.orthographicSize;
        markingProjector.material.color=Color.red;

        selectable=GetComponent<Selectable>();
        moveable=GetComponent<Moveable>();
        shootable=GetComponent<Shootable>();

        if (selectable) selectable.enabled=false;
        if (moveable) moveable.enabled=false;
        if (shootable) shootable.enabled=false;
        

    }

    // Update is called once per frame
    void Update()
    {
        Ray ray = cam.ScreenPointToRay (Input.mousePosition);
        RaycastHit hit;
        int layerMaskFloor=LayerMask.GetMask("Floor");
        int layerMaskObstacle=LayerMask.GetMask(new string[]{"Units","Buildings"});
        if (Physics.Raycast(ray, out hit,  100, layerMaskFloor))
        {

            //Debug.DrawLine(ray.origin, hit.point);
            this.transform.position=hit.point;
            //Debug.Log("Hit with " + hit.transform.gameObject.name);

            if (Physics.OverlapSphere(this.transform.position,collisionCheckRadius,layerMaskObstacle).Length>0)
            {
                markingProjector.material.color=Color.red;
            } else
            {
                markingProjector.material.color=Color.green;
            }
            
        }
     
        //Debug.DrawRay(ray.origin, ray.direction * 100, Color.green);
    }

    public GameObject PlaceObject()
    {
        if (markingProjector.material.color==Color.green)
        {
            rigidbody.isKinematic=isKinematic_original;
            rigidbody.detectCollisions=true;
            rigidbody.WakeUp();

            SetLayerRecursivly(layerOriginal);

            markingProjector.enabled=false;

            if (selectable) 
            {
                selectable.enabled=true;
                selectable.Init(this.player);
            }

            if (moveable) 
            {
                moveable.enabled=true;
                moveable.Init(this.player);
            }
            if (shootable) shootable.enabled=true;


            Destroy(this); //Placeable is no longer needed

            return rigidbody.gameObject;
        } else
        {
            return null;
        }
    }

    void SetLayerRecursivly(int layer)
    {
        foreach (Transform trans in GetComponentsInChildren<Transform>(true)) 
        {
            trans.gameObject.layer = layer;
        }
    }

    public void CanclePlacement()
    {
        GetComponent<BTObject>().Destroy();

        
    }
}
