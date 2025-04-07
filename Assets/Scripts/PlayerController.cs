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
    public float distance = 10.0f;
    public Vector2 camPos;
    public float currentX = 0f;
    public float currentY = 0f;
    public float sensivity = 4.0f;
    public Transform lookAt;
    
    public bool isMoving;
    public bool isHurt;
    public bool isDead;
    public float deathTimer;
    public bool usingPrimary;
    public bool usingSecondary;
    public bool usingDefensive;
    public bool usingUlt;
    public bool usingLimit;
    public string lastSkill;
    public float gcd;

    public ItemStats[] inventory;

    public override void HandleMessage(string flag, string value)
    {
        if(flag == "MOVE") {
            if(IsServer) {
                isMoving = true;
                lastInput = value.Vec2Parse();

                SendUpdate("MOVE", value);
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
            rb.velocity = (transform.forward * lastInput.x) + (transform.right * lastInput.y) * 5f;

            if(rb.velocity == Vector3.zero) {
                isMoving = false;
            }
            else if(rb.velocity != Vector3.zero) {
                isMoving = true;
            }

            currentX += camPos.x * sensivity * Time.deltaTime;
            currentY += camPos.y * sensivity * Time.deltaTime;
            rb.rotation = Quaternion.Euler(0, currentX, 0);
        }
        if(IsClient) {
            //perform anim
        }
        if(IsLocalPlayer) {
            currentX += camPos.x * sensivity * Time.deltaTime;
            currentY += camPos.y * sensivity * Time.deltaTime;

            currentY = Mathf.Clamp(currentY, 0f, 50f);

            Vector3 direction = new Vector3(0, 0, -distance);
            Quaternion rotation = Quaternion.Euler(currentY, currentX, 0);
            Camera.main.transform.position = lookAt.position + rotation * direction;
           
            Camera.main.transform.LookAt(lookAt.position);
        }
    }

    public void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag == "Enemy") {
            SendCommand("HURT","true");
        }
    }

    public void Move(InputAction.CallbackContext context) {
        if(context.started || context.performed) {
            SendCommand("MOVE", context.ReadValue<Vector2>().ToString());
        }
        if(context.canceled) {
            SendCommand("MOVE", Vector2.zero.ToString());
        }
    }

    public void Look(InputAction.CallbackContext context) {
        if(context.started) {
            camPos = context.ReadValue<Vector2>();
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
    
    public void OpenInventory(InputAction.CallbackContext context) {
        if(context.started) {
            SendCommand("OPENINV", "true");
        }
    }

    public void Revive(InputAction.CallbackContext context) {
        if(context.started && isDead && deathTimer <= 0) {
            SendCommand("REVIVE", "false");
        }
    }
}
