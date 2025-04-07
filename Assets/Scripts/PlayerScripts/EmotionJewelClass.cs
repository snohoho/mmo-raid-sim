using System;
using System.Collections;
using System.Collections.Generic;
using NETWORK_ENGINE;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EmotionJewelClass : PlayerController
{
    public TextMeshProUGUI nameLabel;
    public string playerName;
    public RectTransform statsPanel;
    TextMeshProUGUI levelText, goldText, meleeText, rangedText, speedText; 
    public RectTransform skillsPanel; 
    TextMeshProUGUI s1, s2, s3, s4;
    public Image[] emotions;
    public TextMeshProUGUI emotionLabel;

    public int level = 1;
    public int gold = 0;
    public int hp = 5;
    public int meleeAtk = 0;
    public int rangedAtk = 50;
    public int speed = 6;
    public float primaryCD;
    public float secondaryCD;
    public float defCD;
    public float ultCD;
    public float gcdMod;
    public int activeEmotion;

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
        if(flag == "STATCHANGE") {
            string[] args = value.Split(',');
            string stat = args[0];
            string bonus = args[1];
            if(IsClient) {
                switch(stat) {
                    case "MATK":
                        meleeAtk = int.Parse(bonus);
                        break;
                    case "RATK":
                        rangedAtk = int.Parse(bonus);
                        break; 
                    case "SPD":
                        speed = int.Parse(bonus);
                        break; 
                }
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
        if(flag == "EMOTION") {
            int emotion = int.Parse(value);
            if(IsClient) {
                activeEmotion = emotion;
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
        if(IsServer) {
            activeEmotion = UnityEngine.Random.Range(0,emotions.Length);
            SendUpdate("EMOTION",activeEmotion.ToString());
        }
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

                levelText.text = level.ToString();
                goldText.text = gold.ToString();
                meleeText.text = meleeAtk.ToString();
                rangedText.text = rangedAtk.ToString();
                speedText.text = speed.ToString();
            }
            if(IsServer) {
                while(isDead) {

                }

                if(isHurt) {
                    hp -= 1;
                    isHurt = false;
                }

                if(lastSkill == "PRIMARY") {
                    lastSkill = "";
                }
                if(lastSkill == "SECONDARY") {
                    //happy == gcd decrease 5s
                    //mad == atk bonus 5s
                    //sad == invlun for 2s
                    //fear == speed bonus 5s
                    switch(activeEmotion) {
                        case 0:
                            gcdMod = -0.5f;
                            break;
                        case 1:
                            meleeAtk *= 2;
                            rangedAtk *= 2;
                            SendUpdate("STATCHANGE","MATK,"+meleeAtk.ToString());
                            SendUpdate("STATCHANGE","RATK,"+rangedAtk.ToString());
                            break;
                        case 2:

                            break;
                        case 3:
                            speed += 2;
                            SendUpdate("STATCHANGE","SPD,"+speed.ToString());
                            break;
                    }
                    StartCoroutine(EmotionTime(activeEmotion));
                    
                    activeEmotion = UnityEngine.Random.Range(0,emotions.Length);
                    SendUpdate("EMOTION",activeEmotion.ToString());
                    lastSkill = "";
                }
                if(lastSkill == "DEFENSIVE") {
                    lastSkill = "";
                }
                if(lastSkill == "ULT") {

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
                    s1.text = primaryCD.ToString("N1");
                }
                if(primaryCD > 0) {
                    primaryCD -= Time.deltaTime;
                }
            }
            if(usingDefensive) {
                if(gcd <= 0) {
                    s1.text = primaryCD.ToString("N1");
                }
                if(primaryCD > 0) {
                    primaryCD -= Time.deltaTime;
                }
            }
            if(usingUlt) {
                if(gcd <= 0) {
                    s1.text = primaryCD.ToString("N1");
                }
                if(primaryCD > 0) {
                    primaryCD -= Time.deltaTime;
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

    public IEnumerator EmotionTime(int emotion) {
        if(emotion == 2) {
            yield return new WaitForSeconds(2f);
        }
        else {
            yield return new WaitForSeconds(5f);
        }
        
        switch(emotion) {
            case 0:
                gcdMod = 0;
                break;
            case 1:
                meleeAtk /= 2;
                rangedAtk /= 2;
                SendUpdate("STATCHANGE","MATK,"+meleeAtk.ToString());
                SendUpdate("STATCHANGE","RATK,"+rangedAtk.ToString());
                break;
            case 2:

                break;
            case 3:
                speed -= 2;
                SendUpdate("STATCHANGE","SPD,"+speed.ToString());
                break;
        }
    }
}
