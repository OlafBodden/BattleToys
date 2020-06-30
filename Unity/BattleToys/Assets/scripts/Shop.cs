﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class Shop : MonoBehaviour
{
    [SerializeField] GameObject itemPrefab;

    public List<ShopItem> shopItems;

    public Transform[] itemSlots;

    public BTPlayer player;

    private Shop instance;

    public Shop Instance { get { return instance; }}

    void Awake()
    {
        instance=this;
        
    }


    // Start is called before the first frame update
    void Start()
    {
        //Shop is not open... Hide all slots.
        for (int i=0; i<itemSlots.Length; i++)
        {
            itemSlots[i].gameObject.SetActive(false);
        }
        this.gameObject.SetActive(false);
    }

    public void OpenShop(BTPlayer player)
    {
        this.player=player;


        //Shop is now open. Show slots.        
        for (int i=0; i<shopItems.Count; i++)
        {
            ShopItem item=shopItems[i];
            itemSlots[i].GetComponent<ShopItemSlot>().Activate(item, this);
        }

        transform.DOMove(new Vector3(150,0,0),1f,false).From(true);//.SetRelative();
    }

    public void PlayerFinishedShopping()
    {
        player.FinishedShopping();

    }



    public void Hide()
    {
        

        transform.DOMove(new Vector3(150,0,0),1f,false).SetRelative(true).OnComplete(DisableMe);//.SetRelative();
    }

    void DisableMe()
    {
        this.gameObject.SetActive(false);
    }
}
