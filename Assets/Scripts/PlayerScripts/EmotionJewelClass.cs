using System;
using System.Collections;
using System.Collections.Generic;
using NETWORK_ENGINE;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EmotionJewelClass : PlayerController
{
    public Image[] emotions;
    public TextMeshProUGUI emotionLabel;

    public int activeEmotion;
    public int nextEmotion;

    public override void HandleMessage(string flag, string value)
    {
        base.HandleMessage(flag, value);
        
        if(flag == "EMOTION") {
            int emotion = int.Parse(value);
            if(IsClient) {
                activeEmotion = emotion;
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
                emotions[0].gameObject.SetActive(false);
                emotions[1].gameObject.SetActive(false);
                emotions[2].gameObject.SetActive(false);
                emotions[3].gameObject.SetActive(false);
                emotions[activeEmotion].gameObject.SetActive(true);
                switch(activeEmotion) {
                    case 0:
                        emotionLabel.text = "HAPPY";
                        break;
                    case 1:
                        emotionLabel.text = "ANGRY";
                        break;
                    case 2:
                        emotionLabel.text = "SAD";
                        break;
                    case 3:
                        emotionLabel.text = "FEARFUL";
                        break;
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

                if(primaryHB.activeSelf || ultHB.activeSelf) {
                    dmgBonus = 1f;
                }
                primaryHB.SetActive(false);
                secondaryHB.SetActive(false);
                defHB.SetActive(false);
                ultHB.SetActive(false);
                activeEmotion = nextEmotion;

                while(isDead) {

                }

                if(lastSkill == "PRIMARY") {
                    skillDmg = 50;
                    primaryHB.SetActive(true);

                    lastSkill = "";
                }
                if(lastSkill == "SECONDARY") {
                    //happy == gcd decrease 5s
                    //mad == atk bonus 5s
                    //sad == invlun for 2s
                    //fear == speed bonus 5s
                    secondaryHB.SetActive(true);
                    StartCoroutine(EmotionTime(activeEmotion));
                    if(dmgBonus < 3f) {
                        dmgBonus += 0.5f;
                    }
                    
                    nextEmotion = UnityEngine.Random.Range(0,emotions.Length);
                    SendUpdate("EMOTION",nextEmotion.ToString());

                    lastSkill = "";
                }
                if(lastSkill == "DEFENSIVE") {
                    buffTimer = 2f;
                    defHB.SetActive(true);
                    StartCoroutine(InvulnTimer(buffTimer));

                    nextEmotion = UnityEngine.Random.Range(0,emotions.Length);
                    SendUpdate("EMOTION",nextEmotion.ToString());

                    lastSkill = "";
                }
                if(lastSkill == "ULT") {
                    skillDmg = 200;
                    ultHB.SetActive(true);

                    lastSkill = "";
                }
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
                gcd = 1f + gcdMod + gcdBase;
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
                gcd = 1f + gcdMod + gcdBase;
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
                gcd = 1f + gcdMod + gcdBase;
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
                gcd = 1f + gcdMod + gcdBase;
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
}
