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
    public RectTransform itemDescPanel;
    public bool firstTime;

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
                int price = allItems[randItem].itemRarity * 1000;
                itemPanel.GetChild(slot).GetComponent<Image>().sprite = allItems[randItem].itemSprite;
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
                            player.GetComponent<PlayerInventory>().inventory[i].gameObject.SetActive(true);
                            player.GetComponent<PlayerInventory>().inventory[i].StartCoroutine(SlowUpdate());
                            SendUpdate("INVENTORY",i+","+slot);
                            break;
                        }
                        i++;
                    }
                    player.gold -= buyPrice[slot];
                    player.GetComponent<PlayerInventory>().newItem = true;
                }
                
                SendUpdate("BUY", player.gold+","+slot); 
            }
            if(IsClient) {
                string[] args = value.Split(',');
                int newGold = int.Parse(args[0]);
                int slot = int.Parse(args[1]);

                if(player.gold != newGold) {
                    itemPanel.GetChild(slot).GetChild(1).gameObject.SetActive(true);
                }
                player.gold = newGold;

                AudioManager.Instance.CreateSource(AudioManager.Instance.audioClips[7]);
            }
        }
        if(flag == "SELL") {
            if(IsServer) {
                int slot = int.Parse(value);

                int price = player.GetComponent<PlayerInventory>().inventory[slot].itemRarity * 500;
                player.GetComponent<PlayerInventory>().inventory[slot] = null;
                player.gold += price;
                player.GetComponent<PlayerInventory>().newItem = true;

                SendUpdate("SELL", player.gold+","+slot);
            }
            if(IsClient) {
                string[] args = value.Split(',');
                int price = int.Parse(args[0]);
                int slot = int.Parse(args[1]);

                player.gold = price;
                player.GetComponent<PlayerInventory>().inventory[slot] = null;

                AudioManager.Instance.CreateSource(AudioManager.Instance.audioClips[10]);
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
        firstTime = true;
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
                        invPanel.GetChild(i).GetComponent<Image>().sprite = inventory[i].itemSprite;
                        invPanel.GetChild(i).GetChild(0).GetComponent<TextMeshProUGUI>().text = (inventory[i].itemRarity * 500).ToString();
                    }
                }
            }

            yield return new WaitForSeconds(MyCore.MasterTimer);
        }
    }

    void Update()
    {
        if(IsClient) {
            if(player.gold < 500) {
                refreshButton.interactable = false;
            }
            else if(player.gold >= 500) {
                refreshButton.interactable = true;
            }
            itemDescPanel.transform.position = Input.mousePosition;
        }
    }

    public void RefreshShop() {
        SendCommand("REFRESH", "true");
        refreshButton.interactable = false;
    }

    public IEnumerator Refresh() {
        if(!firstTime) {
            player.gold -= 500;
            player.SendUpdate("GOLD", player.gold.ToString());
        }
        for(int i=0; i<itemsForSale.Length; i++) {
            if(IsServer) {
                int randItem = Random.Range(0,allItems.Length);
                int price = allItems[randItem].itemRarity * 1000;
                itemsForSale[i] = allItems[randItem];
                buyPrice[i] = price;

                SendUpdate("REFRESH", i+","+randItem);
            }

            yield return new WaitForSeconds(MyCore.MasterTimer);
        }

        firstTime = false;
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
        itemDescPanel.gameObject.SetActive(true);
        TextMeshProUGUI name = itemDescPanel.GetChild(0).GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI desc = itemDescPanel.GetChild(1).GetComponent<TextMeshProUGUI>();

        name.text = itemsForSale[slot].name;
        desc.text = itemsForSale[slot].itemDescription;
    }

    public void HoverInvItem(int slot) {
        itemDescPanel.gameObject.SetActive(true);
        TextMeshProUGUI name = itemDescPanel.GetChild(0).GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI desc = itemDescPanel.GetChild(1).GetComponent<TextMeshProUGUI>();

        name.text = inventory[slot].name;
        desc.text = inventory[slot].itemDescription;
    }

    public void UnhoverItem() {
        itemDescPanel.gameObject.SetActive(false);
    }
}
