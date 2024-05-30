using AudioSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Let people attach this as a way to spawn audio when it starts
//Also let them have it as a function 
//Also probably split it into many events such as:
//On destroy events, On Enable Events, triggers, collisions etc etc

public class AudioAttachment : MonoBehaviour
{
    public string SoundName;
    public bool PlayOnStart;

    void Start()
    {
        //if (PlayOnStart)        
    }


    public void PlaySound()
    {
        //
    }
}
