using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Shot : Mirror.NetworkBehaviour
{
    // Start is called before the first frame update        public float destroyAfter = 5;
    public Rigidbody rigidBody;
    public float maxForce = 50000;

    public float destroyAfter = 12;

    public override void OnStartServer()
    {
        Invoke(nameof(DestroySelf), destroyAfter);
    }

    // set velocity for server and client. this way we don't have to sync the
    // position, because both the server and the client simulate it.
    public void Fire(float percentage)
    {
        rigidBody=GetComponent<Rigidbody>();
        rigidBody.AddForce(transform.forward * maxForce * percentage);
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
}
