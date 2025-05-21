using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using NETWORK_ENGINE;
using UnityEngine.AI;
using UnityEngine.UI;

[RequireComponent(typeof(NavMeshAgent))]

public class NavMeshController : NetworkComponent
{
    public int HP = 1500;
    public int Gold;
    public int XP = 20;
    public int randx;
    public int randz;
    public int count;
    public bool isArrived;
    public MeshRenderer renderer;
    public Material[] MColor;
    public GameObject atkHB;
    public GameObject hb;
    public Vector3 Goal;
    public Transform CurrentPos;
    NavMeshAgent MyAgent;
    public Slider hpBar;

    public Animator animator;

    public static Vector3 VectorFromString(string value) //parse vector3 from given string value
    {
        char[] temp = { '(', ')' };
        string[] args = value.Trim(temp).Split(',');
        return new Vector3(float.Parse(args[0].Trim()), float.Parse(args[1].Trim()), float.Parse(args[2].Trim()));
    }

    public override void HandleMessage(string flag, string value)
    {
        if(flag == "HP") {  //client-side synchronization
            if(IsClient) {
                HP = int.Parse(value);
            }
        }

        if(flag == "ARRIVED")
        {
            if(IsClient)
            {
                isArrived = bool.Parse(value);
            }
        }

        if(flag == "COUNT")
        {
            if(IsClient)
            {
                count = int.Parse(value);
            }
        }

        if(flag == "GOAL")
        {
            if(IsClient)
            {
                Goal = NavMeshController.VectorFromString(value);
            }
        }

        if(flag == "DOINGATTACK")
        {
            StartCoroutine(SetAnimatorBool(animator, "DoingAttack"));
        }
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
            randx = UnityEngine.Random.Range(-47,48);  //set initial  x and z coordinates for navmesh
            randz = UnityEngine.Random.Range(-47,48);
        }

        while(IsServer)
        {
            yield return new WaitForSeconds(.1f);
            CurrentPos = this.gameObject.GetComponent<Transform>();
            PlayerController[] pc;
            pc = FindObjectsOfType<PlayerController>();
            foreach(PlayerController p in pc) {  //for every player on the field
                float distanceToPlayer = Vector3.Distance(CurrentPos.position, p.gameObject.transform.position);  //check navmesh's distance to each player
                if(distanceToPlayer < 6f)  //if close enough to a player
                {
                    Goal = p.gameObject.transform.position;  //begin chasing player
                    MyAgent.SetDestination(Goal);
                    SendUpdate("GOAL", Goal.ToString("F2"));
                } else {
                    Goal = new Vector3(randx, 0, randz);
                    MyAgent.SetDestination(Goal);
                    SendUpdate("GOAL", Goal.ToString("F2"));
                }
            }
            if(MyAgent.remainingDistance<3f) //once enemy arrives at random destination or gets close enough to player, turn hitbox on and prepare to attack
            {
                atkHB.SetActive(true);
                hb.SetActive(true);
                renderer.material = MColor[0];  //hitbox is yellow to indicate enemy is about to attack
                SendUpdate("ARRIVED", true.ToString());
                Goal = CurrentPos.position;
                MyAgent.SetDestination(Goal);
                SendUpdate("GOAL", Goal.ToString("F2"));
                ++count;
                SendUpdate("COUNT", count.ToString());
                if(count == 2)  //after two seconds, attack
                {
                    renderer.material = MColor[1];  //hitbox changes to red to indicate enemy is attacking
                    SendUpdate("DOINGATTACK", "0");
                    SendUpdate("ATTACKING", count.ToString());
                    randx = UnityEngine.Random.Range(-47,48);  //when done with attack, pick new random point on map for next destination
                    randz = UnityEngine.Random.Range(-47,48);
                    Goal = new Vector3(randx, 0, randz);
                    yield return new WaitForSeconds(.1f);
                    MyAgent.SetDestination(Goal);
                    SendUpdate("GOAL", Goal.ToString("F2"));
                    count = 0;   //set timer back to zero
                    SendUpdate("COUNT", count.ToString());
                }
                if(count == 0)
                {
                    renderer.material = MColor[0];  //set hitbox back to yellow color and turn them off
                    SendUpdate("ARRIVED", false.ToString());
                    hb.SetActive(false);
                    atkHB.SetActive(false);
                }
                yield return new WaitForSeconds(1f);  //count variable increments every second to simulate a timer
            } else {
                renderer.material = MColor[0];  //only begin counting and preparing to attack when at destination
                SendUpdate("ARRIVED", false.ToString());
                hb.SetActive(false);
                atkHB.SetActive(false);
                count = 0;
                SendUpdate("COUNT", count.ToString());
            }
            
            if(HP<=0) {
                MyCore.NetDestroyObject(NetId);
            }

        }
    }

    public IEnumerator SetAnimatorBool(Animator anim, string boolToSet)
    {
        anim.SetBool(boolToSet, true);
        yield return new WaitForEndOfFrame();
        anim.SetBool(boolToSet, false);
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
        if(IsClient)  //make sure each client on the server sees the same thing
        {
            hpBar.value = HP;
            MyAgent.SetDestination(Goal);
            if(isArrived)
            {
                hb.SetActive(true);
            }
            if(count < 2)
            {
                renderer.material = MColor[0];
            }
            if(count == 2)
            {
                renderer.material = MColor[1];
            }
            if(!isArrived)
            {
                hb.SetActive(false);
            }
        }
    }
}