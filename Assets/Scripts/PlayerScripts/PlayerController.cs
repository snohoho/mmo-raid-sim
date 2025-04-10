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
    public float dmgBonus = 1f;
    public float primaryCD;
    public float secondaryCD;
    public float defCD;
    public float ultCD;
    public float gcd;
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
    public string lastSkill;

    public TextMeshProUGUI nameLabel;
    public string playerName;
    public RectTransform statsPanel;
    public TextMeshProUGUI levelText, goldText, meleeText, rangedText, speedText; 
    public RectTransform skillsPanel; 
    public TextMeshProUGUI s1, s2, s3, s4;
    public Image gcd1, gcd2, gcd3, gcd4;
    

    public override void HandleMessage(string flag, string value)
    {
        if(flag == "MOVE") {
            if(IsServer) {
                isMoving = true;
                lastInput = value.Vec2Parse();

                SendUpdate("MOVE", value);
            }
        }
        if(flag == "INTERACT") {
            if(IsClient) {
                withinInteract = bool.Parse(value);
            }
        }
        if(flag == "HURT") {
            if(IsServer) {
                isHurt = bool.Parse(value);

                SendUpdate("HURT", value);
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
    }

    public override void NetworkedStart()
    {

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
            }
            else if(rb.velocity != Vector3.zero) {
                isMoving = true;
            } 
        }
        if(IsClient) {
            //perform anim
        }

        //handle cooldown timers
        if(IsLocalPlayer) {
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
            }
            if(usingSecondary) {
                s2.text = secondaryCD.ToString("N1");
                if(secondaryCD > 0) {
                    secondaryCD -= Time.deltaTime;
                }
            }
            if(usingDefensive) {
                s3.text = defCD.ToString("N1");
                if(defCD > 0) {
                    defCD -= Time.deltaTime;
                }
            }
            if(usingUlt) {
                s4.text = ultCD.ToString("N1");
                if(ultCD > 0) {
                    ultCD -= Time.deltaTime;
                }
            }
        }
    }

    public void OnTriggerEnter(Collider col)
    {
        if(col.gameObject.CompareTag("Enemy") && !invuln) {
           if(IsServer) {
                hp--;
                SendUpdate("HURT",hp.ToString());
           }
        }
        if(col.gameObject.CompareTag("Item")) {
            if(IsServer) {
                withinInteract = true;
                SendUpdate("INTERACT","true");
            }
        }
    }

    public void OnTriggerExit(Collider col)
    {
        if(col.gameObject.CompareTag("Item")) {
            if(IsServer) {
                withinInteract = false;
                SendUpdate("INTERACT","false");
            }
        }
    }

    public void OnTriggerStay(Collider col)
    {
        
    }

    public void Move(InputAction.CallbackContext context) {
        if(context.started || context.performed) {
            SendCommand("MOVE", context.ReadValue<Vector2>().ToString());
        }
        if(context.canceled) {
            SendCommand("MOVE", Vector2.zero.ToString());
        }
    }

    public void UsePrimary(InputAction.CallbackContext context) {
        if(context.started && !usingPrimary) {
            SendCommand("PRIMARY", "true");
        }
    }

    public void UseSecondary(InputAction.CallbackContext context) {
        if(context.started && !usingSecondary) {
            SendCommand("SECONDARY", "true");
        }
    }

    public void UseDefensive(InputAction.CallbackContext context) {
        if(context.started && !usingDefensive) {
            SendCommand("DEFENSIVE", "true");
        }
    }

    public void UseUlt(InputAction.CallbackContext context) {
        if(context.started && !usingUlt) {
            SendCommand("ULT", "true");
        }
    }

    public void UseLimit(InputAction.CallbackContext context) {
        if(context.started && !usingLimit) {
            SendCommand("LIMIT", "true");
        }
    }

    public void Interact(InputAction.CallbackContext context) {
        if(context.started && withinInteract) {
            SendCommand("INTERACT","true");
        }
    }

    public void Revive(InputAction.CallbackContext context) {
        if(context.started && isDead && deathTimer <= 0) {
            SendCommand("REVIVE", "false");
        }
    }

    public IEnumerator InvulnTimer(float invulnTime) {
        invuln = true;

        yield return new WaitForSeconds(invulnTime);

        invuln = false;
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
                SendUpdate("STATCHANGE","MATK,"+meleeAtk.ToString());
                SendUpdate("STATCHANGE","RATK,"+rangedAtk.ToString());
                break;
            case 2:
                invuln = true;
                break;
            case 3:
                speed += spdBonus;
                SendUpdate("STATCHANGE","SPD,"+speed.ToString());
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
                SendUpdate("STATCHANGE","MATK,"+meleeAtk.ToString());
                SendUpdate("STATCHANGE","RATK,"+rangedAtk.ToString());
                break;
            case 2:
                invuln = false;
                break;
            case 3:
                speed -= spdBonus;
                SendUpdate("STATCHANGE","SPD,"+speed.ToString());
                break;
        }
    }
}
