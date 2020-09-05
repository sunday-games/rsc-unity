using UnityEngine;
using System.Collections;

public class Sound : Core
{
    public AudioSource button;
    public AudioSource getCoin;
    public AudioSource getCoins;
    public AudioSource luckyWheel;
    public AudioSource getExperience;
    public AudioSource popupAnimationMoveUpDown;
    public AudioSource winPrize;
    public AudioSource punch;

    public AudioSource voice;
    public Voices voiceClips
    {
        get
        {
            if (Localization.language == SystemLanguage.Russian) return ruVoices;
            else return enVoices;
        }
    }
    public Voices enVoices;
    public Voices ruVoices;
    [System.Serializable]
    public class Voices
    {
        public AudioClip ready;
        public float readyTime;
        public AudioClip set;
        public float setTime;
        public AudioClip cat;
        public float catTime;
        [Space(10)]
        public AudioClip disco;
        [Space(10)]
        public AudioClip[] hurryUp;
        public AudioClip timeIsOut;
        [Space(10)]
        public AudioClip[] combo1;
        public AudioClip[] combo2;
        public AudioClip[] combo3;
        public AudioClip[] combo4;
        public AudioClip[] combo5;
    }

    bool _voiceON;
    public bool voiceON
    {
        get { return _voiceON; }
        set
        {
            _voiceON = value;
            PlayerPrefs.SetString("voice", _voiceON ? "on" : "off");
        }
    }

    bool _ON;
    public bool ON
    {
        get { return _ON; }
        set
        {
            _ON = value;
            PlayerPrefs.SetString("sound", _ON ? "on" : "off");
        }
    }

    void Start()
    {
        _ON = PlayerPrefs.GetString("sound", "on") == "on";
        _voiceON = PlayerPrefs.GetString("voice", "on") == "on";

        // Analytic.Event("Options", "SFX", ON ? "ON" : "OFF");
        // Analytic.Event("Options", "Voice", voiceON ? "ON" : "OFF");
    }

    public void PlayVoice(AudioClip clip)
    {
        if (!_voiceON || clip == null) return;

        if (voice.isPlaying) voice.Stop();

        voice.clip = clip;
        voice.Play();
    }
    public void PlayVoiceCombo(int chain)
    {
        if (balance.voiceCombo[0] > chain) return;

        AudioClip v;
        do
        {
            if (chain < balance.voiceCombo[1]) v = voiceClips.combo1[Random.Range(0, voiceClips.combo1.Length)];
            else if (chain < balance.voiceCombo[2]) v = voiceClips.combo2[Random.Range(0, voiceClips.combo2.Length)];
            else if (chain < balance.voiceCombo[3]) v = voiceClips.combo3[Random.Range(0, voiceClips.combo3.Length)];
            else if (chain < balance.voiceCombo[4]) v = voiceClips.combo4[Random.Range(0, voiceClips.combo4.Length)];
            else v = voiceClips.combo5[Random.Range(0, voiceClips.combo5.Length)];
        }
        while (lastVoice != null && v == lastVoice);

        PlayVoice(v);
        lastVoice = v;
    }
    AudioClip lastVoice = null;

    public void Play(AudioSource source) { if (ON) source.Play(); }
    public void Stop(AudioSource source) { source.Stop(); }

    public void PlaySoundButton() { Play(button); }
}
