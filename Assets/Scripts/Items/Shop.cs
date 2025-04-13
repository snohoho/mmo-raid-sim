using System.Collections;
using System.Collections.Generic;
using NETWORK_ENGINE;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Shop : NetworkComponent
{
    public PlayerController player;
    public ItemStats[] allItems;
    public RectTransform itemPanel;
    public ItemStats[] itemsForSale;
    public int[] buyPrice;
    public RectTransform invPanel;
    public ItemStats[] inventory;
    public int[] sellPrice;
    public TextMeshProUGUI currentGold;
    public Button refreshButton;

    public override void HandleMessage(string flag, string value)
    {
        if(flag == "REFRESH") {
            if(IsServer) {
                StartCoroutine(Refresh());
            }
            if(IsClient) {
                string[] args = value.Split(',');
                int slot = int.Parse(args[0]);
                int randItem = int.Parse(args[1]);

                itemsForSale[slot] = allItems[randItem];
                int price = allItems[randItem].itemRarity * 100;
                itemPanel.GetChild(slot).GetChild(0).gameObject.GetComponent<TextMeshProUGUI>().text = price + "G"; 
                itemPanel.GetChild(slot).GetChild(1).gameObject.SetActive(false);
                if(slot == 4) {
                    refreshButton.interactable = true;
                }
            }
        }
        if(flag == "BUY") {
            if(IsServer) {
                int slot = int.Parse(value);

                if(player.gold - buyPrice[slot] >= 0) {
                    int i=0;
                    foreach(ItemStats item in player.GetComponent<PlayerInventory>().inventory) {
                        if(item == null) {
                            player.GetComponent<PlayerInventory>().inventory[i] = itemsForSale[slot];
                            SendUpdate("INVENTORY",i+","+slot);
                            break;
                        }
                        i++;
                    }
                    player.gold -= buyPrice[slot];
                }
                
                SendUpdate("BUY", player.gold+","+slot); 
            }
            if(IsClient) {
                string[] args = value.Split(',');
                int price = int.Parse(args[0]);
                int slot = int.Parse(args[1]);

                player.gold = price;
                if(player.gold == price) {
                    itemPanel.GetChild(slot).GetChild(1).gameObject.SetActive(true);
                }
            }
        }
        if(flag == "SELL") {
            if(IsServer) {
                int slot = int.Parse(value);

                int price = player.GetComponent<PlayerInventory>().inventory[slot].itemRarity * 50;
                player.GetComponent<PlayerInventory>().inventory[slot] = null;
                player.gold += price;

                SendUpdate("SELL", player.gold+","+slot);
            }
            if(IsClient) {
                string[] args = value.Split(',');
                int price = int.Parse(args[0]);
                int slot = int.Parse(args[1]);

                player.gold = price;
                player.GetComponent<PlayerInventory>().inventory[slot] = null;
            }
        }
        if(flag == "INVENTORY") {
            if(IsClient) {
                string[] args = value.Split(',');
                int invSlot = int.Parse(args[0]);
                int buySlot = int.Parse(args[1]);

                player.GetComponent<PlayerInventory>().inventory[invSlot] = itemsForSale[buySlot];
            }
        }
    }

    public override void NetworkedStart()
    {
        allItems = FindAnyObjectByType<ItemManager>().items;
        RefreshShop();
    }

    public override IEnumerator SlowUpdate()
    {
        while(IsConnected) {
            if(!IsLocalPlayer) {
                transform.GetChild(3).gameObject.SetActive(false);
            }
            if(IsLocalPlayer) {
                if(player.inShop) {
                    transform.GetChild(0).gameObject.SetActive(false);
                    transform.GetChild(3).gameObject.SetActive(true);
                    Cursor.lockState = CursorLockMode.None;
                }
                if(!player.inShop) {
                    transform.GetChild(0).gameObject.SetActive(true);
                    transform.GetChild(3).gameObject.SetActive(false);
                    Cursor.lockState = CursorLockMode.Locked;
                }

                currentGold.text = player.gold + "G";
                inventory = player.GetComponent<PlayerInventory>().inventory;
                for(int i=0; i<inventory.Length; i++) {
                    if(inventory[i] == null) {
                        invPanel.GetChild(i).gameObject.SetActive(false);
                    }
                    else if(inventory[i] != null) {
                        invPanel.GetChild(i).gameObject.SetActive(true);
                        invPanel.GetChild(i).GetChild(0).GetComponent<TextMeshProUGUI>().text = (inventory[i].itemRarity * 50).ToString();
                    }
                }
            }

            yield return new WaitForSeconds(MyCore.MasterTimer);
        }
    }

    void Update()
    {
        
    }

    public void RefreshShop() {
        Debug.Log("REFRESH");
        SendCommand("REFRESH", "true");
        refreshButton.interactable = false;
    }

    public IEnumerator Refresh() {
        for(int i=0; i<itemsForSale.Length; i++) {
            if(IsServer) {
                int randItem = Random.Range(0,allItems.Length);
                int price = allItems[randItem].itemRarity * 100;
                itemsForSale[i] = allItems[randItem];
                buyPrice[i] = price;

                SendUpdate("REFRESH", i+","+randItem);
            }

            yield return new WaitForSeconds(MyCore.MasterTimer);
        }
    }

    public void BuyItem(int slot) {
        Debug.Log("BUY " + slot);
        if((inventory != null || inventory.Length != 0) && !itemPanel.GetChild(slot).GetChild(1).gameObject.activeSelf) {
            SendCommand("BUY",slot.ToString());
        } 
    }

    public void SellItem(int slot) {
        if(inventory != null || inventory.Length != 0) {
            SendCommand("SELL",slot.ToString());
        }
    }

    public void HoverShopItem(int slot) {

    }

    public void HoverInvItem(int slot) {

    }
}
