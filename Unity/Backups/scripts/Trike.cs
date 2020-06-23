using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Trike : NetworkBehaviour
{

    [SerializeField] Transform centerOfMass;
    Rigidbody _rigidbody;



    void Awake()
    {
        _rigidbody=GetComponent<Rigidbody>();

        _rigidbody.centerOfMass=centerOfMass.localPosition;
    }
}
