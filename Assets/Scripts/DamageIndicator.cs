using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageIndicator : MonoBehaviour
{
    public Vector3 startPos;
    public Vector3 startRot;
    public Vector3 startScale;
    public Vector3 targetPos;
    public Vector3 targetRot;
    public float time;

    void Start()
    {
        startPos = transform.position;
        startRot = transform.rotation.eulerAngles;
        startScale = transform.localScale;
        
        float randDirX = Random.Range(1f,3f);
        float randDirY = Random.Range(1f,3f);
        float randRot = Random.Range(-15f,-30f);

        targetPos = startPos + new Vector3(randDirX, randDirY, 0f);
        targetRot = startRot + new Vector3(0, 0, randRot);
    }

    void Update()
    {
        time += Time.deltaTime;

        float lifetime = 0.5f;

        if(time > lifetime) {
            Destroy(gameObject);
        } 

        transform.position = Vector3.Lerp(startPos, targetPos, Mathf.Sin(time / lifetime));
        transform.rotation = Quaternion.Lerp(Quaternion.Euler(startRot), Quaternion.Euler(targetRot), Mathf.Sin(time / lifetime));
        transform.localScale = Vector3.Lerp(startScale, Vector3.zero, Mathf.Sin(time / lifetime));
    }
}
