using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

using Newtonsoft.Json;

namespace DCL.Protobuf
{
    public static class ProtobufEditor
    {
        private const string PATH_TO_COMPONENTS_DEFINITIONS = "/DCLPlugins/ECS7/ECSComponents/Definitions";
        private const string PATH_TO_COMPONENTS = "/DCLPlugins/ECS7/ECSComponents/";
        private const string PATH_TO_PROTO = "/DCLPlugins/ECS7/ProtocolBuffers/Editor/";
        private const string PATH_TO_FILES = "/DCLPlugins/ECS7/ProtocolBuffers";
        private const string PROTO_FILENAME = "protoc";
        
        [MenuItem("Decentraland/Protobuf/Download proto definitionsssss")]
        public static void DownloadProtos()
        {
            WebClient client = new WebClient();
            Stream data;
            StreamReader reader;
            string jsonString;
            Dictionary<string, object> dictA, dictB;

            // Download the data of @dcl/ecs
            client = new WebClient();
            data = client.OpenRead(@"https://registry.npmjs.org/@dcl/ecs");
            reader = new StreamReader(data);
            jsonString = reader.ReadToEnd();
            data.Close();
            reader.Close();
            
            // Process the response
			dictA = JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonString);
            dictB = JsonConvert.DeserializeObject<Dictionary<string, object>>(dictA["dist-tags"].ToString());
            
            string nextVersion = dictB["next"].ToString();
            UnityEngine.Debug.Log("@dcl/ecs next version: " + nextVersion);
            
            // Download the "package.json" of @dcl/ecs@next
            client = new WebClient();
            data = client.OpenRead(@"https://registry.npmjs.org/@dcl/ecs/" + nextVersion);
            reader = new StreamReader(data);
            jsonString = reader.ReadToEnd();
            data.Close();
            reader.Close();

            // Process the response
            dictA = JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonString);
            dictB = JsonConvert.DeserializeObject<Dictionary<string, object>>(dictA["dist"].ToString());

            string tgzUrl = dictB["tarball"].ToString();
            UnityEngine.Debug.Log("@dcl/ecs@next url: " + tgzUrl);
            
            // Download package
            client = new WebClient();
            client.DownloadFile(tgzUrl, "dcl-ecs-next.tgz");
            UnityEngine.Debug.Log("File downloaded dcl-ecs-next.tgz");

            string destPackage = "dcl-ecs-" + nextVersion;
            if (Directory.Exists(destPackage))
            {
                Directory.Delete(destPackage, true);
            }
            Directory.CreateDirectory(destPackage);
            
            ProcessStartInfo startInfo = new ProcessStartInfo() { FileName = "tar", Arguments = "-xvzf dcl-ecs-next.tgz -C " + destPackage };
            Process proc = new Process() { StartInfo = startInfo };
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.RedirectStandardOutput = true;
            proc.StartInfo.RedirectStandardError = true;
            proc.Start();

            string output = proc.StandardOutput.ReadToEnd();
            string error = proc.StandardError.ReadToEnd();
            proc.WaitForExit();
            
            UnityEngine.Debug.Log("Unzipped dcl-ecs-next.tgz");

            string componentDefinitionPath = Application.dataPath + PATH_TO_COMPONENTS_DEFINITIONS;
                
            if (Directory.Exists(componentDefinitionPath))
                Directory.Delete(componentDefinitionPath, true);
            
            Directory.Move(destPackage + "/package/dist/components/definitions", componentDefinitionPath);
            Directory.Delete(destPackage, true);
            if (File.Exists("dcl-ecs-next.tgz"))
                File.Delete("dcl-ecs-next.tgz");
        }

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
