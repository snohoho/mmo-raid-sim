using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using NETWORK_ENGINE;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
//using UnityEngine.UIElements;
//using UnityEngine.UIElements;

public class LobbyManager2 : NetworkCore
{
    // Start is called before the first frame update

    public string publicIPAddress;
    public string privateIPAddress;
    public int portMininimum;
    public int portMaxinimum;
    public bool isGameServer;
    public bool isClient;
    public NetworkCore gameCore;
    public string localGameID;
    public int gameCounter;
    public Dictionary<int, System.Diagnostics.Process> gameServers;
    public float maxGameTime;
    public GameObject gameRoomContent;
    public InputField gameNameInput;
    public Button createGameButton;
    public GameObject StartingScreen;
    public GameObject LobbyScreen;

    
    protected override void Start()
    {
        base.Start();
        gameServers = new Dictionary<int, System.Diagnostics.Process>();
        if (gameCore == null)
        {
            throw new System.Exception("Could not find core!");
        }
        string[] args = System.Environment.GetCommandLineArgs();
        foreach (string a in args)
        {
            try
            {
                //Starting a Game Server....
                if (a.StartsWith("PORT_"))
                {
                    isGameServer = true;
                    string[] temp = a.Split('_');
                    int port = int.Parse(temp[1]);
                    localGameID = temp[3];
                    gameCore.PortNumber = port+portMininimum;
                    IP = privateIPAddress;
                    gameCore.IP = IP;
                    StartCoroutine(StartClient());
                    
                    StartCoroutine(SlowStart());
                }
            }
            catch (System.Exception e)
            {
                Debug.Log("Exception caught starting the server: " + e.ToString());
            }
            //Starting the Master Game Server
            if (a.Contains("MASTER"))
            {
                IP = privateIPAddress;
                StartServer();
            }

        }
/*#if UNITY_EDITOR
        IP = privateIPAddress;
        StartServer();
#endif*/
        if (!IsConnected)
        {
            StartCoroutine(SlowAgentStart());
        }


    }

    public IEnumerator SlowStart()
    {
        yield return new WaitForSeconds(1);
        yield return new WaitUntil(() => LocalConnectionID >= 0);
        gameCore.UI_StartServer();
        yield return new WaitUntil(() => gameCore.IsServer);
        isGameServer = true;
    }
    
    public IEnumerator SlowAgentStart()
    {

        Debug.Log("Attempting to connect to public IP.");
        IP = publicIPAddress;
        yield return StartCoroutine(StartClient());
        if (!IsConnected)
        {
            Debug.Log("Attempting to connect to private IP.");
            IP = privateIPAddress;
            yield return StartCoroutine(StartClient());
            if (!IsConnected)
            {
                Debug.Log("Attempting to connec to local host.");
                IP = "127.0.0.1";
                yield return StartCoroutine(StartClient());
                if (!IsConnected)
                {
                    throw new System.Exception("ERROR: COULD NOT CONECT TO SERVER!");
                }
            }
        }
        gameCore.IP = IP;
    }

    public void StartNewGame(string s)
    {
        if (IsServer)
        {
#if !UNITY_WEBGL
            try
            {
                System.Diagnostics.Process proc = new System.Diagnostics.Process();
                proc.StartInfo.UseShellExecute = true;
                string[] args = System.Environment.GetCommandLineArgs();
                
#if UNITY_EDITOR
                //You will need to change this to your executable name if you want to run a server in the editor.
                proc.StartInfo.FileName = "C:\\LobbyManager2\\My project\\WindowsBuild\\NewLobbyManager.exe";
#else
                proc.StartInfo.FileName = args[0];
#endif
                proc.StartInfo.Arguments = "PORT_" + gameCounter + "_GAMEID_" + s + " -batchmode -nographics >GameServer" + gameCounter + "Log.txt";

                gameServers.Add(gameCounter, proc);
                gameCounter++;
                proc.Start();
            }
            catch (System.Exception e)
            {
                Debug.Log("EXCEPTION - in creating a game!!! - " + e.ToString());
            }
#endif
            }
    }

    public void UI_CreateGame()
    {
        foreach(LobbyAgentManager lb in FindObjectsOfType<LobbyAgentManager>())
        {
            if(lb.IsLocalPlayer)
            {
                if (StartingScreen != null)
                {
                    StartingScreen.SetActive(true);
                }
                lb.hostGameName = gameNameInput.text;
                lb.SendCommand("STARTGAME", gameNameInput.text);
                
                //start coroutine to wait for the game to be made.
            }
        }
    }

    public override void OnSlowUpdate()
    {
        base.OnSlowUpdate();
        if (gameNameInput.text.Length > 2)
        {
            createGameButton.interactable = true;
        }
        else
        {
            createGameButton.interactable = false;
        }
    }
    // Update is called once per frame


    public void DisableUI()
    {
        StartingScreen.SetActive(false);
        LobbyScreen.SetActive(false);
    }
    public override void OnClientDisconnectCleanup(int id)
    {
        if(!gameCore.IsConnected && !IsServer && !isGameServer)
        {
            SceneManager.LoadScene(0);
        }
    }

    public void slowDisconnect()
    {
        StartCoroutine(SlowDisc());
    }
    public IEnumerator SlowDisc()
    {
        if (IsClient)
        {
            yield return new WaitUntil( () => (gameCore.IsConnected && gameCore.IsClient)) ;
            Disconnect(0);
        }
    }

    public override void UI_Quit()
    {
        base.UI_Quit();
        if (gameCore.IsConnected && gameCore.IsClient)
        {
            gameCore.UI_Quit();
        }
        
        
    }
    public void OnApplicationQuit()
    {
        if (IsConnected)
        {
            UI_Quit();
        }

    }
}
