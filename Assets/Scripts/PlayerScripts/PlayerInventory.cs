using System.Collections;
using System.Collections.Generic;
using NETWORK_ENGINE;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInventory : NetworkComponent
{
    public RectTransform invPanel;
    public ItemStats[] inventory = new ItemStats[10];

    public override void HandleMessage(string flag, string value)
    {
        
    }

    public override void NetworkedStart()
    {
        
    }

    public override IEnumerator SlowUpdate()
    {
        while(IsConnected) {
            if(IsLocalPlayer) {
                for(int i=0; i<10; i++) {
                    if(inventory[i] != null) {
                        invPanel.GetChild(i).gameObject.SetActive(true);
                    }
                    else if(inventory[i] == null) {
                        invPanel.GetChild(i).gameObject.SetActive(false);
                    }
                }
            }
            if(IsServer) {
                
            }

            yield return new WaitForSeconds(MyCore.MasterTimer);
        }
    }

    void Start()
    {
        
    }

    void Update()
    {
        
    }

    public void OpenInventory(InputAction.CallbackContext context) {
        if(context.started || context.performed) {
            if(IsLocalPlayer) {
                Cursor.lockState = CursorLockMode.None;
                invPanel.gameObject.SetActive(true);
            }
        }
        if(context.canceled) {
            if(IsLocalPlayer) {
                Cursor.lockState = CursorLockMode.Locked;
                invPanel.gameObject.SetActive(false);
            }
        }
    }

    public void HoverInventoryItem(int slot) {
        Debug.Log("hovering item in slot " + slot);
    }

    public void DropItem(int slot) {
        Debug.Log("dropping item in slot " + slot);
    }
}
