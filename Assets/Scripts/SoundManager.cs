using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public AudioSource audioSource;
    public AudioClip click;
    public AudioClip burn;
    public AudioClip success;
    public AudioClip failure;
    public static SoundManager soundInstance;

    private void Awake()
    {
        if(soundInstance != null && soundInstance != this)
        {
            Destroy(this.gameObject);
            return;
        }

        soundInstance = this;
        DontDestroyOnLoad(this);
    }
}
