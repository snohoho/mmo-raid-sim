using System.Collections;
using System.Collections.Generic;
using NETWORK_ENGINE;
using TMPro;
using UnityEngine;

public class PlayerController : NetworkComponent
{
    public bool isMoving;
    public bool usingPrimary;
    public bool usingSecondary;
    public bool usingDefensive;
    public bool usingUlt;

    public override void HandleMessage(string flag, string value)
    {

    }

    public override void NetworkedStart()
    {

    }

    public override IEnumerator SlowUpdate()
    {
        yield return new WaitForSeconds(1);
    }

    void Start()
    {
        
    }

    void Update()
    {
        
    }
}
