using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BTObjectView : MonoBehaviour
{
    public Canvas hudCanvas;

    public Image healthBarMask;

    Hitable hitable;

    void OnEnable()
    {
        if (hitable==null) hitable=this.transform.GetComponent<Hitable>();
        
        if (hitable)
        {
            hitable.OnHealthValueChanged +=OnHealthValueChange;
        } else
        {
            hudCanvas.enabled=false;
        }
    }

     void OnDisable()
    {
        if (hitable)
        {
            hitable.OnHealthValueChanged -=OnHealthValueChange;
        }
    }

    void OnHealthValueChange(float percentage)
    {
        healthBarMask.fillAmount=percentage;
    }


}
