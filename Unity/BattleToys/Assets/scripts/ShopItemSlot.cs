using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShopItemSlot : MonoBehaviour
{
    ShopItem shopItem;
    Shop shop;

    public void Activate(ShopItem shopItem, Shop shop)
    {
        this.shopItem=shopItem;
        this.shop=shop;

        this.gameObject.SetActive(true);

        this.transform.GetChild(0).GetComponent<Image>().sprite=shopItem.icon;
    }

    public void Deactivate()
    {
        this.gameObject.SetActive(false);
    }

    public void Click()
    {
        Debug.Log("I was clicked: " + shopItem.name);

        shop.player.InstantiateShopItem(shopItem.prefab);
    }
}
