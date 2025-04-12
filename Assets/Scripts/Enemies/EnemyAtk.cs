using System.Collections;
using System.Collections.Generic;
using System.Net;
using NETWORK_ENGINE;
using UnityEngine;

public class EnemyAtk : NetworkComponent
{
    public NavMeshController hitbox;
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

    void OnTriggerStay(Collider col)
    {
        if(col.gameObject.CompareTag("Player"))
        {
            if(IsServer)
            {
                if(hitbox.count == 4)
                {
                    if(col.gameObject.GetComponent<PlayerController>().invuln = false)
                        {
                            Debug.Log(col.gameObject.name + " INVULN");
                            hitbox.atkHB.SetActive(false);
                        } else {
                            col.gameObject.GetComponent<PlayerController>().hp -= 1;
                            Debug.Log("PLAYER HIT");
                            SendUpdate("HURT",col.gameObject.GetComponent<PlayerController>().hp.ToString());
                            hitbox.atkHB.SetActive(false);
                        }
                }
            }
        }
    }
}
