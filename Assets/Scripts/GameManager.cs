using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NETWORK_ENGINE;
using UnityEditor;
using UnityEngine.UI;
using Unity.VisualScripting;

public class GameManager : NetworkComponent
{
    public bool gameStarted;
    public bool gameFinished;

    public override void HandleMessage(string flag, string value) 
    {
        if(flag == "GAMESTART" && IsClient) {
            gameStarted = true;
            
            NetworkPlayerManager[] npm = FindObjectsOfType<NetworkPlayerManager>();
            foreach(NetworkPlayerManager n in npm) {
                n.transform.GetChild(0).gameObject.SetActive(false);
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
                MyCore.NetCreateObject(0, n.Owner, GameObject.Find("spawn" + n.Owner).transform.position);
            }

            Debug.Log("starting game");
            SendUpdate("GAMESTART", "1");
            MyCore.NotifyGameStart();

            while(!gameFinished && gameStarted) {
                Debug.Log("running game");

                yield return new WaitForSeconds(10f);

                gameFinished = true;
            }

            Debug.Log("finishing game");
            SendUpdate("GAMEFINISH", "1");

            yield return new WaitForSeconds(1f);
            
            Debug.Log("game finished");
            StartCoroutine(MyCore.DisconnectServer());
        }

        yield return new WaitForSeconds(.1f);
    }

    void Start()
    {
        
    }

    void Update()
    {
        
    }
}
