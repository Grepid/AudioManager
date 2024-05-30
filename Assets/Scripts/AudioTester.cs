using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AudioSystem;

public class AudioTester : MonoBehaviour
{
    [ContextMenu("Singles/SingleTest")]
    private void SingleTest()
    {
        AudioPlayer x = AudioManager.DefaultPlay("itboy");
        x?.BindToAudioEnd(this, "TestMessage");
    }

    private void TestMessage()
    {
        print("Message recieved");
    }
    
}
