using System;
using System.Collections;
using System.Collections.Generic;
using NETWORK_ENGINE;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FisterClass : PlayerController
{
    public Slider gremlinBar;
    
    public float gremlin;
    public bool gremmingOut;
    public bool uiGremmingOut;
    public bool defBonus;
    public bool inCr;

    public override void HandleMessage(string flag, string value)
    {
        base.HandleMessage(flag, value);

        if(flag == "GREMLIN") {
            int newGrem = int.Parse(value);
            if(IsClient) {
                gremlin = newGrem;
                if(gremmingOut && gremlin <= 0) {
                    gremmingOut = false;
                }
                if(gremlin >= 100) {
                    gremmingOut = true;
                }
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
        if(IsLocalPlayer) {
            levelText = statsPanel.transform.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>();
            goldText = statsPanel.transform.GetChild(1).gameObject.GetComponent<TextMeshProUGUI>();
            meleeText = statsPanel.transform.GetChild(2).gameObject.GetComponent<TextMeshProUGUI>();
            rangedText = statsPanel.transform.GetChild(3).gameObject.GetComponent<TextMeshProUGUI>();
            speedText = statsPanel.transform.GetChild(4).gameObject.GetComponent<TextMeshProUGUI>();

            levelText.text = level.ToString();
            goldText.text = gold.ToString();
            meleeText.text = meleeAtk.ToString();
            rangedText.text = rangedAtk.ToString();
            speedText.text = speed.ToString();
        }
    }

    public override IEnumerator SlowUpdate()
    {
        while(IsConnected) {
            if(IsLocalPlayer) {
                gremlinBar.value = gremlin;

                levelText.text = level.ToString();
                goldText.text = gold.ToString();
                meleeText.text = meleeAtk.ToString();
                rangedText.text = rangedAtk.ToString();
                speedText.text = speed.ToString();
            }
            if(IsServer) {
                if((primaryHB.activeSelf || secondaryHB.activeSelf || ultHB.activeSelf) && !gremmingOut && !inCr) {
                    dmgBonus = 1f;
                }
                if(!inCr) {
                    primaryHB.SetActive(false);
                    secondaryHB.SetActive(false);
                    defHB.SetActive(false);
                    ultHB.SetActive(false);
                }   

                while(isDead) {

                }

                if(lastSkill == "PRIMARY") {
                    if(!gremmingOut) gremlin += 10f;

                    skillDmg = 100;
                    defBonus = false;
                    invuln = false;
                    primaryHB.SetActive(true);
                    
                    lastSkill = "";
                }
                if(lastSkill == "SECONDARY") {
                    if(!gremmingOut) gremlin += 10f;

                    StartCoroutine(SecondaryHitboxes());
                    defBonus = false;
                    invuln = false;

                    lastSkill = "";
                }
                if(lastSkill == "DEFENSIVE") {
                    if(!gremmingOut) gremlin += 20f;

                    invuln = true;
                    if(!defBonus) {
                        dmgBonus += 0.5f;
                        defBonus = true;
                    }

                    lastSkill = "";
                }
                if(lastSkill == "ULT") {
                    if(!gremmingOut) gremlin += 30f;

                    StartCoroutine(UltHitboxes());
                    defBonus = false;
                    invuln = false;

                    lastSkill = "";
                }

                if(gremmingOut) {
                    gremlin -= 1f;
                    if(gremlin <= 0f) {
                        gremlin = 0f;
                    }
                }
                
                if(gremlin <= 0) {
                    gremlin = 0;
                    if(gremmingOut) {
                        gremmingOut = false;
                        dmgBonus = 1f;
                        gcdMod = 0f;
                    }
                }
                else if(gremlin >= 100) {
                    gremlin = 100;
                    if(!gremmingOut) {
                        dmgBonus = 2f;
                        gcdMod -= 0.5f;
                    }
                    gremmingOut = true;
                }

                SendUpdate("GREMLIN", gremlin.ToString());
            }

            yield return new WaitForSeconds(MyCore.MasterTimer);
        }
    }

    void Start()
    {
        
    }

    public override void Update()
    {       
        base.Update();

        if(IsServer) {
            if(usingPrimary && primaryCD <= 0 && gcd <= 0) {
                //actual cd gets set here
                primaryCD = 0.5f;
                gcd = 1f + gcdMod;
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
                    SendUpdate("PRIMARY", "false");
                }
            }

            if(usingSecondary && secondaryCD <= 0 && gcd <= 0) {
                //actual cd gets set here
                secondaryCD = 0.5f;
                gcd = 1f + gcdMod;
                SendUpdate("GBLCD",gcd.ToString());
                SendUpdate("SECONDARYCD",secondaryCD.ToString());
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
                    SendUpdate("SECONDARY", "false");
                }
            }

            if(usingDefensive && defCD <= 0 && gcd <= 0) {
                //actual cd gets set here
                defCD = 1f;
                gcd = 1f + gcdMod;
                SendUpdate("GBLCD",gcd.ToString());
                SendUpdate("DEFCD",defCD.ToString());
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
                    SendUpdate("DEFENSIVE", "false");
                }
            }

            if(usingUlt && ultCD <= 0 && gcd <= 0) {
                //actual cd gets set here
                ultCD = 10f;
                gcd = 1f + gcdMod;
                SendUpdate("GBLCD",gcd.ToString());
                SendUpdate("ULTCD",ultCD.ToString());
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
                    SendUpdate("ULT", "false");
                }
            }
        }
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

    public IEnumerator SecondaryHitboxes() {
        inCr = true;
        skillDmg = 0;
        int hbCount = 0;
        float totalDmgBonus = dmgBonus;
       
        while(hbCount < 2) {
            skillDmg += 75;
            dmgBonus = totalDmgBonus;

            secondaryHB.SetActive(true); 
            yield return new WaitForSeconds(0.05f);

            secondaryHB.SetActive(false);
            yield return new WaitForSeconds(0.15f);

            hbCount++;
        }
        
        dmgBonus = 1f;
        inCr = false;
    }

    public IEnumerator UltHitboxes() {
        inCr = true;
        skillDmg = 0;
        int hbCount = 0;
        float totalDmgBonus = dmgBonus;
       
        while(hbCount < 5) {
            skillDmg += 50;
            totalDmgBonus += 0.2f;
            dmgBonus = totalDmgBonus;

            ultHB.SetActive(true); 
            yield return new WaitForSeconds(0.05f);

            ultHB.SetActive(false);
            yield return new WaitForSeconds(0.15f);

            hbCount++;
        }

        dmgBonus = 1f;
        inCr = false;
    }
}