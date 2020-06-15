using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hitable : MonoBehaviour
{
    


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //Called by Shootable to check, if this hitable can still be shot (or allready exploding)
    public bool IsAlive()
    {
        return true; //ToDo: e.g. false, if object not destroyed, but in explode-mode
    }

    public void SelectAsTarget()
    {
        BTObject bTObject=this.transform.GetComponent<BTObject>();

        bTObject?.SelectObjectAsTarget();
    }

    public void DeSelectAsTarget()
    {
        BTObject bTObject=this.transform.GetComponent<BTObject>();

        bTObject?.DeSelectObjectAsTarget();
    }
}
