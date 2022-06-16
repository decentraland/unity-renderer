
using System;
using System.Collections;
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

namespace DCL.Protobuf
{
    public static class ProtobufEditor
    {
        private const string REALPATH_TO_COMPONENTS_DEFINITIONS = "/DCLPlugins/ECS7/ProtocolBuffers/Definitions";
        private const string PATH_TO_COMPONENTS = "/DCLPlugins/ECS7/ProtocolBuffers/Generated/PBFiles";
        private const string SUBPATH_TO_COMPONENTS_COMMON = "/Common";
        private const string TEMPPATH_TO_COMPONENTS_DEFINITIONS = "/DCLPlugins/ECS7/ProtocolBuffers/DefinitionsTemp";
        private const string SUBPATH_TO_COMPONENTS_DEFINITIONS_COMMON = "/common";
        private const string PATH_TO_PROTO = "/DCLPlugins/ECS7/ProtocolBuffers/Editor/";
        private const string PATH_TO_COMPONENT_IDS = "/DCLPlugins/ECS7/ECSComponents/ComponentID.cs";
        private const string PROTO_COMMAND = "protoc";

        [MenuItem("Decentraland/Protobuf/UpdateModels")]
        public static void UpdateModels()
        {
            DownloadProtoDefinitions();
            GenerateComponentCode();
        }

        [MenuItem("Decentraland/Protobuf/Download proto definitions (For debugging)")]
        public static void DownloadProtoDefinitions()
        {
            WebClient client = new WebClient();
            Stream data;
            StreamReader reader;
            string libraryJsonString;
            Dictionary<string, object> libraryContent, libraryInfo;

            // Download the data of decentraland-ecs
            client = new WebClient();
            data = client.OpenRead(@"https://registry.npmjs.org/decentraland-ecs");
            reader = new StreamReader(data);
            libraryJsonString = reader.ReadToEnd();
            data.Close();
            reader.Close();
            
            // Process the response
			libraryContent = JsonConvert.DeserializeObject<Dictionary<string, object>>(libraryJsonString);
            libraryInfo = JsonConvert.DeserializeObject<Dictionary<string, object>>(libraryContent["dist-tags"].ToString());
            
            string nextVersion = libraryInfo["next"].ToString();
            UnityEngine.Debug.Log("decentraland-ecs next version: " + nextVersion);
            
            // Download the "package.json" of decentraland-ecs@next
            client = new WebClient();
            data = client.OpenRead(@"https://registry.npmjs.org/decentraland-ecs/" + nextVersion);
            reader = new StreamReader(data);
            libraryJsonString = reader.ReadToEnd();
            data.Close();
            reader.Close();

            // Process the response
            libraryContent = JsonConvert.DeserializeObject<Dictionary<string, object>>(libraryJsonString);
            libraryInfo = JsonConvert.DeserializeObject<Dictionary<string, object>>(libraryContent["dist"].ToString());

            string tgzUrl = libraryInfo["tarball"].ToString();
            UnityEngine.Debug.Log("decentraland-ecs@next url: " + tgzUrl);
            
            // Download package
            client = new WebClient();
            client.DownloadFile(tgzUrl, "decentraland-ecs-next.tgz");
            UnityEngine.Debug.Log("File downloaded decentraland-ecs-next.tgz");

            string destPackage = "decentraland-ecs" + nextVersion;
            if (Directory.Exists(destPackage))
                Directory.Delete(destPackage, true);
            
            try
            {
                Directory.CreateDirectory(destPackage);

                // We unzip the library
                ProcessStartInfo startInfo = new ProcessStartInfo() { FileName = "tar", Arguments = "-xvzf decentraland-ecs-next.tgz -C " + destPackage, CreateNoWindow = true};
                Process proc = new Process() { StartInfo = startInfo };
                proc.Start();

                proc.WaitForExit(5 * 1000);

                UnityEngine.Debug.Log("Unzipped decentraland-ecs-next.tgz");

                if (File.Exists(destPackage + "/package/dist/ecs7/proto-definitions/common/id.proto"))
                {
                    File.Delete(destPackage + "/package/dist/ecs7/proto-definitions/common/id.proto");
                }
                    
                string componentDefinitionPath = Application.dataPath + REALPATH_TO_COMPONENTS_DEFINITIONS;

                if (Directory.Exists(componentDefinitionPath))
                    Directory.Delete(componentDefinitionPath, true);

                // We move the definitions to their correct path
                Directory.Move(destPackage + "/package/dist/ecs7/proto-definitions", componentDefinitionPath);
                UnityEngine.Debug.Log("Success copying definitions in " + componentDefinitionPath);

            }
            catch (Exception e)
            {
                Debug.LogError("The download has failed " + e.Message);
            }
            finally // We delete the downloaded package
            {
                Directory.Delete(destPackage, true);
                if (File.Exists("decentraland-ecs-next.tgz"))
                    File.Delete("decentraland-ecs-next.tgz");
            }
        }
        
        
        struct ProtoComponent
        {
            public string componentName;
            public int componentId;
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
            DirectoryInfo dir = new DirectoryInfo(Application.dataPath + TEMPPATH_TO_COMPONENTS_DEFINITIONS + SUBPATH_TO_COMPONENTS_DEFINITIONS_COMMON);
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
            Debug.Log("Starting regenerate ");
            bool ok = false;
            
            string tempOutputPath = Application.dataPath + PATH_TO_COMPONENTS + "temp";
            try
            {
                List<ProtoComponent> components = GetComponents();

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
                {
                    GenerateComponentIdEnum(components);
                }
            }
            catch (Exception e)
            {
                Debug.LogError("The component code generation has failed: " + e.Message);
            }

            if (ok)
            {
                string outputPath = Application.dataPath + PATH_TO_COMPONENTS;
                if (Directory.Exists(outputPath))
                {
                    Directory.Delete(outputPath, true);
                }
                
                Directory.Move(tempOutputPath, outputPath);
            } else if (Directory.Exists(tempOutputPath)) {           
                Directory.Delete(tempOutputPath, true);
            }
            
            if (Directory.Exists(Application.dataPath + TEMPPATH_TO_COMPONENTS_DEFINITIONS)) {           
                Directory.Delete(Application.dataPath + TEMPPATH_TO_COMPONENTS_DEFINITIONS, true);
            }
        }

        private static void CreateTempDefinitions()
        {
            if (Directory.Exists(Application.dataPath + TEMPPATH_TO_COMPONENTS_DEFINITIONS))
            {
                Directory.Delete(Application.dataPath + TEMPPATH_TO_COMPONENTS_DEFINITIONS, true);
            }
            
            ProtobufEditorHelper.CloneDirectory(Application.dataPath + REALPATH_TO_COMPONENTS_DEFINITIONS, Application.dataPath + TEMPPATH_TO_COMPONENTS_DEFINITIONS);
        }
        
        private static void GenerateComponentIdEnum(List<ProtoComponent> components)
        {
            string componentCsFileContent = "namespace DCL.ECS7\n{\n    public static class ComponentID \n    {\n";

            componentCsFileContent += $"        public const int TRANSFORM = 1;\n";
            foreach (ProtoComponent component in components )
            {
                string componentUpperCaseName = ProtobufEditorHelper.ToSnakeCase(component.componentName).ToUpper();
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
            {
                return true;
            }
            
            // We prepare the paths for the conversion
            string filePath = Application.dataPath + TEMPPATH_TO_COMPONENTS_DEFINITIONS + SUBPATH_TO_COMPONENTS_DEFINITIONS_COMMON ;

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
            // This is the console to convert the proto
            ProcessStartInfo startInfo = new ProcessStartInfo() { FileName = PROTO_COMMAND, Arguments = finalArguments };
            
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
                    if (line.IndexOf("common/id.proto") == -1 && line.IndexOf("(ecs_component_id)") == -1)
                    {
                        outLines.Add(line);
                    }
                }
                
                outLines.Add("package decentraland.ecs;");
                outLines.Add("option csharp_namespace = \"DCL.ECSComponents\";");
                
                File.WriteAllLines(file.FullName, outLines.ToArray());
            }
        }
    }
}
