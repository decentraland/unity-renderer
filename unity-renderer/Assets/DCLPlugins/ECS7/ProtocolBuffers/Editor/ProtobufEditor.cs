
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
        private const string PATH_TO_COMPONENTS = "/DCLPlugins/ECS7/ProtocolBuffers/Generated";
        private const string PATH_TO_COMPONENTS_DEFINITIONS = "/DCLPlugins/ECS7/ProtocolBuffers/Generated/Definitions";
        private const string PATH_TO_PROTO = "/DCLPlugins/ECS7/ProtocolBuffers/Editor/";
        private const string PROTO_FILENAME = "protoc";

        [MenuItem("Decentraland/Protobuf/UpdateModels")]
        public static void UpdateModels()
        {
            DownloadProtoDefinitions();
            CompileAllProtobuffDefinitions();
        }

        [MenuItem("Decentraland/Protobuf/Download proto definitions (For debugging)")]
        public static void DownloadProtoDefinitions()
        {
            WebClient client = new WebClient();
            Stream data;
            StreamReader reader;
            string libraryJsonString;
            Dictionary<string, object> libraryContent, libraryInfo;

            // Download the data of @dcl/ecs
            client = new WebClient();
            data = client.OpenRead(@"https://registry.npmjs.org/@dcl/ecs");
            reader = new StreamReader(data);
            libraryJsonString = reader.ReadToEnd();
            data.Close();
            reader.Close();
            
            // Process the response
			libraryContent = JsonConvert.DeserializeObject<Dictionary<string, object>>(libraryJsonString);
            libraryInfo = JsonConvert.DeserializeObject<Dictionary<string, object>>(libraryContent["dist-tags"].ToString());
            
            string nextVersion = libraryInfo["next"].ToString();
            UnityEngine.Debug.Log("@dcl/ecs next version: " + nextVersion);
            
            // Download the "package.json" of @dcl/ecs@next
            client = new WebClient();
            data = client.OpenRead(@"https://registry.npmjs.org/@dcl/ecs/" + nextVersion);
            reader = new StreamReader(data);
            libraryJsonString = reader.ReadToEnd();
            data.Close();
            reader.Close();

            // Process the response
            libraryContent = JsonConvert.DeserializeObject<Dictionary<string, object>>(libraryJsonString);
            libraryInfo = JsonConvert.DeserializeObject<Dictionary<string, object>>(libraryContent["dist"].ToString());

            string tgzUrl = libraryInfo["tarball"].ToString();
            UnityEngine.Debug.Log("@dcl/ecs@next url: " + tgzUrl);
            
            // Download package
            client = new WebClient();
            client.DownloadFile(tgzUrl, "dcl-ecs-next.tgz");
            UnityEngine.Debug.Log("File downloaded dcl-ecs-next.tgz");

            string destPackage = "dcl-ecs-" + nextVersion;
            if (Directory.Exists(destPackage))
                Directory.Delete(destPackage, true);
            
            try
            {
                Directory.CreateDirectory(destPackage);

                // We unzip the library
                ProcessStartInfo startInfo = new ProcessStartInfo() { FileName = "tar", Arguments = "-xvzf dcl-ecs-next.tgz -C " + destPackage, CreateNoWindow = true};
                Process proc = new Process() { StartInfo = startInfo };
                proc.Start();

                proc.WaitForExit(5 * 1000);

                UnityEngine.Debug.Log("Unzipped dcl-ecs-next.tgz");

                string componentDefinitionPath = Application.dataPath + PATH_TO_COMPONENTS_DEFINITIONS;

                if (Directory.Exists(componentDefinitionPath))
                    Directory.Delete(componentDefinitionPath, true);

                // We move the definitions to their correct path
                Directory.Move(destPackage + "/package/dist/components/definitions", componentDefinitionPath);
                UnityEngine.Debug.Log("Success copying definitions in " + componentDefinitionPath);
            }
            catch (Exception e)
            {
                Debug.LogError("The download has failed " + e.Message);
            }
            finally // We delete the downloaded package
            {
                Directory.Delete(destPackage, true);
                if (File.Exists("dcl-ecs-next.tgz"))
                    File.Delete("dcl-ecs-next.tgz");
            }
        }

        [MenuItem("Decentraland/Protobuf/Regenerate models (For debugging)")]
        public static void CompileAllProtobuffDefinitions()
        {
            Debug.Log("Starting regenerate ");
            
            // We get all the files that are proto
            DirectoryInfo dir = new DirectoryInfo(Application.dataPath + PATH_TO_COMPONENTS_DEFINITIONS);
            FileInfo[] info = dir.GetFiles("*.proto");

            int convertedCount = 0;
            int failedCount = 0;
            
            foreach (FileInfo file in info)
            { 
                // We ensure that only proto files are converted, this shouldn't be necessary but just in case
                if(!file.Name.Contains(".proto"))
                    continue;
                
                // We get output path
                string fileNameWithouthExtension = file.Name.Replace(file.Extension, "");
                string outputPath = Application.dataPath + PATH_TO_COMPONENTS;
                
                // We compile the proto
                bool compile = CompileProtobufFile(outputPath,file.Name);
                
                if (compile)
                {
                    convertedCount++;
                }
                else
                {
                    failedCount++;
                    Debug.LogError(file.Name + " model has failed in the conversion");
                }
            }

            Debug.Log("Models has been converted. Success: " +convertedCount + "    Failed: "+failedCount);
        }

        private static bool CompileProtobufFile(string outputPath , string protoFileName)
        {
            // We prepare the paths for the conversion
            string filePath = Application.dataPath + PATH_TO_COMPONENTS_DEFINITIONS;
            string proto_path = Application.dataPath + PATH_TO_PROTO + PROTO_FILENAME;
            string finalArguments = $"\"{filePath}/{protoFileName}\" --csharp_out \"{outputPath}\" --proto_path \"{filePath}\"";
            
            // This is the console to convert the proto
            ProcessStartInfo startInfo = new ProcessStartInfo() { FileName = proto_path, Arguments = finalArguments };
            
            Process proc = new Process() { StartInfo = startInfo };
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.RedirectStandardOutput = true;
            proc.StartInfo.RedirectStandardError = true;
            proc.Start();
            
            string error = proc.StandardError.ReadToEnd();
            proc.WaitForExit();

            if (error != "")
            {
                UnityEngine.Debug.LogError("Protobuf Unity failed : " + error);
                return false;
            }
            return true;
        }
    }
}
