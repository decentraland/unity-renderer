
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

using Newtonsoft.Json;

namespace DCL.Protobuf
{
    public static class ProtobufEditor
    {
        private const string PATH_TO_GENERATED = "/DCLPlugins/ECS7/ProtocolBuffers/Generated";
        private const string PATH_TO_COMPONENTS_DEFINITIONS = "/DCLPlugins/ECS7/ProtocolBuffers/Generated/Definitions";
        private const string PATH_TO_COMPONENTS = "/DCLPlugins/ECS7/ProtocolBuffers/Generated/Protos";
        private const string PATH_TO_FOLDER = "/DCLPlugins/ECS7/ProtocolBuffers/Editor/";
        private const string PATH_TO_PROTO = "/DCLPlugins/ECS7/ProtocolBuffers/Editor/bin/";
        
        private const string PROTO_FILENAME = "protoc";
        private const string DOWNLOADED_VERSION_FILENAME = "downloadedVersion.gen.txt";
        private const string COMPILED_VERSION_FILENAME = "compiledVersion.gen.txt";
        private const string EXECUTABLE_VERSION_FILENAME = "executableVersion.gen.txt";
        
        private const string PROTO_VERSION = "3.12.3";

        [MenuItem("Decentraland/Protobuf/UpdateModels")]
        public static void UpdateModels()
        {
            if(!IsProtoVersionValid())
                DownloadProtobuffExecutable();
            
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
                WriteVersion(nextVersion, DOWNLOADED_VERSION_FILENAME);
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
            
            // We get output path
            string outputPath = Application.dataPath + PATH_TO_COMPONENTS;
            
            if (Directory.Exists(outputPath))
                Directory.Delete(outputPath, true);
            
            Directory.CreateDirectory(outputPath);
            
            foreach (FileInfo file in info)
            { 
                // We ensure that only proto files are converted, this shouldn't be necessary but just in case
                if(!file.Name.Contains(".proto"))
                    continue;

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
            string path = Application.dataPath + PATH_TO_FOLDER;
            WriteVersion(PROTO_VERSION,COMPILED_VERSION_FILENAME,path);

            Debug.Log("Models has been converted. Success: " +convertedCount + "    Failed: "+failedCount);
        }

        private static bool IsProtoVersionValid()
        {
            string path = Application.dataPath +PATH_TO_GENERATED + EXECUTABLE_VERSION_FILENAME;
            string version = GetVersion(path);
            return version == PROTO_VERSION;
        }

        [MenuItem("Decentraland/Protobuf/Download proto executable")]
        public static void DownloadProtobuffExecutable()
        {
            // Download package
            string machine  = null;
            string executableName = "protoc";
#if UNITY_EDITOR_WIN
            machine = "win64";
            executableName = "protoc.exe";
#endif
            
            string name = $"protoc-{PROTO_VERSION}-{machine}.zip";
            string url = $"https://github.com/protocolbuffers/protobuf/releases/download/v{PROTO_VERSION}/{name}";
            string zipProtoFileName = "protoc";
            WebClient client = new WebClient();
            client.DownloadFile(url, zipProtoFileName);
            string destPackage = "protobuf";

            try
            {

                Directory.CreateDirectory(destPackage);

                // We unzip the library
                ProcessStartInfo startInfo = new ProcessStartInfo() { FileName = "tar", Arguments = "-xvzf protoc -C " + destPackage, CreateNoWindow = true };
                Process proc = new Process() { StartInfo = startInfo };
                proc.Start();

                proc.WaitForExit(5 * 1000);

                UnityEngine.Debug.Log("Unzipped protoc");

                string outputPath = Application.dataPath + PATH_TO_PROTO + executableName;

                if (File.Exists(outputPath))
                    File.Delete(outputPath);

                // We move the definitions to their correct path
                Directory.Move(destPackage + "/bin/" + executableName, outputPath);
                WriteVersion(PROTO_VERSION, EXECUTABLE_VERSION_FILENAME);
                UnityEngine.Debug.Log("Success copying definitions in " + outputPath);

            }
            catch (Exception e)
            {
                Debug.LogError("The download has failed " + e.Message);
            }
            finally
            {
                File.Delete(zipProtoFileName);
                if (Directory.Exists(destPackage))
                    Directory.Delete(destPackage, true);
            }
        }

        [MenuItem("Decentraland/Protobuf/Test project compile (For debugging)")]
        // Unccoment this line to make it work with the compilation time
 //       [InitializeOnLoadMethod]
        private static void OnProjectLoadedInEditor()
        {
            var currentDownloadedVersion = EditorPrefs.GetString("Version", "NotDownloaded");
            var currentVersion = GetCompiledVersion();
            if (currentVersion != currentDownloadedVersion)
                UpdateModels();
        }

        
        private static string GetCompiledVersion()
        {
            string path = Application.dataPath + PATH_TO_FOLDER + COMPILED_VERSION_FILENAME;
            //Read the text from directly from the test.txt file
            StreamReader reader = new StreamReader(path);
            string version = reader.ReadToEnd();
            reader.Close();
            return version;
        }

        private static string GetVersion(string path)
        {
            string text = null;
            
            if (!File.Exists(path))
                return null;
            
            // Opening the existing file for reading
            using (FileStream fs = File.OpenRead(path))
            {
                int totalBytes = (int)fs.Length;
                byte[] bytes = new byte[totalBytes];
                int bytesRead = 0;

                while (bytesRead < totalBytes)
                {
                    int len = fs.Read(bytes, bytesRead, totalBytes);
                    bytesRead += len;
                }

                text = Encoding.UTF8.GetString(bytes);
            }
            return text; 
        }

        private static void WriteVersion(string version, string filename)
        {
            string path = Application.dataPath +PATH_TO_GENERATED+"/";
            WriteVersion(version, filename, path);
        }
        
        private static void WriteVersion(string version, string filename, string path)
        {
            string filePath = path + filename;
            var sr = File.CreateText(filePath);
            sr.WriteLine (version);
            sr.Close();
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
            string fileNameWithouthExtension = protoFileName.Replace(".proto", "");
            string correctPath = outputPath + "/" + fileNameWithouthExtension + ".gen.cs";

            Directory.Move(outputPath +"/"+fileNameWithouthExtension+".cs" ,correctPath);
            return true;
        }
    }
}
