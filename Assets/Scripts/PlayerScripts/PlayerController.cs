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
    public GameObject hitboxVisualization;
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
    public bool inMenu;
    public string lastSkill;

    public RectTransform worldSpaceUI;
    public string playerName;
    public int playerClass;
    public RectTransform statsPanel;
    public TextMeshProUGUI levelText, goldText, meleeText, rangedText, speedText; 
    public RectTransform skillsPanel; 
    public TextMeshProUGUI s1, s2, s3, s4;
    public Image gcd1, gcd2, gcd3, gcd4;
    public RectTransform otherPlayersPanel;
    public Sprite[] playerClassIcons;
    public RectTransform pauseMenu;

    public Animator animator;
    public GameObject shield;
    public GameObject damageIndicator;
    public GameObject model;

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
                lastInput = Vector3.zero;
                inShop = bool.Parse(value);

                SendUpdate("SHOP",inShop.ToString());
            }
            if(IsClient) {
                inShop = bool.Parse(value);
            }
        }
        if(flag == "HURT") {
            if(IsClient) {
                if(IsLocalPlayer) {
                    AudioManager.Instance.CreateSource(AudioManager.Instance.audioClips[24]);
                }
                isHurt = bool.Parse(value);
                hp--;
                isHurt = false;
                StartCoroutine(InvulnBlink());
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
                    hitboxVisualization.transform.GetChild(0).GetComponent<ParticleSystem>().Play();
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
                    hitboxVisualization.transform.GetChild(1).GetComponent<ParticleSystem>().Play();
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
                    hitboxVisualization.transform.GetChild(2).GetComponent<ParticleSystem>().Play();
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
                    hitboxVisualization.transform.GetChild(3).GetComponent<ParticleSystem>().Play();
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

                if(IsLocalPlayer) {
                    AudioManager.Instance.CreateSource(AudioManager.Instance.audioClips[21]);
                }
            }
        }
        if(flag == "DAMAGE") {
            if(IsClient) {
                totalDamage += int.Parse(value);
                SpawnDamageIndicator(int.Parse(value));
            }
        }
        if(flag == "MENU") {
            if(IsServer) {
                inMenu = bool.Parse(value);

                SendUpdate("MENU",value);
            }
            if(IsClient) {
                inMenu = bool.Parse(value);
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
            if(inMenu && !inShop) {
                Cursor.lockState = CursorLockMode.None;
                pauseMenu.gameObject.SetActive(true);
            }
            else if(!inMenu && !inShop) {
                Cursor.lockState = CursorLockMode.Locked;
                pauseMenu.gameObject.SetActive(false);
            }

            if(isDead && deathTimer > 0f) {
                worldSpaceUI.transform.GetChild(1).gameObject.SetActive(true);
                deathTimer -= Time.deltaTime;
                worldSpaceUI.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = "KNOCKED OUT...\n" + deathTimer.ToString("N1");
            }
            if(deathTimer <= 0) {
                worldSpaceUI.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = "PRESS ANY BUTTON TO REVIVE\n" + deathTimer.ToString("N1");
            }

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
        if((context.started || context.performed) && !inShop && !isDead && !inMenu) {
            SendCommand("MOVE", context.ReadValue<Vector2>().ToString());
        }
        if(context.canceled) {
            SendCommand("MOVE", Vector2.zero.ToString());
        }
    }

    public void UsePrimary(InputAction.CallbackContext context) {
        if(context.started && !usingPrimary && !inShop && !isDead && !inMenu) {
            SendCommand("PRIMARY", "true");
        }
    }

    public void UseSecondary(InputAction.CallbackContext context) {
        if(context.started && !usingSecondary && !inShop && !isDead && !inMenu) {
            SendCommand("SECONDARY", "true");
        }
    }

    public void UseDefensive(InputAction.CallbackContext context) {
        if(context.started && !usingDefensive && !inShop && !isDead && !inMenu) {
            SendCommand("DEFENSIVE", "true");
        }
    }

    public void UseUlt(InputAction.CallbackContext context) {
        if(context.started && !usingUlt && !inShop && !isDead && !inMenu) {
            SendCommand("ULT", "true");
        }
    }

    public void UseLimit(InputAction.CallbackContext context) {
        if(context.started && !usingLimit && !inShop && !isDead) {
            SendCommand("LIMIT", "true");
        }
    }

    public void Interact(InputAction.CallbackContext context) {
        if(context.started && withinInteract && !isDead && !inMenu) {
            rb.velocity = Vector3.zero;
            SendCommand("SHOP",(!inShop).ToString());
        }
    }

    public void Revive(InputAction.CallbackContext context) {
        if(context.started && isDead && deathTimer <= 0 && !inMenu) {
            SendCommand("REVIVE", "false");
        }
    }

    public void OpenMenu(InputAction.CallbackContext context) {
        if(context.started && !inShop) {
            SendCommand("MENU", (!inMenu).ToString());
        }
    }

    public IEnumerator SetPrefs() {
        foreach(NetworkPlayerManager n in FindObjectsOfType<NetworkPlayerManager>()) {
            if(n.Owner == Owner) {
                playerName = n.playerName;
                playerClass = n.playerClass;
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
                        currPlayer.GetChild(1).GetComponent<TextMeshProUGUI>().text = "KNOCKED OUT";
                    }
                    else {
                        currPlayer.GetChild(1).GetComponent<TextMeshProUGUI>().text = "HP: " + player.hp + "/" + player.maxHp;
                    }
                    currPlayer.GetChild(2).GetComponent<Image>().sprite = playerClassIcons[player.playerClass];
                    ct++;
                }

                yield return null;
            }
        }
    }
    
    public void SpawnDamageIndicator(int damage) {
        GameObject newDmg = Instantiate(damageIndicator, new Vector3(1,1,-1) + transform.position, transform.rotation);
        newDmg.GetComponentInChildren<TextMeshProUGUI>().text = damage.ToString();
    }

    public IEnumerator DistributeGoldExp(int gold, int exp) {
        if(IsServer) {
            foreach(PlayerController player in FindObjectsOfType<PlayerController>()) {
                Debug.Log("gold earned " + gold);
                Debug.Log("exp earned " + exp);
                player.gold += gold;
                player.exp += exp;
                player.SendUpdate("GOLD", player.gold.ToString());
                player.SendUpdate("EXP", player.exp.ToString());
                 
                yield return null;
            }
        }
    }

    public IEnumerator SetAnimation(Animator anim, string boolToSet)
    {
        anim.SetBool(boolToSet, true);
        yield return new WaitForEndOfFrame();
        anim.SetBool(boolToSet, false);
    }

    public IEnumerator InvulnTimer(float invulnTime) {
        invuln = true;
        SendUpdate("INVULN", invuln.ToString());

        yield return new WaitForSeconds(invulnTime);

        invuln = false;
        SendUpdate("INVULN", invuln.ToString());
    }

    public IEnumerator InvulnBlink() {
        invuln = true;
        SendUpdate("INVULN", invuln.ToString());

        int flickerCt = 0;
        while(flickerCt < 5) {
            model.SetActive(false);
            yield return new WaitForSeconds(0.1f);
            model.SetActive(true);
            yield return new WaitForSeconds(0.1f);

            flickerCt++;
        }

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
                SendUpdate("INVULN", "true");
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
                SendUpdate("INVULN", "false");
                break;
            case 3:
                speed -= spdBonus;
                SendUpdate("STATCHANGE","SPD,"+speed);
                break;
        }
    }
}