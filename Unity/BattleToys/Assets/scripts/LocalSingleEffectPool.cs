using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class LocalSingleEffectPool : SerializedMonoBehaviour
{
    public EffectType effectType;
    public int size=10;
    public GameObject prefab;


    List<GameObject> inGame;
    Stack<GameObject> inStock;

    public void Init()
    {
        inGame=new List<GameObject>();
        inStock=new Stack<GameObject>();

        for (int i=0; i<size; i++)
        {
            GameObject go=GameObject.Instantiate(prefab, Vector3.zero, Quaternion.identity);
            go.transform.SetParent(this.transform);
            go.SetActive(false);


            inStock.Push(go);
        }
    }

    public GameObject GetObjectFromPool()
    {
        GameObject go=inStock?.Pop();

        if (go!=null) 
        {
            inGame.Add(go);
            go.SetActive(true);
        }

        return go;
    }

    public void ReturnGameObjectToPool(GameObject go)
    {
        if (go==null) return;

        if (inGame.Remove(go)==true)
        {
            go.SetActive(false);
            inStock.Push(go);
        }
    }
}
