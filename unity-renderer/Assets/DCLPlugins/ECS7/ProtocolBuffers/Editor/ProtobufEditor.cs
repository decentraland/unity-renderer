using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

using Newtonsoft.Json;

using System.IO.Compression;
using UnityEditor.Compilation;
using ICSharpCode.SharpZipLib.GZip;
using ICSharpCode.SharpZipLib.Tar;
using ICSharpCode.SharpZipLib.Zip;

namespace DCL.Protobuf
{
    [InitializeOnLoad]
    public static class ProtobufEditor
    {
        static ProtobufEditor()
        {
            CompilationPipeline.compilationStarted += OnProjectCompile;
            OnProjectCompile();
        }
        
        private const bool VERBOSE = true;

        private const string PATH_TO_GENERATED = "/DCLPlugins/ECS7/ProtocolBuffers/Generated/";
        private const string REALPATH_TO_COMPONENTS_DEFINITIONS = "/DCLPlugins/ECS7/ProtocolBuffers/Definitions";
        private const string PATH_TO_COMPONENTS = "/DCLPlugins/ECS7/ProtocolBuffers/Generated/PBFiles";
        private const string SUBPATH_TO_COMPONENTS_COMMON = "/Common";
        private const string TEMPPATH_TO_COMPONENTS_DEFINITIONS = "/DCLPlugins/ECS7/ProtocolBuffers/DefinitionsTemp";
        private const string PATHNAME_TO_COMPONENTS_DEFINITIONS_COMMON = "common";
        private const string PATH_TO_COMPONENT_IDS = "/DCLPlugins/ECS7/ProtocolBuffers/Generated/ComponentID/ComponentID.gen.cs";
        private const string PATH_TO_FOLDER = "/DCLPlugins/ECS7/ProtocolBuffers/Editor/";
        private const string PATH_TO_PROTO = "/DCLPlugins/ECS7/ProtocolBuffers/Editor/bin/";
        
        private const string PROTO_FILENAME = "protoc";
        private const string DOWNLOADED_VERSION_FILENAME = "downloadedVersion.gen.txt";
        private const string COMPILED_VERSION_FILENAME = "compiledVersion.gen.txt";
        private const string EXECUTABLE_VERSION_FILENAME = "executableVersion.gen.txt";

        private const string PROTO_VERSION = "3.20.1";

        // Use this parameter when you want a fixed version of the @dcl/protocol, otherwise leave it empty
        //private const string FIXED_NPM_PACKAGE_LINK = "https://sdk-team-cdn.decentraland.org/@dcl/protocol/branch//dcl-protocol-1.0.0-2611997102.commit-8e362ff.tgz";
        private const string FIXED_NPM_PACKAGE_LINK = "";

        private const string NPM_PACKAGE = "@dcl/protocol";
        private const string NPM_PACKAGE_PROTO_DEF = "/package/ecs/components/";

        private struct ProtoComponent
        {
            public string componentName;
            public int componentId;
        }
        
        private static void VerboseLog(string message)
        {
            if (VERBOSE)
            {
                Debug.Log(message);
            }    
        }
        
        [MenuItem("Decentraland/Protobuf/UpdateModels with the latest version")]
        public static void UpdateModels()
        {
            var lastVersion = GetLatestProtoVersion();
            UpdateModels(lastVersion);
        }
        
        public static void UpdateModels(string version)
        {
            if (!IsProtoVersionValid())
                DownloadProtobuffExecutable();

             DownloadProtoDefinitions(version);
             GenerateComponentCode(version);
             CompilationPipeline.RequestScriptCompilation();
        }
        
        [MenuItem("Decentraland/Protobuf/Download latest proto definitions (For debugging)")]
        public static void DownloadLatestProtoDefinitions()
        {
            var nextVersion = GetLatestProtoVersion();
            DownloadProtoDefinitions(nextVersion);
        }

        public static void DownloadProtoDefinitions(string version)
        {
            WebClient client;
            Stream data;
            StreamReader reader;
            string libraryJsonString;
            Dictionary<string, object> libraryContent, libraryInfo;

            VerboseLog("Downloading " + NPM_PACKAGE + " version: " + version);

            // Download the "package.json" of {NPM_PACKAGE}@version
            client = new WebClient();
            data = client.OpenRead(@"https://registry.npmjs.org/" + NPM_PACKAGE + "/" + version);
            reader = new StreamReader(data);
            libraryJsonString = reader.ReadToEnd();
            data.Close();
            reader.Close();

            // Process the response
            libraryContent = JsonConvert.DeserializeObject<Dictionary<string, object>>(libraryJsonString);
            libraryInfo = JsonConvert.DeserializeObject<Dictionary<string, object>>(libraryContent["dist"].ToString());

            string tgzUrl = libraryInfo["tarball"].ToString();

            // If we have a fixed version, use it
            if (FIXED_NPM_PACKAGE_LINK.Length > 0) {
                tgzUrl = FIXED_NPM_PACKAGE_LINK;
            }

            VerboseLog(NPM_PACKAGE + "@" + version + "url: " + tgzUrl);

            // Download package
            string packageWithoutSlash = NPM_PACKAGE.Replace("/", "-"); // Replace / because in the file system is interpreted as a folder
            string packageName = packageWithoutSlash + "-" + version + ".tgz";
            client = new WebClient();
            client.DownloadFile(tgzUrl, packageName);
            VerboseLog("File downloaded " + packageName);

            string destPackage = packageWithoutSlash + "-" + version;
            if (Directory.Exists(destPackage))
                Directory.Delete(destPackage, true);

            try
            {
                Directory.CreateDirectory(destPackage);

                Untar(packageName,destPackage);
                VerboseLog("Untar " + packageName);

                if (File.Exists(destPackage + NPM_PACKAGE_PROTO_DEF + "/" + PATHNAME_TO_COMPONENTS_DEFINITIONS_COMMON + "/id.proto"))
                {
                    File.Delete(destPackage + NPM_PACKAGE_PROTO_DEF + "/" + PATHNAME_TO_COMPONENTS_DEFINITIONS_COMMON + "/id.proto");
                }

                string componentDefinitionPath = Application.dataPath + REALPATH_TO_COMPONENTS_DEFINITIONS;

                if (Directory.Exists(componentDefinitionPath))
                    Directory.Delete(componentDefinitionPath, true);

                // We move the definitions to their correct path
                Directory.Move(destPackage + NPM_PACKAGE_PROTO_DEF, componentDefinitionPath);

                VerboseLog("Success copying definitions in " + componentDefinitionPath);

            }
            catch (Exception e)
            {
                Debug.LogError("The download has failed " + e.Message);
            }
            finally // We delete the downloaded package
            {
                Directory.Delete(destPackage, true);
                if (File.Exists(packageName))
                    File.Delete(packageName);
            }
        }

        private static List<ProtoComponent> GetComponents()
        {
            // We get all the files that are proto
            DirectoryInfo dir = new DirectoryInfo(Application.dataPath + REALPATH_TO_COMPONENTS_DEFINITIONS);
            FileInfo[] info = dir.GetFiles("*.proto");
            List<ProtoComponent> components = new List<ProtoComponent>();
            
            foreach (FileInfo file in info)
            {
                // We ensure that only proto files are converted, this shouldn't be necessary but just in case
                if (!file.Name.Contains(".proto"))
                    continue;
                
                string protoContent = File.ReadAllText(file.FullName);
                
                ProtoComponent component = new ProtoComponent();
                component.componentName = file.Name.Substring(0, file.Name.Length - 6);
                component.componentId = -1;
                
                Regex regex = new Regex(@" *option +\(ecs_component_id\) += +[0-9]+ *;");
                var result = regex.Match(protoContent);
                if (result.Length > 0)
                {
                    string componentIdStr = result.Value.Split('=')[1].Split(';')[0];
                    component.componentId = int.Parse(componentIdStr);
                }
                
                components.Add(component);
            }

            return components;
        }     
        
        private static List<string> GetComponentsCommon()
        {
            // We get all the files that are proto
            DirectoryInfo dir = new DirectoryInfo(Application.dataPath + TEMPPATH_TO_COMPONENTS_DEFINITIONS + "/" + PATHNAME_TO_COMPONENTS_DEFINITIONS_COMMON);
            FileInfo[] info = dir.GetFiles("*.proto");
            List<string> components = new List<string>();
            
            foreach (FileInfo file in info)
            {
                // We ensure that only proto files are converted, this shouldn't be necessary but just in case
                if (!file.Name.Contains(".proto"))
                    continue;
                
                components.Add(file.Name);
            }

            return components;
        }
        [MenuItem("Decentraland/Protobuf/Regenerate models (For debugging)")]
        public static void GenerateComponentCode()
        {
            GenerateComponentCode("LocalMachine");
        }
        
        public static void GenerateComponentCode(string versionNameToCompile)
        {
            Debug.Log("Starting regenerate ");
            bool ok = false;
            
            string tempOutputPath = Application.dataPath + PATH_TO_COMPONENTS + "temp";
            try
            {
                List<ProtoComponent> components = GetComponents();
                components = components.OrderBy( component => component.componentId).ToList();

                if (Directory.Exists(tempOutputPath))
                {
                    Directory.Delete(tempOutputPath, true);
                }
                Directory.CreateDirectory(tempOutputPath);
                Directory.CreateDirectory(tempOutputPath + SUBPATH_TO_COMPONENTS_COMMON);

                CreateTempDefinitions();
                AddNamespaceAndPackage();

                ok = CompileAllComponents(components, tempOutputPath);
                ok &= CompileComponentsCommon(tempOutputPath + SUBPATH_TO_COMPONENTS_COMMON);

                if (ok)
                    GenerateComponentIdEnum(components);
            }
            catch (Exception e)
            {
                Debug.LogError("The component code generation has failed: " + e.Message);
            }

            if (ok)
            {
                string outputPath = Application.dataPath + PATH_TO_COMPONENTS;
                if (Directory.Exists(outputPath))
                    Directory.Delete(outputPath, true);
                
                Directory.Move(tempOutputPath, outputPath);
                
                string path = Application.dataPath + PATH_TO_FOLDER;
                WriteVersion(versionNameToCompile, COMPILED_VERSION_FILENAME, path);
            } 
            else if (Directory.Exists(tempOutputPath)) 
            {           
                Directory.Delete(tempOutputPath, true);
            }
            
            if (Directory.Exists(Application.dataPath + TEMPPATH_TO_COMPONENTS_DEFINITIONS)) {           
                Directory.Delete(Application.dataPath + TEMPPATH_TO_COMPONENTS_DEFINITIONS, true);
            }
        }

        private static void CreateTempDefinitions()
        {
            if (Directory.Exists(Application.dataPath + TEMPPATH_TO_COMPONENTS_DEFINITIONS))
                Directory.Delete(Application.dataPath + TEMPPATH_TO_COMPONENTS_DEFINITIONS, true);

            ProtobufEditorHelper.CloneDirectory(Application.dataPath + REALPATH_TO_COMPONENTS_DEFINITIONS, Application.dataPath + TEMPPATH_TO_COMPONENTS_DEFINITIONS);
        }
        
        private static void GenerateComponentIdEnum(List<ProtoComponent> components)
        {
            string componentCsFileContent = "/* Autogenerated file, DO NOT EDIT! */\n\nnamespace DCL.ECS7\n{\n    public static class ComponentID \n    {\n";

            componentCsFileContent += $"        public const int TRANSFORM = 1;\n";
            foreach (ProtoComponent component in components )
            {
                string componentUpperCaseName = ProtobufEditorHelper.ToSnakeCase(component.componentName).ToUpper();
                
                // Special case where NFT is created from NFTProto, instead of N_F_T_SHAPE we set NFT_SHAPE
                if (componentUpperCaseName.Contains("N_F_T"))
                    componentUpperCaseName = "NFT_SHAPE";
               
                // Special case where GLTF is created from GLTFProto, instead of G_L_T_F_SHAPE we set GLTF_SHAPE
                if (componentUpperCaseName.Contains("G_L_T_F"))
                    componentUpperCaseName = "GLTF_SHAPE";
                
                componentCsFileContent += $"        public const int {componentUpperCaseName} = {component.componentId.ToString()};\n";
            }
            componentCsFileContent += "    }\n}\n";
            
            File.WriteAllText(Application.dataPath + PATH_TO_COMPONENT_IDS, componentCsFileContent);
        }
        
        private static bool CompileAllComponents(List<ProtoComponent> components, string outputPath)
        {
            if (components.Count == 0)
            {
                UnityEngine.Debug.LogError("There are no components to generate!!");
                return false;
            }
            
            // We prepare the paths for the conversion
            string filePath = Application.dataPath + TEMPPATH_TO_COMPONENTS_DEFINITIONS;

            List<string> paramsArray = new List<string>
            {
                $"--csharp_out \"{outputPath}\"", 
                $"--proto_path \"{filePath}\""
            };
            
            foreach(ProtoComponent component in components)
            {
                paramsArray.Add($"\"{filePath}/{component.componentName}.proto\"");    
            }
            
            return ExecProtoCompilerCommand(string.Join(" ", paramsArray));
        }

        private static bool CompileComponentsCommon(string outputPath)
        {
            List<string> commonFiles = GetComponentsCommon();

            if (commonFiles.Count == 0)
                return true;

            // We prepare the paths for the conversion
            string filePath = Application.dataPath + TEMPPATH_TO_COMPONENTS_DEFINITIONS + "/" + PATHNAME_TO_COMPONENTS_DEFINITIONS_COMMON ;

            List<string> paramsArray = new List<string>
            {
                $"--csharp_out \"{outputPath}\"", 
                $"--proto_path \"{filePath}\""
            };
            
            foreach(string protoFile in commonFiles)
            {
                paramsArray.Add($"\"{filePath}/{protoFile}\"");    
            }
            
            return ExecProtoCompilerCommand(string.Join(" ", paramsArray));
        }
        
        private static bool ExecProtoCompilerCommand(string finalArguments)
        {
            string proto_path = GetPathToProto();
            
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

        private static void AddNamespaceAndPackage()
        {
            // We get all the files that are proto
            DirectoryInfo dir = new DirectoryInfo(Application.dataPath + TEMPPATH_TO_COMPONENTS_DEFINITIONS);
            FileInfo[] info = dir.GetFiles("*.proto");
            
            foreach (FileInfo file in info)
            {
                // We ensure that only proto files are converted, this shouldn't be necessary but just in case
                if (!file.Name.Contains(".proto"))
                    continue;

                string protoContent = File.ReadAllText(file.FullName);
                List<string> lines = protoContent.Split('\n').ToList();
                List<string> outLines = new List<string>();
                
                foreach ( string line in lines )
                {
                    if (line.IndexOf(PATHNAME_TO_COMPONENTS_DEFINITIONS_COMMON + "/id.proto") == -1 && line.IndexOf("(ecs_component_id)") == -1)
                    {
                        outLines.Add(line);
                    }
                }
                
                outLines.Add("package decentraland.ecs;");
                outLines.Add("option csharp_namespace = \"DCL.ECSComponents\";");
                
                File.WriteAllLines(file.FullName, outLines.ToArray());
            }
        }
        
        private static bool IsProtoVersionValid()
        {
            string path = Application.dataPath + PATH_TO_GENERATED + EXECUTABLE_VERSION_FILENAME;
            string version = GetVersion(path);
            string protoPath = GetPathToProto();
            
            // If we are in windows, we add the extension of the file
#if UNITY_EDITOR_WIN
            protoPath += ".exe";
#endif
            return version == PROTO_VERSION && File.Exists(protoPath);
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
        
        public static string GetLatestProtoVersion()
        {
            WebClient client;
            Stream data;
            StreamReader reader;
            string libraryJsonString;
            Dictionary<string, object> libraryContent, libraryInfo;

            // Download the data of decentraland-/ecs
            client = new WebClient();
            data = client.OpenRead(@"https://registry.npmjs.org/" + NPM_PACKAGE);
            if (data == null)
            {
                return "";
            }
            
            reader = new StreamReader(data);
            libraryJsonString = reader.ReadToEnd();
            data.Close();
            reader.Close();
            
            // Process the response
            libraryContent = JsonConvert.DeserializeObject<Dictionary<string, object>>(libraryJsonString);
            libraryInfo = JsonConvert.DeserializeObject<Dictionary<string, object>>(libraryContent["dist-tags"].ToString());

            string nextVersion = libraryInfo["next"].ToString();
            return nextVersion;
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

                string outputPathDir = Application.dataPath + PATH_TO_PROTO ;
                string outputPath = outputPathDir + executableName;

                if (File.Exists(outputPath))
                    File.Delete(outputPath);

                if (!Directory.Exists(outputPathDir))
                    Directory.CreateDirectory(outputPathDir);
                
                // We move the executable to his correct path
                Directory.Move(destPackage + "/bin/" + executableName, outputPath);
                
#if UNITY_EDITOR_OSX || UNITY_EDITOR_LINUX
                AddExecutablePermisson(GetPathToProto());
#endif
                
                WriteVersion(PROTO_VERSION, EXECUTABLE_VERSION_FILENAME);
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

        private static string GetPathToProto()
        {
            return Application.dataPath + PATH_TO_PROTO + PROTO_FILENAME;
        }
        
#if UNITY_EDITOR_OSX || UNITY_EDITOR_LINUX
        private static bool AddExecutablePermisson(string path)
        {
            // This is the console to convert the proto
            ProcessStartInfo startInfo = new ProcessStartInfo() { FileName = "chmod", Arguments = $"+x \"{path}\"" };
            
            Process proc = new Process() { StartInfo = startInfo };
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.RedirectStandardOutput = true;
            proc.StartInfo.RedirectStandardError = true;
            proc.Start();
            
            string error = proc.StandardError.ReadToEnd();
            proc.WaitForExit();

            if (error != "")
            {
                UnityEngine.Debug.LogError("`chmod +x protoc` failed : " + error);
                return false;
            }
            return true;
        }
#endif

        
        private static void Untar(string name, string path)
        {
            using (Stream inStream = File.OpenRead (name))
            using (Stream gzipStream = new GZipInputStream (inStream)) {
                TarArchive tarArchive = TarArchive.CreateInputTarArchive(gzipStream, Encoding.ASCII);
                tarArchive.ExtractContents (path);
            }
        }

        private static void Unzip(string name, string path)        
        {
            FastZip fastZip = new FastZip();
            string fileFilter = null;

            fastZip.ExtractZip(name, path, fileFilter);
        }
        
        private static void OnProjectCompile(object test)
        {
            OnProjectCompile();
        }            

        [MenuItem("Decentraland/Protobuf/Test project compile (For debugging)")]
        private static void OnProjectCompile()
        {
            // TODO: Delete this return line to make the generation of the proto based on your machine 
            return;

            // The compiled version is a file that lives in the repo, if your local version is distinct it will generated them
            var currentDownloadedVersion = GetDownloadedVersion();
            var currentVersion = GetCompiledVersion();
            if (currentVersion != currentDownloadedVersion)
                UpdateModels(currentVersion);
        }
    }
}
