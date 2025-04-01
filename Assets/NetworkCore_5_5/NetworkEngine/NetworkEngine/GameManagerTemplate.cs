using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NETWORK_ENGINE;
public class GameManagerTemplate : NetworkComponent
{
    public bool GameStarted;
    public bool GameOver;
    //Team Score
    //Individual Score Here...
    //etc.

    public override void HandleMessage(string flag, string value)
    {
        //if flag == "GAMESTART"
        //   Want to disable PlayerInfo
        if (flag == "GAMESTART" && IsClient)
        {
            GameStarted = true;
            NetworkPlayerManager[] npm = Object.FindObjectsOfType<NetworkPlayerManager>();
            foreach (NetworkPlayerManager n in npm)
            {
                //n.enabled = false;
                //OR
                //disable visualization
                n.GetComponent<Renderer>().enabled = false;
            }
        }
    }
    public override void NetworkedStart()
    {

    }
    public override IEnumerator SlowUpdate()
    {
        if (IsServer)
        {
            //while(all players have not hit ready)
            //Wait
            NetworkPlayerManager[] players;
            bool tempGameStarted = true;
            do
            {
                players = GameObject.FindObjectsOfType<NetworkPlayerManager>();
                tempGameStarted = true;

                foreach (NetworkPlayerManager n in players)
                {
                    if (!n.ready)
                    {
                        tempGameStarted = false;
                    }
                }
                yield return new WaitForSeconds(1);

            } while (!tempGameStarted || players.Length < 2);

            players = GameObject.FindObjectsOfType<NetworkPlayerManager>();
            foreach (NetworkPlayerManager n in players)
            {
                //GameObject t = myCore.NetCreateObject(...) -- instantiate the correct prefab
                //Foreach sychronized option
                //Set an option on the new character.
                //t.team = n.team
            }
            //Go to each NetworkPlayerManager and look at their options
            //Create the appropriate character for their options
            //GameObject temp = MyCore.NetCreateObject(1,Owner,new Vector3);
            //temp.GetComponent<MyCharacterScript>().team = //set the team;

            //SendUpdate "GAMESTART"
            SendUpdate("GAMESTART","1");
            MyCore.NotifyGameStart();

            while (!GameOver)
            {
                //Game is playing.
                //This is where you implement turn-based logic
                //Maintain score...
                //Maintain any measurment/metrics.
                yield return new WaitForSeconds(.1f);
            }
            //Wait until the game ends...
            SendUpdate("GAMEOVER", "1");
            //diable controls...
            //Show score screen
            yield return new WaitForSeconds(30);


            //MyId.NotifyDirty();
            StartCoroutine(MyCore.DisconnectServer());
        }
        yield return new WaitForSeconds(.1f);
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
