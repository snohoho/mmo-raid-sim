using System.Collections;
using System.Collections.Generic;
using NETWORK_ENGINE;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCamera : NetworkComponent
{
    public Vector2 camPos;
    public float currentX = 0f;
    public float currentY = 0f;
    public float sensivity = 5.0f;
    public float threshold = 0.1f;
    public Rigidbody rb;
    
    public override void HandleMessage(string flag, string value)
    {
        if(flag == "LOOK") {
            if(IsServer) {
                camPos = value.Vec2Parse();

                SendUpdate("LOOK", value);
            }
            if(IsLocalPlayer) {
                camPos = value.Vec2Parse();
            }
        }
    }

    public override void NetworkedStart()
    {
        rb = gameObject.GetComponent<Rigidbody>();
    }

    public override IEnumerator SlowUpdate()
    {
        while(IsConnected) {
            

            yield return new WaitForSeconds(MyCore.MasterTimer);
        }
    }

    void Start()
    {
        
    }

    void Update()
    {
        if(IsServer) {
            currentX += camPos.x * sensivity * Time.deltaTime;
            rb.MoveRotation(Quaternion.Euler(0, currentX, 0));
        }
        if(IsLocalPlayer) {
            currentX += camPos.x * sensivity * Time.deltaTime;
            currentY += camPos.y * sensivity * Time.deltaTime;

            currentY = Mathf.Clamp(currentY, 0f, 30f);

            Vector3 direction = new Vector3(0, 1f, -10f);
            Quaternion rotation = Quaternion.Euler(currentY, currentX, 0);

            Camera.main.transform.position = transform.position + rotation * direction;
            Camera.main.transform.LookAt(transform.position);
        }
    }

    public void Look(InputAction.CallbackContext context) {
        if(context.started) {
            if(IsLocalPlayer) {
                //camPos = context.ReadValue<Vector2>();
                SendCommand("LOOK", context.ReadValue<Vector2>().ToString());
            }
        }
        if(context.canceled) {
            if(IsLocalPlayer) {
                //camPos = Vector2.zero;
                SendCommand("LOOK", Vector2.zero.ToString());
            }
        }
    }
}
