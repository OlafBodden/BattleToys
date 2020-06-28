using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocalEffect : MonoBehaviour
{
    public EffectType effectType;
    private ParticleSystem _particleSystem;

    [SerializeField] float lifeTime=1f;
    
    // Start is called before the first frame update
    void Awake()
    {
        _particleSystem=GetComponentInChildren<ParticleSystem>();

    }

    void OnEnable()
    {
        _particleSystem.Simulate(0,true,true);
        _particleSystem.Play();

        Invoke(nameof(ReturnToStock), lifeTime);
    }

    void ReturnToStock()
    {
        BTLocalGameManager.Instance.ReturnLocalEffectToStock(this);
    }
}
