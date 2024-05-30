using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AudioSystem
{
    //NEED TO COMMENT AND ORGANISE
    public class AudioPlayer : MonoBehaviour
    {
        bool m_initialised;
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
            if (m_initialised) return;
            AudioSource = source;
            SoundClass = sound;
            m_initialised = true;
        }

        private void Update()
        {
            if (!AudioSource){ Destroy(gameObject); return; }
            if(AudioSource.time >= AudioSource.clip.length && !SoundClass.loop)
            {
                foreach(KeyValuePair<MonoBehaviour, string> pair in bindActions)
                {
                    pair.Key.SendMessage(pair.Value);
                }
                Destroy(gameObject);
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
            print("Test sdfisdf");
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
