using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

///This code was written by Dr. Bradford A. Towle Jr.
///And is intended for educational use only.
///4/11/2021

using NETWORK_ENGINE;
public class GameRoomButton : MonoBehaviour
{
    public int port;
    public NetworkCore gameCore;

    // Start is called before the first frame update
    void Start()
    {
        
    }
    public void SetText()
    {
        /*MyCore = GameObject.FindObjectOfType<NetworkCore>();
        MyLobby = GameObject.FindObjectOfType<LobbyManager>();
        if (MyCore == null || MyLobby == null)
        {
            throw new System.Exception("ERROR: Could not find NetworkCore or Network Lobby");
        }
        MyText.text = "Game Name: "+GameName + "\nPlayers: " + Players.ToString() + "/" + MyCore.MaxConnections.ToString() + "\t\tGame ID:" + name;
    */}
    public void JoinGame()
    {
        LobbyManager2 l2 = FindObjectOfType<LobbyManager2>();
        if (gameCore == null)
        {
            gameCore = l2.gameCore;
        }
        gameCore.PortNumber = port;
        gameCore.UI_StartClient();
        //Disable Lobby UI

        l2.DisableUI();
        l2.slowDisconnect();
    }

  
    // Update is called once per frame
    void Update()
    {
        
    }
}
