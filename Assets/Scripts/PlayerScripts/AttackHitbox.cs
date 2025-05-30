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
                enemy.SendUpdate("HP", enemy.HP.ToString());
                Debug.Log("ENEMY HIT: " + col.gameObject.name + "\nDAMAGE DEALT: " + (int)damage);
                controller.dmgBonus = controller.dmgBonusBase;
                controller.SendUpdate("DAMAGE", ((int)damage).ToString());

                if(enemy.HP <= 0) {
                    Debug.Log("award exp and gold");
                    MyCore.NetDestroyObject(enemy.NetId);
                    controller.StartCoroutine(controller.DistributeGoldExp(enemy.Gold,enemy.XP));
                }
            }       
        }
        if(col.gameObject.CompareTag("Fubuzilla")) {
            if(IsServer) {
                BossHitboxes enemy = col.gameObject.GetComponent<BossHitboxes>();
                float damage = 0f;
                if(gameObject.CompareTag("Melee")) {
                    damage = (controller.meleeAtk + controller.skillDmg) * controller.dmgBonus; 
                }
                else if(gameObject.CompareTag("Ranged")) {
                    damage = (controller.rangedAtk + controller.skillDmg) * controller.dmgBonus; 
                }
                
                
                enemy.hp -= (int)damage;
                enemy.SendUpdate("HP", enemy.hp.ToString());
                controller.totalDamage += (int)damage;
                Debug.Log("ENEMY HIT: " + col.gameObject.name + "\nDAMAGE DEALT: " + damage);
                controller.dmgBonus = controller.dmgBonusBase;
                controller.SendUpdate("DAMAGE", ((int)damage).ToString());

                if(enemy.hp <= 0) {
                    Debug.Log("FUBUZILLA KILLED !!");
                    GameMaster gameMaster = FindAnyObjectByType<GameMaster>();
                    gameMaster.gameFinished = true;
                }
            }
        }
    }
}
