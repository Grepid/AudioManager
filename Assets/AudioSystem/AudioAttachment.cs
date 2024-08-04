using AudioSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

//Let people attach this as a way to spawn audio when it starts
//Also let them have it as a function 
//Also probably split it into many events such as:
//On destroy events, On Enable Events, triggers, collisions etc etc

public class AudioAttachment : MonoBehaviour
{
    [NonReorderable]
    public AttachmentEvent[] events;

    void Start()
    {
        foreach(AttachmentEvent e in events)
        {
            if (e.type == AttachmentEventType.OnStart) Play(e);
        }
    }

    private void OnDestroy()
    {
        foreach (AttachmentEvent e in events)
        {
            if (e.type == AttachmentEventType.OnDestroy) Play(e);
        }
    }

    #region Collision Enter

    private void OnCollisionEnter(Collision collision)
    {
        foreach (AttachmentEvent e in events)
        {
            if (e.type == AttachmentEventType.OnCollisionEnter) Play(e);
        }
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        foreach (AttachmentEvent e in events)
        {
            if (e.type == AttachmentEventType.OnCollisionEnter) Play(e);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        foreach (AttachmentEvent e in events)
        {
            if (e.type == AttachmentEventType.OnCollisionEnter) Play(e);
        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        foreach (AttachmentEvent e in events)
        {
            if (e.type == AttachmentEventType.OnCollisionEnter) Play(e);
        }
    }
    #endregion

    #region Collision Exit

    private void OnCollisionExit(Collision collision)
    {
        foreach (AttachmentEvent e in events)
        {
            if (e.type == AttachmentEventType.OnCollisionExit) Play(e);
        }
    }
    private void OnCollisionExit2D(Collision2D collision)
    {
        foreach (AttachmentEvent e in events)
        {
            if (e.type == AttachmentEventType.OnCollisionExit) Play(e);
        }
    }
    private void OnTriggerExit(Collider other)
    {
        foreach (AttachmentEvent e in events)
        {
            if (e.type == AttachmentEventType.OnCollisionExit) Play(e);
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        foreach (AttachmentEvent e in events)
        {
            if (e.type == AttachmentEventType.OnCollisionExit) Play(e);
        }
    }
    #endregion


    public void Play(AttachmentEvent e)
    {
        switch (e.followType)
        {
            case SoundFollowType.None:
                AudioManager.Play(e.soundToPlay, transform.position);
                break;

            case SoundFollowType.Camera:
                AudioManager.Play(e.soundToPlay, Camera.main.gameObject,true);
                break;

            case SoundFollowType.Self:
                AudioManager.Play(e.soundToPlay, gameObject, true);
                break;

            case SoundFollowType.Target:
                if(e.followTarget == null)
                {
                    Debug.LogWarning("Target is null, yet you want the audio to follow it");
                    e.followType = SoundFollowType.None;
                    Play(e);
                    return;
                }
                AudioManager.Play(e.soundToPlay, e.followTarget, true);
                break;
        }
    }
}
