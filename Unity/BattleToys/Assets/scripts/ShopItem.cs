using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "NewShopItem", menuName = "ShopItem", order = 51)]
public class ShopItem : ScriptableObject
{

    public new string name="defaultName";
    public GameObject prefab;

    public Sprite icon;

    public string description = "No description available";

    public int costPoints=10;


}
