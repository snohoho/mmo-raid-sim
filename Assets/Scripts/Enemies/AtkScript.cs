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
        if(col.gameObject.CompareTag("Player")) { //if player stays inside hitbox
            if(IsServer) {
                PlayerController player = col.gameObject.GetComponent<PlayerController>();
                if(hitboxes.count == 5){  //5 second timer
                    if(gameObject.CompareTag("Atk1")) {   //perform the following code for each of the boss' attacks
                        if(player.invuln)  //player is invulnerable and takes no damage
                        {
                            Debug.Log(col.gameObject.name + " INVULN");
                            hitboxes.count = 0;
                        } else {  //player is not invulnerable and takes damage
                            player.hp -= 1;
                            Debug.Log("PLAYER HIT" + "\nHP = " + player.hp);
                            player.SendUpdate("HURT",true.ToString());
                            hitboxes.count = 0;
                        }
                    }
                    if(gameObject.CompareTag("Atk2")) {  //boss attack 2
                        if(player.invuln)
                        {
                            Debug.Log(col.gameObject.name + " INVULN");
                            hitboxes.count = 0;
                        } else {
                            player.hp -= 1;
                            Debug.Log("PLAYER HIT" + "\nHP = " + player.hp);
                            player.SendUpdate("HURT",true.ToString());
                            hitboxes.count = 0;
                        }
                    }
                    if(gameObject.CompareTag("Atk3")) {  //boss attack 3
                        if(player.invuln)
                        {
                            Debug.Log(col.gameObject.name + " INVULN");
                            hitboxes.count = 0;
                        } else {
                            player.hp -= 1;
                            Debug.Log("PLAYER HIT" + "\nHP = " + player.hp);
                            player.SendUpdate("HURT",true.ToString());
                            hitboxes.count = 0;
                        }
                    }
                    if(gameObject.CompareTag("Atk4")) {  //boss attack 4
                        if(player.invuln)
                        {
                            Debug.Log(col.gameObject.name + " INVULN");
                            hitboxes.count = 0;
                        } else {
                            player.hp -= 1;
                            Debug.Log("PLAYER HIT" + "\nHP = " + player.hp);
                            player.SendUpdate("HURT",true.ToString());
                            hitboxes.count = 0;
                        }
                    }
                }
            }
        }
    }
}
