using System.Collections;
using System.Collections.Generic;
using System.Net;
using NETWORK_ENGINE;
using UnityEngine;
using UnityEngine.UI;

public class BossHitboxes : NetworkComponent
{
    public int randAtk;
    public int hp = 3000000;
    public int count;
    public Material[] MColor;
    public GameObject atk1HB;  //set hitbox gameobjects for turning on and off
    public GameObject atk2HB;
    public GameObject atk3HB;
    public GameObject atk4HB;

    public GameObject hb1;   //set gameobjects for visualization of hitboxes
    public GameObject hb2;
    public GameObject hb31;
    public GameObject hb32;
    public GameObject hb33;
    public GameObject hb34;
    public GameObject hb41;
    public GameObject hb42;

    public MeshRenderer renderer1;  //set renderers for changing color of hitboxes
    public MeshRenderer renderer2;
    public MeshRenderer renderer31;
    public MeshRenderer renderer32;
    public MeshRenderer renderer33;
    public MeshRenderer renderer34;
    public MeshRenderer renderer41;
    public MeshRenderer renderer42;

    public Animator animator;
    public Slider hpBar;
    public bool dead;
    public GameObject particles;

    public override void HandleMessage(string flag, string value)
    {
        if(flag == "HP") {     //client-side synching of values
            if(IsClient) {
                hp = int.Parse(value);
            }
        }

        if(flag == "ATK")
        {
            if(IsClient)
            {
                randAtk = int.Parse(value);
            }
        }

        if(flag == "COUNT")
        {
            if(IsClient)
            {
                count = int.Parse(value);
            }
        }

        if(flag == "ATTACK1")
        {
            if (IsClient)
            {
                StartCoroutine(SetAnimatorBool(animator, "Attack3"));
                particles.transform.GetChild(0).GetComponent<ParticleSystem>().Play();
            }
        }

        if (flag == "ATTACK2")
        {
            if (IsClient)
            {
                StartCoroutine(SetAnimatorBool(animator, "Attack2"));
                particles.transform.GetChild(1).GetComponent<ParticleSystem>().Play();
            }
        }

        if (flag == "ATTACK3")
        {
            if (IsClient)
            {
                StartCoroutine(SetAnimatorBool(animator, "Attack4"));
                particles.transform.GetChild(2).GetComponent<ParticleSystem>().Play();
            }
        }

        if (flag == "ATTACK4")
        {
            if (IsClient)
            {
                StartCoroutine(SetAnimatorBool(animator, "Attack1"));
                particles.transform.GetChild(3).GetComponent<ParticleSystem>().Play();
            }
        }
        if (flag == "DEAD")
        {
            if (IsClient)
            {
                dead = true;
                StartCoroutine(SetAnimatorBool(animator, "Dead"));
                particles.transform.GetChild(4).GetComponent<ParticleSystem>().Play();
            }
        }
    }

    public override void NetworkedStart()
    {
        
    }

    public override IEnumerator SlowUpdate()
    {
        while(IsServer)
        {
            if(hp <= 0) {
                dead = true;
                SendUpdate("DEAD", "true");
            }
            
            count++;
            if(count == 2 && !dead)
            {
                randAtk = UnityEngine.Random.Range(1,5); //set random attack after two seconds
                if(randAtk == 1)
                {
                    atk1HB.SetActive(true);
                    hb1.SetActive(true);
                    renderer1.material = MColor[0];  //yellow color to warn of incoming attack
                }
                if(randAtk == 2)
                {
                    atk2HB.SetActive(true);
                    hb2.SetActive(true);
                    renderer2.material = MColor[0];
                }
                if(randAtk == 3)
                {
                    atk3HB.SetActive(true);
                    hb31.SetActive(true);
                    hb32.SetActive(true);
                    hb33.SetActive(true);
                    hb34.SetActive(true);
                    renderer31.material = MColor[0];
                    renderer32.material = MColor[0];
                    renderer33.material = MColor[0];
                    renderer34.material = MColor[0];
                }
                if(randAtk == 4)
                {
                    atk4HB.SetActive(true);
                    hb41.SetActive(true);
                    hb42.SetActive(true);
                    renderer41.material = MColor[0];
                    renderer42.material = MColor[0];
                }
                SendUpdate("ATK", randAtk.ToString());
                SendUpdate("COUNT", count.ToString());
            }
            if(count == 5) //after 5 seconds, attack goes off
            {
                if(randAtk == 1)
                {
                    renderer1.material = MColor[1];  //red to indicate boss is attacking
                    SendUpdate("ATTACK1", "0");
                }
                if(randAtk == 2)
                {
                    renderer2.material = MColor[1];
                    SendUpdate("ATTACK2", "0");
                }
                if(randAtk == 3)
                {
                    renderer31.material = MColor[1];
                    renderer32.material = MColor[1];
                    renderer33.material = MColor[1];
                    renderer34.material = MColor[1];
                    SendUpdate("ATTACK3", "0");
                }
                if(randAtk == 4)
                {
                    renderer41.material = MColor[1];
                    renderer42.material = MColor[1];
                    SendUpdate("ATTACK4", "0");
                }
                SendUpdate("COUNT", count.ToString());
                yield return new WaitForSeconds(.1f);
                count = 0;  //set timer back to zero
            }
            if(count == 0)
            {
                hb1.SetActive(false);  //turn of active hitboxes and set their colors back to yellow
                hb2.SetActive(false);
                hb31.SetActive(false);
                hb32.SetActive(false);
                hb33.SetActive(false);
                hb34.SetActive(false);
                hb41.SetActive(false);
                hb42.SetActive(false);
                renderer1.material = MColor[0];
                renderer2.material = MColor[0];
                renderer31.material = MColor[0];
                renderer32.material = MColor[0];
                renderer33.material = MColor[0];
                renderer34.material = MColor[0];
                renderer41.material = MColor[0];
                renderer42.material = MColor[0];
                atk1HB.SetActive(false);
                atk2HB.SetActive(false);
                atk3HB.SetActive(false);
                atk4HB.SetActive(false);
                SendUpdate("COUNT", count.ToString());
            }
            yield return new WaitForSeconds(1f); //count variable increments every second to simulate timer
        }
    }

    public IEnumerator SetAnimatorBool(Animator anim, string boolToSet)
    {
        anim.SetBool(boolToSet, true);
        yield return new WaitForSeconds(.1f);
        anim.SetBool(boolToSet, false);
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(IsClient) //ensure all clients on the server see the same thing
        {
            hpBar.value = hp;
            if(count == 2 && !dead)
            {
                if(randAtk == 1)
                {
                    hb1.SetActive(true);
                }
                if(randAtk == 2)
                {
                    hb2.SetActive(true);
                }
                if(randAtk == 3)
                {
                    hb31.SetActive(true);
                    hb32.SetActive(true);
                    hb33.SetActive(true);
                    hb34.SetActive(true);
                }
                if(randAtk == 4)
                {
                    hb41.SetActive(true);
                    hb42.SetActive(true);
                }
            }

            if(count == 5)
            {
                if(randAtk == 1)
                {
                    renderer1.material = MColor[1];
                }
                if(randAtk == 2)
                {
                    renderer2.material = MColor[1];
                }
                if(randAtk == 3)
                {
                    renderer31.material = MColor[1];
                    renderer32.material = MColor[1];
                    renderer33.material = MColor[1];
                    renderer34.material = MColor[1];
                }
                if(randAtk == 4)
                {
                    renderer41.material = MColor[1];
                    renderer42.material = MColor[1];
                }
            }

            if(count == 0)
            {
                hb1.SetActive(false);
                hb2.SetActive(false);
                hb31.SetActive(false);
                hb32.SetActive(false);
                hb33.SetActive(false);
                hb34.SetActive(false);
                hb41.SetActive(false);
                hb42.SetActive(false);
                renderer1.material = MColor[0];
                renderer2.material = MColor[0];
                renderer31.material = MColor[0];
                renderer32.material = MColor[0];
                renderer33.material = MColor[0];
                renderer34.material = MColor[0];
                renderer41.material = MColor[0];
                renderer42.material = MColor[0];
                atk1HB.SetActive(false);
                atk2HB.SetActive(false);
                atk3HB.SetActive(false);
                atk4HB.SetActive(false);
            }
        }
    }
}
