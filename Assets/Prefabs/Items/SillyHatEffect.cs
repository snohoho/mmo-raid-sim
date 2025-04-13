using System.Collections;
using System.Collections.Generic;
using System.Threading;
using NETWORK_ENGINE;
using UnityEngine;

public class SillyHatEffect : ItemStats
{
    bool HasLollipop = false;
    bool StatsSet2 = false;
    public override void HandleMessage(string flag, string value)
    {
        if (flag == "EFFECT")
        {
            if (IsClient)
            {
                //handle client side effects for ui or particles
            }
        }
    }

    public override void NetworkedStart()
    {

    }

    public override IEnumerator SlowUpdate()
    {
        if (IsServer)
        {
            //perform item effects

            while (!StatsSet2)
            {
                ItemStats[] inv = GetComponent<PlayerInventory>().inventory;
                foreach (ItemStats item in inv)
                {
                    if (item.name == "Lollipop")
                    {
                        HasLollipop = true;
                    }

                    if (HasLollipop)
                    {
                        ItemStats stats = GetComponent<ItemStats>();
                        stats.rangedBonus += 130;
                        stats.meleeBonus += 130;
                        StatsSet2 = true;
                    }
                }
            }
        }

        yield return new WaitForSeconds(MyCore.MasterTimer);
    }
}