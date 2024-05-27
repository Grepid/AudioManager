using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AudioSystem
{
    public class AudioPlayer : MonoBehaviour
    {
        private List<KeyValuePair<MonoBehaviour,string>> bindActions = new List<KeyValuePair<MonoBehaviour,string>>();
        public AudioSource AudioSource;
        public Sound SoundClass;

        public bool wasPausedByESC;
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
    }
}
