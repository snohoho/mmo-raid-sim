using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TitleRotation : MonoBehaviour
{
    private float rot;
    [SerializeField] private float rotSpd = 0.25f;
    [SerializeField] private float minRot = -2.5f;
    [SerializeField] private float maxRot = 2.5f;

    void Update()
    {
        float rot = Mathf.SmoothStep(minRot, maxRot, Mathf.PingPong(Time.unscaledTime * rotSpd, 1));
		transform.rotation = Quaternion.Euler(0,0,rot);
    }
}
