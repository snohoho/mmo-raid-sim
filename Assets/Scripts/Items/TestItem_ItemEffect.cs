using System.Collections;
using System.Collections.Generic;
using System.Threading;
using NETWORK_ENGINE;
using UnityEngine;

public class TestItem_ItemEffect : NetworkComponent
{
    Object[] playerClassScripts = new Object[4];

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
        StartCoroutine(PopulateClassScripts());
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

    public IEnumerator PopulateClassScripts() {
        NetworkPlayerManager[] npms = FindObjectsOfType<NetworkPlayerManager>();
        foreach(NetworkPlayerManager n in npms) {
            if(n.Owner == Owner) {
                switch(n.playerClass) {
                    case 0:
                        playerClassScripts[Owner] = gameObject.GetComponent<QueenClass>();
                        break;
                    case 1:
                        playerClassScripts[Owner] = gameObject.GetComponent<FisterClass>();
                        break;
                    case 2:
                        playerClassScripts[Owner] = gameObject.GetComponent<SoundDemonClass>();
                        break;
                    case 3:
                        playerClassScripts[Owner] = gameObject.GetComponent<EmotionJewelClass>();
                        break;
                }
            }

            yield return new WaitForEndOfFrame();
        }
    }
}
