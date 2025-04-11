using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BillboardSprite : MonoBehaviour
{
    void Start()
    {
        
    }

    void Update()
    {
        transform.forward = Camera.main.transform.forward;
    }
}
