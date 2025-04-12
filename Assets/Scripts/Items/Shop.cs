using System.Collections;
using System.Collections.Generic;
using NETWORK_ENGINE;
using TMPro;
using UnityEngine;

public class Shop : NetworkComponent
{
    public ItemManager itemManager;
    public PlayerController[] shoppers;
    public PlayerInventory playerInventory;
    public ItemStats[] itemsForSale = new ItemStats[5];
    public ItemStats[] displayInventory = new ItemStats[10]; 
    public RectTransform invPanel;
    public RectTransform itemsPanel;
    public int[] sellPrice;
    public int[] buyPrice;

    public override void HandleMessage(string flag, string value)
    {
        if(flag == "SALEITEM") {
            string[] args = value.Split(',');
            int slot = int.Parse(args[0]);
            int randItem = int.Parse(args[1]);
            int price = int.Parse(args[2]);

            if(IsClient) {
                itemsForSale[slot] = itemManager.items[randItem];
                itemsPanel.GetChild(slot).GetChild(0).GetComponent<TextMeshProUGUI>().text = price + "G";
                buyPrice[slot] = price;
            }
        }
        if(flag == "BUY") {
            if(IsClient) {
                
            }
        }
    }

    public override void NetworkedStart()
    {
        shoppers = FindObjectsOfType<PlayerController>();
        RefreshShop();
    }

    public override IEnumerator SlowUpdate()
    {
        if(IsServer) {

        }
        if(IsLocalPlayer) {
            for(int i=0; i<10; i++) {
                if(displayInventory[i] != null) {
                    invPanel.GetChild(i).gameObject.SetActive(true);
                }
                else if(displayInventory[i] == null) {
                    invPanel.GetChild(i).gameObject.SetActive(false);
                }
            }
        }

        yield return new WaitForSeconds(MyCore.MasterTimer);
    }

    void Start()
    {
        
    }

    void Update()
    {
        
    }

    public void RefreshShop() {
        if(IsServer) {
            for(int i=0; i<5; i++) {
                int randItem = Random.Range(0, itemManager.items.Length);
                itemsForSale[i] = itemManager.items[randItem];
                
                int rarity = itemsForSale[i].itemRarity;
                switch(rarity) {
                    case 1:
                        buyPrice[i] = 100;
                        break;
                    case 2:
                        buyPrice[i] = 200;
                        break;
                    case 3:
                        buyPrice[i] = 300;
                        break;
                    case 4:
                        buyPrice[i] = 400;
                        break;
                    case 5:
                        buyPrice[i] = 500;
                        break;
                }
                SendUpdate("SALEITEM",i+","+randItem+","+buyPrice);
            }
        }
        if(IsClient) {

        }
    }

    public void BuyItem(int slot) {
        if(IsLocalPlayer) {
            playerInventory.inventory[0] = itemsForSale[slot];
            foreach(PlayerController shopper in shoppers) {
                if(shopper.Owner == playerInventory.Owner) {
                    shopper.gold -= buyPrice[slot];
                }
            }
        }
    }

    public void SellItem(int slot) {
        int rarity = playerInventory.inventory[slot].itemRarity;
        switch(rarity) {
            case 1:
                sellPrice[slot] = 50;
                break;
            case 2:
                sellPrice[slot] = 100;
                break;
            case 3:
                sellPrice[slot] = 150;
                break;
            case 4:
                sellPrice[slot] = 200;
                break;
            case 5:
                sellPrice[slot] = 250;
                break;
        }
    }

    public void HoverItem(int slot) {

    }
}
