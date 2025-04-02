using System.Collections;
using System.Collections.Generic;
using NETWORK_ENGINE;
using UnityEngine;

public class PlayerRigidbody : NetworkComponent
{
    public Vector3 lastPos;
    public Vector3 lastVel;

    public float threshold = 0.1f, eThreshold = 1.0f;
    public bool useAdjusted = false;
    public Vector3 adjustedVel = Vector3.zero;

    public Rigidbody rb;

    public override void HandleMessage(string flag, string value)
    {
        if(flag == "CHANGEPOS") {
            if(IsClient) {
                lastPos = NetworkCore.Vector3FromString(value);

                if(useAdjusted) {
                    adjustedVel = lastPos - rb.position; 
                }
                if((lastPos - rb.position).magnitude > eThreshold) {
                    rb.position = lastPos;
                    adjustedVel = Vector3.zero;
                }
            }
        }
        if(flag == "CHANGEVEL") {
            if(IsClient) {
                Debug.Log(lastVel);
                lastVel = NetworkCore.Vector3FromString(value);
                if(lastVel.magnitude <= 0.1f) {
                    adjustedVel = Vector3.zero;
                }
            }
        }
    }

    public override void NetworkedStart()
    {
        
    }

    public override IEnumerator SlowUpdate()
    {
        while(IsConnected) {
            if(IsServer) {
                if((rb.position - lastPos).magnitude > threshold) {
                    lastPos = rb.position;
                    SendUpdate("CHANGEPOS", rb.position.ToString());
                }
                if((rb.velocity - lastVel).magnitude > threshold) {
                    lastVel = rb.velocity;
                    SendUpdate("CHANGEVEL", rb.velocity.ToString());
                }
                if(IsDirty) {
                    SendUpdate("CHANGEPOS", rb.position.ToString());
                    SendUpdate("CHANGEVEL", rb.velocity.ToString());

                    IsDirty = false;   
                }
            }
            
            yield return new WaitForSeconds(MyCore.MasterTimer);
        }
    }

    void Start()
    {
        
    }

    void Update()
    {
        if (IsClient){
            rb.velocity = lastVel;

            bool barelyMovingOrStopped = lastVel.magnitude < 0.01f;

            if (barelyMovingOrStopped){
                rb.velocity += adjustedVel;
            }
        }
    }
}
