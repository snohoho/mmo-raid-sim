using System.Collections;
using System.Collections.Generic;
using NETWORK_ENGINE;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Shop : NetworkComponent
{
    public ItemStats[] items;
    public PlayerController[] shoppers;
    public PlayerInventory playerInventory;
    public ItemStats[] itemsForSale = new ItemStats[5];
    public ItemStats[] displayInventory = new ItemStats[10]; 
    public RectTransform invPanel;
    public RectTransform itemsPanel;
    public int[] sellPrice = new int[5];
    public int[] buyPrice = new int[5];
    public Button refreshButton;
    public bool refreshing;

    public override void HandleMessage(string flag, string value)
    {
        if(flag == "SALEITEM") {
            string[] args = value.Split(',');
            int slot = int.Parse(args[0]);
            int randItem = int.Parse(args[1]);

            if(IsClient) {
                Debug.Log(slot + " " + randItem);
                itemsForSale[slot] = items[randItem];
                int rarity = itemsForSale[slot].itemRarity;
                switch(rarity) {
                    case 1:
                        buyPrice[slot] = 100;
                        break;
                    case 2:
                        buyPrice[slot] = 200;
                        break;
                    case 3:
                        buyPrice[slot] = 300;
                        break;
                    case 4:
                        buyPrice[slot] = 400;
                        break;
                    case 5:
                        buyPrice[slot] = 500;
                        break;
                }
                itemsPanel.GetChild(slot).GetChild(0).GetComponent<TextMeshProUGUI>().text = buyPrice[slot] + "G";
            }
        }
        if(flag == "BUY") {
            if(IsServer) {
                Debug.Log("BUY ITEM");
            }
        }
        if(flag == "REFRESH") {
            Debug.Log("TEST");
            if(IsServer) {
                Debug.Log("refresh server flag");
                refreshing = bool.Parse(value);
                StartCoroutine(RefreshShop());

                SendUpdate("REFRESH", refreshing.ToString());
            }
            if(IsClient) {
                Debug.Log("refresh client flag");
                refreshing = bool.Parse(value);
                refreshButton.interactable = !refreshing;
            }
        }
    }

    public override void NetworkedStart()
    {
        shoppers = FindObjectsOfType<PlayerController>();
        StartCoroutine(RefreshShop());
    }

    public override IEnumerator SlowUpdate()
    {
        while(IsConnected) {
            if(IsClient) {
                foreach(PlayerController shopper in shoppers) {
                    transform.GetChild(0).gameObject.SetActive(shopper.inShop);
                    if(shopper.inShop) {
                        Cursor.lockState = CursorLockMode.None;
                    }
                    if(!shopper.inShop) {
                        Cursor.lockState = CursorLockMode.Locked;
                    }
                }
            }
            if(IsServer) {

            }

            yield return new WaitForSeconds(MyCore.MasterTimer);
        }
    }

    void Start()
    {
        
    }

    void Update()
    {
        
    }

    public void ButtonRefresh(bool refresh) {
        if(IsClient) {
            Debug.Log("START REFRESH");
            SendCommand("REFRESH",refresh.ToString());
        }
    }

    public IEnumerator RefreshShop() {
        if(IsServer) {
            Debug.Log("START REFRESH SERVER");
            for(int i=0; i<itemsForSale.Length; i++) {
                int randItem = Random.Range(0, items.Length);
                itemsForSale[i] = items[randItem];
                
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
                SendUpdate("SALEITEM",i+","+randItem);
                yield return new WaitForSeconds(MyCore.MasterTimer);
            }

            SendUpdate("REFRESH", "false");
        }
    }

    public void BuyItem(int slot) {
        if(IsClient) {
            //handles VISUALS
            Debug.Log("BUY ITEM");
            SendCommand("BUY", slot.ToString());
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

    public void ReplaceItem(int slot) {

    }

    public void HoverItem(int slot) {

    }
}
