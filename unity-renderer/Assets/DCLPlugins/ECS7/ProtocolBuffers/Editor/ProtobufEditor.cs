using System;
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
        private const string PATH_TO_COMPONENTS = "/DCLPlugins/ECS7/ECSComponents/";
        private const string PATH_TO_PROTO = "/DCLPlugins/ECS7/ProtocolBuffers/Editor/";
        private const string PATH_TO_FILES = "/DCLPlugins/ECS7/ProtocolBuffers";
        private const string PROTO_FILENAME = "protoc";

        [MenuItem("Decentraland/Protobuf/Regenerate models")]
        public static void UpdateModels()
        {
            Debug.Log("Starting update");
            string outputPath = Application.dataPath + PATH_TO_COMPONENTS + "BoxShape/Data";
            bool compile = CompileProtobufSystemPath(outputPath,"BoxShape");
            Debug.Log("Models has been updated: " + compile);
        }

        private static bool CompileProtobufSystemPath(string outputPath , string protoFileName)
        {
            string filePath = Application.dataPath + PATH_TO_FILES;
            string proto_path = Application.dataPath + PATH_TO_PROTO + PROTO_FILENAME;
            string finalArguments = $"\"{filePath}/{protoFileName}.proto\" --csharp_out \"{outputPath}\" --proto_path \"{filePath}\"";

            //string finalArguments1 = $"\"D:\\UnityProjects\\unity-renderer\\unity-renderer\\Assets/DCLPlugins/ECS7/ProtocolBuffers/BoxShape.proto\" --csharp_out \"D:/Borrar/bin/\"  --proto_path \"D:/UnityProjects/unity-renderer/unity-renderer/Assets/DCLPlugins/ECS7/ProtocolBuffers\" "; 
            //string finalArguments = $"\"D:\\UnityProjects\\TestProtobuf\\Assets/Script/BoxShape.proto\" --csharp_out \"D:\\UnityProjects\\TestProtobuf\\Assets\\Script\"  --proto_path \"D:\\UnityProjects\\TestProtobuf\\Assets\\Script\" ";
            
            UnityEngine.Debug.Log("Protobuf Unity : Final arguments :\n" + finalArguments);
              ProcessStartInfo startInfo = new ProcessStartInfo() { FileName = proto_path, Arguments = finalArguments };
            
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
            UnityEngine.Debug.Log("Protobuf Unity : Compiled " + protoFileName);

            if (error != "")
            {
                UnityEngine.Debug.LogError("Protobuf Unity : " + error);
                return false;
            }
            return true;
        }
    }
}
