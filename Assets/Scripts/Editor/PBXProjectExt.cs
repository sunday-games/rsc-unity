#if UNITY_IOS
using UnityEditor.iOS.Xcode;

public static class PBXProjectExt
{
    /// The library must be specified with the '.dylib' extension
    public static void AddDynamicLibraryToProject(this PBXProject project, string targetGuid, string library)
    {
        string fileGuid = project.AddFile("usr/lib/" + library, "Frameworks/" + library, PBXSourceTree.Sdk);
        project.AddFileToBuild(targetGuid, fileGuid);
    }

    public static bool ContainsKey(this PlistDocument plist, string key)
    {
        return plist.root.values.ContainsKey(key);
    }
}
#endif