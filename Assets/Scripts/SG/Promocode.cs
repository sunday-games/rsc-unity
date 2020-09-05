using System.Collections.Generic;
using CodeStage.AntiCheat.ObscuredTypes;

namespace SG
{
    public class Promocode
    {
        public long id;
        public string code;
        public Dictionary<string, object> action;

        public bool isAlreadyActivated { get { return ObscuredPrefs.HasKey(id.ToString()); } }

        public Promocode() { }

        public Promocode(Dictionary<string, object> data)
        {
            id = (long)data["id"];
            code = (string)data["code"];
            action = Json.Deserialize((string)data["action"]) as Dictionary<string, object>;
        }

        public virtual void Activate()
        {
            ObscuredPrefs.SetString(id.ToString(), "activated");

            Analytic.EventPropertiesImportant("Promocode",
                new Dictionary<string, object>() { { "code", code }, { "action", Json.Serialize(action) } });
        }

        public enum SpecialCodes { RELOG0, RELOG1 }
        public static string ActivateIfSpecial(string code)
        {
            if (code == SpecialCodes.RELOG0.ToString())
            {
                RemoteLogger.Disable();
                return "Remote Logger OFF";
            }
            else if (code == SpecialCodes.RELOG1.ToString())
            {
                RemoteLogger.Enable();
                return "Remote Logger ON";
            }

            return null;
        }
    }
}