using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Courtain : MonoBehaviour
{
    [SerializeField] GameObject goExplosion;

    public void RaiseCourtain()
    {
        goExplosion.SetActive(true);

        //Debug.Log("RaiseCourtain " + this.gameObject.name);

        foreach (Rigidbody r in this.gameObject.GetComponentsInChildren<Rigidbody>())
        {
            //Debug.Log(r.gameObject.name);
            r.AddExplosionForce(7500,Vector3.zero,300);
        }

        Invoke("Destroy", 8);
    }

    void Destroy()
    {
        Destroy(this.gameObject);
    }
}
