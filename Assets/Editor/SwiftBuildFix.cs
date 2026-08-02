using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;

public class SwiftBuildFix
{
   
    public static void OnPostProcessBuild(BuildTarget buildTarget, string path)
    {
        if (buildTarget!= BuildTarget.iOS) return;

        string projPath = PBXProject.GetPBXProjectPath(path);
        PBXProject proj = new PBXProject();
        proj.ReadFromFile(projPath);

        string targetGuid = proj.GetUnityFrameworkTargetGuid();

        proj.SetBuildProperty(targetGuid, "ALWAYS_EMBED_SWIFT_STANDARD_LIBRARIES", "NO");

        string swiftFilePath = Path.Combine(path, "SwiftFix.swift");
        File.WriteAllText(swiftFilePath, "import Foundation");

        string fileGuid = proj.AddFile(swiftFilePath, "SwiftFix.swift", PBXSourceTree.Source);
        proj.AddFileToBuild(targetGuid, fileGuid);

        proj.WriteToFile(projPath);
        
        Debug.Log("SwiftBuildFix: Correção de Swift aplicada ao UnityFramework.");
    }
}
