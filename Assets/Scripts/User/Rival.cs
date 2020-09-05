using UnityEngine;
using System.Collections;

public class Rival
{
    public static string ID_ME = "me";

    public string id;
    public string name;
    public int level;
    public string facebookID;
    public long record;

    public Texture2D userPicTexture = null;
    public Rect userPicTextureRect = Core.zeroRect;

    public string userPicUrl;
    public int width;
    public int height;

    public Rival(string id, string name, int level, string facebookID, long record, string userPicUrl = null, int width = 130, int height = 130)
    {
        this.id = id;
        this.name = name;
        this.level = level;
        this.facebookID = facebookID;
        this.record = record;
        this.userPicUrl = userPicUrl;
        this.width = width;
        this.height = height;
    }

    public bool isPlayer { get { return id == Core.user.id || id == ID_ME || facebookID == Core.user.facebookId; } }
}