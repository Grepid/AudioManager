using AudioSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AttachmentEventType {OnStart,OnDestroy,OnCollisionEnter,OnCollisionExit}
public enum SoundFollowType {None,Camera,Self,Target}
[System.Serializable]
public class AttachmentEvent
{
    public AttachmentEventType type;
    public SoundFollowType followType;
    [Tooltip("The object the sound will follow. Leave Null if follow is not target.")]
    public GameObject followTarget;
    public string soundName;
}
