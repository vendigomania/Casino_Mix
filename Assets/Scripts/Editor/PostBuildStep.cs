using UnityEditor;
using UnityEditor.Callbacks;
#if UNITY_IOS
using UnityEditor.iOS.Xcode;
#endif
using System.IO;

public class PostBuildStep
{
    // Set the IDFA request description:
    const string k_TrackingDescription = "We use your information in order to enhance your game experience and measuring the performance of our game.";

    [PostProcessBuild(0)]
    public static void OnPostProcessBuild(BuildTarget buildTarget, string pathToXcode)
    {
        if (buildTarget == BuildTarget.iOS)
        {
            AddPListValues(pathToXcode);
        }
    }

    // Implement a function to read and write values to the plist file:
    static void AddPListValues(string pathToXcode)
    {
        // Retrieve the plist file from the Xcode project directory:
        string plistPath = pathToXcode + "/Info.plist";
        PlistDocument plistObj = new PlistDocument();


        // Read the values from the plist file:
        plistObj.ReadFromString(File.ReadAllText(plistPath));

        // Set values from the root object:
        PlistElementDict plistRoot = plistObj.root;

        // Set the description key-value in the plist:
        plistRoot.SetString("NSUserTrackingUsageDescription", k_TrackingDescription);

        // Save changes to the plist:
        File.WriteAllText(plistPath, plistObj.WriteToString());
    }
}