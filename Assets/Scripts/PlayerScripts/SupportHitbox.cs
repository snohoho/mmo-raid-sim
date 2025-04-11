using System.Collections;
using System.Collections.Generic;
using System.Net;
using NETWORK_ENGINE;
using UnityEngine;

public class SupportHitbox : NetworkComponent
{
    public PlayerController controller;
    public override void HandleMessage(string flag, string value)
    {
        
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

    void Update()
    {
        
    }

    void OnTriggerEnter(Collider col)
    {
        if(col.gameObject.CompareTag("Player")) {
            if(IsServer) {
                Debug.Log("support effect on " + col.gameObject.name);

                if(gameObject.tag == "Invuln") {
                    col.gameObject.GetComponent<PlayerController>().StartCoroutine(col.gameObject.GetComponent<PlayerController>().InvulnTimer(controller.buffTimer));
                }
                if(gameObject.tag == "Emotion") {
                    int emotion = controller.GetComponent<EmotionJewelClass>().activeEmotion;
                    col.gameObject.GetComponent<PlayerController>().StartCoroutine(col.gameObject.GetComponent<PlayerController>().EmotionTime(emotion));
                }
            }
        }
    }
}
