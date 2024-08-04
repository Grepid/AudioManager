using AudioSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AttachmentEventType {OnStart,OnDestroy,OnCollisionEnter,OnCollisionExit}
public enum SoundFollowType {None,Camera,Self,Target}
[System.Serializable]
public class AttachmentEvent
{
    [Tooltip("The action you want to trigger the audio")]
    public AttachmentEventType type;
    [Tooltip("How you want the sound to position itself")]
    public SoundFollowType followType;
    [Tooltip("The object the sound will follow. Leave Null if follow is not target.")]
    public GameObject followTarget;
    [Tooltip("The sound you want to play (It is recommended to not play looping audio as you do not " +
        "recieve a AudioPlayer to handle stopping it)")]
    public Sound soundToPlay;
}
