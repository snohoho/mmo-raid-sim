using System.Collections;
using System.Collections.Generic;
using NETWORK_ENGINE;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerInventory : NetworkComponent
{
    public PlayerController player;
    public RectTransform invPanel;
    public Sprite emptyImg;
    public ItemStats[] inventory = new ItemStats[10];
    public int baseHP;
    public int baseMelAtk;
    public int baseRngAtk;
    public int baseSpd;
    public int totalHPBonus;
    public int totalMelAtkBonus;
    public int totalRngAtkBonus;
    public int totalSpdBonus;
    public float totalDmgBonus;
    public float totalGcdMod;
    public bool newItem;

    public override void HandleMessage(string flag, string value)
    {
        if(flag == "HP") {
            if(IsClient) {
                int hpToGain = int.Parse(value) - player.maxHp;
                player.maxHp = int.Parse(value);
                player.hp += hpToGain;
            }
        }
        if(flag == "MEL") {
            if(IsClient) {
                player.meleeAtk = int.Parse(value);
            }
        }
        if(flag == "RNG") {
            if(IsClient) {
                player.rangedAtk = int.Parse(value);
            }
        }
        if(flag == "SPD") {
            if(IsClient) {
                player.speed = int.Parse(value);
            }
        }
        if(flag == "DMG") {
            if(IsClient) {
                player.dmgBonusBase = float.Parse(value);
            }
        }
        if(flag == "GCD") {
            if(IsClient) {
                player.gcdBase = float.Parse(value);
            }
        }
    }

    public override void NetworkedStart()
    {
        
    }

    public override IEnumerator SlowUpdate()
    {
        while(IsConnected) {
            if(IsLocalPlayer) {
                for(int i=0; i<inventory.Length; i++) {
                    if(inventory[i] != null) {
                        invPanel.GetChild(i).gameObject.SetActive(true);
                        invPanel.GetChild(i).GetComponent<Image>().sprite = inventory[i].itemSprite; 
                    }
                    else if(inventory[i] == null) {
                        invPanel.GetChild(i).GetComponent<Image>().sprite = emptyImg;
                    }
                }
            }
            if(IsServer && newItem) {
                int tempHpBonus = baseHP;
                int tempMelBonus = baseMelAtk;
                int tempRngBonus = baseRngAtk;
                int tempSpdBonus = baseSpd;
                float tempDmgBonus = player.dmgBonusBase;
                float tempGcdMod = player.gcdBase;
                
                for(int i=0; i<inventory.Length; i++) {              
                    if(inventory[i] != null) {
                        tempHpBonus += inventory[i].hpBonus;
                        tempMelBonus += inventory[i].meleeBonus;
                        tempRngBonus += inventory[i].rangedBonus;
                        tempSpdBonus += inventory[i].spdBonus;
                        tempDmgBonus += inventory[i].dmgBonus;
                        tempGcdMod += inventory[i].gcdMod;
                    }
                    else if(inventory[i] == null) {
                        continue;
                    }
                }

                totalHPBonus = tempHpBonus;
                player.maxHp = totalHPBonus;
                SendUpdate("HP",totalHPBonus.ToString());

                totalMelAtkBonus = tempMelBonus;
                player.meleeAtk = totalMelAtkBonus;
                SendUpdate("MEL",totalMelAtkBonus.ToString());

                totalRngAtkBonus = tempRngBonus;
                player.rangedAtk = totalRngAtkBonus;
                SendUpdate("RNG",tempRngBonus.ToString());

                totalSpdBonus = tempSpdBonus;
                player.speed = totalSpdBonus;
                SendUpdate("SPD",tempSpdBonus.ToString());

                totalDmgBonus = tempDmgBonus;
                player.dmgBonusBase = totalDmgBonus;
                SendUpdate("DMG",totalDmgBonus.ToString());

                totalGcdMod = tempGcdMod;
                player.gcdBase = totalGcdMod;
                SendUpdate("GCD",totalGcdMod.ToString());

                newItem = false;
            }

            yield return new WaitForSeconds(MyCore.MasterTimer);
        }
    }

    void Start()
    {
        baseHP = player.maxHp;
        baseMelAtk = player.meleeAtk;
        baseRngAtk = player.rangedAtk;
        baseSpd = player.speed;
    }

    void Update()
    {
        
    }

    public void OpenInventory(InputAction.CallbackContext context) {
        if(context.started || context.performed) {
            if(IsLocalPlayer) {
                Cursor.lockState = CursorLockMode.None;
                invPanel.gameObject.SetActive(true);
            }
        }
        if(context.canceled) {
            if(IsLocalPlayer) {
                Cursor.lockState = CursorLockMode.Locked;
                invPanel.gameObject.SetActive(false);
            }
        }
    }

    public void HoverInventoryItem(int slot) {
        Debug.Log("hovering item in slot " + slot);
    }

    public void DropItem(int slot) {
        Debug.Log("dropping item in slot " + slot);
    }
}
