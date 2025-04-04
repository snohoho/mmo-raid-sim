using System.Collections;
using System.Collections.Generic;
using NETWORK_ENGINE;
using TMPro;
using UnityEngine;

public class FortitudeClass : PlayerController
{
    public TextMeshProUGUI nameLabel;
    public string playerName;
    public RectTransform skillsPanel;
    TextMeshProUGUI s1, s2, s3, s4;

    public int hp = 7;
    public int meleeAtk = 50;
    public int rangedAtk = 50;
    public int speed = 5;
    public float primaryCD;
    public float secondaryCD;
    public float defCD;
    public float ultCD;

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
        if(flag == "GBLCD") {
            float cd = float.Parse(value);
            if(IsClient) {
                gcd = cd;
            }
        }
        if(flag == "PRIMARYCD") {
            float cd = float.Parse(value);
            if(IsClient) {
                primaryCD = cd;
            }
        }
        if(flag == "SECONDARYCD") {
            float cd = float.Parse(value);
            if(IsClient) {
                secondaryCD = cd;
            }
        }
        if(flag == "DEFCD") {
            float cd = float.Parse(value);
            if(IsClient) {
                defCD = cd;
            }
        }
        if(flag == "ULTCD") {
            float cd = float.Parse(value);
            if(IsClient) {
                ultCD = cd;
            }
        }
    }

    public override void NetworkedStart()
    {
        StartCoroutine(SetPrefs());
        //disable other player uis
        if(!IsLocalPlayer) {
            transform.GetChild(0).gameObject.SetActive(false);
        }
    }

    public override IEnumerator SlowUpdate()
    {
        while(IsConnected) {
            if(IsLocalPlayer) {
                //Debug.Log(gcd + " " + primaryCD);
                s1 = skillsPanel.GetChild(0).GetChild(0).gameObject.GetComponent<TextMeshProUGUI>();
                s2 = skillsPanel.GetChild(1).GetChild(0).gameObject.GetComponent<TextMeshProUGUI>();
                s3 = skillsPanel.GetChild(2).GetChild(0).gameObject.GetComponent<TextMeshProUGUI>();
                s4 = skillsPanel.GetChild(3).GetChild(0).gameObject.GetComponent<TextMeshProUGUI>();
                if(gcd > 0) {
                    s1.text = gcd.ToString("N1");
                    s2.text = gcd.ToString("N1");
                    s3.text = gcd.ToString("N1");
                    s4.text = gcd.ToString("N1");
                    gcd -= Time.deltaTime;
                }
                else {
                    if(usingPrimary) {
                        s1.text = primaryCD.ToString("N1");
                        primaryCD -= Time.deltaTime;
                    }
                }
            }
            if(IsServer) {
                while(isDead) {

                }

                if(isHurt) {
                    hp -= 1;
                    isHurt = false;
                }

                if(usingPrimary && primaryCD <= 0 && gcd <= 0) {
                    //actual cd gets set here
                    primaryCD = 2f;
                    gcd = 1f;
                    SendUpdate("GBLCD",gcd.ToString());
                    SendUpdate("PRIMARYCD",primaryCD.ToString());
                }
                else if(usingPrimary) {
                    if(primaryCD > 0) {
                        primaryCD -= Time.deltaTime;
                    }
                    if(gcd > 0) {
                        gcd -= Time.deltaTime;
                    }

                    if(primaryCD <= 0 && gcd <= 0) {
                        usingPrimary = false;
                        Debug.Log("SENDING UPDATE");
                        SendUpdate("PRIMARY", "false");
                    }
                }

                if(usingSecondary && secondaryCD <= 0 && gcd <= 0) {
                    //actual cd gets set here
                    secondaryCD = 1f;
                    gcd = 2f;
                }
                else if(usingSecondary) {
                    if(secondaryCD > 0) {
                        secondaryCD -= Time.deltaTime;
                    }
                    if(gcd > 0) {
                        gcd -= Time.deltaTime;
                    }
                    
                    if(secondaryCD <= 0 && gcd <= 0) {
                        usingSecondary = false;
                    }
                }

                if(usingDefensive && defCD <= 0 && gcd <= 0) {
                    //actual cd gets set here
                    defCD = 1f;
                    gcd = 2f;
                }
                else if(usingDefensive) {
                    if(defCD > 0) {
                        defCD -= Time.deltaTime;
                    }
                    if(gcd > 0) {
                        gcd -= Time.deltaTime;
                    }
                    
                    if(defCD <= 0 && gcd <= 0) {
                        usingDefensive = false;
                    }
                }

                if(usingUlt && ultCD <= 0 && gcd <= 0) {
                    //actual cd gets set here
                    ultCD = 1f;
                    gcd = 2f;
                }
                else if(usingUlt) {
                    if(ultCD > 0) {
                        ultCD -= Time.deltaTime;
                    }
                    if(gcd > 0) {
                        gcd -= Time.deltaTime;
                    }
                    
                    if(ultCD <= 0 && gcd <= 0) {
                        usingUlt = false;
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

    public IEnumerator SetPrefs() {
        foreach(NetworkPlayerManager n in FindObjectsOfType<NetworkPlayerManager>()) {
            if(n.Owner == Owner) {
                playerName = n.playerName;
                nameLabel.text = playerName;
            }

            yield return new WaitForEndOfFrame();
        }
    }
}
