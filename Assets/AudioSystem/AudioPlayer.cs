using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AudioSystem
{
    //NEED TO COMMENT AND ORGANISE
    public class AudioPlayer : MonoBehaviour
    {
        public bool followTarget;
        public GameObject target;
        public bool wasPausedByESC;
        private List<KeyValuePair<MonoBehaviour,string>> bindActions = new List<KeyValuePair<MonoBehaviour,string>>();
        public AudioSource AudioSource
        {
            get;
            private set;
        }
        public Sound SoundClass
        {
            get;
            private set;
        }
        public void Initialise(AudioSource source,Sound sound)
        {
            AudioSource = source;
            SoundClass = sound;
        }

        private void Update()
        {
            if (!AudioSource || !AudioSource.clip){ Stop(); return; }
            if(AudioSource.time >= AudioSource.clip.length && !SoundClass.loop)
            {
                foreach(KeyValuePair<MonoBehaviour, string> pair in bindActions)
                {
                    pair.Key.SendMessage(pair.Value);
                }
                Stop();
            }
            if (followTarget)
            {
                transform.position = target.transform.position;
            }
        }
        public void BindToAudioEnd(MonoBehaviour target, string methodName)
        {
            bindActions.Add(new KeyValuePair<MonoBehaviour,string>(target,methodName));
        }

        public void Pause()
        {
            AudioSource.Pause();
        }
        public void UnPause()
        {
            AudioSource.UnPause();
        }

        public void Stop()
        {
            AudioManager.StopAudio(this);
        } 
        public void UpdateAudio()
        {
            AudioManager.UpdateAudio(this);
        }
        public void FadeOut(float time)
        {
            AudioManager.FadeOut(this,time);
        }
        public void FadeOut()
        {
            AudioManager.FadeOut(this);
        }

        public void FadeIn()
        {
            AudioManager.FadeIn(this);
        }
        public void FadeIn(float time)
        {
            AudioManager.FadeIn(this,time);
        }
    }
}
