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
    public TextMeshProUGUI selectedName;
    public TMP_Dropdown classDropdown;
    public Image classImg;
    public TextMeshProUGUI selectedClass;
    public Sprite[] classSprites;

    public bool ready;
    public string playerName;
    public int playerClass;    

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
            if(IsClient) {
                selectedName.text = playerName;
            }
            if(IsServer) {
                SendUpdate("NAME", value);
            }
        }
        if(flag == "CLASS") {
            playerClass = int.Parse(value);
            if(IsClient) {
                classImg.sprite = classSprites[playerClass];
                selectedClass.text = classDropdown.options[playerClass].text;
            }
            if(IsServer) {
                SendUpdate("CLASS", value);
            }
        }
    }

    public override void NetworkedStart()
    {
        if(IsLocalPlayer) {
            selectedName.GameObject().SetActive(false);
            selectedClass.GameObject().SetActive(false);
        }
        if(!IsLocalPlayer) {
            nameInput.GameObject().SetActive(false);
            classDropdown.GameObject().SetActive(false);
        }
        readyToggle.interactable = false;

        lobbyUIPos.localPosition = new Vector2((Owner-2)*480, 0);
    }

    public override IEnumerator SlowUpdate()
    {
        while(IsConnected) {
            if(IsLocalPlayer && playerName == "") {
                readyToggle.interactable = false;
            }
            else if(IsLocalPlayer && playerName != "") {
                readyToggle.interactable = true;
            }

            if(IsServer) {
                if(IsDirty) {
                    SendUpdate("NAME", playerName);
                    SendUpdate("CLASS", playerClass.ToString());

                    IsDirty = false;
                }
            }

            yield return new WaitForSeconds(.1f);
        }  
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

    public void PlayerClass(int playerClass) {
        //0 = queen (liz - tank)
        //1 = fister (gigi - melee dps)
        //2 = sounddemon (rissa - range dps)
        //3 = emotionjewel (biboo - support)
        if(IsLocalPlayer) {
            SendCommand("CLASS", playerClass.ToString());
        }
    } 

    void Start()
    {
        
    }

    void Update()
    {
        
    }
}
