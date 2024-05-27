using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AudioSystem;

public class AudioTester : MonoBehaviour
{
    private AudioManager m_AudioManager;

    [ContextMenu("Singles/SingleTest")]
    private void SingleTest()
    {
        AudioPlayer x = AudioManager.DefaultPlay("itboy");
    }
    
}
