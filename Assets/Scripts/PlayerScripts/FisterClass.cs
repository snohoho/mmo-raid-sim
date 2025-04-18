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

        if(flag == "PRIMARY") {
            if(IsServer && gcd <= 0 && primaryCD <= 0) {
                primaryCD = 0.3f;
                gcd = 1f + gcdMod + gcdBase;
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
                AudioManager.Instance.CreateSource(AudioManager.Instance.audioClips[2]);
            }
        }
        if(flag == "DEFENSIVE") {
            if(IsServer && gcd <= 0 && defCD <= 0) {
                defCD = 6f;
                gcd = 1.2f + gcdMod + gcdBase;
                SendUpdate("GBLCD",gcd.ToString());
                SendUpdate("DEFCD",defCD.ToString());
            }
            if(IsClient && usingDefensive) {
                AudioManager.Instance.CreateSource(AudioManager.Instance.audioClips[28]);
            }
        }
        if(flag == "ULT") {
            if(IsServer && gcd <= 0 && ultCD <= 0) {
                ultCD = 16f;
                gcd = 1f + gcdMod + gcdBase;
                SendUpdate("GBLCD",gcd.ToString());
                SendUpdate("ULTCD",ultCD.ToString());
            }
            if(IsClient && usingUlt) {
                AudioManager.Instance.CreateSource(AudioManager.Instance.audioClips[3]);
                AudioManager.Instance.CreateSource(AudioManager.Instance.audioClips[36]);
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

    public override IEnumerator SlowUpdate()
    {
        while(IsConnected) {
            if(IsLocalPlayer) {
                gremlinBar.value = gremlin;

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

                if((primaryHB.activeSelf || secondaryHB.activeSelf || ultHB.activeSelf) && !gremmingOut && !inCr) {
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
                    if(!gremmingOut) gremlin += 15f;

                    skillDmg = 150;
                    defBonus = false;
                    invuln = false;
                    SendUpdate("INVULN", "false");
                    primaryHB.SetActive(true);
                    
                    lastSkill = "";
                }
                if(lastSkill == "SECONDARY") {
                    if(!gremmingOut) gremlin += 10f;

                    StartCoroutine(SecondaryHitboxes());
                    defBonus = false;
                    invuln = false;
                    SendUpdate("INVULN", "false");
                    

                    lastSkill = "";
                }
                if(lastSkill == "DEFENSIVE") {
                    gremlin += 20f;

                    invuln = true;
                    SendUpdate("INVULN", "true");
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
                    SendUpdate("INVULN", "false");

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
                        dmgBonus = dmgBonusBase;
                        gcdMod = gcdBase;
                    }
                }
                else if(gremlin >= 100) {
                    gremlin = 100;
                    if(!gremmingOut) {
                        dmgBonus = dmgBonusBase+2f;
                        gcdMod = -0.5f + gcdBase;
                    }
                    gremmingOut = true;
                }

                SendUpdate("GREMLIN", gremlin.ToString());
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
                    Debug.Log("test");
                    primaryCD -= Time.deltaTime;
                }

                if(primaryCD <= 0 && gcd <= 0) {
                    Debug.Log("test2");
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

    public IEnumerator SecondaryHitboxes() {
        inCr = true;
        skillDmg = 0;
        int hbCount = 0;
        float totalDmgBonus = dmgBonus;
       
        while(hbCount < 2) {
            skillDmg += 75;
            totalDmgBonus += 0.5f;
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
            skillDmg += 75;
            totalDmgBonus += 0.3f;
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