using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class HPBarUI : MonoBehaviour
{
    public PlayerController owner;
    public GameObject segment;
    public List<GameObject> segments;
    public Transform fill;
    public Slider slider;

    void Start()
    {
        while(segments.Count != owner.maxHp) {
            AddSegment();
        }
    }

    void Update()
    {
        slider.value = owner.hp;
        //update the amount of segments if the maxHp value changes from like items or shit
        if(slider.maxValue < owner.maxHp) {
            AddSegment();
            slider.maxValue++;
        }
        else if(slider.maxValue > owner.maxHp) {
            RemoveSegment();
            slider.maxValue--;
        }
        if(segments.Count < owner.maxHp) {
            AddSegment();
        }
        else if(segments.Count > owner.maxHp) {
            RemoveSegment();
        }
    }

    public void AddSegment() {
        segments.Add(Instantiate(segment, fill));
    }

    public void RemoveSegment() {
        Destroy(segments.Last());
        segments.RemoveAt(segments.Count-1);
    }

    public void UpdateSegment(float value) {
        int sliderValue = (int)value;
        Debug.Log(sliderValue + " " + segments.Count);

        if(sliderValue < segments.Count) {
            RemoveSegment();
        }
        else if(sliderValue > segments.Count) {
            AddSegment();
        }
    }
}
