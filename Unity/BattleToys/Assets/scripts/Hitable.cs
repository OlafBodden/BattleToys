using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Handles Damages, taken by an object
/// Each object, that can be hit by enemy, should have this behavior
/// </summary>
public class Hitable : MonoBehaviour
{
    
    BTObject btObject;
    float currentHealth;
    float maxHealth;

    //Called by BTObject, after it is placed
    public void Init(BTObject btObject, float maxHealth)
    {
        this.btObject=btObject;
        this.currentHealth=maxHealth;
        this.maxHealth=maxHealth;

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

    //Called by enemy-Shootable to apply damage
    public void TakeDamage(float amount, TakeDamageEffectTpye effectType)
    {
        currentHealth-=amount;


        if (currentHealth<0)
        {
            Die();
        } else
        {
            //ToDo: Play Hit-Effect
        }
    }


    void Die()
    {
        //ToDo: Play effect, bevor Destroy...
   
        Destroy(this.gameObject);
    }


}

public enum TakeDamageEffectTpye
{
    Nothing,
    GunshotHitSparcle,
    CannonballHitSparcle,
    LaserHitSparcle,
    Explosion
}
