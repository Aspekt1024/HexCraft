using System.Diagnostics;
using System.IO;
using System.Linq;
using UnityEditor;

namespace Aspekt.Build
{
    public class BuildTools
    {
        [MenuItem("File/Build Game (Windows-Developer)")]
        public static void BuildWindowsDev()
        {
            var options = new BuildPlayerOptions()
            {
                target = BuildTarget.StandaloneWindows,
                options = BuildOptions.Development,
                extraScriptingDefines = new string[] {},
            };
            
            var defaultPath = "C:\\Users\\dszab\\Desktop\\Builds\\Hexcraft";

            var path = defaultPath;
            var runAfterBuild = false;
            
            var cla = System.Environment.GetCommandLineArgs();
            for (int i = 0; i < cla.Length - 1; i++)
            {
                switch (cla[i])
                {
                    case "-buildpath":
                        path = cla[i + 1];
                        break;
                    case "-run":
                        runAfterBuild = true;
                        break;
                };
            }
            
            Build(options, path, runAfterBuild);
        }
        
        private static void Build(BuildPlayerOptions options, string path, bool runAfterBuild)
        {
            string executableName = "";
            switch (options.target)
            {
                case BuildTarget.StandaloneWindows:
                    executableName = "HexCraft.exe";
                    break;
            }

            var executablePath = Path.Combine(path, executableName);
            options.locationPathName = executablePath;

            options.scenes = EditorBuildSettings.scenes.Select(s => s.path).ToArray();
            
            BuildPipeline.BuildPlayer(options);

            // Copy a file from the project folder to the build folder, alongside the built game.
            //FileUtil.CopyFileOrDirectory("Assets/Templates/Readme.txt", path + "Readme.txt");

            if (runAfterBuild)
            {
                var proc = new Process
                {
                    StartInfo = { FileName = executablePath }
                };
                proc.Start();
            }
        }
    }   
}