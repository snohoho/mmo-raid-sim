using System.Collections;
using System.Collections.Generic;
using NETWORK_ENGINE;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class NetworkPlayerManager : NetworkComponent
{
    public RectTransform lobbyUIPos;
    public Toggle readyToggle;
    public TMP_InputField nameInput;

    public bool ready;
    public string playerName;

    public override void HandleMessage(string flag, string value)
    {
        if(flag == "READY") {
            ready = bool.Parse(value);
            if(IsServer) {
                SendUpdate("READY", value);
            }
        }
        if(flag == "NAME") {
            playerName = value;
            if(IsServer) {
                SendUpdate("NAME", value);
            }
        }
    }

    public override void NetworkedStart()
    {
        if(!IsLocalPlayer) {
            nameInput.GameObject().SetActive(false);
            readyToggle.GameObject().SetActive(false);
        }
        lobbyUIPos.localPosition = new Vector2((Owner-2)*480, 0);
    }

    public override IEnumerator SlowUpdate()
    {
        
        yield return new WaitForSeconds(.1f);
    }

    public void NameInput(string name) {
        if(IsLocalPlayer) {
            SendCommand("NAME", name);
        }
    }

    public void Ready(bool ready) {
        if(IsLocalPlayer) {
            SendCommand("READY", ready.ToString());
        }
    }

    void Start()
    {
        
    }

    void Update()
    {
        
    }
}
