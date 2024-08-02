using AudioSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AttachmentEventType {OnStart,OnDestroy,OnCollisionEnter,OnCollisionExit}
[System.Serializable]
public class AttachmentEvent
{
    public AttachmentEventType type;
    public string soundName;
}
