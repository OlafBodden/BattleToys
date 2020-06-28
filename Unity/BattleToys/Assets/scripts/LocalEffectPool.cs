using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;


public class LocalEffectPool : SerializedMonoBehaviour
{
    //[SerializeField] PoolSetting[] poolSettings;
    public Dictionary<EffectType,LocalSingleEffectPool> poolDictionary;



    void Awake()
    {
        LocalSingleEffectPool[] effectPools= GetComponentsInChildren<LocalSingleEffectPool>();
        poolDictionary=new Dictionary<EffectType, LocalSingleEffectPool>();
        foreach (LocalSingleEffectPool effectPool in effectPools)
        {
            effectPool.Init();
            poolDictionary.Add(effectPool.effectType,effectPool);
        }
    }

    public GameObject GetObjectFromPool(EffectType effectType)
    {
        return poolDictionary[effectType]?.GetObjectFromPool();
    }

    public void  ReturnGameObjectToPool(GameObject go, EffectType effectType)
    {
        poolDictionary[effectType]?.ReturnGameObjectToPool(go);
    }

    public void ReturnGameObjectToPool(LocalEffect le)
    {
        poolDictionary[le.effectType].ReturnGameObjectToPool(le.gameObject);
    }


}



[Serializable]
public class PoolSetting
{
    public EffectType effectType;
    public GameObject prefab;
    public int size=10;
}

public enum EffectType
{
    Nothing,
    SparkleSmall
    ,SparcleMedium
    ,SparcleBig
    ,ExplosionSmall
    ,ExplosionMedium
    ,ExplosionBig
}
