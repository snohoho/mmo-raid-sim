using System;
using System.Collections;
using System.Collections.Generic;
using NETWORK_ENGINE;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class SoundDemonClass : PlayerController
{
    public RectTransform notesPanel;
    GameObject n1, n2, n3;
    
    public bool defBonus;
    public bool[] note = new bool[3] {false, false, false};
    public bool inCr;

    public override void HandleMessage(string flag, string value)
    {
        base.HandleMessage(flag, value);

        if(flag == "PRIMARY") {
            if(IsServer && gcd <= 0 && primaryCD <= 0) {
                primaryCD = 0.5f;
                gcd = 1.4f + gcdMod + gcdBase;
                SendUpdate("GBLCD",gcd.ToString());
                SendUpdate("PRIMARYCD",primaryCD.ToString());
            }
            if(IsClient && usingPrimary) {
                AudioManager.Instance.CreateSource(AudioManager.Instance.audioClips[20]);
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
                AudioManager.Instance.CreateSource(AudioManager.Instance.audioClips[17]);
            }
        }
        if(flag == "DEFENSIVE") {
            if(IsServer && gcd <= 0 && defCD <= 0) {
                defCD = 3f;
                gcd = 1.2f + gcdMod + gcdBase;
                SendUpdate("GBLCD",gcd.ToString());
                SendUpdate("DEFCD",defCD.ToString());
            }
            if(IsClient && usingDefensive) {
                AudioManager.Instance.CreateSource(AudioManager.Instance.audioClips[27]);
            }
        }
        if(flag == "ULT") {
            if(IsServer && gcd <= 0 && ultCD <= 0) {
                ultCD = 18f;
                gcd = 1f + gcdMod + gcdBase;
                SendUpdate("GBLCD",gcd.ToString());
                SendUpdate("ULTCD",ultCD.ToString());
            }
            if(IsClient && usingUlt) {
                AudioManager.Instance.CreateSource(AudioManager.Instance.audioClips[25]);
                AudioManager.Instance.CreateSource(AudioManager.Instance.audioClips[37]);
            }
        }

        if(flag == "NOTE") {
            int index = int.Parse(value);
            if(IsClient) {
                switch(index) {
                    case 0:
                        note[index] = true;
                        break;
                    case 1:
                        note[index] = true;
                        break;
                    case 2:
                        note[index] = true;
                        break;
                    case 3:
                        note[0] = false;
                        note[1] = false;
                        note[2] = false;
                        break;
                }
            }
        }
    }

    public override IEnumerator SlowUpdate()
    {
        while(IsConnected) {
            if(IsLocalPlayer) {
                n1 = notesPanel.GetChild(0).gameObject;
                n2 = notesPanel.GetChild(1).gameObject;
                n3 = notesPanel.GetChild(2).gameObject;
                n1.SetActive(note[0]);
                n2.SetActive(note[1]);
                n3.SetActive(note[2]);

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
                    note[0] = true;

                    skillDmg = 100;
                    primaryHB.SetActive(true);
                    defBonus = false;

                    lastSkill = "";
                    SendUpdate("NOTE","0");
                }
                if(lastSkill == "SECONDARY") {
                    note[1] = true;
                    
                    skillDmg = 50;
                    secondaryHB.SetActive(true);
                    defBonus = false;

                    lastSkill = "";
                    SendUpdate("NOTE","1");
                }
                if(lastSkill == "DEFENSIVE") {
                    note[2] = true;

                    if(!defBonus) {
                        dmgBonus = 0.5f + dmgBonusBase;
                        defBonus = true;
                    }
                    
                    lastSkill = "";
                    SendUpdate("NOTE","2");
                }
                if(lastSkill == "ULT") {
                    StartCoroutine(UltHitboxes());
                    defBonus = false;

                    lastSkill = "";
                    SendUpdate("NOTE","3");
                }
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
                    //Debug.Log("test");
                    primaryCD -= Time.deltaTime;
                }

                if(primaryCD <= 0 && gcd <= 0) {
                    //Debug.Log("test2");
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

    public IEnumerator UltHitboxes() {
        inCr = true;
        skillDmg = 0;
        float totalDmgBonus = dmgBonus;
       
        for(int i=0; i<note.Length; i++) {
            if(note[i]) {
                skillDmg += 150;
                totalDmgBonus += 0.5f;
                dmgBonus = totalDmgBonus;

                ultHB.SetActive(true); 
                yield return new WaitForSeconds(0.05f);

                ultHB.SetActive(false);
                yield return new WaitForSeconds(0.15f);
            }
            
            yield return null;
        }

        note[0] = false;
        note[1] = false;
        note[2] = false;
        SendUpdate("NOTE","3");
        dmgBonus = 1f;
        inCr = false;
    }
}