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
                float damage;
                if(hitboxes.count == 10){
                    if(gameObject.CompareTag("Atk1")) {
                        if(col.gameObject.GetComponent<PlayerController>().invuln = false)
                        {
                            damage = 0;
                            col.gameObject.GetComponent<PlayerController>().hp -= (int)damage;
                            Debug.Log("PLAYER HIT: " + col.gameObject.name + "\nDAMAGE DEALT: " + damage);
                            hitboxes.atk1HB.SetActive(false);
                        } else {
                            damage = hitboxes.atkDmg;
                            col.gameObject.GetComponent<PlayerController>().hp -= (int)damage;
                            Debug.Log("PLAYER HIT: " + col.gameObject.name + "\nDAMAGE DEALT: " + damage);
                            hitboxes.atk1HB.SetActive(false);
                        }
                    }
                    if(gameObject.CompareTag("Atk2")) {
                        if(col.gameObject.GetComponent<PlayerController>().invuln = false)
                        {
                            damage = 0;
                            col.gameObject.GetComponent<PlayerController>().hp -= (int)damage;
                            Debug.Log("PLAYER HIT: " + col.gameObject.name + "\nDAMAGE DEALT: " + damage);
                            hitboxes.atk2HB.SetActive(false);
                        } else {
                            damage = hitboxes.atkDmg;
                            col.gameObject.GetComponent<PlayerController>().hp -= (int)damage;
                            Debug.Log("PLAYER HIT: " + col.gameObject.name + "\nDAMAGE DEALT: " + damage);
                            hitboxes.atk2HB.SetActive(false);
                        }
                    }
                    if(gameObject.CompareTag("Atk3")) {
                        if(col.gameObject.GetComponent<PlayerController>().invuln = false)
                        {
                            damage = 0;
                            col.gameObject.GetComponent<PlayerController>().hp -= (int)damage;
                            Debug.Log("PLAYER HIT: " + col.gameObject.name + "\nDAMAGE DEALT: " + damage);
                            hitboxes.atk3HB.SetActive(false);
                        } else {
                            damage = hitboxes.atkDmg;
                            col.gameObject.GetComponent<PlayerController>().hp -= (int)damage;
                            Debug.Log("PLAYER HIT: " + col.gameObject.name + "\nDAMAGE DEALT: " + damage);
                            hitboxes.atk3HB.SetActive(false);
                        }
                    }
                    if(gameObject.CompareTag("Atk4")) {
                        if(col.gameObject.GetComponent<PlayerController>().invuln = false)
                        {
                            damage = 0;
                            col.gameObject.GetComponent<PlayerController>().hp -= (int)damage;
                            Debug.Log("PLAYER HIT: " + col.gameObject.name + "\nDAMAGE DEALT: " + damage);
                            hitboxes.atk4HB.SetActive(false);
                        } else {
                            damage = hitboxes.atkDmg;
                            col.gameObject.GetComponent<PlayerController>().hp -= (int)damage;
                            Debug.Log("PLAYER HIT: " + col.gameObject.name + "\nDAMAGE DEALT: " + damage);
                            hitboxes.atk4HB.SetActive(false);
                        }
                    }
                }
            }
        }
    }
}
