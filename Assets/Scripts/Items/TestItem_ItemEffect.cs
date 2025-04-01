using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestItem_ItemEffect : MonoBehaviour
{
    public IEnumerator Effect(GameObject itemOwner) {
        
        yield return new WaitForEndOfFrame();
    }
}
