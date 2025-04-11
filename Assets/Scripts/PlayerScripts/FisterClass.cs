using System;
using System.Collections;
using System.Collections.Generic;
using NETWORK_ENGINE;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FisterClass : PlayerController
{
    public TextMeshProUGUI nameLabel;
    public string playerName;
    public RectTransform statsPanel;
    TextMeshProUGUI levelText, goldText, meleeText, rangedText, speedText; 
    public RectTransform skillsPanel; 
    TextMeshProUGUI s1, s2, s3, s4;
    public Slider gremlinBar;

    public int level = 1;
    public int gold = 0;
    
    public float gremlin;
    public bool gremmingOut;
    public bool uiGremmingOut;
    public bool defBonus;
    public bool inCr;

    public override void HandleMessage(string flag, string value)
    {
        base.HandleMessage(flag, value);

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

        //handles cooldown timers
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
            if(usingPrimary) {
                if(gcd <= 0) {
                    s1.text = primaryCD.ToString("N1");
                }
                if(primaryCD > 0) {
                    primaryCD -= Time.deltaTime;
                }
            }
            if(usingSecondary) {
                if(gcd <= 0) {
                    s2.text = secondaryCD.ToString("N1");
                }
                if(secondaryCD > 0) {
                    secondaryCD -= Time.deltaTime;
                }
            }
            if(usingDefensive) {
                if(gcd <= 0) {
                    s3.text = defCD.ToString("N1");
                }
                if(defCD > 0) {
                    defCD -= Time.deltaTime;
                }
            }
            if(usingUlt) {
                if(gcd <= 0) {
                    s4.text = ultCD.ToString("N1");
                }
                if(ultCD > 0) {
                    ultCD -= Time.deltaTime;
                }
            }
            
        }
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