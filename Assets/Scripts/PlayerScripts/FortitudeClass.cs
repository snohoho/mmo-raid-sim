using System.Collections;
using System.Collections.Generic;
using NETWORK_ENGINE;
using TMPro;
using UnityEngine;

public class FortitudeClass : PlayerController
{
    public TextMeshProUGUI nameLabel;
    public string playerName;

    public int hp = 7;
    public int meleeAtk = 50;
    public int rangedAtk = 50;
    public int speed = 5;
    public float primaryCD;
    public float secondaryCD;
    public float defCD;
    public float ultCD;
    public float gcd;
    public bool dead;
    public bool deathTimer;

    public ItemStats[] inventory;

    public override void HandleMessage(string flag, string value)
    {
        base.HandleMessage(flag, value);

        if(flag == "STATUP") {
            int invSlot = int.Parse(value);
            if(IsServer) {
                hp += inventory[invSlot].hpBonus;
                meleeAtk += inventory[invSlot].meleeBonus;
                rangedAtk += inventory[invSlot].rangedBonus;
                speed += inventory[invSlot].spdBonus;

                SendUpdate("STATUP", value);  
            }
        }
    }

    public override void NetworkedStart()
    {
        foreach(NetworkPlayerManager n in FindObjectsOfType<NetworkPlayerManager>()) {
            if(n.Owner == Owner) {
                playerName = n.playerName;
                nameLabel.text = playerName;
            }
        }
    }

    public override IEnumerator SlowUpdate()
    {
        while(IsConnected) {
            if(IsLocalPlayer) {
                if(usingPrimary) {
                    //change visual on UI
                }
            }
            if(IsServer) {
                if(gcd >= 0) {
                    usingPrimary = false;
                    usingSecondary = false;
                    usingDefensive = false;
                    usingUlt = false;
                    switch(lastSkill) {
                        case "PRIMARY":
                            usingPrimary = true;
                            break;
                        case "SECONDARY":
                            usingSecondary = true;
                            break;
                        case "DEFENSIVE":
                            usingDefensive = true;
                            break;
                        case "ULT":
                            usingUlt = true;
                            break;
                    }
                }

                if(usingPrimary && primaryCD <= 0) {
                    //actual cd gets set here
                    primaryCD = 1f;
                    gcd = 1f;
                }
                else if(usingPrimary && primaryCD > 0) {
                    primaryCD -= Time.deltaTime;
                    gcd -= Time.deltaTime;
                    if(primaryCD <= 0 && gcd <= 0) {
                        usingPrimary = false;
                    }
                }
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
}
