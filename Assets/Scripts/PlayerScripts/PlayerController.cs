using System;
using System.Collections;
using System.Collections.Generic;
using NETWORK_ENGINE;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerController : NetworkComponent
{
    public Vector3 lastInput;
    public Rigidbody rb;
    
    public int hp;
    public int maxHp;
    public int meleeAtk;
    public int rangedAtk;
    public int speed;
    public float skillDmg;
    public float dmgBonusBase = 1f;
    public float dmgBonus;
    public float primaryCD;
    public float secondaryCD;
    public float defCD;
    public float ultCD;
    public float gcd;
    public float gcdBase = 0f;
    public float gcdMod = 0f;
    public float gcdMax;
    public bool invuln;
    public float buffTimer;
    public GameObject primaryHB;
    public GameObject secondaryHB;
    public GameObject defHB;
    public GameObject ultHB;
    public int level = 1;
    public int gold = 0;
    public int exp = 0;
    public int totalDamage = 0;

    public bool isMoving;
    public bool isHurt;
    public bool isDead;
    public float deathTimer;
    public bool usingPrimary;
    public bool usingSecondary;
    public bool usingDefensive;
    public bool usingUlt;
    public bool usingLimit;
    public bool withinInteract;
    public bool inShop;
    public string lastSkill;

    public RectTransform worldSpaceUI;
    public string playerName;
    public RectTransform statsPanel;
    public TextMeshProUGUI levelText, goldText, meleeText, rangedText, speedText; 
    public RectTransform skillsPanel; 
    public TextMeshProUGUI s1, s2, s3, s4;
    public Image gcd1, gcd2, gcd3, gcd4;
    public RectTransform otherPlayersPanel;

    public Animator animator;
    public GameObject shield;


    public override void HandleMessage(string flag, string value)
    {
        if(flag == "MOVE") {
            if(IsServer) {
                isMoving = true;
                lastInput = value.Vec2Parse();
            }
            if(IsClient) {
                isMoving = bool.Parse(value);
            }
        }
        if(flag == "INTERACT") {
            if(IsServer) {
                withinInteract = bool.Parse(value);

                SendUpdate("INTERACT","true");
            }
            if(IsClient) {
                withinInteract = bool.Parse(value);
            }
        }
        if(flag == "SHOP") {
            if(IsServer) {
                inShop = bool.Parse(value);

                SendUpdate("SHOP",inShop.ToString());
            }
            if(IsClient) {
                inShop = bool.Parse(value);
            }
        }
        if(flag == "HURT") {
            if(IsClient) {
                isHurt = bool.Parse(value);
                hp--;
                isHurt = false;
            }
        }
        if(flag == "DEAD") {
            if(IsClient) {
                isDead = true;
                invuln = true;
                deathTimer = float.Parse(value);
            }
        }
        if(flag == "REVIVE") {
            if(IsServer) {
                hp = maxHp;
                isDead = bool.Parse(value);
                StartCoroutine(InvulnTimer(3f));

                SendUpdate("REVIVE", value);
            }
            if(IsClient) {
                hp = maxHp;
                isDead = bool.Parse(value);
                worldSpaceUI.transform.GetChild(1).gameObject.SetActive(false);
            }
        }
        if(flag == "INVULN") {
            if(IsClient) {
                invuln = bool.Parse(value);
                shield.SetActive(invuln);
            }
        }
        if(flag == "PRIMARY") {
            if(IsServer && gcd <= 0 && primaryCD <= 0) {
                usingPrimary = bool.Parse(value);
                lastSkill = flag;
                
                SendUpdate("PRIMARY", value);
            }
            if(IsClient) {
                usingPrimary = bool.Parse(value);
                if(usingPrimary) {
                    StartCoroutine(SetAnimation(animator, "DoingPrimary"));
                }
            }
        }
        if(flag == "SECONDARY") {
            if(IsServer && gcd <= 0 && secondaryCD <= 0) {
                usingSecondary = bool.Parse(value);
                lastSkill = flag;

                SendUpdate("SECONDARY", value);
            }
            if(IsClient) {
                usingSecondary = bool.Parse(value);
                if(usingSecondary) {
                    StartCoroutine(SetAnimation(animator, "DoingSecondary"));
                }
            }
        }
        if(flag == "DEFENSIVE") {
            if(IsServer && gcd <= 0 && defCD <= 0) {
                usingDefensive = bool.Parse(value);
                lastSkill = flag;

                SendUpdate("DEFENSIVE", value);
            }
            if(IsClient) {
                usingDefensive = bool.Parse(value);
                if(usingDefensive) {
                    StartCoroutine(SetAnimation(animator, "DoingDefensive"));
                }
            }
        }
        if(flag == "ULT") {
            if(IsServer && gcd <= 0 && ultCD <= 0) {
                usingUlt = bool.Parse(value);
                lastSkill = flag;

                SendUpdate("ULT", value);
            }
            if(IsClient) {
                usingUlt = bool.Parse(value);
                if(usingUlt){
                    StartCoroutine(SetAnimation(animator, "DoingSpecial"));
                }
            }
        }
        if(flag == "LIMIT") {
            if(IsServer) {
                usingLimit = bool.Parse(value);

                SendUpdate("LIMIT", value);
            }
        }
        if(flag == "GBLCD") {
            float cd = float.Parse(value);
            if(IsClient) {
                gcd = cd;
                gcdMax = cd;
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
        if(flag == "GOLD") {
            if(IsServer) {
                gold = int.Parse(value);
            }
            if(IsClient) {
                gold = int.Parse(value);
            }
        }
        if(flag == "EXP") {
            if(IsServer) {
                exp = int.Parse(value);
            }
            if(IsClient) {
                exp = int.Parse(value);
            }
        }
        if(flag == "LVLUP") {
            if(IsClient) {
                level = int.Parse(value);
                meleeAtk += 10;
                rangedAtk += 10;
            }
        }
        if(flag == "DAMAGE") {
            if(IsClient) {
                totalDamage = int.Parse(value);
            }
        }
    }

    public override void NetworkedStart()
    {
        StartCoroutine(SetPrefs());
        StartCoroutine(UpdatePlayerInfo());
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

            levelText.text = "LVL" + level.ToString();
            goldText.text = gold.ToString() + "G";
            meleeText.text = meleeAtk.ToString();
            rangedText.text = rangedAtk.ToString();
            speedText.text = speed.ToString();
        }
    }

    public override IEnumerator SlowUpdate()
    {
        
        yield return new WaitForSeconds(MyCore.MasterTimer);
    }

    public IEnumerator SetAnimation(Animator anim, string boolToSet)
    {
        anim.SetBool(boolToSet, true);
        yield return new WaitForEndOfFrame();
        anim.SetBool(boolToSet, false);
    }

    void Start()
    {
        
    }

    public virtual void Update()
    {
        //handle rigidbody
        if(IsServer) {
            rb.velocity = (transform.forward * lastInput.y + transform.right * lastInput.x) * speed;

            if(rb.velocity == Vector3.zero) {
                isMoving = false;
                SendUpdate("MOVE",isMoving.ToString());
            }
            else if(rb.velocity != Vector3.zero) {
                isMoving = true;
                SendUpdate("MOVE",isMoving.ToString());
            } 
        }
        if(IsClient) {
            //perform anim
            animator.SetBool("Walking", isMoving);
            animator.SetBool("Dead", isDead);
        }

        //handle cooldown timers
        if(IsLocalPlayer) {
            if(isDead && deathTimer > 0f) {
                worldSpaceUI.transform.GetChild(1).gameObject.SetActive(true);
                deathTimer -= Time.deltaTime;
                worldSpaceUI.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = "KNOCKED OUT...\n" + deathTimer.ToString("N1");
            }
            if(deathTimer <= 0) {
                worldSpaceUI.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = "PRESS ANY BUTTON TO REVIVE\n" + deathTimer.ToString("N1");
            }

            s1 = skillsPanel.GetChild(0).GetChild(0).gameObject.GetComponent<TextMeshProUGUI>();
            s2 = skillsPanel.GetChild(1).GetChild(0).gameObject.GetComponent<TextMeshProUGUI>();
            s3 = skillsPanel.GetChild(2).GetChild(0).gameObject.GetComponent<TextMeshProUGUI>();
            s4 = skillsPanel.GetChild(3).GetChild(0).gameObject.GetComponent<TextMeshProUGUI>();
            gcd1 = skillsPanel.GetChild(0).GetChild(1).gameObject.GetComponent<Image>();
            gcd2 = skillsPanel.GetChild(1).GetChild(1).gameObject.GetComponent<Image>();
            gcd3 = skillsPanel.GetChild(2).GetChild(1).gameObject.GetComponent<Image>();
            gcd4 = skillsPanel.GetChild(3).GetChild(1).gameObject.GetComponent<Image>();
            if(gcd > 0) {
                gcd1.fillAmount = gcd/gcdMax;
                gcd2.fillAmount = gcd/gcdMax;
                gcd3.fillAmount = gcd/gcdMax;
                gcd4.fillAmount = gcd/gcdMax;
                gcd -= Time.deltaTime;
            }
            if(gcd <= 0) {
                gcd1.fillAmount = 0;
                gcd2.fillAmount = 0;
                gcd3.fillAmount = 0;
                gcd4.fillAmount = 0;
            }
            if(usingPrimary) {
                s1.text = primaryCD.ToString("N1");
                if(primaryCD > 0) {
                    primaryCD -= Time.deltaTime;
                }
                if(primaryCD <= 0) {
                    primaryCD = 0;
                    s1.text = "";
                }
            }
            if(usingSecondary) {
                s2.text = secondaryCD.ToString("N1");
                if(secondaryCD > 0) {
                    secondaryCD -= Time.deltaTime;
                }
                if(secondaryCD <= 0) {
                    secondaryCD = 0;
                    s2.text = "";
                }
            }
            if(usingDefensive) {
                s3.text = defCD.ToString("N1");
                if(defCD > 0) {
                    defCD -= Time.deltaTime;
                }
                if(defCD <= 0) {
                    defCD = 0;
                    s3.text = "";
                }
            }
            if(usingUlt) {
                s4.text = ultCD.ToString("N1");
                if(ultCD > 0) {
                    ultCD -= Time.deltaTime;
                }
                if(ultCD <= 0) {
                    ultCD = 0;
                    s4.text = "";
                }
            }


        }
    }

    public void OnTriggerEnter(Collider col)
    {
        if(col.gameObject.CompareTag("Shop")) {
            if(IsServer) {
                withinInteract = true;
                SendUpdate("INTERACT","true");
            }
        }
    }

    public void OnTriggerExit(Collider col)
    {
        if(col.gameObject.CompareTag("Shop")) {
            if(IsServer) {
                withinInteract = false;
                SendUpdate("INTERACT","false");
            }
        }
    }

    public void Move(InputAction.CallbackContext context) {
        if((context.started || context.performed) && !inShop && !isDead) {
            SendCommand("MOVE", context.ReadValue<Vector2>().ToString());
        }
        if(context.canceled) {
            SendCommand("MOVE", Vector2.zero.ToString());
        }
    }

    public void UsePrimary(InputAction.CallbackContext context) {
        if(context.started && !usingPrimary && !inShop && !isDead) {
            SendCommand("PRIMARY", "true");
        }
    }

    public void UseSecondary(InputAction.CallbackContext context) {
        if(context.started && !usingSecondary && !inShop && !isDead) {
            SendCommand("SECONDARY", "true");
        }
    }

    public void UseDefensive(InputAction.CallbackContext context) {
        if(context.started && !usingDefensive && !inShop && !isDead) {
            SendCommand("DEFENSIVE", "true");
        }
    }

    public void UseUlt(InputAction.CallbackContext context) {
        if(context.started && !usingUlt && !inShop && !isDead) {
            SendCommand("ULT", "true");
        }
    }

    public void UseLimit(InputAction.CallbackContext context) {
        if(context.started && !usingLimit && !inShop && !isDead) {
            SendCommand("LIMIT", "true");
        }
    }

    public void Interact(InputAction.CallbackContext context) {
        if(context.started && withinInteract && !isDead) {
            SendCommand("SHOP",(!inShop).ToString());
        }
    }

    public void Revive(InputAction.CallbackContext context) {
        if(context.started && isDead && deathTimer <= 0) {
            SendCommand("REVIVE", "false");
        }
    }

    public IEnumerator SetPrefs() {
        foreach(NetworkPlayerManager n in FindObjectsOfType<NetworkPlayerManager>()) {
            if(n.Owner == Owner) {
                playerName = n.playerName;
                worldSpaceUI.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = playerName;
            }

            yield return null;
        }
    }

    public IEnumerator UpdatePlayerInfo() {
        while(IsConnected) {
            int ct = 0;
            foreach(PlayerController player in FindObjectsOfType<PlayerController>()) {
                if(player.Owner != Owner) {
                    Transform currPlayer = otherPlayersPanel.transform.GetChild(ct);
                    currPlayer.gameObject.SetActive(true);
                    
                    currPlayer.GetChild(0).GetComponent<TextMeshProUGUI>().text = player.playerName;
                    if(player.isDead) {
                        currPlayer.GetChild(1).GetComponent<TextMeshProUGUI>().text = player.deathTimer.ToString("N1");
                    }
                    else {
                        currPlayer.GetChild(1).GetComponent<TextMeshProUGUI>().text = "HP: " + player.hp + "/" + player.maxHp;
                    }
                    ct++;
                }

                yield return null;
            }
        }
    }

    public IEnumerator DistributeGoldExp(int gold = 0, int exp = 0) {
        if(IsServer) {
            foreach(PlayerController player in FindObjectsOfType<PlayerController>()) {
                Debug.Log("gold earned " + gold);
                Debug.Log("exp earned " + exp);
                player.gold += gold;
                player.exp += exp;
                SendUpdate("GOLD", player.gold.ToString());
                SendUpdate("EXP", player.exp.ToString());
                 
                yield return null;
            }
        }
    }

    public IEnumerator InvulnTimer(float invulnTime) {
        invuln = true;
        SendUpdate("INVULN", invuln.ToString());

        yield return new WaitForSeconds(invulnTime);

        invuln = false;
        SendUpdate("INVULN", invuln.ToString());
    }

    public IEnumerator EmotionTime(int emotion) {
        Debug.Log(gameObject.name + " emotion: " + emotion);
        int melBonus = meleeAtk;
        int rngBonus = rangedAtk;
        int spdBonus = 2;
        float newGcdBonus = -0.5f; 
        switch(emotion) {
            case 0:
                gcdMod += newGcdBonus;
                break;
            case 1:
                meleeAtk += melBonus;
                rangedAtk += rngBonus;
                SendUpdate("STATCHANGE","MATK,"+meleeAtk);
                SendUpdate("STATCHANGE","RATK,"+rangedAtk);
                break;
            case 2:
                invuln = true;
                break;
            case 3:
                speed += spdBonus;
                SendUpdate("STATCHANGE","SPD,"+speed);
                break;
        }

        if(emotion == 2) {
            yield return new WaitForSeconds(2f);
        }
        else {
            yield return new WaitForSeconds(5f);
        }

        Debug.Log(gameObject.name + "emotion end: " + emotion);
        switch(emotion) {
            case 0:
                gcdMod -= newGcdBonus;
                break;
            case 1:
                meleeAtk -= melBonus;
                rangedAtk -= rngBonus;
                SendUpdate("STATCHANGE","MATK,"+meleeAtk);
                SendUpdate("STATCHANGE","RATK,"+rangedAtk);
                break;
            case 2:
                invuln = false;
                break;
            case 3:
                speed -= spdBonus;
                SendUpdate("STATCHANGE","SPD,"+speed);
                break;
        }
    }
}