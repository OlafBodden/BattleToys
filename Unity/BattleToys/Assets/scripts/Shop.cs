using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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


        for (int i=0; i<shopItems.Count; i++)
        {
            ShopItem item=shopItems[i];
            itemSlots[i].GetComponent<ShopItemSlot>().Activate(item, this);
            //itemSlots[i].gameObject.SetActive(true);
            //itemSlots[i].transform.GetChild(0).GetComponent<Image>().sprite=item.icon;
        }

        for (int i=shopItems.Count; i<itemSlots.Length; i++)
        {
            itemSlots[i].gameObject.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
