using System.Collections;
using System.Collections.Generic;
using System.Threading;
using NETWORK_ENGINE;
using UnityEngine;

public class JrenchjoatEffect : ItemStats
{
    bool HasJloves = false;
    bool HasJedora = false;
    bool HasJorts = false;
    bool HasJocks = false;
    bool StatsSet = false;
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

            while (!StatsSet)
            {
                ItemStats[] inv = GetComponent<PlayerInventory>().inventory;
                foreach (ItemStats item in inv)
                {
                    if (item.name == "Jloves")
                    {
                        HasJloves = true;
                    }
                    if (item.name == "Jedora")
                    {
                        HasJedora = true;
                    }
                    if (item.name == "Jorts")
                    {
                        HasJorts = true;
                    }
                    if (item.name == "Jocks")
                    {
                        HasJocks = true;
                    }

                    if (HasJedora && HasJloves && HasJocks && HasJorts)
                    {
                        ItemStats stats = GetComponent<ItemStats>();
                        stats.rangedBonus += 300;
                        stats.meleeBonus += 300;
                        stats.hpBonus += 5;
                        stats.spdBonus += 5;
                        stats.dmgBonus += 0.5f;
                        stats.gcdMod -= 0.25f;
                        StatsSet = true;
                    }
                }
            }
        }
        
        yield return new WaitForSeconds(MyCore.MasterTimer);
    }
    
    void Update()
    {
        
    }
}
