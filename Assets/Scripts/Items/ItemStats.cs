using System.Collections;
using System.Collections.Generic;
using System.Threading;
using NETWORK_ENGINE;
using UnityEngine;

public class ItemStats : NetworkComponent
{
    public int itemID;
    public int itemRarity;
    public string itemName;
    public string itemDescription;
    public Sprite itemSprite;
    
    public int hpBonus;
    public int meleeBonus;
    public int rangedBonus;
    public int spdBonus;
    public float dmgBonus;
    public float gcdMod;

    public override void HandleMessage(string flag, string value)
    {
        if(flag == "EFFECT") {
            if(IsClient) {
                //handle client side effects for ui or particles
            }
        }
    }

    public override void NetworkedStart()
    {

    }

    public override IEnumerator SlowUpdate()
    {
        if(IsServer) {
            //perform item effects
        }

        yield return new WaitForSeconds(MyCore.MasterTimer);
    }

    void Update()
    {

    }
}
