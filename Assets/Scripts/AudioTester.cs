using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AudioSystem;

public class AudioTester : MonoBehaviour
{
    private AudioPlayer player;
    [ContextMenu("Singles/SingleTest")]
    private void SingleTest()
    {
        player = AudioManager.DefaultPlay("itboy");
        player?.BindToAudioEnd(this, "TestMessage");
        Invoke("TestStop", 0.2f);
    }

    private void TestMessage()
    {
        print("Message recieved");
    }
    private void TestStop()
    {
        player.Stop();
    }
    
}
