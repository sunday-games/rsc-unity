using UnityEngine;
using System.Collections;
using DG.Tweening;

namespace SG.RSC
{
    public class Music : MonoBehaviour
    {
        [Range(0, 1)]
        public float volumeNormal = 0.6f;

        public AudioSource menu;
        public AudioSource game;
        public AudioSource result;
        public AudioClip[] gameClips;

        bool _ON;
        public bool ON
        {
            get { return _ON; }
            set
            {
                _ON = value;
                PlayerPrefs.SetString("music", _ON ? "on" : "off");
            }
        }

        void Start()
        {
            _ON = PlayerPrefs.GetString("music", "on") == "on";

            // Analytic.Event("Options", "Music", ON ? "ON" : "OFF");
        }

        bool firstGameClipPlayed = false;
        AudioSource current;
        public void Switch(AudioSource next, float fadeTime = 0)
        {
            if (!ON || next == current) return;

            if (next == game)
            {
                game.clip = firstGameClipPlayed ? gameClips[Random.Range(0, gameClips.Length)] : gameClips[0];
                firstGameClipPlayed = true;
            }

            if (current == null)
            {
                next.volume = volumeNormal;
                next.Play();

                current = next;
                return;
            }

            if (fadeTime > 0)
                iTween.AudioTo(current.gameObject, iTween.Hash("volume", 0, "time", fadeTime,
                    "oncomplete", "Set", "oncompletetarget", gameObject, "oncompleteparams", next));
            else
                Set(next);
        }
        void Set(AudioSource next)
        {
            if (current != null) current.Stop();

            next.volume = volumeNormal;
            next.Play();

            current = next;
        }

        public void SetVolume(float volume, float fadeTime = 0)
        {
            if (!ON || current == null) return;

            if (fadeTime > 0) iTween.AudioTo(current.gameObject, volume, 1, fadeTime);
            else current.volume = volume;
        }

        public void TurnOff(float fadeTime = 0)
        {
            if (current != null)
            {
                if (fadeTime > 0) current.DOFade(0, fadeTime).OnComplete(() => Stop());
                else Stop();
            }
        }

        void Stop()
        {
            current.Stop();
            current.volume = volumeNormal;
            current = null;
        }

    }
}