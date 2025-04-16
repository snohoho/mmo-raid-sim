using System.Collections;
using System.Collections.Generic;
using System.Net;
using NETWORK_ENGINE;
using UnityEngine;

public class BossHitboxes : NetworkComponent
{
    public int randAtk;
    public int hp = 100000;
    public int count;
    public GameObject atk1HB;
    public GameObject atk2HB;
    public GameObject atk3HB;
    public GameObject atk4HB;
    public override void HandleMessage(string flag, string value)
    {
        
    }

    public override void NetworkedStart()
    {
        
    }

    public override IEnumerator SlowUpdate()
    {
        while(IsServer)
        {
            count++;
            if(count == 5)
            {
                randAtk = UnityEngine.Random.Range(1,5);
                if(randAtk == 1)
                {
                    atk1HB.SetActive(true);
                }
                if(randAtk == 2)
                {
                    atk2HB.SetActive(true);
                }
                if(randAtk == 3)
                {
                    atk3HB.SetActive(true);
                }
                if(randAtk == 4)
                {
                    atk4HB.SetActive(true);
                }
            }
            if(count == 10)
            {
                yield return new WaitForSeconds(.1f);
                count = 0;
            }
            if(count == 0)
            {
                atk1HB.SetActive(false);
                atk2HB.SetActive(false);
                atk3HB.SetActive(false);
                atk4HB.SetActive(false);
            }
            yield return new WaitForSeconds(1f);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
