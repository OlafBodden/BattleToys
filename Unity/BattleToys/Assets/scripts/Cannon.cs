using System.Collections;
using System.Collections.Generic;
using Mirror;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;
using UnityEngine.UIElements;

public class Cannon : NetworkBehaviour
{


    public Transform centerOfMass;

    [AssetsOnly]
    public GameObject cannonBallPrefab;

    public Transform shootPosition;

    public Transform FireringMechanism;

    [Range(5,40)]
    public float shootPower = 10f;

    public float FireringMeachnismMaxZ=-1.74f;

    public float FireringMeachnismMinZ=-2.55f;
 
    public Transform cannonPipe;
    private Rigidbody _rigidbody;

    private Rigidbody cannonBallRigidBody;

    private Animator _animator;

    private NetworkAnimator _networkAnimator;

    public RectTransform shootPowerPanel;

    public float currentPower;

    public float powerIncreaseRate=2f;

    public float rotatePipeSpeed=5f;

    public GameObject prefabExplosion;

    public GameObject prefabShootEffect;

    public float cannonRotY=0f;

    // Start is called before the first frame update
    void Awake()
    {
        _rigidbody=GetComponent<Rigidbody>();

        _rigidbody.centerOfMass=centerOfMass.localPosition;

        //_animator=GetComponent<Animator>();

        //_networkAnimator=GetComponent<NetworkAnimator>();

        shootPowerPanel.sizeDelta=new Vector2(0,0);
    }


    private void FixedUpdate() 
    {
        PhysRotateCannon();
    }


    public override void OnStartAuthority() 
    {
        if (!base.hasAuthority) return;

        foreach(BTPlayer p in Transform.FindObjectsOfType<BTPlayer>())
        {
            if (p.hasAuthority)
            {
               // p.SetCannon(this);
            }
        }
    }

    [Client]
    void UpdatePower(float power)
    {
        currentPower=Mathf.Min(power,100f);
        shootPowerPanel.sizeDelta=new Vector2(power,0);

        SetFireringMechanismPosition();
    }

    [Client]
    void SetFireringMechanismPosition()
    {
        if (base.hasAuthority)
        {


            float deltaZ=(FireringMeachnismMinZ-FireringMeachnismMaxZ) * currentPower / 100f;

            Vector3 pos=FireringMechanism.localPosition;

            pos.z=FireringMeachnismMaxZ + deltaZ;

            FireringMechanism.localPosition=pos;
        }
    }

    [Client]
    public void ResetPower()
    {
        UpdatePower(0);
    }


    [Client]
    public void IncreasePower()
    {
        UpdatePower(currentPower+powerIncreaseRate * Time.deltaTime);  
    }

    [Client]
    public void Fire()
    {

        CmdFireCannon(currentPower/100f, shootPosition.position,cannonPipe.rotation);

        ResetPower();
    }

    [Command]
    void CmdFireCannon(float power, Vector3 pos, Quaternion rot)
    {
        //myCannon.CmdFireCannon();
        GameObject go=GameObject.Instantiate(cannonBallPrefab,pos,rot);
        Shot shot=go.GetComponent<Shot>();

        shot.Fire(power);
        //Rigidbody cannonBallRigidBody = go.transform.GetComponent<Rigidbody>();
        //cannonBallRigidBody.isKinematic=false;

        //go.transform.parent=myCannon.transform;

        //cannonBallRigidBody.transform.position=shootPosition.position;
        //cannonBallRigidBody.isKinematic=false;
        //cannonBallRigidBody.transform.parent=null;

        //cannonBallRigidBody.gameObject.AddComponent(typeof(FadeOutAfterSeconds));
        //cannonBallRigidBody.velocity=shootPower * cannonPipe.forward;

        NetworkServer.Spawn(go,base.connectionToClient);

        GameObject goEffect=GameObject.Instantiate(prefabShootEffect,pos,rot);
        NetworkServer.Spawn(goEffect,base.connectionToClient);
    }

    [Client]
    public void RotateCannonPipe(int rotX)
    {
        if (!base.isServer)
        {
            cannonPipe.Rotate(Vector3.left,rotX * rotatePipeSpeed * Time.deltaTime);
        }

        CmdRotateCannonPipe(rotX);

    }

    [Command]
    void CmdRotateCannonPipe(int rotX)
    {
        cannonPipe.Rotate(Vector3.left,rotX * rotatePipeSpeed * Time.deltaTime);
    }

    void PhysRotateCannon()
    {
        if ((!base.isServer) && (cannonRotY!=0)) 
        {
            //this.transform.Rotate(Vector3.up,cannonRotY);
            GetComponent<Rigidbody>().MoveRotation(Quaternion.Euler(new Vector3(0, cannonRotY - 180, 0)));
            

            cannonRotY=0f;
        } else if (cannonRotY!=0)
        {
            CmdRotateCannon(cannonRotY);
        }
    }

    [Client]
    public void RotateCannon(int rotY)
    {
        if (base.hasAuthority)
        {
            cannonRotY+=rotY* rotatePipeSpeed * Time.deltaTime;
        }
       
    }

    [Command]
    void CmdRotateCannon(float cannonRotY)
    {
        GetComponent<Rigidbody>().MoveRotation(Quaternion.Euler(new Vector3(0, cannonRotY - 180, 0)));
        //this.transform.Rotate(Vector3.up,cannonRotY);
    }

    void OnCollisionEnter(Collision collisionInfo)
    {
        if (collisionInfo.transform.GetComponent<Shot>())
        {
            CmdSpanExplosion(this.transform.position);
        }
    }

    [Command]
    void CmdSpanExplosion(Vector3 pos)
    {
        GameObject explosion=GameObject.Instantiate(prefabExplosion,pos,Quaternion.identity);

        NetworkServer.Spawn(explosion,base.connectionToClient);
    }

}
