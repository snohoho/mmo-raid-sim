using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NETWORK_ENGINE;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using System;

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
    public AudioSource bgMusicSource;
    public AudioClip[] music;
    public bool gameLost;
    public Animator fadeTransition;

    public override void HandleMessage(string flag, string value) 
    {
        if(flag == "GAMESTART") {
            if(IsClient) {
                StartCoroutine(FadeTransition());

                Cursor.lockState = CursorLockMode.Locked;
                NetworkPlayerManager[] npm = FindObjectsOfType<NetworkPlayerManager>();
                foreach(NetworkPlayerManager n in npm) {
                    n.transform.GetChild(0).gameObject.SetActive(false);
                }

                timerPanel.gameObject.SetActive(true);
                bgMusicSource.resource = music[1];
                bgMusicSource.Play();
            }
        }
        if(flag == "BOSSSTART") {
            if(IsClient) {
                StartCoroutine(FadeTransition());

                bgMusicSource.resource = music[2];
                bgMusicSource.Play();
            }
        }
        if(flag == "GAMEFINISH") {
            if(IsClient) {
                StartCoroutine(FadeTransition());

                Cursor.lockState = CursorLockMode.None;
                gameFinished = bool.Parse(value);
                bgMusicSource.resource = music[0];
                bgMusicSource.Play();

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
                    finalStats.text =
                    header +
                    "Time\n" + string.Format("{0:00}:{1:00}", timeSpent/60, timeSpent%60) + "\n" +
                    "Team DPS\n" + Mathf.Round(totalDamage/timeSpent) + "\n";
                    
                    finalStatsPanel.gameObject.SetActive(true);
                    player.gameObject.SetActive(false);
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

            Debug.Log("starting game");
            SendUpdate("GAMESTART", "1");
            yield return new WaitForSeconds(0.45f);

            //spawn shop first
            MyCore.NetCreateObject(34, -1, new Vector3(0,0,0));
            
            //spawn players
            npm = FindObjectsOfType<NetworkPlayerManager>();
            foreach(NetworkPlayerManager n in npm) {
                //create objects
                MyCore.NetCreateObject(n.playerClass, n.Owner, GameObject.Find("spawn" + n.Owner).transform.position);
            }

            MyCore.NotifyGameStart();
            
            PlayerController[] players = FindObjectsOfType<PlayerController>();
            while(!grindPhaseFinished) {
                Debug.Log("grind phase start");
                time = 180f;
                SendUpdate("TIMER", time.ToString());
                StartCoroutine(SpawnEnemies());
                StartCoroutine(CheckDeaths(players));
                
                yield return new WaitUntil(() => gameFinished == true || gameLost == true || time <= 0f);


                SendUpdate("BOSSSTART", "true");
                yield return new WaitForSeconds(0.45f);

                //destroy all enemies
                foreach(NavMeshController enemy in FindObjectsOfType<NavMeshController>()) {
                    MyCore.NetDestroyObject(enemy.NetId);

                    yield return null;
                }

                //teleport players back to spawn
                foreach(PlayerController player in players) {
                    player.transform.position = GameObject.Find("spawn" + player.Owner).transform.position;
                    player.inShop = false;
                    player.SendUpdate("SHOP", false.ToString());
                    player.withinInteract = false;
                    player.SendUpdate("INTERACT", false.ToString());
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
                
                yield return new WaitForSeconds(1f);

                if(time <= 0f) {
                    MyCore.NetDestroyObject(FindAnyObjectByType<BossHitboxes>().NetId);
                    gameLost = true;
                    SendUpdate("GAMELOST", "true");
                }
                foreach(NavMeshController enemy in FindObjectsOfType<NavMeshController>()) {
                    MyCore.NetDestroyObject(enemy.NetId);

                    yield return null;
                }
                gameFinished = true;
            }

            Debug.Log("game finished");
            yield return new WaitForSeconds(0.45f);
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

    public IEnumerator FadeTransition() {
        Debug.Log("START FADE OUT");
        fadeTransition.SetBool("FadeIn", false);
        fadeTransition.SetBool("FadeOut", true);

        yield return new WaitForSeconds(0.5f);

        Debug.Log("START FADE IN");
        fadeTransition.SetBool("FadeOut", false);
        fadeTransition.SetBool("FadeIn", true);
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
                
            }
        }
    }
}
