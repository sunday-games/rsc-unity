using System.Diagnostics;
using System.Threading;

namespace SG
{
    public static class Log
    {
        public static class Tags
        {
            public static readonly string[] ALL = new string[] { INIT, PLAYER, UI, BLOCKCHAIN, ADS, ZONE, STATE, NETWORK, PATHFINDER, SPACE_PATHFINDER, CONTRACTS, BOTS, VIVOX, TASKS };

            public const string DEBUG = "LOG_DEBUG";
            public const string INIT = "LOG_INIT";
            public const string PLAYER = "LOG_PLAYER";
            public const string UI = "LOG_UI";
            public const string BLOCKCHAIN = "LOG_BLOCKCHAIN";
            public const string ADS = "LOG_ADS";
            public const string ZONE = "LOG_ZONE";
            public const string STATE = "LOG_STATE";
            public const string NETWORK = "LOG_NETWORK";
            public const string PATHFINDER = "LOG_PATHFINDER";
            public const string SPACE_PATHFINDER = "LOG_SPACE_PATHFINDER";
            public const string CONTRACTS = "LOG_CONTRACTS";
            public const string BOTS = "LOG_BOTS";
            public const string VIVOX = "LOG_VIVOX";
            public const string TASKS = "LOG_TASKS";
        }

        public static readonly LogInit Init = new LogInit();
        public class LogInit : LogBase
        {
            [Conditional(Tags.INIT)] public void Info(object msg) => UnityEngine.Debug.Log(Format(msg));
            [Conditional(Tags.INIT + "_DEBUG")] public void Debug(object msg) => UnityEngine.Debug.Log(Format(msg));
        }

        public static readonly LogPlayer Player = new LogPlayer();
        public class LogPlayer : LogBase
        {
            [Conditional(Tags.PLAYER)] public void Info(object msg) => UnityEngine.Debug.Log(Format(msg));
            [Conditional(Tags.PLAYER + "_DEBUG")] public void Debug(object msg) => UnityEngine.Debug.Log(Format(msg));
        }

        public static readonly LogUI UI = new LogUI();
        public class LogUI : LogBase
        {
            [Conditional(Tags.UI)] public void Info(object msg) => UnityEngine.Debug.Log(Format(msg));
            [Conditional(Tags.UI + "_DEBUG")] public void Debug(object msg) => UnityEngine.Debug.Log(Format(msg));
        }

        public static readonly LogBlockchain Blockchain = new LogBlockchain();
        public class LogBlockchain : LogBase
        {
            [Conditional(Tags.BLOCKCHAIN)] public void Info(object msg) => UnityEngine.Debug.Log(Format(msg));
            [Conditional(Tags.BLOCKCHAIN + "_DEBUG")] public void Debug(object msg) => UnityEngine.Debug.Log(Format(msg));
        }

        public static readonly LogAds Ads = new LogAds();
        public class LogAds : LogBase
        {
            [Conditional(Tags.ADS)] public void Info(object msg) => UnityEngine.Debug.Log(Format(msg));
            [Conditional(Tags.ADS + "_DEBUG")] public void Debug(object msg) => UnityEngine.Debug.Log(Format(msg));
        }

        public static LogZone Zone = new LogZone();
        public class LogZone : LogBase
        {
            [Conditional(Tags.ZONE)] public void Info(object msg) => UnityEngine.Debug.Log(Format(msg));
            [Conditional(Tags.ZONE + "_DEBUG")] public void Debug(object msg) => UnityEngine.Debug.Log(Format(msg));
        }

        public static readonly LogState State = new LogState();
        public class LogState : LogBase
        {
            [Conditional(Tags.STATE)] public void Info(object msg) => UnityEngine.Debug.Log(Format(msg));
            [Conditional(Tags.STATE + "_DEBUG")] public void Debug(object msg) => UnityEngine.Debug.Log(Format(msg));
        }

        public static readonly LogNetwork Network = new LogNetwork();
        public class LogNetwork : LogBase
        {
            [Conditional(Tags.NETWORK)] public void Info(object msg) => UnityEngine.Debug.Log(Format(msg));
            [Conditional(Tags.NETWORK + "_DEBUG")] public void Debug(object msg) => UnityEngine.Debug.Log(Format(msg));
        }

        public static readonly LogPathfinder Pathfinder = new LogPathfinder();
        public class LogPathfinder : LogBase
        {
            [Conditional(Tags.PATHFINDER)] [UnityEngine.HideInCallstackAttribute] public void Info(object msg) => UnityEngine.Debug.Log(Format(msg));
            [Conditional(Tags.PATHFINDER + "_DEBUG")] [UnityEngine.HideInCallstackAttribute] public void Debug(object msg) => UnityEngine.Debug.Log(Format(msg));
        }

        public static LogSpacePathfinder SpacePathfinder = new LogSpacePathfinder();
        public class LogSpacePathfinder : LogBase
        {
            [Conditional(Tags.SPACE_PATHFINDER)] public void Info(object msg) => UnityEngine.Debug.Log(Format(msg));
            [Conditional(Tags.SPACE_PATHFINDER + "_DEBUG")] public void Debug(object msg) => UnityEngine.Debug.Log(Format(msg));
        }

        public static readonly LogContracts Contracts = new LogContracts();
        public class LogContracts : LogBase
        {
            [Conditional(Tags.CONTRACTS)] public void Info(object msg) => UnityEngine.Debug.Log(Format(msg));
            [Conditional(Tags.CONTRACTS + "_DEBUG")] public void Debug(object msg) => UnityEngine.Debug.Log(Format(msg));
        }

        public static readonly LogBots Bots = new LogBots();
        public class LogBots : LogBase
        {
            [Conditional(Tags.BOTS)] public void Info(object msg) => UnityEngine.Debug.Log(Format(msg));
            [Conditional(Tags.BOTS + "_DEBUG")] public void Debug(object msg) => UnityEngine.Debug.Log(Format(msg));
        }

        public static readonly LogVivox Vivox = new LogVivox();
        public class LogVivox : LogBase
        {
            [Conditional(Tags.VIVOX)] public void Info(object msg) => UnityEngine.Debug.Log(Format(msg));
            [Conditional(Tags.VIVOX + "_DEBUG")] public void Debug(object msg) => UnityEngine.Debug.Log(Format(msg));
        }

        public static readonly LogTasks Tasks = new LogTasks();
        public class LogTasks : LogBase
        {
            [Conditional(Tags.TASKS)] public void Info(object msg) => UnityEngine.Debug.Log(Format(msg));
            [Conditional(Tags.TASKS + "_DEBUG")] public void Debug(object msg) => UnityEngine.Debug.Log(Format(msg));
        }

        public class LogBase
        {
            public void Warning(object msg) => UnityEngine.Debug.LogWarning(Format(msg));
            public void Error(object msg) => UnityEngine.Debug.LogError(Format(msg));
        }

        public static void Info(object msg) => UnityEngine.Debug.Log(Format(msg));
        [Conditional(Tags.DEBUG)] public static void Debug(object msg) => UnityEngine.Debug.Log(Format(msg));
        public static void Warning(object msg) => UnityEngine.Debug.LogWarning(Format(msg));
        public static void Error(object msg) => UnityEngine.Debug.LogError(Format(msg));

        public static bool IsTag(string tag) => Configurator.Instance.LogTags.Contains(tag);

        private static object Format(object msg) =>
#if UNITY_SERVER || UNITY_STANDALONE
            $"{System.DateTime.Now:o} [{Thread.CurrentThread.ManagedThreadId}] {msg}";
#else
            msg;
#endif
    }
}
