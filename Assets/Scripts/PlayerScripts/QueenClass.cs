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
    public Slider heatBar;
    
    public int heat;
    public bool overheat;
    public bool uiOverheat;
    public bool inCr;

    public override void HandleMessage(string flag, string value)
    {
        base.HandleMessage(flag, value);

        if(flag == "PRIMARY") {
            if(IsServer && gcd <= 0 && primaryCD <= 0) {
                primaryCD = 0.5f;
                gcd = 1.2f + gcdMod + gcdBase;
                SendUpdate("GBLCD",gcd.ToString());
                SendUpdate("PRIMARYCD",primaryCD.ToString());
            }
            if(IsClient && usingPrimary) {
                AudioManager.Instance.CreateSource(AudioManager.Instance.audioClips[1]);
            }
        }
        if(flag == "SECONDARY") {
            if(IsServer && gcd <= 0 && secondaryCD <= 0) {
                secondaryCD = 0.5f;
                gcd = 1.4f + gcdMod + gcdBase;
                SendUpdate("GBLCD",gcd.ToString());
                SendUpdate("SECONDARYCD",secondaryCD.ToString());
            }
            if(IsClient && usingSecondary) {
                AudioManager.Instance.CreateSource(AudioManager.Instance.audioClips[16]);
            }
        }
        if(flag == "DEFENSIVE") {
            if(IsServer && gcd <= 0 && defCD <= 0) {
                defCD = 6f;
                gcd = 1.4f + gcdMod + gcdBase;
                SendUpdate("GBLCD",gcd.ToString());
                SendUpdate("DEFCD",defCD.ToString());
            }
            if(IsClient && usingDefensive) {
                AudioManager.Instance.CreateSource(AudioManager.Instance.audioClips[27]);
            }
        }
        if(flag == "ULT") {
            if(IsServer && gcd <= 0 && ultCD <= 0) {
                ultCD = 4f;
                gcd = 1f + gcdMod + gcdBase;
                SendUpdate("GBLCD",gcd.ToString());
                SendUpdate("ULTCD",ultCD.ToString());
            }
            if(IsClient && usingUlt) {
                AudioManager.Instance.CreateSource(AudioManager.Instance.audioClips[25]);
                AudioManager.Instance.CreateSource(AudioManager.Instance.audioClips[35]);
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
        if (flag == "ULT")
        {
            if (IsClient)
            {
                if (usingUlt)
                {
                    StartCoroutine(SetAnimation(animator, "DoingSpecial"));
                }
            }
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

                levelText.text = "LVL" + level.ToString();
                goldText.text = gold.ToString() + "G";
                meleeText.text = meleeAtk.ToString();
                rangedText.text = rangedAtk.ToString();
                speedText.text = speed.ToString();
            }
            if(IsServer) {
                if(exp >= 100) {
                    exp -= 100;
                    meleeAtk += 10;
                    rangedAtk += 10;
                    level++;
                    SendUpdate("LVLUP",level.ToString());
                }

                if((primaryHB.activeSelf || secondaryHB.activeSelf || ultHB.activeSelf) && !inCr) {
                    dmgBonus = dmgBonusBase;
                }
                if(!inCr) {
                    primaryHB.SetActive(false);
                    secondaryHB.SetActive(false);
                    defHB.SetActive(false);
                    ultHB.SetActive(false);
                }

                if(hp <= 0) {
                    isDead = true;
                    invuln = true;
                }
                while(isDead) {
                    deathTimer = 10f;
                    SendUpdate("DEAD",deathTimer.ToString());
                    yield return new WaitUntil(() => !isDead);
                }

                if(lastSkill == "PRIMARY") {
                    heat += 10;

                    skillDmg = 100 + heat;
                    primaryHB.SetActive(true);

                    lastSkill = "";
                }
                if(lastSkill == "SECONDARY") {
                    heat += 20;
                    
                    skillDmg = 150 + heat;
                    secondaryHB.SetActive(true);

                    lastSkill = "";
                }
                if(lastSkill == "DEFENSIVE") {
                    if(heat >= 50) {
                        buffTimer = 5f;
                        heat -= 50;
                    }
                    else if(heat < 50) {
                        buffTimer = 3f;
                        heat -= 20;
                    }

                    defHB.SetActive(true);
                    StartCoroutine(InvulnTimer(buffTimer)); 

                    lastSkill = "";
                }
                if(lastSkill == "ULT") {
                    if(overheat) {
                        ultHB.SetActive(true);
                        skillDmg = 500 + heat;
                        dmgBonus = dmgBonusBase;
                        heat -= 50;
                    }
                    else if(!overheat) {
                        if(heat < 30) {
                            skillDmg = 300 + heat;
                            dmgBonus += 0.2f + dmgBonusBase;
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
                        gcdMod = gcdBase + 0f;
                    }
                }
                else if(heat >= 100) {
                    heat = 100;
                    if(!overheat) {
                        gcdMod = gcdBase + 1f;
                    }
                    overheat = true;
                }

                SendUpdate("HEAT", heat.ToString());
            }

            yield return new WaitForSeconds(MyCore.MasterTimer);
        }
    }

    public override void Update()
    {       
        base.Update();

        if(IsServer) {
            if(isDead && deathTimer > 0f) {
                deathTimer -= Time.deltaTime;
            }
            
            if(gcd > 0) {
                gcd -= Time.deltaTime;
            }

            if(usingPrimary) {
                if(primaryCD > 0) {
                    primaryCD -= Time.deltaTime;
                }

                if(primaryCD <= 0 && gcd <= 0) {
                    usingPrimary = false;
                    SendUpdate("PRIMARY", "false");
                }
            }

            if(usingSecondary) {
                if(secondaryCD > 0) {
                    secondaryCD -= Time.deltaTime;
                }
                
                if(secondaryCD <= 0 && gcd <= 0) {
                    usingSecondary = false;
                    SendUpdate("SECONDARY", "false");
                }
            }

            if(usingDefensive) {
                if(defCD > 0) {
                    defCD -= Time.deltaTime;
                }
                
                if(defCD <= 0 && gcd <= 0) {
                    usingDefensive = false;
                    SendUpdate("DEFENSIVE", "false");
                }
            }

            if(usingUlt) {
                if(ultCD > 0) {
                    ultCD -= Time.deltaTime;
                }
                
                if(ultCD <= 0 && gcd <= 0) {
                    usingUlt = false;
                    SendUpdate("ULT", "false");
                }
            }
        }
    }

    public IEnumerator FlashOverheat(GameObject obj) {
        bool flash = false;    
        while(uiOverheat) {
            obj.SetActive(flash);
            flash = !flash;
            yield return new WaitForSeconds(0.1f);
        }
        yield return null;
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

            yield return new WaitForSeconds(0.05f);
        }
        dmgBonus = 1;
        inCr = false;
    }
}
