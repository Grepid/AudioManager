using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//This class only exists to accompany an Audio Source within its GameObject to be able to keep track of what Sound Class it came from
//So that the Type can be determined for seperate Audio Control

public class CustomAudioPlayer : MonoBehaviour
{
    public AudioSource audioSource;
    public Sound soundClass;

    public bool wasPausedByESC;
    private void Update()
    {
        if (!audioSource){ Destroy(gameObject); return; }
        if(audioSource.time >= audioSource.clip.length && !soundClass.loop)
        {
            Destroy(gameObject);
        }
    }
}
