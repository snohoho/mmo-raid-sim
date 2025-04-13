using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NETWORK_ENGINE;
using UnityEditor;
using UnityEngine.UI;

public class GameMaster : NetworkComponent
{
    public bool gameStarted;
    public bool grindPhaseFinished;
    public bool gameFinished;
    public float time;
    public int totalDamage;

    public override void HandleMessage(string flag, string value) 
    {
        if(flag == "GAMESTART") {
            if(IsClient) {
                Cursor.lockState = CursorLockMode.Locked;
                NetworkPlayerManager[] npm = FindObjectsOfType<NetworkPlayerManager>();
                foreach(NetworkPlayerManager n in npm) {
                    n.transform.GetChild(0).gameObject.SetActive(false);
                }
            }
        }
        if(flag == "GAMEFINISH") {
            if(IsClient) {
                //disable movement
            }
        }
        if(flag == "TIMER") {
            if(IsClient) {
                time = int.Parse(value);
            }
        }
    }

    public override void NetworkedStart()
    {

    }

    public override IEnumerator SlowUpdate()
    {
        if(IsServer) {
            NetworkPlayerManager[] npm;
            bool tempStart;

            //waiting for players to join lobby
            Debug.Log("waitin for players");
            do {
                npm = FindObjectsOfType<NetworkPlayerManager>();
                tempStart = true;

                foreach(NetworkPlayerManager n in npm) {
                    if(!n.ready) {
                        tempStart = false;
                    }
                }

                yield return new WaitForSeconds(.1f);
            } while(!tempStart || npm.Length < 1);

            npm = FindObjectsOfType<NetworkPlayerManager>();
            foreach(NetworkPlayerManager n in npm) {
                //create objects
                MyCore.NetCreateObject(n.playerClass, n.Owner, GameObject.Find("spawn" + n.Owner).transform.position);
            }
            MyCore.NetCreateObject(34, -1, new Vector3(6,0,5));

            Debug.Log("starting game");
            SendUpdate("GAMESTART", "1");
            MyCore.NotifyGameStart();
            Debug.Log("grind phase start");
            
            while(!grindPhaseFinished) {
                time = 0f;
                SendUpdate("TIMER", time.ToString());
                StartCoroutine(SpawnEnemies());
                
                yield return new WaitUntil(() => time >= 180f);

                grindPhaseFinished = true;
            }

            if(grindPhaseFinished)
            {
                MyCore.NetCreateObject(29, -1, new Vector3(0, 0, 0));
            }

            while(!gameFinished) {
                Debug.Log("boss phase start");

                yield return new WaitForSeconds(5f);
                gameFinished = true;
            }

            Debug.Log("finishing game");
            SendUpdate("GAMEFINISH", "true");

            yield return new WaitForSeconds(1f);
            
            Debug.Log("game finished");
            //StartCoroutine(MyCore.DisconnectServer());
        }

        yield return new WaitForSeconds(MyCore.MasterTimer);
    }

    public IEnumerator SpawnEnemies() 
    {
        while(!grindPhaseFinished) {
            MyCore.NetCreateObject(UnityEngine.Random.Range(30,34), -1, GameObject.Find("EnemySpawn1").transform.position);
            MyCore.NetCreateObject(UnityEngine.Random.Range(30,34), -1, GameObject.Find("EnemySpawn2").transform.position);
            MyCore.NetCreateObject(UnityEngine.Random.Range(30,34), -1, GameObject.Find("EnemySpawn3").transform.position);
            MyCore.NetCreateObject(UnityEngine.Random.Range(30,34), -1, GameObject.Find("EnemySpawn4").transform.position);

            yield return new WaitForSeconds(20f);
        }
    }

    void Start()
    {
        
    }

    void Update()
    {
        if(IsServer) {
            time += Time.deltaTime;
        }
        if(IsClient) {
            time += Time.deltaTime;
        }
    }
}
