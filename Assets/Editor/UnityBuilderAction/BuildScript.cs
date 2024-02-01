using System;
using System.IO;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace UnityBuilderAction
{
    /// <summary>
    /// Script needed when building via GameCI in GitHub Actions. Allows for custom command line arguments.
    /// </summary>
    public static class BuildScript
    {
        private static readonly string Eol = Environment.NewLine;

        public static void BuildGame()
        {
            // Get command line arguments
            Dictionary<string, string> options = GetValidatedOptions();

            BuildTarget buildTarget = (BuildTarget)Enum.Parse(typeof(BuildTarget), options["buildTarget"]);

            // Build player
            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
            buildPlayerOptions.locationPathName = options["customBuildPath"];
            buildPlayerOptions.target = buildTarget;
            // GameCI lacks support for Linux Standalone Server builds, so we have to specify the server build as a subtarget here
            if (buildTarget == BuildTarget.StandaloneLinux64)
            {
                buildPlayerOptions.subtarget = (int) StandaloneBuildSubtarget.Server;
            }
            // If we're building WebGL, disable decompression fallback to avoid an error
            else if (buildTarget == BuildTarget.WebGL)
            {
                PlayerSettings.WebGL.decompressionFallback = false;
            }
            buildPlayerOptions.options = BuildOptions.Development;       

            // Copy Scene List from the Build Settings Window
            string[] sceneList = new string[EditorBuildSettings.scenes.Length];
            for (int i = 0; i < EditorBuildSettings.scenes.Length; i++)
            {
                sceneList[i] = EditorBuildSettings.scenes[i].path;
            }
            buildPlayerOptions.scenes = sceneList;

            // BUild the player
            BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
            
            // Post-build result
            BuildSummary summary = report.summary;
            switch (summary.result)
            {
                case BuildResult.Succeeded:
                    Debug.Log($"Build succeeded! Wrote {summary.totalSize} bytes.");
                    EditorApplication.Exit(0);
                    break;
                case BuildResult.Failed:
                    Debug.Log($"Build failed! Encountered {summary.totalErrors} errors/exceptions.");
                    EditorApplication.Exit(101);
                    break;
                case BuildResult.Cancelled:
                    Debug.Log($"Build cancelled!");
                    EditorApplication.Exit(102);
                    break;
                case BuildResult.Unknown:
                default:
                    Debug.Log($"Build result is unknown! This should not happen.");
                    EditorApplication.Exit(103);
                    break;
            }
        }

        /// <summary>
        /// Retrieves build-specific command line arguments. These are not runtime arguments; they are provided as part of the GitHub Actions workflow.
        /// </summary>
        /// <returns></returns>
        private static Dictionary<string, string> GetValidatedOptions()
        {
            // Parse through the command line arguments given
            ParseCommandLineArguments(out Dictionary<string, string> validatedOptions);

            // Verify the project path was given
            if (!validatedOptions.TryGetValue("projectPath", out string _))
            {
                Console.WriteLine("Missing argument -projectPath");
                EditorApplication.Exit(110);
            }

            // Verify the build target was given
            if (!validatedOptions.TryGetValue("buildTarget", out string buildTarget))
            {
                Console.WriteLine("Missing argument -buildTarget");
                EditorApplication.Exit(120);
            }

            // Verify we can interpret the build target as an enum
            if (!Enum.IsDefined(typeof(BuildTarget), buildTarget ?? string.Empty))
            {
                EditorApplication.Exit(121);
            }

            // Verify a custom build path was given
            if (!validatedOptions.TryGetValue("customBuildPath", out string _))
            {
                Console.WriteLine("Missing argument -customBuildPath");
                EditorApplication.Exit(130);
            }

            return validatedOptions;
        }

        /// <summary>
        /// Parses the arguments provided in the environment's command line, then adds them to a given dictionary of arguments
        /// </summary>
        /// <param name="providedArguments"></param>
        private static void ParseCommandLineArguments(out Dictionary<string, string> providedArguments)
        {
            providedArguments = new Dictionary<string, string>();
            string[] args = Environment.GetCommandLineArgs();

            Console.WriteLine(
                $"{Eol}" +
                $"###########################{Eol}" +
                $"#    Parsing settings     #{Eol}" +
                $"###########################{Eol}" +
                $"{Eol}"
            );

            // Extract flags with optional values
            for (int current = 0, next = 1; current < args.Length; current++, next++)
            {
                // Parse flag
                bool isFlag = args[current].StartsWith("-");
                if (!isFlag) continue;
                string flag = args[current].TrimStart('-');

                // Parse optional value
                bool flagHasValue = next < args.Length && !args[next].StartsWith("-");
                string value = flagHasValue ? args[next].TrimStart('-') : "";

                // No secret params used in the project - add this back in if you need secret params in the build.
                // bool secret = Secrets.Contains(flag);
                // string displayValue = secret ? "*HIDDEN*" : "\"" + value + "\"";

                // Assign flag to value in dictionary
                Console.WriteLine($"Found flag \"{flag}\" with value {value}.");
                providedArguments.Add(flag, value);
            }
        }

        // Original method used in editor.
        [MenuItem("MyTools/Make Linux Server Build")]
        public static void BuildLinuxServer()
        {
            // Get filename
            string path = EditorUtility.SaveFolderPanel("Choose Location of Built Game", "", "");

            // Build player
            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
            buildPlayerOptions.locationPathName = path + "/StandaloneLinux64ServerBuild";
            buildPlayerOptions.target = BuildTarget.StandaloneLinux64;
            buildPlayerOptions.subtarget = (int)StandaloneBuildSubtarget.Server;
            buildPlayerOptions.options = BuildOptions.None;

            //Copy Scene List from the Build Settings Window
            string[] sceneList = new string[EditorBuildSettings.scenes.Length];
            for (int i = 0; i < EditorBuildSettings.scenes.Length; i++)
            { sceneList[i] = EditorBuildSettings.scenes[i].path; }

            buildPlayerOptions.scenes = sceneList;

            BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
        }
    }
}
