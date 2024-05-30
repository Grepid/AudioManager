using UnityEngine.Audio;
using System;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Linq;

//TO-DO

//Change everything to Static methods referencing the instance
//Add Instance Null checks to the start of every method
//Move from instantiate every call to a grab from a pool
//Add every form of action you can take on an AudioPlayer to the AudioPlayer class passing "this" as the AudioPlayer parameter
//Allow binding of multiple string methods to the end of audio player \\-||


namespace AudioSystem
{
    public class AudioManager : MonoBehaviour
    {

        [Tooltip("The sole singleton instance of this class")]
        private static AudioManager s_instance;
        public static AudioManager Instance
        {
            get
            {
                if (s_instance == null)
                {
                    Debug.LogError("No Instance Found");
                    return null;
                }
                return s_instance;
            }
        }


        [Tooltip("The name that the GameObject created when playing a sound will be given for identification purposes")]
        public string TempSoundSourceIdentifier;




        [Tooltip("A dictionary of Sound Type to Volume Level for that type.")]
        //The cool thing about this new way of doing it, is if you want to add a new audio type with
        //a seperate audio level you can adjust, all you need to do is add the type to the Enum in the Sound class
        //and it will be perfectly synced up with the system. Then just create its own settings slider and done
        public static Dictionary<SoundType, float> s_Volumes
        {
            get; 
            private set;
        }

        [Tooltip("The list of sounds the game has to be able to play")]
        [NonReorderable]
        public Sound[] Sounds;

        [HideInInspector]
        public static AudioPlayer currentMusic;

        public static Dictionary<AudioPlayer, Coroutine> overtimeEffects
        {
            get;
            private set;
        }

        public static List<AudioPlayer> AllPlayersInScene
        {
            get;
            private set;
        }

        public const float defaultFadeTime = 1;

        public static List<AudioPlayer> AllActiveAudio
        {
            get
            {
                return AllPlayersInScene.FindAll(x => x.AudioSource.isPlaying);
            }
        }


        #region Catches

        /// <summary>
        /// Returns true if all is valid.
        /// </summary>
        private static bool FullValidCheck
        {
            get
            {
                if (InstanceNull) return false;
                if (SoundsEmpty) return false;

                return true;
            }
        }

        /// <summary>
        /// Returns true if there is no active Instance
        /// </summary>
        private static bool InstanceNull
        {
            get
            {
                if (s_instance == null)
                {
                    Debug.LogError("AudioManager has no Instance");
                    return true;
                }
                return false;
            }
            
        }

        /// <summary>
        /// Returns true if there are no sounds in the system
        /// </summary>
        private static bool SoundsEmpty
        {
            get
            {
                if (Instance.Sounds.Length <= 0)
                {
                    Debug.LogError("AudioManager cannot have 0 sounds");
                    return true;
                }
                return false;
            }
        }

        #endregion


        private void Awake()
        {
            if (s_instance == null) s_instance = this;
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
            overtimeEffects = new Dictionary<AudioPlayer, Coroutine>();
            AllPlayersInScene = new List<AudioPlayer>();
            SetupVolumes();
        }

        /// <summary>
        /// This sets up a dictionary of SoundType to Float which can be referenced from anywhere to access the volume level for a given
        /// sound type. This method makes it really easy to associate the type of sound to a volume level.
        /// </summary>
        private static void SetupVolumes()
        {
            //Creates the empty dictionary
            s_Volumes = new Dictionary<SoundType, float>();
            //Loops through every type there is in the Enum
            foreach (SoundType type in Enum.GetValues(typeof(SoundType)))
            {
                //Adds the type to the dictionary, and gives it a starting volume of 1 (100%)
                s_Volumes.Add(type, 1);
            }
            // Will set the audio levels to what it was saved at afterwards. If nothing was found, will just leave it at 100%
            //TODO though
            LoadSavedAudioLevels();
        }
        private static void LoadSavedAudioLevels()
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
        public static AudioPlayer DefaultPlay(string name)
        {
            if (!FullValidCheck) return null;
            GameObject focus = new GameObject();
            Sound s = Array.Find(Instance.Sounds, sound => sound.name == name);
            if (s == null)
            {
                Debug.LogWarning("Sound not found");
                s = new Sound();
            }

            focus.name = Instance.TempSoundSourceIdentifier;

            AudioSource audSource = focus.AddComponent<AudioSource>();
            AdjustAudioSource(audSource, s);

            AudioPlayer player = focus.AddComponent<AudioPlayer>();
            player.AudioSource = audSource;
            player.SoundClass = s;



            audSource.volume = SoundTypeVolume(s.type);

            audSource.Play();

            //if (!audSource.loop) StartCoroutine(DestroyUsedAudio(focus));

            AllPlayersInScene.Add(player);

            return player;
        }

        /// <summary>
        /// Will use the DefaultPlay() Function to create a GameObject and CustomAudioPlayer then will
        /// set its position to the Camera's position. This function will effectively act as UI Sound whether
        /// or not the Sound played is set to 2D or 3D Audio
        /// </summary>
        /// <param name="name"></param>
        /// <returns>The associated CustomAudioPlayer with the Sound Played</returns>
        public static AudioPlayer Play(string name)
        {
            AudioPlayer player;
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
        public static AudioPlayer Play(string name, GameObject goOrigin)
        {
            AudioPlayer player;
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
        public static AudioPlayer Play(string name, Vector3 posOrigin)
        {
            AudioPlayer player;
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
        public static AudioPlayer[] PlayInSequence(string[] sounds)
        {
            AudioPlayer[] players = new AudioPlayer[sounds.Length];
            int i = 0;
            foreach (string sound in sounds)
            {
                string cleanedSound = sound.Replace(" ", "");
                players[i] = Play(cleanedSound);
                players[i].AudioSource.Pause();
                i++;
            }
            Instance.StartCoroutine(IPlayInSequence(players));

            return players;
        }

        /// <summary>
        /// Plays sounds 1 after eachother when given a string with names of sounds seperated with commas
        /// </summary>
        /// <param name="sounds"></param>
        /// <returns></returns>
        public static AudioPlayer[] PlayInSequence(string sounds)
        {
            string[] soundsSegmented = sounds.Split(',', StringSplitOptions.RemoveEmptyEntries);

            return PlayInSequence(soundsSegmented);
        }
        private static IEnumerator IPlayInSequence(AudioPlayer[] players)
        {
            //print("About to play " + players.Length + " audio tracks");
            foreach (AudioPlayer player in players)
            {
                if (player == null) continue;
                player.AudioSource.Play();
                //print("Now playing " + player.soundClass.name);
                while (true)
                {
                    if (player.AudioSource == null) break;
                    if (player.AudioSource.time >= player.AudioSource.clip.length) break;
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
        public static float SoundTypeVolume(SoundType type)
        {
            float v = s_Volumes[SoundType.Master] * s_Volumes[type];
            return v;
        }

        public static void SetTypeVolume(SoundType type, float volume)
        {
            s_Volumes[type] = volume;
        }

        /// <summary>
        /// Handles stopping audio and then handling it's removal from the scene
        /// </summary>
        /// <param name="player"></param>
        public static void StopAudio(AudioPlayer player)
        {
            if (player == null) return;
            if (overtimeEffects.Keys.Contains(player)) StopOvertimeEffect(player);
            player.AudioSource.Stop();
            Destroy(player.gameObject);
        }

        /// <summary>
        /// Stops every playing audio of the given type
        /// </summary>
        /// <param name="soundType"></param>
        public static void StopAllAudioOfType(SoundType soundType)
        {
            foreach (AudioPlayer ap in FindObjectsOfType<AudioPlayer>())
            {
                if (ap.SoundClass.type != soundType) return;
                StopAudio(ap);
            }
        }

        public static void StopAllAudio()
        {
            foreach (AudioPlayer ap in FindObjectsOfType<AudioPlayer>())
            {
                StopAudio(ap);
            }
        }

        /// <summary>
        /// Will adjust the given AudioSource to have the properties the Sound class it is given has.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="sound"></param>
        public static void AdjustAudioSource(AudioSource source, Sound sound)
        {
            source.clip = sound.clip;

            source.volume = SoundTypeVolume(sound.type);
            source.pitch = sound.pitch;

            source.loop = sound.loop;

            source.spatialBlend = sound.spatialBlend;

            source.minDistance = sound.minDistance;
            source.maxDistance = sound.maxDistance;

            source.dopplerLevel = sound.dopplerLevel;
        }

        public static void SetAudioLevel(SoundType type, float level)
        {
            s_Volumes[type] = level;
            UpdateAllAudio();
        }

        /// <summary>
        /// Will update all audio clips in the scene to have the most up to date settings.
        /// Typically used for when Volume changes
        /// </summary>
        public static void UpdateAllAudio()
        {

            foreach (AudioPlayer player in AllPlayersInScene)
            {
                UpdateAudio(player);
            }
        }

        /// <summary>
        /// Updates a single piece of playing audio. Typically just called from the UpdateAllAudio() function,
        /// however can be called if needed for only 1 piece of audio
        /// </summary>
        /// <param name="player"></param>
        public static void UpdateAudio(AudioPlayer player)
        {
            AdjustAudioSource(player.AudioSource, player.SoundClass);
        }

        public static void PauseAllAudio()
        {

            foreach (AudioPlayer player in AllPlayersInScene)
            {
                if (!player.AudioSource.isPlaying) break;
                player.wasPausedByESC = true;
                player.AudioSource.Pause();
                //print(player.SoundClass.name + " was paused");
            }
        }
        public static void UnpauseAllAudio()
        {
            foreach (AudioPlayer player in AllPlayersInScene)
            {
                if (player.wasPausedByESC)
                {
                    player.AudioSource.UnPause();
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
        public static void CrossFade(AudioPlayer In, AudioPlayer Out, float fadeTime)
        {
            FadeIn(In, fadeTime);
            FadeOut(Out, fadeTime);
        }

        /// <summary>
        /// CrossFade call that uses defaultFadeTime as the fade timer
        /// </summary>
        /// <param name="In"></param>
        /// <param name="Out"></param>
        public static void CrossFade(AudioPlayer In, AudioPlayer Out)
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
        public static void FadeIn(AudioPlayer In, float fadeTime, bool fromCurrentVol)
        {
            Fade(In, fadeTime, fromCurrentVol, true);
        }

        public static void FadeIn(AudioPlayer In, float fadeTime)
        {
            Fade(In, fadeTime, false, true);
        }

        /// <summary>
        /// Fade call that uses defaultFadeTime as the fade timer
        /// </summary>
        /// <param name="In"></param>
        /// <param name="Out"></param>
        public static void FadeIn(AudioPlayer In)
        {
            FadeIn(In, defaultFadeTime);
        }

        /// <summary>
        /// Fade call that uses defaultFadeTime as the fade timer
        /// </summary>
        /// <param name="In"></param>
        /// <param name="Out"></param>
        public static void FadeIn(AudioPlayer In, bool fromCurrentVol)
        {
            FadeIn(In, defaultFadeTime, fromCurrentVol);
        }

        #endregion


        private static IEnumerator FadeInEnum(AudioPlayer In, float fadeTime, bool fromCurrentVol)
        {

            //Creates variables outside of loop
            bool finished = false;
            float timeTaken = 0;
            float startVol = fromCurrentVol ? In.AudioSource.volume : 0;
            //If the audio isn't already playing just plays it
            if (!In.AudioSource.isPlaying) In.AudioSource.Play();

            //This runs every frame thanks to IEnumerator and the yield return null at the end.
            //It Lerps the audio up to its expected level based on modifiers over the specified time
            //Then when the volume has reached expected levels, finishes
            while (!finished)
            {
                In.AudioSource.volume = Mathf.Lerp(startVol, SoundTypeVolume(In.SoundClass.type), (timeTaken / fadeTime));
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
        public static void FadeOut(AudioPlayer Out, float fadeTime)
        {
            Fade(Out, fadeTime, false, false);
        }

        /// <summary>
        /// FadeOut call that uses defaultFadeTime as the fade timer
        /// </summary>
        /// <param name="In"></param>
        /// <param name="Out"></param>
        public static void FadeOut(AudioPlayer Out)
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
        private static IEnumerator FadeOutEnum(AudioPlayer Out, float fadeTime)
        {
            //Creates variables outside of loop
            bool finished = false;
            float timeTaken = 0;
            float outStartVol = Out.AudioSource.volume;

            //This runs every frame thanks to IEnumerator and the yield return null at the end.
            //It lerps between the current audio's level down to 0 over the specified time
            //Then when the volume has reached expected levels, finishes
            while (!finished)
            {
                if (!Out) yield break;
                Out.AudioSource.volume = Mathf.Lerp(outStartVol, 0, (timeTaken / fadeTime));
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

        private static void Fade(AudioPlayer x, float fadeTime, bool fromCurrentVol, bool In)
        {
            if (x == null) return;
            if (overtimeEffects.Keys.Contains(x))
            {
                StopOvertimeEffect(x);
            }

            if (In) overtimeEffects.Add(x, Instance.StartCoroutine(FadeInEnum(x, fadeTime, fromCurrentVol)));
            else overtimeEffects.Add(x, Instance.StartCoroutine(FadeOutEnum(x, fadeTime)));
        }

        #endregion

        public static void StopOvertimeEffect(AudioPlayer player)
        {
            if (!overtimeEffects.ContainsKey(player)) return;
            Coroutine toStop = overtimeEffects[player];
            overtimeEffects.Remove(player);
            Instance.StopCoroutine(toStop);
        }
    }
}

