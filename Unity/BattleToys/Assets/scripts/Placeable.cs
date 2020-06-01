using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class Placeable : MonoBehaviour
{
    public GameObject circlePrefab;

    public Material materialGreen;

    public Material materialRed;

    BTPlayer player; 
    Camera cam;

    Rigidbody rigidbody;

    Transform markingCircle;

    bool isKinematic_original=false; 

    public void Init(BTPlayer player)
    {
        this.player=player;
        cam=player.GetComponentInChildren<Camera>();
        this.gameObject.layer=LayerMask.NameToLayer("Placeable");

        rigidbody=GetComponent<Rigidbody>();

        if (rigidbody!=null)
        {
            isKinematic_original=rigidbody.isKinematic;
            rigidbody.detectCollisions=false;
            rigidbody.isKinematic=true;
        }

        markingCircle=GameObject.Instantiate(circlePrefab,this.transform.position,Quaternion.identity).transform;



    }

    // Update is called once per frame
    void Update()
    {
        Ray ray = cam.ScreenPointToRay (Input.mousePosition);
        RaycastHit hit;
        int layerMask=LayerMask.GetMask("Floor");
        if (Physics.Raycast(ray, out hit,  100, layerMask))
        {

            Debug.DrawLine(ray.origin, hit.point);
            this.transform.position=hit.point;
            markingCircle.position=this.transform.position;
            Debug.Log("Hit with " + hit.transform.gameObject.name);
            
        }
     
        //Debug.DrawRay(ray.origin, ray.direction * 100, Color.green);
    }

    public void PlaceObject()
    {
        rigidbody.isKinematic=isKinematic_original;
        rigidbody.detectCollisions=true;
        

        Destroy(this); //Placeable is no longer needed
    }

    public void CanclePlacement()
    {
        Destroy(this.gameObject);
    }
}
