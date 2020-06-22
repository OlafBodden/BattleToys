using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FadeOutAfterSeconds : MonoBehaviour
{
    public float seconds=15f;

    public float StartTime=0f;

    bool isFadingOut=false;

    Rigidbody _rigidbody;

    // Start is called before the first frame update
    void Start()
    {
        StartTime=Time.time;

        _rigidbody=GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!isFadingOut)
        {
            if (Time.time-seconds > StartTime)
            {
                isFadingOut=true;     
            }
        } else
        {   
            if (_rigidbody)
            {
                Destroy(_rigidbody);
            }

            Vector3 pos=this.transform.position;

            pos.y -= 2* Time.deltaTime;

            this.transform.position=pos;

            if (pos.y<-2) Destroy(this.gameObject);
        }
    }
}
