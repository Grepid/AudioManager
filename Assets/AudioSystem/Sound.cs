using UnityEngine.Audio;
using UnityEngine;


//Allows the class to be edited within the inspector when in Arrays

//This is the Enum for the types of sounds there are
public enum SoundType{Unset,Master,Music,UI,SFX}

//This class is used for creating new sounds that can be played by adding a new one to the AudioManager GameObject's "Sounds" Array and changing the values
[System.Serializable]
public class Sound
{
    [Tooltip("The name you need to call in Code to be able to play this sound")]
    public string name;

    [Tooltip("The type of sound that it is (Useful for sorting different volume levels)")]
    public SoundType type;

    [Tooltip("The MP3 Of the sound that will play when called")]
    public AudioClip clip;

    [Tooltip("The volume of the clip (Most often will be controlled by the AudioManager)")]
    [Range(0f, 1f)]
    public float volume;
    [Tooltip("The Pitch that the audio will play at (<1 deeper and >1 higher pitched)")]
    [Range(0.1f, 3f)]
    public float pitch;

    [Tooltip("If this audio is designed to loop (Music etc)")]
    public bool loop;

    [Tooltip("If the Audio is 2D or 3D. 2D Will play equally in both ears, and 3D Will play directionally depending on where the Source is to the Camera")]
    [Range(0f, 1f)]
    public float spatialBlend;

    [Tooltip("The minimum range for Sound Falloff")]
    [Range(1f, 1000f)]
    public float minDistance;
    [Tooltip("The Maximum range the sound can be heard from")]
    [Range(1f, 1000f)]
    public float maxDistance;

    [Tooltip("How strong the doppler shift is (The faster a sound source is travelling towards you, the higher pitch it is and further away it is getting, the deeper it is)")]
    [Range(0f, 5f)]
    public float dopplerLevel;
}
