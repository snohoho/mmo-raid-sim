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
        if(col.gameObject.CompareTag("Player"))  //if player stays in hitbox
        {
            if(IsServer)
            {
                PlayerController player = col.gameObject.GetComponent<PlayerController>();
                if(hitbox.count == 2)  //if two seconds have passed
                {
                    if(player.invuln)  //player is invulnerable and takes no damage
                    {
                        Debug.Log(col.gameObject.name + " INVULN");
                        hitbox.count = 0;
                    } else {  //player is not invulnerable and takes damage
                        player.hp -= 1;
                        if(player.hp <= 0) {
                            player.isDead = true;
                            player.invuln = true;
                        }
                        if(player.hp > 0) {
                            player.StartCoroutine(player.InvulnTimer(1f)); //player is invulnerable for a second to prevent them from taking multiple instances of damage
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
