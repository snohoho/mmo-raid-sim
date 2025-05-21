using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NETWORK_ENGINE;

public class NetworkTransform : NetworkComponent
{
    public Vector3 LastPosition;
    public Vector3 LastRotation;
    public float MinThreshold = .1f;
    public float MaxThreshold = .5f;
    public bool Smooth = true;
    public float NavMeshSpeed = 1;
    public float RotationSpeed = Mathf.PI / 2;

    public static Vector3 VectorFromString(string value)  //parse Vector3 value from given string value
    {
        char[] temp = { '(', ')' };
        string[] args = value.Trim(temp).Split(',');
        return new Vector3(float.Parse(args[0].Trim()), float.Parse(args[1].Trim()), float.Parse(args[2].Trim()));
    }

    public override void HandleMessage(string flag, string value)
    {
        if(flag == "POS")  //synchronization of position and rotation vector3 values
        {
            Vector3 temp = NetworkTransform.VectorFromString(value);
            if ( (temp-this.transform.position).magnitude > MaxThreshold || !Smooth)
            {
                this.transform.position = temp;
            }
            LastPosition = temp;
        }

        if (flag == "ROT")
        {
            Vector3 temp = NetworkTransform.VectorFromString(value);
            if ((temp - this.transform.rotation.eulerAngles).magnitude < MaxThreshold || !Smooth)
            {
                Quaternion qt = new Quaternion();
                qt.eulerAngles = temp;
                this.transform.rotation = qt;
            }
            LastRotation = temp;
        }
    }

    public override void NetworkedStart()
    {
        
    }

    public override IEnumerator SlowUpdate()
    {
        while(MyCore.IsConnected)
        {
            if(IsServer)  //server holds and sends position and rotation values of enemy navmesh movement to players
            {
                float DistCheck = (this.transform.position - LastPosition).magnitude;
                if(DistCheck > MinThreshold)
                {
                    SendUpdate("POS", this.transform.position.ToString("F2"));
                    LastPosition = this.transform.position;
                }
                
                float CheckRotation = (this.transform.rotation.eulerAngles - LastRotation).magnitude;
                if(CheckRotation > MinThreshold)
                {
                    SendUpdate("ROT", this.transform.rotation.eulerAngles.ToString("F2"));
                    LastRotation = this.transform.rotation.eulerAngles;
                }

                if(IsDirty)
                {
                    SendUpdate("POS", this.transform.position.ToString("F2"));
                    SendUpdate("ROT", this.transform.rotation.eulerAngles.ToString("F2"));
                    IsDirty = false;
                }
            }
            yield return new WaitForSeconds(.05f);
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(IsClient && Smooth)  //every player sees the same exact enemy movement
        {
            this.transform.position = Vector3.Lerp(this.transform.position, LastPosition, .2f);
            Quaternion qt = new Quaternion();
            qt.eulerAngles = Vector3.Lerp(this.transform.rotation.eulerAngles, LastRotation, RotationSpeed * Time.deltaTime);
            this.transform.rotation = qt;
        }
    }
}
