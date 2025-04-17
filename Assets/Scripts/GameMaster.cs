using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NETWORK_ENGINE;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

public class GameMaster : NetworkComponent
{
    public bool gameStarted;
    public bool grindPhaseFinished;
    public bool gameFinished;
    public float time;
    public int totalDamage;
    public RectTransform timerPanel;
    public TextMeshProUGUI timer;
    public RectTransform finalStatsPanel;
    public TextMeshProUGUI finalStats;
    public bool gameLost;

    public override void HandleMessage(string flag, string value) 
    {
        if(flag == "GAMESTART") {
            if(IsClient) {
                Cursor.lockState = CursorLockMode.Locked;
                NetworkPlayerManager[] npm = FindObjectsOfType<NetworkPlayerManager>();
                foreach(NetworkPlayerManager n in npm) {
                    n.transform.GetChild(0).gameObject.SetActive(false);
                }
                timerPanel.gameObject.SetActive(true);
            }
        }
        if(flag == "GAMEFINISH") {
            if(IsClient) {
                gameFinished = bool.Parse(value);

                int timeSpent = (int)(300f-time);
                foreach(PlayerController player in FindObjectsOfType<PlayerController>()) {
                    totalDamage += player.totalDamage;
                }
                string header = "";
                if(!gameLost) {
                    header = "FUBUZILLA HAS BEEN DEFEATED!\n";
                }
                else if(gameLost) {
                    header = "FUBUZILLA HAS DEFEATED YOU..!\n";
                }

                foreach(PlayerController player in FindObjectsOfType<PlayerController>()) {
                    player.transform.GetChild(0).gameObject.SetActive(false);

                    finalStats.text =
                    header +
                    "Time\n" + string.Format("{0:00}:{1:00}", timeSpent/60, timeSpent%60) + "\n" +
                    "Team DPS\n" + Mathf.Round(totalDamage/timeSpent) + "\n" +
                    "Personal DPS\n" + Mathf.Round(player.totalDamage/timeSpent);
                }
                
            }
        }
        if(flag == "GAMELOST") {
            if(IsClient) {
                gameLost = bool.Parse(value);
            }
        }
        if(flag == "TIMER") {
            if(IsClient) {
                time = float.Parse(value);
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

            //spawn shop first
            MyCore.NetCreateObject(34, -1, new Vector3(0,0,0));
            
            //spawn players
            npm = FindObjectsOfType<NetworkPlayerManager>();
            foreach(NetworkPlayerManager n in npm) {
                //create objects
                MyCore.NetCreateObject(n.playerClass, n.Owner, GameObject.Find("spawn" + n.Owner).transform.position);
            }

            Debug.Log("starting game");
            SendUpdate("GAMESTART", "1");
            MyCore.NotifyGameStart();
            
            PlayerController[] players = FindObjectsOfType<PlayerController>();
            while(!grindPhaseFinished) {
                Debug.Log("grind phase start");
                time = 180f;
                SendUpdate("TIMER", time.ToString());
                StartCoroutine(SpawnEnemies());
                StartCoroutine(CheckDeaths(players));
                
                yield return new WaitUntil(() => gameFinished == true || gameLost == true || time <= 0f);

                //destroy all enemies
                foreach(NavMeshController enemy in FindObjectsOfType<NavMeshController>()) {
                    MyCore.NetDestroyObject(enemy.NetId);

                    yield return null;
                }

                //teleport players back to spawn
                foreach(PlayerController player in players) {
                    player.transform.position = GameObject.Find("spawn" + player.Owner).transform.position;
                    player.inShop = false;
                    player.withinInteract = false;
                }

                //spawn boss
                MyCore.NetCreateObject(29, -1, new Vector3(0, 0, 0));
                
                //destroy shop
                MyCore.NetDestroyObject(FindAnyObjectByType<ItemManager>().NetId);
                grindPhaseFinished = true;
            }

            while(!gameFinished) {
                Debug.Log("boss phase start");
                time = 300f;
                SendUpdate("TIMER", time.ToString());

                yield return new WaitUntil(() => gameFinished == true || gameLost == true || time <= 0f);

                if(time <= 0f) {
                    gameLost = true;
                    SendUpdate("GAMELOST", "true");
                }
                gameFinished = true;
            }

            Debug.Log("game finished");
            SendUpdate("GAMEFINISH", "true");

            //wait on final screen for x amount of seconds
            yield return new WaitForSeconds(15f);
            
            //kill server
            StartCoroutine(MyCore.DisconnectServer());
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

            yield return new WaitForSeconds(5f);
        }
        while(!gameFinished && grindPhaseFinished) {
            MyCore.NetCreateObject(UnityEngine.Random.Range(30,34), -1, GameObject.Find("EnemySpawn1").transform.position);
            MyCore.NetCreateObject(UnityEngine.Random.Range(30,34), -1, GameObject.Find("EnemySpawn2").transform.position);
            MyCore.NetCreateObject(UnityEngine.Random.Range(30,34), -1, GameObject.Find("EnemySpawn3").transform.position);
            MyCore.NetCreateObject(UnityEngine.Random.Range(30,34), -1, GameObject.Find("EnemySpawn4").transform.position);

            yield return new WaitForSeconds(10f);
        }
    }

    public IEnumerator CheckDeaths(PlayerController[] players) {
        int deathCt = 0;
        while(!gameFinished) {
            foreach(PlayerController player in players) {
                if(player.isDead) {
                    deathCt++;
                }

                yield return null;
            }
            if(deathCt == players.Length) {
                gameLost = true;
                gameFinished = true;
            }
            deathCt = 0;
        }
    }


    void Update()
    {
        if(IsServer) {
            if(!gameFinished) time -= Time.deltaTime;
        }
        if(IsClient) {
            if(!gameFinished) {
                time -= Time.deltaTime;
                int min = (int)(time/60);
                int sec = (int)(time%60);
                timer.text = string.Format("{0:00}:{1:00}", min, sec);
            }
            if(gameFinished) {
                finalStatsPanel.gameObject.SetActive(true);
            }
        }
    }
}
