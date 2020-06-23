using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hitable : MonoBehaviour
{
    
    BTObject btObject;
    float currentHealth;
    float maxHealth;

    // Start is called before the first frame update
    public void Init(BTObject btObject, float maxHealth)
    {
        this.btObject=btObject;
        this.currentHealth=maxHealth;
        this.maxHealth=maxHealth;

    }

    //Called by Shootable to check, if this hitable can still be shot (or allready exploding)
    public bool IsAlive()
    {
        return (currentHealth>0); //ToDo: e.g. false, if object not destroyed, but in explode-mode
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
