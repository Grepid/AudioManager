using UnityEngine.Audio;
using System;
using UnityEngine;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Collections;
using System.Linq;


/*
 * Notes for self 
 * Scaling on difference center 
 * https://discussions.unity.com/t/scaling-an-object-from-a-different-center/2508
 */

public class AudioManager : MonoBehaviour
{
    [Tooltip("The name that the GameObject created when playing a sound will be given for identification purposes")]
    public string tempSoundSourceIdentifier;

    [Tooltip("The list of sounds the game has to be able to play. (The NonReorderable Tag is there to prevent a UI Bug with dropdowns in Arrays in the inspector)")]
    [NonReorderable]
    public Sound[] sounds;

    [Tooltip("The sole singleton instance of this class")]
    public static AudioManager instance;

    [Tooltip("A dictionary of Sound Type to Volume Level for that type.")]
    //The cool thing about this new way of doing it, is if you want to add a new audio type with
    //a seperate audio level you can adjust, all you need to do is add the type to the Enum in the Sound class
    //and it will be perfectly synced up with the system. Then just create its own settings slider and done
    public static Dictionary<SoundType, float> volumes;

    [SerializeField] private float defaultFadeTime;

    [SerializeField] private int debugIndexToPlay;
    [SerializeField] private string debugNameToPlay;

    public CustomAudioPlayer[] AllPlayersInScene
    {
        get
        {
            CustomAudioPlayer[] players = FindObjectsOfType<CustomAudioPlayer>();
            return players;
        }
    }

    [HideInInspector]
    public CustomAudioPlayer currentMusic;

    public Dictionary<CustomAudioPlayer, Coroutine> overtimeEffects = new Dictionary<CustomAudioPlayer, Coroutine>();

    private void Awake()
    {
        if (instance == null) instance = this;
        else
        {
            Destroy(this);
            return;
        }
        DontDestroyOnLoad(this);

        Initialise();
    }
    private void Initialise()
    {
        overtimeEffects = new Dictionary<CustomAudioPlayer, Coroutine>();
        SetupVolumes();        
    }

    private void Update()
    {
        
    }

    /// <summary>
    /// This sets up a dictionary of SoundType to Float which can be referenced from anywhere to access the volume level for a given
    /// sound type. This method makes it really easy to associate the type of sound to a volume level.
    /// </summary>
    private void SetupVolumes()
    {
        //Creates the empty dictionary
        volumes = new Dictionary<SoundType, float>();
        //Loops through every type there is in the Enum
        foreach (SoundType type in Enum.GetValues(typeof(SoundType)))
        {
            //Adds the type to the dictionary, and gives it a starting volume of 1 (100%)
            volumes.Add(type, 1);
        }
        // Will set the audio levels to what it was saved at afterwards. If nothing was found, will just leave it at 100%
        //TODO though
        LoadSavedAudioLevels();
    }
    private void LoadSavedAudioLevels()
    {
        // To be Implemented
    }

    #region PlaySounds

    /// <summary>
    /// The base function for spawning audio. Will create a gameobject with a CustomAudio player and an AudioSource at 0,0,0.
    /// Use this for specific use cases, otherwise use the Play() Overrides.
    /// </summary>
    /// <param name="name"></param>
    /// <returns>CustomAudioPlayer attached to a GameObject for the sound played</returns>
    private CustomAudioPlayer DefaultPlay(string name)
    {
        GameObject focus = new GameObject();
        Sound s = Array.Find(sounds, sound => sound.name == name);
        if (s == null)
        {
            Debug.LogWarning("Sound not found");
            s = new Sound();
        }

        focus.name = tempSoundSourceIdentifier;

        AudioSource audSource = focus.AddComponent<AudioSource>();
        AdjustAudioSource(audSource, s);

        CustomAudioPlayer player = focus.AddComponent<CustomAudioPlayer>();
        player.audioSource = audSource;
        player.soundClass = s;
        


        audSource.volume = SoundTypeVolume(s);

        audSource.Play();

        //if (!audSource.loop) StartCoroutine(DestroyUsedAudio(focus));



        return player;
    }

    /// <summary>
    /// Will use the DefaultPlay() Function to create a GameObject and CustomAudioPlayer then will
    /// set its position to the Camera's position. This function will effectively act as UI Sound whether
    /// or not the Sound played is set to 2D or 3D Audio
    /// </summary>
    /// <param name="name"></param>
    /// <returns>The associated CustomAudioPlayer with the Sound Played</returns>
    public CustomAudioPlayer Play(string name)
    {
        CustomAudioPlayer player;
        player = Play(name, Camera.main.gameObject);
        return player;
    }

    /// <summary>
    /// Will use the DefaultPlay() Function to create a GameObject and CustomAudioPlayer then will
    /// parent it to the GameObject passed in and set its location to the object.
    /// Typically used for playing audio from a specific object that follows the object (E.G Vehicle Engine Noises)
    /// </summary>
    /// <param name="name"></param>
    /// <param name="goOrigin"></param>
    /// <returns>The associated CustomAudioPlayer with the Sound Played</returns>
    public CustomAudioPlayer Play(string name, GameObject goOrigin)
    {
        CustomAudioPlayer player;
        player = DefaultPlay(name);
        player.gameObject.transform.parent = goOrigin.transform;
        player.gameObject.transform.position = goOrigin.transform.position;

        return player;
    }

    /// <summary>
    /// Will use the DefaultPlay() Function to create a GameObject and CustomAudioPlayer then will
    /// set the sound's position to the passed in V3.
    /// Typically used for playing audio that when created, will not move from where it was created (E.G Gunshot)
    /// </summary>
    /// <param name="name"></param>
    /// <param name="posOrigin"></param>
    /// <returns></returns>
    public CustomAudioPlayer Play(string name, Vector3 posOrigin)
    {
        CustomAudioPlayer player;
        player = DefaultPlay(name);
        player.transform.position = posOrigin;

        return player;
    }

    /// <summary>
    /// Used as the base of PlayInSequence but useable in its own write, it takes a string of sounds and plays them 1 after the other.
    /// It returns the array of CustomAudioPlayer instances in the respective order of how you inputed them.
    /// </summary>
    /// <param name="sounds"></param>
    /// <returns></returns>
    public CustomAudioPlayer[] PlayInSequence(string[] sounds)
    {
        CustomAudioPlayer[] players = new CustomAudioPlayer[sounds.Length];
        int i = 0;
        foreach (string sound in sounds)
        {
            string cleanedSound = sound.Replace(" ", "");
            players[i] = Play(cleanedSound);
            players[i].audioSource.Pause();
            i++;
        }
        StartCoroutine(IPlayInSequence(players));
        return players;
    }

    /// <summary>
    /// Plays sounds 1 after eachother when given a string with names of sounds seperated with commas
    /// </summary>
    /// <param name="sounds"></param>
    /// <returns></returns>
    public CustomAudioPlayer[] PlayInSequence(string sounds)
    {
        string[] soundsSegmented = sounds.Split(',',StringSplitOptions.RemoveEmptyEntries);

        return PlayInSequence(soundsSegmented);
    }
    private IEnumerator IPlayInSequence(CustomAudioPlayer[] players)
    {
        //print("About to play " + players.Length + " audio tracks");
        foreach (CustomAudioPlayer player in players)
        {
            if(player == null) continue;
            player.audioSource.Play();
            //print("Now playing " + player.soundClass.name);
            while (true)
            {
                if (player.audioSource == null) break;
                if (player.audioSource.time >= player.audioSource.clip.length) break;
                yield return null;
            }
        }
    }


    #endregion

    /// <summary>
    /// Will calculate volume for a specific audio clip based on the SoundType from the Sound class passed in
    /// </summary>
    /// <param name="s"></param>
    /// <returns>The 0 to 1 Float representing the volume that specific Audio clip should be at</returns>
    public float SoundTypeVolume(Sound s)
    {
        float v = SoundTypeVolume(s.type);
        return v;
    }
    public float SoundTypeVolume(SoundType type)
    {
        float v = volumes[SoundType.Master] * volumes[type];
        return v;
    }

    /// <summary>
    /// Will be automatically called for any audio clip that does not loop to handle destroying it when it is finished.
    /// </summary>
    /// <param name="go"></param>
    /// <returns></returns>
    private IEnumerator DestroyUsedAudio(GameObject go)
    {
        if (go == null) yield break;
        AudioSource audioSource = go.GetComponent<AudioSource>();
        if(audioSource .clip == null)
        {
            Destroy(go);
            yield break;
        }
        yield return new WaitForSeconds(audioSource.clip.length);
        Destroy(go);
    }

    /// <summary>
    /// Handles stopping audio and then handling it's removal from the scene
    /// </summary>
    /// <param name="player"></param>
    public void StopAudio(CustomAudioPlayer player)
    {
        if (player == null) return;
        if(overtimeEffects.Keys.Contains(player)) StopOvertimeEffect(player);
        player.audioSource.Stop();
        Destroy(player.gameObject);
    }

    /// <summary>
    /// Stops every playing audio of the given type
    /// </summary>
    /// <param name="soundType"></param>
    public void StopAllAudioOfType(SoundType soundType)
    {
        foreach(CustomAudioPlayer ap in FindObjectsOfType<CustomAudioPlayer>())
        {
            if (ap.soundClass.type != soundType) return;
            StopAudio(ap);
        }
    }

    public void StopAllAudio()
    {
        foreach (CustomAudioPlayer ap in FindObjectsOfType<CustomAudioPlayer>())
        {
            StopAudio(ap);
        }
    }

    /// <summary>
    /// Will adjust the given AudioSource to have the properties the Sound class it is given has.
    /// </summary>
    /// <param name="source"></param>
    /// <param name="sound"></param>
    public void AdjustAudioSource(AudioSource source, Sound sound)
    {
        source.clip = sound.clip;

        source.volume = SoundTypeVolume(sound);
        source.pitch = sound.pitch;

        source.loop = sound.loop;

        source.spatialBlend = sound.spatialBlend;

        source.minDistance = sound.minDistance;
        source.maxDistance = sound.maxDistance;

        source.dopplerLevel = sound.dopplerLevel;
    }

    public void SetAudioLevel(SoundType type,float level)
    {
        volumes[type] = level;
        UpdateAllAudio();
    }

    /// <summary>
    /// Will update all audio clips in the scene to have the most up to date settings.
    /// Typically used for when Volume changes
    /// </summary>
    public void UpdateAllAudio()
    {
        
        foreach(CustomAudioPlayer player in AllPlayersInScene)
        {
            UpdateAudio(player);
        }
    }

    /// <summary>
    /// Updates a single piece of playing audio. Typically just called from the UpdateAllAudio() function,
    /// however can be called if needed for only 1 piece of audio
    /// </summary>
    /// <param name="player"></param>
    public void UpdateAudio(CustomAudioPlayer player)
    {
        AdjustAudioSource(player.audioSource, player.soundClass);
    }
    
    public void PauseAllAudio()
    {
        
        foreach(CustomAudioPlayer player in AllPlayersInScene)
        {
            print(AllPlayersInScene.Length);
            if (!player.audioSource.isPlaying) break;
            player.wasPausedByESC = true;
            player.audioSource.Pause();
            print(player.soundClass.name + " was paused");
        }
    }
    public void UnpauseAllAudio()
    {
        foreach(CustomAudioPlayer player in AllPlayersInScene)
        {
            if (player.wasPausedByESC)
            {
                player.audioSource.UnPause();
            }
        }
    }

    #region Fade
    #region Cross
    /// <summary>
    /// Most low level call of CrossFade. Allows for specifications on fade time.
    /// </summary>
    /// <param name="In"></param>
    /// <param name="Out"></param>
    /// <param name="fadeTime"></param>
    public void CrossFade(CustomAudioPlayer In,CustomAudioPlayer Out,float fadeTime)
    {
        FadeIn(In, fadeTime);
        FadeOut(Out, fadeTime);
    }

    /// <summary>
    /// CrossFade call that uses defaultFadeTime as the fade timer
    /// </summary>
    /// <param name="In"></param>
    /// <param name="Out"></param>
    public void CrossFade(CustomAudioPlayer In, CustomAudioPlayer Out)
    {
        CrossFade(In, Out, defaultFadeTime);
    }
    #endregion
    #region In
    #region InMethods
    /// <summary>
    /// Most low level call of FadeIn. Allows for specifications on fade in time.
    /// </summary>
    /// <param name="In"></param>
    /// <param name="Out"></param>
    /// <param name="fadeTime"></param>
    public void FadeIn(CustomAudioPlayer In,float fadeTime,bool fromCurrentVol)
    {
        Fade(In, fadeTime,fromCurrentVol,true);
    }

    public void FadeIn(CustomAudioPlayer In, float fadeTime)
    {
        Fade(In, fadeTime,false,true);
    }

    /// <summary>
    /// Fade call that uses defaultFadeTime as the fade timer
    /// </summary>
    /// <param name="In"></param>
    /// <param name="Out"></param>
    public void FadeIn(CustomAudioPlayer In)
    {
        FadeIn(In, defaultFadeTime);
    }

    /// <summary>
    /// Fade call that uses defaultFadeTime as the fade timer
    /// </summary>
    /// <param name="In"></param>
    /// <param name="Out"></param>
    public void FadeIn(CustomAudioPlayer In,bool fromCurrentVol)
    {
        FadeIn(In, defaultFadeTime, fromCurrentVol);
    }

    #endregion


    private IEnumerator FadeInEnum(CustomAudioPlayer In,float fadeTime,bool fromCurrentVol)
    {

        //Creates variables outside of loop
        bool finished = false;
        float timeTaken = 0;
        float startVol = fromCurrentVol ? In.audioSource.volume : 0;
        //If the audio isn't already playing just plays it
        if (!In.audioSource.isPlaying) In.audioSource.Play();

        //This runs every frame thanks to IEnumerator and the yield return null at the end.
        //It Lerps the audio up to its expected level based on modifiers over the specified time
        //Then when the volume has reached expected levels, finishes
        while (!finished)
        {
            In.audioSource.volume = Mathf.Lerp(startVol, SoundTypeVolume(In.soundClass), (timeTaken / fadeTime));
            if (timeTaken >= fadeTime)
            {
                finished = true;
            }
            timeTaken += Time.deltaTime;
            yield return null;
        }
        overtimeEffects.Remove(In);
    }
    #endregion
    #region Out
    /// <summary>
    /// Most low level call of FadeOut. Allows for specifications on fade out time.
    /// </summary>
    /// <param name="In"></param>
    /// <param name="Out"></param>
    /// <param name="fadeTime"></param>
    public void FadeOut(CustomAudioPlayer Out, float fadeTime)
    {
        Fade(Out, fadeTime, false,false);
    }

    /// <summary>
    /// FadeOut call that uses defaultFadeTime as the fade timer
    /// </summary>
    /// <param name="In"></param>
    /// <param name="Out"></param>
    public void FadeOut(CustomAudioPlayer Out)
    {
        FadeOut(Out, defaultFadeTime);
    }
    /// <summary>
    /// FadeOut IEnumerator to allow for actions overtime. Not to be called directly, although it technically breaks nothing if done.
    /// Its just more annoying than calling a regular method :)
    /// </summary>
    /// <param name="In"></param>
    /// <param name="Out"></param>
    /// <param name="fadeTime"></param>
    /// <returns></returns>
    private IEnumerator FadeOutEnum(CustomAudioPlayer Out, float fadeTime)
    {
        //Creates variables outside of loop
        bool finished = false;
        float timeTaken = 0;
        float outStartVol = Out.audioSource.volume;

        //This runs every frame thanks to IEnumerator and the yield return null at the end.
        //It lerps between the current audio's level down to 0 over the specified time
        //Then when the volume has reached expected levels, finishes
        while (!finished)
        {
            if (!Out) yield break;
            Out.audioSource.volume = Mathf.Lerp(outStartVol, 0, (timeTaken / fadeTime));
            if (timeTaken >= fadeTime)
            {
                finished = true;
                StopAudio(Out);
            }
            timeTaken += Time.deltaTime;
            yield return null;
        }
        overtimeEffects.Remove(Out);
    }
    #endregion

    private void Fade(CustomAudioPlayer x, float fadeTime, bool fromCurrentVol, bool In)
    {
        if (x == null) return;
        if (overtimeEffects.Keys.Contains(x))
        {
            StopOvertimeEffect(x);
        }

        if (In) overtimeEffects.Add(x, StartCoroutine(FadeInEnum(x, fadeTime,fromCurrentVol)));
        else overtimeEffects.Add(x, StartCoroutine(FadeOutEnum(x, fadeTime)));
    }

    #endregion

    public void StopOvertimeEffect(CustomAudioPlayer player)
    {
        if (!overtimeEffects.ContainsKey(player)) return;
        Coroutine toStop = overtimeEffects[player];
        overtimeEffects.Remove(player);
        StopCoroutine(toStop);
    }

    #region Tests
    public SoundType debugLoop;
    [ContextMenu("DebugOptions/StopAllAudio")]
    public void DebugStopAudio()
    {
        StopAllAudio();
    }

    [ContextMenu("SoundScape/AllSoundsButMusic")]
    public void AllSoundsInARowExcMusic()
    {
        string soundsToPlay = string.Empty;
        foreach (Sound s in sounds)
        {
            if (s.type != SoundType.Music && !s.loop) soundsToPlay += s.name + ",";
        }
        PlayInSequence(soundsToPlay);
    }

    [ContextMenu("SoundScape/DebugIndex")]
    public void PlayDebugIndex()
    {
        if (debugIndexToPlay <= 0 || debugIndexToPlay > sounds.Length) return;
        Play(sounds[debugIndexToPlay-1].name);
    }
    [ContextMenu("SoundScape/DebugName")]
    public void PlayDebugName()
    {
        Play(debugNameToPlay);
    }

    [ContextMenu("SoundScape/DebugType")]
    public void PlayAllOfDebugType()
    {
        string play = string.Empty;
        foreach(Sound s in sounds)
        {
            if (s.type == debugLoop && !s.loop)
            {
                play += s.name;
                play+= ",";
            }
        }
        PlayInSequence(play);
    }

    [ContextMenu("DebugOptions/PauseAllAudio")]
    public void PauseAudio()
    {
        PauseAllAudio();
    }
    [ContextMenu("DebugOptions/UnPauseAllAudio")]
    public void UnPauseAudio()
    {
        UnpauseAllAudio();
    }
    #endregion
}

