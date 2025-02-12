using UnityEngine;

namespace SG
{
    public class Music : MonoBehaviour
    {
        public AudioSource Source;

        private void Start()
        {
            SetVolume(Configurator.Instance.Settings.musicVolume);
        }

        public void SetVolume(float volume)
        {
            if (Source == null)
                return;

            Source.volume = volume;
        }
    }
}