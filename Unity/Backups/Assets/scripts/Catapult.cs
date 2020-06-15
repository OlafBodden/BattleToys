using System.Collections;
using System.Collections.Generic;
using Mirror;
using Sirenix.OdinInspector;
using UnityEngine;

public class Catapult : NetworkBehaviour
{

     public Transform centerOfMass;

    [AssetsOnly]
    public GameObject cannonBallPrefab;
    // Start is called before the first frame update

    Rigidbody _rigidbody;


    void Awake()
    {
        _rigidbody=GetComponent<Rigidbody>();

        _rigidbody.centerOfMass=centerOfMass.localPosition;
    }

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
