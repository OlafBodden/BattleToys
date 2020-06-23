using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Placeable : MonoBehaviour
{

    BTPlayer player;    //Reference to our player
    BTObject btObject;  //reference to BTObject of this gameobject

    Camera cam;         //Reference to players camera. We need this for raycasts

    Rigidbody rigidbody;    //Reference to rididbody of this gameobject. 
                            //We need this to disable it during placing and reenable it after

    Projector markingProjector;     //projector for displaying our marking-ring


    float collisionCheckRadius=0f;  //if other objects are inside this range, we can't place
                                    //Will be set to markingProjector orthographic-size

    int layerOriginal;              //Needed to shift this gameobjects and all of it's children to Placing-Layer, while placing


    
    //Called by BTPlayer
    //- starts placing this object by mouse
    //- shifts this gameobject to Placing-Layer
    //- disable rigidbody
    //- shows placing-marking-ring
    public void Init(BTPlayer player, BTObject btObject)
    {
        //<Get References>
        this.player=player;
        this.btObject = btObject;
        cam =player.GetComponentInChildren<Camera>();
        rigidbody = GetComponent<Rigidbody>();

        markingProjector = GetComponentInChildren<Projector>();
        if (markingProjector == null) Debug.LogError("No Projector found");
        //</Get References>

        //Shift layer of this gameobject (and its children) to fixed Layer "Placeable"
        //This will be reverted, when placing is finished
        layerOriginal = this.gameObject.layer;
        SetLayerRecursivly(LayerMask.NameToLayer("Placeable"));

        //Enable Marking-Ring and set it's default values
        markingProjector.enabled=true;
        collisionCheckRadius=markingProjector.orthographicSize;
        markingProjector.material.color=Color.red;

        //Tell the BTObject, that we are in Place-Mode
        btObject.OnStartPlacing();

    }

    // Update is called once per frame
    void Update()
    {
        if (btObject==null) return; //Wait until we are initialized

        //Checks if we raycast-hit the floor.
        //if not, we cannot place
        //if yes, check if other objects are inside collisionRadius--> if yes, we cannot plye
        //otherwise we are able to place
        //placing itself is NOT done here. It's done by BTPlayer by calling PlaceObject()
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
            
        } else
        {
            Debug.Log($"markingProjector {markingProjector.gameObject.name}");
            markingProjector.material.color = Color.red;
        }
     
        //Debug.DrawRay(ray.origin, ray.direction * 100, Color.green);
    }

    //Called by BTPlayer to place this object
    //Returns this gameobject on success
    //Returns NULL if we cannot place this gameobject
    public GameObject PlaceObject()
    {
        //Check, if we are able to place. ToDo: Make this more save
        if (markingProjector.material.color==Color.green)
        {
            SetLayerRecursivly(layerOriginal);

            markingProjector.enabled=false;

            Destroy(this); //Placeable is no longer needed

            return rigidbody.gameObject;
        } else
        {
            //Can't place here. 
            return null;
        }
    }

    //used to set the layer of GameObject and all its parents to the placement-Layer, while placing
    void SetLayerRecursivly(int layer)
    {
        foreach (Transform trans in GetComponentsInChildren<Transform>(true)) 
        {
            trans.gameObject.layer = layer;
        }
    }

    //Called by BTPlayer, when user cancels Placement
    public void CanclePlacement()
    {
        //Tell the BTObject, that it can remove this object.
        GetComponent<BTObject>().Destroy();    
    }
}
