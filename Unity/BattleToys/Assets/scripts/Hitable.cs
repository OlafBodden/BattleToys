using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;



/// <summary>
/// Handles Damages, taken by an object
/// Each object, that can be hit by enemy, should have this behavior
/// </summary>
public class Hitable : NetworkBehaviour
{
    
    public Transform preferedHitPositionTransform;
    BTObject btObject;

    [SyncVar]
    float currentHealth;

    [SyncVar]
    float maxHealth;

    public delegate void HealthValueChanged(float newHealthValuePercentage);
	public event HealthValueChanged OnHealthValueChanged;

    //Called by BTObject, after it is placed
    public void Init(BTObject btObject, float maxHealth)
    {
        this.btObject=btObject;
        this.currentHealth=maxHealth;
        this.maxHealth=maxHealth;

        if (OnHealthValueChanged!=null) OnHealthValueChanged(currentHealth/maxHealth);
    }

    //Called by enemy-Shootable to check, if this hitable can still be shot (or allready exploding)
    //Called by our BTObject to check if we are alive
    public bool IsAlive()
    {
        return (currentHealth>0); //ToDo: e.g. false, if object not destroyed, but in explode-mode
    }

    //Called by enemy-Shootable to target this 
    public void SelectAsTarget()
    {
        BTObject bTObject=this.transform.GetComponent<BTObject>();

        bTObject?.SelectObjectAsTarget();
    }

    //Called by enemy-Shootable to unregister it as target
    public void DeSelectAsTarget()
    {
        BTObject bTObject=this.transform.GetComponent<BTObject>();

        bTObject?.DeSelectObjectAsTarget();
    }

    // //Called by enemy-Shootable to apply damage
    // public void TakeDamage(float amount, TakeDamageEffectTpye effectType)
    // {
    //     if (!base.hasAuthority) return;

    //     CmdTakeDamage(amount);


    //     //ToDo: Play Hit-Effect


    // }

    public Vector3 GetPreferedHitPosition()
    {
        return preferedHitPositionTransform.position;
    }

    [Command(ignoreAuthority = true)]
    public void CmdTakeDamage(float amount, Vector3 hitPosition, Vector3 hitNormal, EffectType effectType)
    {
        Debug.Log($"Hitable - CmdTakeDamage({amount})");
        currentHealth-=amount;

        RpcTakeDamage(currentHealth, hitPosition, hitNormal, effectType);
    }

    [ClientRpc]
    public void RpcTakeDamage(float newCurrentHealth, Vector3 hitPosition,  Vector3 hitNormal,EffectType effectType)
    {
        currentHealth=newCurrentHealth;

        Debug.Log($"Hitable - RpcTakeDamage({newCurrentHealth})");

        if (OnHealthValueChanged!=null) OnHealthValueChanged(currentHealth/maxHealth);

        if (currentHealth>0) //If we are still alive
        {
            PlayHitEffect(hitPosition,  hitNormal, effectType);

        } else //If we are dying
        {
            PlayHitEffect(hitPosition, hitNormal, EffectType.ExplosionMedium);
        }

        if (base.hasAuthority)
        {
            if (currentHealth<=0)
            {
                Invoke("Die",0.1f);
            } 
        }
    }

    [Client]
    void PlayHitEffect(Vector3 hitPosition,  Vector3 hitNormal,EffectType effectType)
    {
        //ToDo: Add some sound here

        Debug.Log($"PlayHitEffect({this.gameObject.name}). Now play Effect of type {effectType} at position {hitPosition}" );

        BTLocalGameManager.Instance.PlayLocalEffect(effectType,hitPosition,Quaternion.Euler(hitNormal));
    }

    void Die()
    {
        //Let the Server destroy this object
        btObject.Destroy();
    }


}

// public enum TakeDamageEffectTpye
// {
//     Nothing,
//     GunshotHitSparcle,
//     CannonballHitSparcle,
//     LaserHitSparcle,
//     Explosion
// }
