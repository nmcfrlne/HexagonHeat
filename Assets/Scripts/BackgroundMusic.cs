using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundMusic : MonoBehaviour
{
    public static BackgroundMusic musicInstace;
    public AudioSource audioSource;

    private void Awake()
    {
        if(musicInstace != null && musicInstace != this)
        {
            Destroy(this.gameObject);
            return;
        }

        musicInstace = this;
        DontDestroyOnLoad(this);
    }
}
