using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    private static AudioManager _Default;
    public static AudioManager Default {
        get {
            if(_Default == null) {
                _Default = GameObject.Find("AudioManager").GetComponent<AudioManager>(); 
            }
            return _Default;
        }
    }
    
    public GameObject audioSource;
    public AudioClip[] audioClips;
    
    public void CreateSource(AudioClip clip) {
        var newSource = Instantiate(audioSource);    
        AudioSource source = newSource.GetComponent<AudioSource>();
        source.clip = clip;
        float length = clip.length;

        source.Play();
        Destroy(newSource, length);
    }
}
