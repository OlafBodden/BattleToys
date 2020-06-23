using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


/// <summary>
/// The shop contains many shopItemSlots, in which the ShopItems are placed
/// --> The shop contains many shopItemSlots. One ShopItemSlot contains 1 ShopItem or nothing
/// This class is responsible for handling user-clicks on a shopItem(-Slot)
/// </summary>
public class ShopItemSlot : MonoBehaviour
{
    ShopItem shopItem;
    Shop shop;

    /// <summary>
    /// Gets activated by code. See Shop
    /// </summary>
    /// <param name="shopItem"></param>
    /// <param name="shop"></param>
    public void Activate(ShopItem shopItem, Shop shop)
    {
        this.shopItem=shopItem;
        this.shop=shop;

        this.gameObject.SetActive(true);

        this.transform.GetChild(0).GetComponent<Image>().sprite=shopItem.icon;
    }

    /// <summary>
    /// Deactivates a shopItemSlot. 
    /// Used by the Shop. As there can be more shopItemSlots than ShopItems, not all of the predefined ShopItemSlots are neccessary
    /// </summary>
    public void Deactivate()
    {
        this.gameObject.SetActive(false);
    }

    /// <summary>
    /// Callback from UI. User has clicked on a shopItemSlot
    /// </summary>
    public void Click()
    {
        if ((shop.player!=null) && (shop.player.IsShopping())) //Check, if everything is OK
        {
            //Tell the player to instantiate the prefab, that is contained by the shopItem, he clicked on
            shop.player.InstantiateShopItem(shopItem.prefab);
        }
    }
}
