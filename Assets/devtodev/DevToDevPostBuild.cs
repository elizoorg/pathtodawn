#if UNITY_IOS
using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;
namespace DevToDev
{
   public class DevToDevPostBuild
   {
       const string APP_TARGET_NAME = "Unity-iPhone";
       [PostProcessBuildAttribute(1)]
       public static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject)
       {
           if (target != BuildTarget.iOS)
           {
               return;
           }
           iOSPostBuild(pathToBuiltProject);
       }
       private static void iOSPostBuild(string projPath)
       {
           string pbxprojPath = projPath + "/Unity-iPhone.xcodeproj/project.pbxproj";
           PBXProject proj = new PBXProject();
           proj.ReadFromString(File.ReadAllText(pbxprojPath));
           string projectGuid = proj.TargetGuidByName(APP_TARGET_NAME);
           proj.AddFrameworkToProject(projectGuid, "AdSupport.framework", true);
           // IOS 14. Xcode 12 required.
           //proj.AddFrameworkToProject(projectGuid, "AppTrackingTransparency.framework", true);
           File.WriteAllText(pbxprojPath, proj.WriteToString());
       }
   }
}
#endif