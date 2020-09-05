#if UNITY_IOS
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;
using UnityEngine;
using System.IO;
using System;
using System.Linq;
using System.Collections.Generic;

public static class PostprocessIOS
{
    [PostProcessBuild(512)]
    public static void OnPostProcessBuild(BuildTarget buildTarget, string projectPath)
    {
        if (buildTarget == BuildTarget.iOS)
        {
            var pbxprojPath = PBXProject.GetPBXProjectPath(projectPath);

            UpdatePlist(plistPath: Path.Combine(projectPath, "Info.plist"));


            // Чета не работает
            //var project = new PBXProject();
            //project.ReadFromString(File.ReadAllText(pbxprojPath));
            //UpdateProperties(project, pbxprojPath);



            //#if UNITY_5
            //            // MakeUnity5Adjustments
            //            try
            //            {
            //                // Disable arc for FbUnityInterface.mm
            //                string fileGuid = project.FindFileGuidByProjectPath("Facebook/FbUnityInterface.mm");
            //                if (fileGuid == null) Debug.LogError("FbUnityInterface.mm not found!");

            //                project.SetCompileFlagsForFile(target, fileGuid, new List<string> { "-fno-objc-arc" });

            //                File.WriteAllText(pbxprojPath, project.WriteToString());
            //            }
            //            catch (Exception e)
            //            {
            //                Debug.LogException(e);
            //                Debug.LogError("Failed to make Unity 5 Adjustments.");
            //            }
            //#endif
        }
    }

    static void UpdateProperties(PBXProject project, string pbxprojPath)
    {
        string target = project.TargetGuidByName(PBXProject.GetUnityTargetName());

        project.SetBuildProperty(target, "ENABLE_BITCODE", "NO");

        File.WriteAllText(pbxprojPath, project.WriteToString());
    }

    static void UpdatePlist(string plistPath)
    {
        Debug.Log("Info.plist - Updating...");

        try
        {
            var plist = new PlistDocument();
            plist.ReadFromString(File.ReadAllText(plistPath));


            plist.root.SetString("NSCalendarsUsageDescription", "Advertising");
            plist.root.SetString("NSBluetoothPeripheralUsageDescription", "Advertising");
            plist.root.SetString("NSPhotoLibraryUsageDescription", "Game Replay Saving");
            plist.root.SetString("NSCameraUsageDescription", "Game Replay Recording");


            // plist.root.CreateDict("NSAppTransportSecurity").SetBoolean("NSAllowsArbitraryLoads", true);


            // WhitelistApps(plistInfoFile);


            File.WriteAllText(plistPath, plist.WriteToString());

            Debug.Log("Info.plist - Successfully updated");
        }
        catch (Exception e)
        {
            Debug.LogException(e);
            Debug.LogError("Info.plist - Failed to update");
        }
    }

    //static void WhitelistApps(PlistDocument plistInfoFile)
    //{
    //    const string LSApplicationQueriesSchemes = "LSApplicationQueriesSchemes";
    //    string[] fbApps =
    //    {
    //            "fbapi",
    //            "fbapi20130214",
    //            "fbapi20130410",
    //            "fbapi20130702",
    //            "fbapi20131010",
    //            "fbapi20131219",
    //            "fbapi20140410",
    //            "fbapi20140116",
    //            "fbapi20150313",
    //            "fbapi20150629",
    //            "fbauth",
    //            "fbauth2",
    //            "fb-messenger-api20140430",
    //            "fb-messenger-api",
    //            "fbshareextension"
    //        };

    //    string[] otherApps = { "kik-share", "kakaolink", "line", "whatsapp" };

    //    var appsArray = plistInfoFile.root.CreateArray(LSApplicationQueriesSchemes);
    //    fbApps.ToList().ForEach(appsArray.AddString);
    //    otherApps.ToList().ForEach(appsArray.AddString);
    //}
}
#endif