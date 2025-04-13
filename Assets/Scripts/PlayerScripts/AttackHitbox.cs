using System.Collections;
using System.Collections.Generic;
using System.Net;
using NETWORK_ENGINE;
using UnityEngine;

public class AttackHitbox : NetworkComponent
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
        if(col.gameObject.CompareTag("Enemy")) {
            if(IsServer) {
                NavMeshController enemy = col.gameObject.GetComponent<NavMeshController>();
                float damage = 0f;
                if(gameObject.CompareTag("Melee")) {
                    damage = (controller.meleeAtk + controller.skillDmg) * controller.dmgBonus; 
                }
                else if(gameObject.CompareTag("Ranged")) {
                    damage = (controller.rangedAtk + controller.skillDmg) * controller.dmgBonus; 
                }
                
                enemy.HP -= (int)damage;
                controller.totalDamage += (int)damage;
                Debug.Log("ENEMY HIT: " + col.gameObject.name + "\nDAMAGE DEALT: " + damage);
                controller.dmgBonus = 1;

                if(enemy.HP <= 0) {
                    Debug.Log("award exp and gold");
                    controller.exp += enemy.XP;
                    controller.gold += enemy.Gold;

                    controller.SendUpdate("EXP",enemy.XP.ToString());
                    controller.SendUpdate("GOLD",enemy.Gold.ToString());
                    controller.SendUpdate("DAMAGE", controller.totalDamage.ToString());
                }
            }       
        }
    }
}
