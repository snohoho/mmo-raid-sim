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
    
    public bool isMoving;
    public bool usingPrimary;
    public bool usingSecondary;
    public bool usingDefensive;
    public bool usingUlt;
    public bool usingLimit;
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
        if(flag == "PRIMARY") {
            if(IsServer) {
                usingPrimary = bool.Parse(value);
                lastSkill = flag;
                SendUpdate("PRIMARY", value);
            }
        }
        if(flag == "SECONDARY") {
            if(IsServer) {
                usingSecondary = bool.Parse(value);
                lastSkill = flag;
                SendUpdate("SECONDARY", value);
            }
        }
        if(flag == "DEFENSIVE") {
            if(IsServer) {
                usingDefensive = bool.Parse(value);
                lastSkill = flag;
                SendUpdate("DEFENSIVE", value);
            }
        }
        if(flag == "ULT") {
            if(IsServer) {
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

    public void Move(InputAction.CallbackContext context) {
        if(context.started || context.performed) {
            Debug.Log("START");
            SendCommand("MOVE", context.ReadValue<Vector2>().ToString());
        }
        if(context.canceled) {
            Debug.Log("STOP");
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

    void Start()
    {
        
    }

    void Update()
    {
        if(IsServer) {
            rb.velocity = new Vector3(lastInput.x, 0f, lastInput.y) * 5f;

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
    }
}
