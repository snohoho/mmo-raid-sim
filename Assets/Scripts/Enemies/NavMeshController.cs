using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using NETWORK_ENGINE;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]

public class NavMeshController : NetworkComponent
{
    public int HP = 1500;
    public int Gold = 5;
    public int XP = 10;
    public int randx = 0;
    public int randz = 0;
    public int count;
    public GameObject atkHB;
    public Vector3 Goal;
    NavMeshAgent MyAgent;

    public override void HandleMessage(string flag, string value)
    {

    }

    public override void NetworkedStart()
    {
        
    }

    public override IEnumerator SlowUpdate()
    {
        if(IsClient)
        {

        }

        if(IsServer)
        {
            randx = UnityEngine.Random.Range(-47,47);
            randz = UnityEngine.Random.Range(-47,47);
            Goal = new Vector3(randx, Goal.y, randz);
            MyAgent.SetDestination(Goal);

            if(HP<=0) {
                MyCore.NetDestroyObject(NetId);
            }
        }

        while(IsServer)
        {
            yield return new WaitForSeconds(.1f);
            if(MyAgent.remainingDistance<.1f)
            {
                atkHB.SetActive(true);
                count++;
                if(count == 4)
                {
                    randx = UnityEngine.Random.Range(-47,47);
                    randz = UnityEngine.Random.Range(-47,47);
                    Goal = new Vector3(randx, Goal.y, randz);
                    yield return new WaitForSeconds(.1f);
                    count = 0;
                    MyAgent.SetDestination(Goal);
                }
                yield return new WaitForSeconds(1f);
            }
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        MyAgent = this.GetComponent<NavMeshAgent>();
        if(MyAgent == null)
        {
            throw new System.Exception("ERROR: COULD NOT FIND NAV MESH AGENT!");
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}