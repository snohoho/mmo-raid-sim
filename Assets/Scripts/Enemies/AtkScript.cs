using System.Collections;
using System.Collections.Generic;
using System.Net;
using NETWORK_ENGINE;
using UnityEngine;

public class AtkScript : NetworkComponent
{
    public BossHitboxes hitboxes;
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

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnTriggerStay(Collider col)
    {
        if(col.gameObject.CompareTag("Player")) {
            if(IsServer) {
                if(hitboxes.count == 10){
                    if(gameObject.CompareTag("Atk1")) {
                        if(col.gameObject.GetComponent<PlayerController>().invuln = false)
                        {
                            Debug.Log(col.gameObject.name + " INVULN");
                            hitboxes.atk1HB.SetActive(false);
                        } else {
                            col.gameObject.GetComponent<PlayerController>().hp -= 1;
                            Debug.Log("PLAYER HIT");
                            SendUpdate("HURT",col.gameObject.GetComponent<PlayerController>().hp.ToString());
                            hitboxes.atk1HB.SetActive(false);
                        }
                    }
                    if(gameObject.CompareTag("Atk2")) {
                        if(col.gameObject.GetComponent<PlayerController>().invuln = false)
                        {
                            Debug.Log(col.gameObject.name + " INVULN");
                            hitboxes.atk2HB.SetActive(false);
                        } else {
                            col.gameObject.GetComponent<PlayerController>().hp -= 1;
                            Debug.Log("PLAYER HIT");
                            SendUpdate("HURT",col.gameObject.GetComponent<PlayerController>().hp.ToString());
                            hitboxes.atk2HB.SetActive(false);
                        }
                    }
                    if(gameObject.CompareTag("Atk3")) {
                        if(col.gameObject.GetComponent<PlayerController>().invuln = false)
                        {
                            Debug.Log(col.gameObject.name + " INVULN");
                            hitboxes.atk3HB.SetActive(false);
                        } else {
                            col.gameObject.GetComponent<PlayerController>().hp -= 1;
                            Debug.Log("PLAYER HIT");
                            SendUpdate("HURT",col.gameObject.GetComponent<PlayerController>().hp.ToString());
                            hitboxes.atk3HB.SetActive(false);
                        }
                    }
                    if(gameObject.CompareTag("Atk4")) {
                        if(col.gameObject.GetComponent<PlayerController>().invuln = false)
                        {
                            Debug.Log(col.gameObject.name + " INVULN");
                            hitboxes.atk4HB.SetActive(false);
                        } else {
                            col.gameObject.GetComponent<PlayerController>().hp -= 1;
                            Debug.Log("PLAYER HIT");
                            SendUpdate("HURT",col.gameObject.GetComponent<PlayerController>().hp.ToString());
                            hitboxes.atk4HB.SetActive(false);
                        }
                    }
                }
            }
        }
    }
}
