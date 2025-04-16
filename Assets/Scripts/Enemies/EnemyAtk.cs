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
                PlayerController player = col.gameObject.GetComponent<PlayerController>();
                if(hitbox.count == 5)
                {
                    if(player.invuln)
                    {
                        Debug.Log(col.gameObject.name + " INVULN");
                        hitbox.count = 0;
                    } else {
                        player.hp -= 1;
                        if(player.hp <= 0) {
                            player.isDead = true;
                            player.invuln = true;
                        }
                        if(player.hp > 0) {
                            player.StartCoroutine(player.InvulnTimer(1f));
                        }
                        Debug.Log("PLAYER HIT" + "\nHP = " + player.hp);
                        player.SendUpdate("HURT",true.ToString());
                        hitbox.count = 0;
                    }
                }
            }
        }
    }
}
