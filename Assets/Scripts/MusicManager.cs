﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicManager : MonoBehaviour {
    
    public AudioClip mainTheme;
    public AudioClip menuTheme;

    private void Start() {
        AudioManager.instance.PlayMusic(mainTheme, 2f);
    }

    private void Update() {
        if ( Input.GetKeyDown(KeyCode.Space) )
            AudioManager.instance.PlayMusic (mainTheme, 3f);
    }
    
}
