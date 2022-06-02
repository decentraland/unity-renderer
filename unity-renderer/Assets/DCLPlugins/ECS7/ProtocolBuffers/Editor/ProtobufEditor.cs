
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Text;
using ICSharpCode.SharpZipLib.GZip;
using ICSharpCode.SharpZipLib.Tar;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

using Newtonsoft.Json;

namespace DCL.Protobuf
{
    public static class ProtobufEditor
    {
        private const bool VERBOSE = false;
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
            if (!IsProtoVersionValid())
                DownloadProtobuffExecutable();

            string downloadedVersion = DownloadProtoDefinitions();
            CompileAllProtobuffDefinitions(downloadedVersion);
        }

        [MenuItem("Decentraland/Protobuf/Download proto definitions (For debugging)")]
        public static string DownloadProtoDefinitions()
        {
            WebClient client;
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

            if (VERBOSE)
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
            if (VERBOSE)
                UnityEngine.Debug.Log("@dcl/ecs@next url: " + tgzUrl);

            // Download package
            string packageName = "dcl-ecs-next.tgz";
            client = new WebClient();
            client.DownloadFile(tgzUrl, packageName);
            if (VERBOSE)
                UnityEngine.Debug.Log("File downloaded dcl-ecs-next.tgz");

            string destPackage = "dcl-ecs-" + nextVersion;
            if (Directory.Exists(destPackage))
                Directory.Delete(destPackage, true);
            
            try
            {
                Directory.CreateDirectory(destPackage);

                // We unzip the library
                Tar(packageName,destPackage);
                Debug.Log("CAMBIOOO ");
                if (VERBOSE)
                    UnityEngine.Debug.Log("Unzipped dcl-ecs-next.tgz");

                string componentDefinitionPath = Application.dataPath + PATH_TO_COMPONENTS_DEFINITIONS;

                if (Directory.Exists(componentDefinitionPath))
                    Directory.Delete(componentDefinitionPath, true);

                // We move the definitions to their correct path
                Directory.Move(destPackage + "/package/dist/components/definitions", componentDefinitionPath);
                WriteVersion(nextVersion, DOWNLOADED_VERSION_FILENAME);
                if (VERBOSE)
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

            return nextVersion;
        }

        [MenuItem("Decentraland/Protobuf/Regenerate models (For debugging)")]
        public static void CompileAllProtobuffDefinitions(string versionNameToCompile)
        {
            if (VERBOSE)
                Debug.Log("Starting regenerating models");

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
                if (!file.Name.Contains(".proto"))
                    continue;

                // We compile the proto
                bool compile = CompileProtobufFile(outputPath, file.Name);

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
            WriteVersion(versionNameToCompile, COMPILED_VERSION_FILENAME, path);

            Debug.Log("Component models has been updated. Success: " + convertedCount + "    Failed: " + failedCount);
        }

        private static bool IsProtoVersionValid()
        {
            string path = Application.dataPath + PATH_TO_GENERATED + EXECUTABLE_VERSION_FILENAME;
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
#elif UNITY_EDITOR_OSX
            machine = "osx-x86_64";
#elif UNITY_EDITOR_LINUX
            machine = "linux-x86_64";
#endif

            // We download the proto executable
            string name = $"protoc-{PROTO_VERSION}-{machine}.zip";
            string url = $"https://github.com/protocolbuffers/protobuf/releases/download/v{PROTO_VERSION}/{name}";
            string zipProtoFileName = "protoc";
            WebClient client = new WebClient();
            client.DownloadFile(url, zipProtoFileName);
            string destPackage = "protobuf";

            try
            {
                Directory.CreateDirectory(destPackage);

                // We unzip the proto executable
                Unzip(zipProtoFileName,destPackage);
                
                if (VERBOSE)
                    UnityEngine.Debug.Log("Unzipped protoc");

                string outputPath = Application.dataPath + PATH_TO_PROTO + executableName;

                if (File.Exists(outputPath))
                    File.Delete(outputPath);

                // We move the executable to his correct path
                Directory.Move(destPackage + "/bin/" + executableName, outputPath);
                WriteVersion(PROTO_VERSION, EXECUTABLE_VERSION_FILENAME);
                if (VERBOSE)
                    UnityEngine.Debug.Log("Success copying definitions in " + outputPath);
            }
            catch (Exception e)
            {
                Debug.LogError("The download of the executable has failed " + e.Message);
            }
            finally
            {
                // We removed everything has has been created and it is not usefull anymore
                File.Delete(zipProtoFileName);
                if (Directory.Exists(destPackage))
                    Directory.Delete(destPackage, true);
            }
        }

        private static void Tar(string name, string path)
        {
            using (Stream inStream = File.OpenRead (name))
            using (Stream gzipStream = new GZipInputStream (inStream)) {
                TarArchive tarArchive = TarArchive.CreateInputTarArchive(gzipStream, Encoding.ASCII);
                tarArchive.ExtractContents (path);
            }
        }
        
        private static void Unzip(string name, string path)
        {
            ZipFile.ExtractToDirectory(name, path);
#if UNITY_EDITOR_WIN
            // ProcessStartInfo startInfo = new ProcessStartInfo() { FileName = "tar", Arguments = "-xvzf " + name + " -C " + path, CreateNoWindow = true };
            // Process proc = new Process() { StartInfo = startInfo };
            // proc.Start();
            //
            // proc.WaitForExit(5 * 1000);
#elif UNITY_EDITOR_OSX || UNITY_EDITOR_LINUX
                // TODO: unzip in mac and linux
#endif
        }
        
        [MenuItem("Decentraland/Protobuf/Test project compile (For debugging)")]
        [InitializeOnLoadMethod]
        private static void OnProjectCompile()
        {
            // The compiled version is a file that lives in the repo, if your local version is distinct it will generated them
            var currentDownloadedVersion = GetDownloadedVersion();
            var currentVersion = GetCompiledVersion();
            if (currentVersion != currentDownloadedVersion)
                UpdateModels();
        }

        private static string GetDownloadedVersion()
        {
            string path = Application.dataPath + PATH_TO_GENERATED + "/" + DOWNLOADED_VERSION_FILENAME;
            return GetVersion(path);
        }

        private static string GetCompiledVersion()
        {
            string path = Application.dataPath + PATH_TO_FOLDER + COMPILED_VERSION_FILENAME;
            return GetVersion(path);
        }

        private static string GetVersion(string path)
        {
            if (!File.Exists(path))
                return "";

            StreamReader reader = new StreamReader(path);
            string version = reader.ReadToEnd();
            reader.Close();

            return version;
        }

        private static void WriteVersion(string version, string filename)
        {
            string path = Application.dataPath + PATH_TO_GENERATED + "/";
            WriteVersion(version, filename, path);
        }

        private static void WriteVersion(string version, string filename, string path)
        {
            string filePath = path + filename;
            var sr = File.CreateText(filePath);
            sr.Write(version);
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

            Directory.Move(outputPath + "/" + fileNameWithouthExtension + ".cs" , correctPath);
            return true;
        }
    }
}
