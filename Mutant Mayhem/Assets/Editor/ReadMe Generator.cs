#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using System.IO;

public class ReadmeGenerator : IPostprocessBuildWithReport
{
    public int callbackOrder => 0;

    public void OnPostprocessBuild(BuildReport report)
    {
        // Get the version number from the Player Settings
        string version = PlayerSettings.bundleVersion;

        // Define the path for the README file in the build directory
        string readmePath = Path.Combine(report.summary.outputPath, "README.txt");

        // Content for the README file, including the version number
        string content = $"Welcome to Mutant Mayhem!\n\n" +
                         $"Version: {version}\n" +
                         "Support: KamJamGames@Gmail.com\n\n" +
                         "Enjoy the game!";

        // Write the content to the README file
        //File.WriteAllText(readmePath, content);
    }
}
#endif
