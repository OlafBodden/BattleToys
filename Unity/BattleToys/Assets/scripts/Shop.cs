using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shop : MonoBehaviour
{
    [SerializeField] GameObject itemPrefab;

    public List<ShopItem> shopItems;


    // Start is called before the first frame update
    void Start()
    {
        foreach (ShopItem item in shopItems)
        {
            
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
