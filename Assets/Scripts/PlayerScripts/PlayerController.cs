using System;
using System.Collections;
using System.Collections.Generic;
using NETWORK_ENGINE;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : NetworkComponent
{
    public Vector3 lastInput;
    public Rigidbody rb;
    
    public int hp;
    public int meleeAtk;
    public int rangedAtk;
    public int speed;
    public float skillDmg;
    public float dmgBonus;
    public float gcd;
    public bool invuln;
    public float buffTimer;

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
            if(IsServer && gcd <= 0) {
                usingPrimary = bool.Parse(value);
                lastSkill = flag;

                SendUpdate("PRIMARY", value);
            }
            if(IsClient) {
                usingPrimary = bool.Parse(value);
            }
        }
        if(flag == "SECONDARY") {
            if(IsServer && gcd <= 0) {
                usingSecondary = bool.Parse(value);
                lastSkill = flag;

                SendUpdate("SECONDARY", value);
            }
        }
        if(flag == "DEFENSIVE") {
            if(IsServer && gcd <= 0) {
                usingDefensive = bool.Parse(value);
                lastSkill = flag;

                SendUpdate("DEFENSIVE", value);
            }
        }
        if(flag == "ULT") {
            if(IsServer && gcd <= 0) {
                usingUlt = bool.Parse(value);
                lastSkill = flag;

                SendUpdate("ULT", value);
            }
        }
        if(flag == "LIMIT") {
            if(IsServer) {
                usingLimit = bool.Parse(value);

                SendUpdate("LIMIT", value);
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
        if(IsServer) {
            rb.velocity = (transform.forward * lastInput.y + transform.right * lastInput.x) * 5f;

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
        if(IsLocalPlayer) {
            
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
}
