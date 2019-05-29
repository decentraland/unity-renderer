#if UNITY_EDITOR && UNITY_WEBGL

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

public class PostProcessWebGL
{
    //The name of the WebGLTemplate. Location in project should be Assets/WebGLTemplates/<YOUR TEMPLATE NAME>
    const string __TemplateToUse = "decentraland";

    [PostProcessBuild]
    public static void ChangeWebGLTemplate(BuildTarget buildTarget, string pathToBuiltProject)
    {
        if (buildTarget != BuildTarget.WebGL)
        {
            return;
        }

        //create template path
        var templatePath = Paths.Combine(Application.dataPath, "WebGLTemplates", __TemplateToUse);

        //Clear the TemplateData folder, built by Unity.
        FileUtilExtended.CreateOrCleanDirectory(Paths.Combine(pathToBuiltProject, "TemplateData"));

        //Copy contents from WebGLTemplate. Ignore all .meta files
        FileUtilExtended.CopyDirectoryFiltered(templatePath, pathToBuiltProject, true, @".*/\.+|\.meta$", true);

        //Replace contents of index.html
        FixIndexHtml(pathToBuiltProject);
    }

    //Replaces %...% defines in index.html
    static void FixIndexHtml(string pathToBuiltProject)
    {
        //Fetch filenames to be referenced in index.html
        string
            webglBuildUrl,
            webglLoaderUrl;

        if (File.Exists(Paths.Combine(pathToBuiltProject, "Build", "UnityLoader.js")))
        {
            webglLoaderUrl = "Build/UnityLoader.js";
        }
        else
        {
            webglLoaderUrl = "Build/UnityLoader.min.js";
        }

        string buildName = pathToBuiltProject.Substring(pathToBuiltProject.LastIndexOf("/") + 1);
        webglBuildUrl = string.Format("Build/{0}.json", buildName);

        //webglLoaderUrl = EditorUserBuildSettings.development? "Build/UnityLoader.js": "Build/UnityLoader.min.js";
        Dictionary<string, string> replaceKeywordsMap = new Dictionary<string, string>
        {
            {
                "%UNITY_WIDTH%",
                PlayerSettings.defaultWebScreenWidth.ToString()
            },
            {
                "%UNITY_HEIGHT%",
                PlayerSettings.defaultWebScreenHeight.ToString()
            },
            {
                "%UNITY_WEB_NAME%",
                PlayerSettings.productName
            },
            {
                "%UNITY_WEBGL_LOADER_URL%",
                webglLoaderUrl
            },
            {
                "%UNITY_WEBGL_BUILD_URL%",
                webglBuildUrl
            }
        };

        string indexFilePath = Paths.Combine(pathToBuiltProject, "index.html");
        Func<string, KeyValuePair<string, string>, string> replaceFunction = (current, replace) =>
            current.Replace(replace.Key, replace.Value);
        if (File.Exists(indexFilePath))
        {
            File.WriteAllText(indexFilePath,
                replaceKeywordsMap.Aggregate<KeyValuePair<string, string>, string>(File.ReadAllText(indexFilePath),
                    replaceFunction));
        }
    }

    class FileUtilExtended
    {
        internal static void CreateOrCleanDirectory(string dir)
        {
            if (Directory.Exists(dir))
            {
                Directory.Delete(dir, true);
            }

            Directory.CreateDirectory(dir);
        }

        //Fix forward slashes on other platforms than windows
        internal static string FixForwardSlashes(string unityPath)
        {
            return ((Application.platform != RuntimePlatform.WindowsEditor) ? unityPath : unityPath.Replace("/", @"\"));
        }

        //Copies the contents of one directory to another.
        public static void CopyDirectoryFiltered(string source, string target, bool overwrite,
            string regExExcludeFilter, bool recursive)
        {
            RegexMatcher excluder = new RegexMatcher()
            {
                exclude = null
            };

            try
            {
                if (regExExcludeFilter != null)
                {
                    excluder.exclude = new Regex(regExExcludeFilter);
                }
            }
            catch (ArgumentException)
            {
                UnityEngine.Debug.Log("CopyDirectoryRecursive: Pattern '" + regExExcludeFilter +
                                      "' is not a correct Regular Expression. Not excluding any files.");

                return;
            }

            CopyDirectoryFiltered(source, target, overwrite, excluder.CheckInclude, recursive);
        }

        internal static void CopyDirectoryFiltered(string sourceDir, string targetDir, bool overwrite,
            Func<string, bool> filtercallback, bool recursive)
        {
            // Create directory if needed
            if (!Directory.Exists(targetDir))
            {
                Directory.CreateDirectory(targetDir);
                overwrite = false;
            }

            // Iterate all files, files that match filter are copied.
            foreach (string filepath in Directory.GetFiles(sourceDir))
            {
                if (filtercallback(filepath))
                {
                    string fileName = Path.GetFileName(filepath);
                    string to = Path.Combine(targetDir, fileName);

                    File.Copy(FixForwardSlashes(filepath), FixForwardSlashes(to), overwrite);
                }
            }

            // Go into sub directories
            if (recursive)
            {
                foreach (string subdirectorypath in Directory.GetDirectories(sourceDir))
                {
                    if (filtercallback(subdirectorypath))
                    {
                        string directoryName = Path.GetFileName(subdirectorypath);
                        CopyDirectoryFiltered(Path.Combine(sourceDir, directoryName),
                            Path.Combine(targetDir, directoryName), overwrite, filtercallback, recursive);
                    }
                }
            }
        }

        internal struct RegexMatcher
        {
            public Regex exclude;

            public bool CheckInclude(string s)
            {
                return exclude == null || !exclude.IsMatch(s);
            }
        }
    }

    class Paths
    {
        //Combine multiple paths using Path.Combine
        public static string Combine(params string[] components)
        {
            if (components.Length < 1)
            {
                throw new ArgumentException("At least one component must be provided!");
            }

            string str = components[0];

            for (int i = 1; i < components.Length; i++)
            {
                str = Path.Combine(str, components[i]);
            }

            return str;
        }
    }
}

#endif