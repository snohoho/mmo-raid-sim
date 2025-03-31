using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using NETWORK_ENGINE;

public class LobbyAgentManager : NetworkComponent
{

    public bool isGameServer;
    public int maxNumPlayers;
    public bool gameStarted;
    public string gameName;
    public int currentNumPlayers;
    public float killTimer;
    public int Creator;
    public NetworkCore gameCore;
    public int port;
    public GameObject buttonPrefab;
    public GameObject myButton;
    public string hostGameName;
    LobbyManager2 myLobby;
    
   


    public override void HandleMessage(string flag, string value)
    {

        
       
        if(flag == "GAMEOVER")
        {
            if(IsServer)
            {
                SendUpdate("GAMEOVER", "1");

            }
            if(IsClient && !isGameServer)
            {
                MyCore.Disconnect(MyId.NetId);
            }
        }
     
       if(flag == "PLAYERS")
        {
            currentNumPlayers = int.Parse(value);
            if (MyCore.IsServer)
            {
                SendUpdate("PLAYERS", value);
            }
        }
       if(flag == "GSTARTED")
        {
            gameStarted = bool.Parse(value);
            if(gameStarted && myButton != null)
            {
                //Destroy My Button.
                Destroy(myButton);
            }
            if(MyCore.IsServer)
            {
                SendUpdate("GSTARTED", value);
            }
        }
       if(flag == "MAXPLAYERS")
        {
            maxNumPlayers = int.Parse(value);
            if(IsServer)
            {
                SendUpdate("MAXPLAYERS", value);
            }
        }
       if(flag == "PORT")
        {
            port= int.Parse(value); 
            if(myButton!= null)
            {
                myButton.GetComponent<GameRoomButton>().port = port;
            }
            if(IsServer)
            {
                SendUpdate("PORT", value);
            }
        }
       if(flag == "ISGS")
        {
            isGameServer= bool.Parse(value);    
            if(IsServer)
            {
                SendUpdate("ISGS", value);
            }
            if(isGameServer && myButton == null)
            {
                //Create Button and add it to the list.
                myButton = GameObject.Instantiate(buttonPrefab, myLobby.gameRoomContent.transform);
                myButton.GetComponent<GameRoomButton>().port = port;
                myButton.GetComponent<GameRoomButton>().gameCore = gameCore;
            }
        }
       if(flag == "GNAME")
        {
            gameName = value;
            if(IsServer)
            {
                SendUpdate("GNAME", gameName);
            }
        }
       if(flag == "STARTGAME")
        {
           

            if (IsServer)
            {
                SendUpdate("STARTGAME", value);   
                myLobby.StartNewGame(value);
            }
        }


    }


    private void OnDestroy()
    {
        if (IsLocalPlayer)
        {
            SendCommand("GSTARTED", true.ToString());
            MyCore.Disconnect(0);

        }
    }

    public override void NetworkedStart()
    {
        if( myLobby.isGameServer)
        {
            gameName = (myLobby.localGameID);
            isGameServer = true;
            gameStarted = false;
            maxNumPlayers = gameCore.MaxConnections;
            port = gameCore.PortNumber;
            currentNumPlayers = 0;
            SendCommand("ISGS", isGameServer.ToString());
            SendCommand("MAXPLAYERS", maxNumPlayers.ToString());
            SendCommand("PORT", port.ToString());
            SendCommand("GNAME", gameName);
            killTimer = myLobby.maxGameTime;
            StartCoroutine(TTL());
        }

    }
    public IEnumerator TTL()
    {
        yield return new WaitForSecondsRealtime(killTimer);
        SendCommand("GSTARTED", true.ToString());
        SendCommand("GAMEOVER", "1");
        yield return new WaitForSeconds(1);
        MyCore.UI_Quit();
        StartCoroutine(slowProcessKill());
        Application.Quit();
    }

    public override IEnumerator SlowUpdate()
    {
        while (MyCore.IsConnected)
        {


            if (isGameServer && myLobby.gameCore.IsServer)
            {
                currentNumPlayers =  gameCore.Connections.Count;
                SendCommand("PLAYERS", currentNumPlayers.ToString());

                gameStarted = !gameCore.IsListening && gameCore.Connections.Count>1;
                SendCommand("GSTARTED",gameStarted.ToString());

                

            }
            if(!IsServer && !isGameServer && !gameCore.IsConnected && hostGameName != "")
            {
                GameRoomButton[] grbs = FindObjectsOfType<GameRoomButton>(default);
                foreach(GameRoomButton grb in grbs)
                {
                    Text t = grb.gameObject.GetComponentInChildren<Text>();
                    if(t.text.Contains(hostGameName))
                    {
                        grb.JoinGame();
                    }
                }
            }
            if(IsServer)
            {
                if(!MyCore.Connections.ContainsKey(Owner))
                {
                    MyCore.Disconnect(Owner);
                }
                if (IsDirty)
                {
                    SendUpdate("PORT", port.ToString());
                    SendUpdate("MAXPLAYERS", maxNumPlayers.ToString());
                    SendUpdate("GSTARTED", gameStarted.ToString());
                    SendUpdate("PLAYERS", currentNumPlayers.ToString());
                    SendUpdate("ISGS", isGameServer.ToString());
                    SendUpdate("GNAME", gameName);
                    IsDirty = false;
                }
            }
            yield return new WaitForSeconds(1);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        hostGameName = "";
        myLobby = FindObjectOfType<LobbyManager2>();
        gameCore = myLobby.gameCore;
    }

    // Update is called once per frame
    void Update()
    {
        if(myButton!= null)
        {
            Text t = myButton.GetComponentInChildren<Text>();
            if(t!=null && isGameServer)
            {
                t.text = gameName+ "("+currentNumPlayers+"/"+maxNumPlayers+")";
            }
        }
    }

    public IEnumerator slowProcessKill()
    {
        yield return new WaitUntil(() => gameCore.Connections.Count == 0);
        MyCore.Disconnect(0);
        Application.Quit();
    }
}
