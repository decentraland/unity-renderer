using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace DCL.Protobuf
{
    public static class ProtobufEditor
    {
        [MenuItem("Decentraland/Protobuf/Regenerate models")]
        public static void UpdateModels()
        {
            Debug.Log("Starting update");
            bool compile =CompileProtobufSystemPath("D:\\UnityProjects\\unity-renderer\\unity-renderer\\Assets\\DCLPlugins\\ECS7\\Editor\\BoxShape.proto");
            Debug.Log("Models Updated " + compile);
        }

        private static bool CompileProtobufSystemPath(string protoFileSystemPath)
        {
            //Do not compile changes coming from UPM package.
            if (protoFileSystemPath.Contains("Packages/com.e7.protobuf-unity"))
                return false;

            if (Path.GetExtension(protoFileSystemPath) == ".proto")
            {
                string outputPath = Path.GetDirectoryName(protoFileSystemPath);

                string options = " --csharp_out \"{0}\" ";
                // foreach (string s in includePaths)
                // {
                //     options += string.Format(" --proto_path \"{0}\" ", s);
                // }
                
                //options += $" --grpc_out={outputPath} --plugin=protoc-gen-grpc={ProtoPrefs.grpcPath}";
                //string combinedPath = string.Join(" ", optionFiles.Concat(new string[] { protoFileSystemPath }));

                string finalArguments = string.Format("\"{0}\"", protoFileSystemPath) + string.Format(options, outputPath);


                UnityEngine.Debug.Log("Protobuf Unity : Final arguments :\n" + finalArguments);


                ProcessStartInfo startInfo = new ProcessStartInfo() { FileName = Application.dataPath, Arguments = finalArguments };

                Process proc = new Process() { StartInfo = startInfo };
                proc.StartInfo.UseShellExecute = false;
                proc.StartInfo.RedirectStandardOutput = true;
                proc.StartInfo.RedirectStandardError = true;
                proc.Start();

                string output = proc.StandardOutput.ReadToEnd();
                string error = proc.StandardError.ReadToEnd();
                proc.WaitForExit();

                if (output != "")
                {
                    UnityEngine.Debug.Log("Protobuf Unity : " + output);
                }
                UnityEngine.Debug.Log("Protobuf Unity : Compiled " + Path.GetFileName(protoFileSystemPath));

                if (error != "")
                {
                    UnityEngine.Debug.LogError("Protobuf Unity : " + error);
                }
                return true;
            }
            return false;
        }
    }
}
