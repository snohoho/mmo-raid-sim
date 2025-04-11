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
    public int time;
    public int count;

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

            Debug.Log("starting game");
            SendUpdate("GAMESTART", "1");
            MyCore.NotifyGameStart();

            while(count < 60)
            {
                count++;
                if(count % 6 == 0)
                {
                    MyCore.NetCreateObject(UnityEngine.Random.Range(30,34), -1, GameObject.Find("EnemySpawn1").transform.position);
                    MyCore.NetCreateObject(UnityEngine.Random.Range(30,34), -1, GameObject.Find("EnemySpawn2").transform.position);
                    MyCore.NetCreateObject(UnityEngine.Random.Range(30,34), -1, GameObject.Find("EnemySpawn3").transform.position);
                    MyCore.NetCreateObject(UnityEngine.Random.Range(30,34), -1, GameObject.Find("EnemySpawn4").transform.position);
                }
                yield return new WaitForSeconds(1f);
            }

            Debug.Log("grind phase start");
            while(!grindPhaseFinished) {
                
                yield return new WaitForSeconds(60f);
            }

            if(grindPhaseFinished)
            {
                MyCore.NetCreateObject(29, -1, new Vector3(0, 0, 0));
            }

            while(!gameFinished) {
                Debug.Log("running game");

                yield return new WaitForSeconds(5f);
            }



            gameFinished = true;
            Debug.Log("finishing game");
            SendUpdate("GAMEFINISH", "true");

            yield return new WaitForSeconds(1f);
            
            Debug.Log("game finished");
            //StartCoroutine(MyCore.DisconnectServer());
        }

        yield return new WaitForSeconds(MyCore.MasterTimer);
    }

    void Start()
    {
        
    }

    void Update()
    {
        
    }
}
