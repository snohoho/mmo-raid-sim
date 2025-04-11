using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Schema;
using NETWORK_ENGINE;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QueenClass : PlayerController
{
    public TextMeshProUGUI nameLabel;
    public string playerName;
    public RectTransform statsPanel;
    TextMeshProUGUI levelText, goldText, meleeText, rangedText, speedText; 
    public RectTransform skillsPanel; 
    TextMeshProUGUI s1, s2, s3, s4;
    public Slider heatBar;

    public int level = 1;
    public int gold = 0;
    
    public int heat;
    public bool overheat;
    public bool uiOverheat;
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
        if(flag == "HEAT") {
            int newHeat = int.Parse(value);
            if(IsClient) {
                heat = newHeat;
                if(overheat && heat <= 0) {
                    overheat = false;
                }
                if(heat >= 100) {
                    overheat = true;
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
                heatBar.value = heat;

                Image heatColor = heatBar.transform.GetChild(2).GetChild(0).gameObject.GetComponent<Image>();
                TextMeshProUGUI heatLabel = heatBar.transform.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>();
                if(heat >= 100) {
                    heatColor.color = new Color32(62,140,238,255);
                    if(!uiOverheat) {
                        heatLabel.text = "OVERHEATED";
                        StartCoroutine(FlashOverheat(heatLabel.gameObject));
                    }

                    uiOverheat = true;
                }
                else if(heat >= 50) {
                    if(!uiOverheat) {
                        heatColor.color = new Color32(213,91,217,255);
                    }
                }
                else if(heat >= 0) {
                    if(heat <= 0 && uiOverheat) {
                        heatLabel.text = "HEAT";
                        uiOverheat = false;
                    }
                    if(!uiOverheat) {
                        heatColor.color = new Color32(241,47,71,255);
                    }
                }

                levelText.text = level.ToString();
                goldText.text = gold.ToString();
                meleeText.text = meleeAtk.ToString();
                rangedText.text = rangedAtk.ToString();
                speedText.text = speed.ToString();
            }
            if(IsServer) {
                if((primaryHB.activeSelf || secondaryHB.activeSelf || ultHB.activeSelf) && !inCr) {
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
                    heat += 10;

                    skillDmg = 100;
                    primaryHB.SetActive(true);

                    lastSkill = "";
                }
                if(lastSkill == "SECONDARY") {
                    heat += 20;
                    
                    skillDmg = 150;
                    secondaryHB.SetActive(true);

                    lastSkill = "";
                }
                if(lastSkill == "DEFENSIVE") {
                    if(heat >= 50) {
                        buffTimer = 3f;
                        heat -= 40;
                    }
                    else if(heat < 50) {
                        buffTimer = 1f;
                        heat -= 20;
                    }

                    defHB.SetActive(true);
                    StartCoroutine(InvulnTimer(buffTimer));

                    lastSkill = "";
                }
                if(lastSkill == "ULT") {
                    if(overheat) {
                        ultHB.SetActive(true);
                        skillDmg = 300;
                        dmgBonus = 1;
                        heat -= 50;
                    }
                    else if(!overheat) {
                        if(heat < 30) {
                            skillDmg = 150;
                            dmgBonus += 0.2f;
                            ultHB.SetActive(true);
                        }
                        StartCoroutine(UltHitboxes());
                    }
                    
                    lastSkill = "";
                }
                
                if(heat <= 0) {
                    heat = 0;
                    if(overheat) {
                        overheat = false;
                        gcdMod = 0f;
                    }
                }
                else if(heat >= 100) {
                    heat = 100;
                    if(!overheat) {
                        gcdMod = 1f;
                    }
                    overheat = true;
                }

                SendUpdate("HEAT", heat.ToString());
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
                ultCD = 1f;
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

    public IEnumerator FlashOverheat(GameObject obj) {
        bool flash = false;    
        while(uiOverheat) {
            obj.SetActive(flash);
            flash = !flash;
            yield return new WaitForSeconds(0.1f);
        }
        yield return new WaitForEndOfFrame();
    }

    public IEnumerator UltHitboxes() {
        inCr = true;
        skillDmg = 0;
        float totalDmgBonus = dmgBonus;

        while(heat >= 30) {
            heat -= 30;
            skillDmg += 100;
            totalDmgBonus += 0.5f;
            dmgBonus = totalDmgBonus;

            ultHB.SetActive(true); 
            yield return new WaitForSeconds(0.05f);

            ultHB.SetActive(false);

            yield return new WaitForSeconds(0.15f);
        }
        dmgBonus = 1;
        inCr = false;
    }
}
