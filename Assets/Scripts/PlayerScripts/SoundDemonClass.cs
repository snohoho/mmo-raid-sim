using System;
using System.Collections;
using System.Collections.Generic;
using NETWORK_ENGINE;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SoundDemonClass : PlayerController
{
    public TextMeshProUGUI nameLabel;
    public string playerName;
    public RectTransform statsPanel;
    TextMeshProUGUI levelText, goldText, meleeText, rangedText, speedText; 
    public RectTransform skillsPanel; 
    TextMeshProUGUI s1, s2, s3, s4;
    public RectTransform notesPanel;
    GameObject n1, n2, n3;

    public int level = 1;
    public int gold = 0;
    public int hp = 5;
    public int meleeAtk = 0;
    public int rangedAtk = 100;
    public int speed = 5;
    public float primaryCD;
    public float secondaryCD;
    public float defCD;
    public float ultCD;
    public float gcdMod;
    public bool note1;
    public bool note2;
    public bool note3;

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
        if(flag == "NOTE") {
            int note = int.Parse(value);
            if(IsClient) {
                switch(note) {
                    case 0:
                        note1 = true;
                        break;
                    case 1:
                        note2 = true;
                        break;
                    case 2:
                        note3 = true;
                        break;
                    case 3:
                        note1 = false;
                        note2 = false;
                        note3 = false;
                        break;
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
                n1 = notesPanel.GetChild(0).gameObject;
                n2 = notesPanel.GetChild(1).gameObject;
                n3 = notesPanel.GetChild(2).gameObject;
                n1.SetActive(note1);
                n2.SetActive(note2);
                n3.SetActive(note3);

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
                    note1 = true;
                    lastSkill = "";
                    SendUpdate("NOTE","0");
                }
                if(lastSkill == "SECONDARY") {
                    note2 = true;
                    lastSkill = "";
                    SendUpdate("NOTE","1");
                }
                if(lastSkill == "DEFENSIVE") {
                    note3 = true;
                    lastSkill = "";
                    SendUpdate("NOTE","2");
                }
                if(lastSkill == "ULT") {
                    int noteCt = 0;
                    if(note1) { noteCt++; }
                    if(note2) { noteCt++; }
                    if(note3) { noteCt++; }

                    int prevAtk = rangedAtk;
                    if(noteCt == 1) {
                        rangedAtk = (int)(rangedAtk*1.2);
                    }
                    else if(noteCt == 2) {
                        rangedAtk = (int)(rangedAtk*1.5);
                    }
                    else if(noteCt == 3) {
                        rangedAtk *= 2;
                    }

                    note1 = false;
                    note2 = false;
                    note3 = false;
                    rangedAtk = prevAtk;

                    lastSkill = "";
                    SendUpdate("NOTE","3");
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
}