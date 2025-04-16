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
    public int Gold;
    public int XP = 20;
    public int randx;
    public int randz;
    public int count;
    public GameObject atkHB;
    public Vector3 Goal;
    public Transform CurrentPos;
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
            randx = UnityEngine.Random.Range(-47,48);
            randz = UnityEngine.Random.Range(-47,48);
            
        }

        while(IsServer)
        {
            yield return new WaitForSeconds(.1f);
            CurrentPos = this.gameObject.GetComponent<Transform>();
            PlayerController[] pc;
            pc = FindObjectsOfType<PlayerController>();
            foreach(PlayerController p in pc) {
                float distanceToPlayer = Vector3.Distance(CurrentPos.position, p.gameObject.transform.position);
                if(distanceToPlayer < 6f)
                {
                    Goal = p.gameObject.transform.position;
                    MyAgent.SetDestination(Goal);
                } else {
                    Goal = new Vector3(randx, 0, randz);
                    MyAgent.SetDestination(Goal);
                }
            }
            if(MyAgent.remainingDistance<3f)
            {
                Goal = CurrentPos.position;
                MyAgent.SetDestination(Goal);
                atkHB.SetActive(true);
                ++count;
                if(count == 5)
                {
                    randx = UnityEngine.Random.Range(-47,48);
                    randz = UnityEngine.Random.Range(-47,48);
                    Goal = new Vector3(randx, 0, randz);
                    yield return new WaitForSeconds(.1f);
                    count = 0;
                    MyAgent.SetDestination(Goal);
                }
                if(count == 0)
                {
                    atkHB.SetActive(false);
                }
                yield return new WaitForSeconds(1f);
            }
            if(HP<=0) {
                MyCore.NetDestroyObject(NetId);
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
        Gold = UnityEngine.Random.Range(75,151);
    }

    // Update is called once per frame
    void Update()
    {
        if(IsClient)
        {
            if(MyAgent.remainingDistance<3f)
            {
                atkHB.SetActive(true);
            }
            
            if(count == 0)
            {
                atkHB.SetActive(false);
            }
        }
    }
}