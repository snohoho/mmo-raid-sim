using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NETWORK_ENGINE;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]

public class NavMeshController : NetworkComponent
{
    public int HP = 10;
    public int Gold = 5;
    public int XP = 10;
    public int Atk = 5;
    public int randx = 0;
    public int randz = 0;
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
            MyAgent.SetDestination(Goal);
        }

        while(IsServer)
        {
            if(MyAgent.remainingDistance<.1f)
            {
                randx = UnityEngine.Random.Range(-47,47);
                randz = UnityEngine.Random.Range(-47,47);
                Goal = new Vector3(randx, Goal.y, randz);
                MyAgent.SetDestination(Goal);
            }
            yield return new WaitForSeconds(.1f);
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