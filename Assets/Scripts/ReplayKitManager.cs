using UnityEngine;

#if REPLAY_KIT
using Prime31;
#endif

public class ReplayKitManager : Core
{
    static bool _ON;
    public static bool ON
    {
        get { return _ON; }
        set
        {
            _ON = value;
            PlayerPrefs.SetString("rec", _ON ? "on" : "off");
        }
    }

    void Start()
    {
        _ON = PlayerPrefs.GetString("rec", "off") == "on";
    }

    public static bool isRecAvailable
    {
        get
        {
            return
#if UNITY_IOS && REPLAY_KIT
            ReplayKit.isReplayKitAvailable();
#else
            false;
#endif
        }
    }

    public static bool isCurrentlyRecording
    {
        get
        {
            return
#if UNITY_IOS && REPLAY_KIT
            ReplayKit.isCurrentlyRecording();
#else
            false;
#endif
        }
    }

    public static bool isExistRec = false;

    public static void RecStart()
    {
#if UNITY_IOS && REPLAY_KIT
        ReplayKit.startRecording(false);
#endif
    }

    public static void RecStop()
    {
#if UNITY_IOS && REPLAY_KIT
        ReplayKit.stopRecording(false);
        isExistRec = true;
#endif
    }

    public static void RecDiscard()
    {
#if UNITY_IOS && REPLAY_KIT
        ReplayKit.discardRecording();
        isExistRec = false;
#endif
    }

    public static void RecShowPreview()
    {
#if UNITY_IOS && REPLAY_KIT
        ReplayKit.showPreviewViewController();
#endif
    }
}
