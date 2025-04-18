using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    private static AudioManager _Instance;
    public static AudioManager Instance {
        get {
            if(_Instance == null) {
                _Instance = GameObject.Find("AudioManager").GetComponent<AudioManager>(); 
            }
            return _Instance;
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
