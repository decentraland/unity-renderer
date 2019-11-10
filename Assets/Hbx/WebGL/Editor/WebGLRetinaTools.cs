//----------------------------------------------
//            Hbx: WebGL
// Copyright © 2017-2018 Hogbox Studios
// WebGLRetinaTools.cs v3.3
// Developed against WebGL build from Unity 2018.1.8f1
// Tested against Unity 5.6.0f3, 2017.3.1f1, 2018.1.6f1, 2018.2.0f2, 2018.1.8.f1
//----------------------------------------------

//#define BROTLISTREAM_AVALIABLE

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Security.Cryptography; // for md5

using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;

#if BROTLISTREAM_AVALIABLE
using Brotli;
#endif

namespace Hbx.WebGL
{
    /// <summary>
    /// Wizard to edit the webgl fix settings
    /// </summary>

    public class EditWebGLSettingsWizard : ScriptableWizard
    {

#if !UNITY_5_5_OR_NEWER
        public enum WebGLCompressionFormat
        {
            Gzip,
            Brotli,
            Disabled
        };
#endif

        // fix settings
        [Serializable]
        public class WebGLRetinaToolsSettings
        {
            public bool autoRunAfterBuild = false;
            public float desktopScale = 1f;
            public float mobileScale = 1f;
            public bool createCopy = false;
            public string copyAppendString = "-fixed";
            public bool disableMobileCheck = false;
            public bool autoLaunchBrowser = false;
            public string autoLaunchURL = "";

            // messages
            public string mobileWarningMessage = "";
            public string genericErrorMessage = "";
            public string unhandledExceptionMessage = "";
            public string outOfMemoryMessage = "";
            public string notEnoughMemoryMessage = "";

            public WebGLRetinaToolsSettings() { }

            public WebGLRetinaToolsSettings(WebGLRetinaToolsSettings copy)
            {
                autoRunAfterBuild = copy.autoRunAfterBuild;
                desktopScale = copy.desktopScale;
                mobileScale = copy.mobileScale;
                createCopy = copy.createCopy;
                copyAppendString = copy.copyAppendString;
                disableMobileCheck = copy.disableMobileCheck;
                autoLaunchBrowser = copy.autoLaunchBrowser;
                autoLaunchURL = copy.autoLaunchURL;
                mobileWarningMessage = copy.mobileWarningMessage;
                genericErrorMessage = copy.genericErrorMessage;
                unhandledExceptionMessage = copy.unhandledExceptionMessage;
                outOfMemoryMessage = copy.outOfMemoryMessage;
                notEnoughMemoryMessage = copy.notEnoughMemoryMessage;
            }

            public bool Equals(WebGLRetinaToolsSettings other)
            {
                return autoRunAfterBuild == other.autoRunAfterBuild &&
                    desktopScale == other.desktopScale &&
                    mobileScale == other.mobileScale &&
                    createCopy == other.createCopy &&
                    copyAppendString == other.copyAppendString &&
                    disableMobileCheck == other.disableMobileCheck &&
                    autoLaunchBrowser == other.autoLaunchBrowser &&
                    autoLaunchURL == other.autoLaunchURL &&
                    mobileWarningMessage == other.mobileWarningMessage &&
                    genericErrorMessage == other.genericErrorMessage &&
                    unhandledExceptionMessage == other.unhandledExceptionMessage &&
                    outOfMemoryMessage == other.outOfMemoryMessage &&
                    notEnoughMemoryMessage == other.notEnoughMemoryMessage;
            }
        }

        public WebGLRetinaToolsSettings _settings = new WebGLRetinaToolsSettings();

        // unity build settings
        public WebGLCompressionFormat _compressionFormat = WebGLCompressionFormat.Gzip;
        public bool _nameFilesAsHashes = false;
        public string _emscriptenArgs = "";

        bool _unappliedChanges = false;

        /// <summary>
        /// Static helper to create a wizard.
        /// </summary>

        public static void CreateWizard()
        {
            EditWebGLSettingsWizard w = ScriptableWizard.DisplayWizard<EditWebGLSettingsWizard>("WebGL Tools Settings", "Close", "Apply");

            // fix settings
            WebGLRetinaTools.LoadSettings();
            w._settings = new WebGLRetinaToolsSettings(WebGLRetinaTools.Settings);

            // unity build settings
#if UNITY_5_5_OR_NEWER
            w._compressionFormat = UnityEditor.PlayerSettings.WebGL.compressionFormat;
            w._nameFilesAsHashes = UnityEditor.PlayerSettings.WebGL.nameFilesAsHashes;
            w._emscriptenArgs = UnityEditor.PlayerSettings.WebGL.emscriptenArgs;
#endif
        }

        /// <summary>
        /// Called by unity each to a value etc change
        /// </summary>

        void OnWizardUpdate()
        {
            helpString = "";
            if (_unappliedChanges) helpString = "You have unapplied changes.";
            errorString = "";
            if (_compressionFormat == WebGLCompressionFormat.Brotli) errorString = "Brotli compression not supported by this tool, please use Gzip or Disabled";
        }

        protected override bool DrawWizardGUI()
        {
            int originalIndent = EditorGUI.indentLevel;

            GUIStyle staticinfostyle = new GUIStyle();
            staticinfostyle.normal.textColor = Color.gray;

            // fix settings
            EditorGUILayout.LabelField("Fix Settings", staticinfostyle);

            WebGLRetinaToolsSettings lastSettings = new WebGLRetinaToolsSettings(_settings);

            string autoruntip = "Should the WebGL Retina Fix be automatically run after each build?";
            _settings.autoRunAfterBuild = EditorGUILayout.Toggle(new GUIContent("Auto Run Fix", autoruntip), _settings.autoRunAfterBuild);

            string deskdprtip = "Scale of full resolution used on desktop, value of 1.0 is full resolution, value of 0.5 is half resolution.";
            _settings.desktopScale = EditorGUILayout.FloatField(new GUIContent("Desktop Scale", deskdprtip), _settings.desktopScale);

            string mobdprtip = "Scale of full resolution used on mobile, value of 1.0 is full resolution, value of 0.5 is half resolution.";
            _settings.mobileScale = EditorGUILayout.FloatField(new GUIContent("Mobile Scale", mobdprtip), _settings.mobileScale);


            string createcopytip = "Should a copy of the build be created, with the fix being applied to the copy?";
            _settings.createCopy = EditorGUILayout.Toggle(new GUIContent("Create Copy", createcopytip), _settings.createCopy);

            if (_settings.createCopy)
            {
                string copystrtip = "String to append to the build folders name when copying.";
                _settings.copyAppendString = EditorGUILayout.TextField(new GUIContent("Copy Append", copystrtip), _settings.copyAppendString);
            }

            string disablemobiletip = "Disable the warning message displayed when running WebGL builds on Mobile devices.";
            _settings.disableMobileCheck = EditorGUILayout.Toggle(new GUIContent("Disable Mobile Check", disablemobiletip), _settings.disableMobileCheck);

            string autolaunch = "Should the browser be automatically opened after a fix is applied?";
            _settings.autoLaunchBrowser = EditorGUILayout.Toggle(new GUIContent("Launch Browser", autolaunch), _settings.autoLaunchBrowser);

            if (_settings.autoLaunchBrowser)
            {
                string launchurltip = "The base URL for your build, e.g. if my build was called mygame and I can goto http://localhost/mysites/mygame to view it, then just enter http://localhost/mysites/";
                _settings.autoLaunchURL = EditorGUILayout.TextField(new GUIContent("Base URL", launchurltip), _settings.autoLaunchURL);
            }

            // messages
            EditorGUILayout.Separator();
            EditorGUILayout.LabelField("Error Messages", staticinfostyle);

            if (!_settings.disableMobileCheck)
            {
                string mobilewarntip = "Change the warning message displayed when WebGL build runs on a Mobile device, empty uses Unity default.";
                _settings.mobileWarningMessage = EditorGUILayout.TextField(new GUIContent("Mobile Warning", mobilewarntip), _settings.mobileWarningMessage);
            }

            string genericerrortip = "Change the generic error message displayed when WebGL build throws an error, empty uses Unity default.";
            _settings.genericErrorMessage = EditorGUILayout.TextField(new GUIContent("Generic Error", genericerrortip), _settings.genericErrorMessage);

            string unhandledexceptip = "Change the error message displayed when WebGL build throws an exception but exceptions are not enabled in Player Settings, empty uses Unity default.";
            _settings.unhandledExceptionMessage = EditorGUILayout.TextField(new GUIContent("Unhandled Exception", unhandledexceptip), _settings.unhandledExceptionMessage);

            string outofmemorytip = "Change the error message displayed when WebGL build runs out of Memory, empty uses Unity default.";
            _settings.outOfMemoryMessage = EditorGUILayout.TextField(new GUIContent("Out Of Memory", outofmemorytip), _settings.outOfMemoryMessage);

            string notenoughmemorytip = "Change the error message displayed when WebGL build can't allocate it's inital memory block, empty uses Unity default.";
            _settings.notEnoughMemoryMessage = EditorGUILayout.TextField(new GUIContent("Not Enough Memory", notenoughmemorytip), _settings.notEnoughMemoryMessage);

            WebGLCompressionFormat lastcompression = _compressionFormat;
#if UNITY_5_5_OR_NEWER
            // webgl build settings
            EditorGUILayout.Separator();
            EditorGUILayout.LabelField("Unity WebGL Settings", staticinfostyle);

            string compressiontip = "Compression format used for WebGL builds.";
            _compressionFormat = (WebGLCompressionFormat)EditorGUILayout.EnumPopup(new GUIContent("Compression Format", compressiontip), _compressionFormat);

            bool lastnamefilesashashes = _nameFilesAsHashes;
            string namefilestip = "Use MD5 hash of the uncompressed files contents as a filename for each file in the build.";
            _nameFilesAsHashes = EditorGUILayout.Toggle(new GUIContent("Name Files As Hashes", namefilestip), _nameFilesAsHashes);

            //string lastemscriptargs = _emscriptenArgs;
            //string emscriptargstip = "Arguments passed to emscripten for WebGL builds.";
            //_emscriptenArgs = EditorGUILayout.TextField(new GUIContent("Emscripten Args", emscriptargstip), _emscriptenArgs);

#endif

            EditorGUI.indentLevel = originalIndent;

            bool changed = !_settings.Equals(lastSettings) || lastcompression != _compressionFormat || lastnamefilesashashes != _nameFilesAsHashes;
            _unappliedChanges |= changed;
            OnWizardUpdate();
            return changed;
        }

        void OnWizardCreate()
        {

        }

        // When the user presses the "Apply" button OnWizardOtherButton is called.
        void OnWizardOtherButton()
        {
            // fix settings
            WebGLRetinaTools.Settings = new WebGLRetinaToolsSettings(_settings);
            WebGLRetinaTools.SaveSettings();

            // webgl build settings
#if UNITY_5_5_OR_NEWER
            UnityEditor.PlayerSettings.WebGL.compressionFormat = _compressionFormat;
            UnityEditor.PlayerSettings.WebGL.nameFilesAsHashes = _nameFilesAsHashes;
            UnityEditor.PlayerSettings.WebGL.emscriptenArgs = _emscriptenArgs;
#endif
            _unappliedChanges = false;
        }
    }


    /// <summary>
    /// Main WegbGL tools class
    /// </summary>

    public static class WebGLRetinaTools
    {
        const string VERSION_STR = "3.3";

        // build json data
        [Serializable]
        public class WebGLBuildJson
        {
            public string companyName = "DefaultCompany";
            public string productName = "WebGLRetinaTools";
            public string dataUrl = "12f04f7a916aa6c68ee968b311f54ce8.unityweb";
            public string wasmCodeUrl = "ea5fac3cfa442baeb21f09bd7211c848.unityweb";
            public string wasmFrameworkUrl = "d38bb163c2c3ddb06ef7f9ae60ffbcd8.unityweb";
            public int TOTAL_MEMORY = 16777216;
            public string[] graphicsAPI = new string[] { "WebGL 2.0", "WebGL 1.0" };
            [Serializable]
            public class WebglContextAttributes
            {
                public bool preserveDrawingBuffer = false;
            }
            public WebglContextAttributes webglContextAttributes;
            public string splashScreenStyle = "Dark";
            public string backgroundColor = "#231F20";
            [Serializable]
            public class CacheControl
            {
                string DEFAULT_DUMMY = "must-revalidate"; // this variable can't be serialized using unity json as it's named default!!!
            }
            public CacheControl cacheControl;
        }

        static WebGLRetinaTools()
        {
#if BROTLISTREAM_AVALIABLE
		    String currentPath = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.Process);
			String dllPath = Path.Combine(Path.Combine(Environment.CurrentDirectory, "Assets"), "Plugins");
		    if(!currentPath.Contains(dllPath))
		    {
		        Environment.SetEnvironmentVariable("PATH", currentPath + Path.PathSeparator + dllPath, EnvironmentVariableTarget.Process);
		    }
#endif
            LoadSettings();
        }

        // editor prefs keys
        const string AutoRunFix_Key = "Hbx.WebGL.AutoRunFix";
        const string DesktopScale_Key = "Hbx.WebGL.DesktopScale";
        const string MobileScale_Key = "Hbx.WebGL.MobileScale";
        const string Disable_MoblieCheck_Key = "Hbx.WebGL.DisableMobileCheck";
        const string CreateCopy_Key = "Hbx.WebGL.CreateCopy";
        const string CopyAppendString_Key = "Hbx.WebGL.CopyAppendString";
        const string LastFixFolder_Key = "Hbx.WebGL.LastFixFolder";
        const string AutoLaunchBrowser_Key = "Hbx.WebGL.AutoLaunchBrowser";
        const string AutoLaunchBrowserURL_Key = "Hbx.WebGL.AutoLaunchBrowserURL";
        const string MobileWarningMessage_Key = "Hbx.WebGL.MobileWarningMessage";
        const string GenericErrorMessage_Key = "Hbx.WebGL.GenericErrorMessage";
        const string UnhandledExceptionMessage_Key = "Hbx.WebGL.UnhandledExceptionMessage";
        const string OutOfMemoryMessage_Key = "Hbx.WebGL.OutOfMemoryMessage";
        const string NotEnoughMemoryMessage_Key = "Hbx.WebGL.NotEnoughMemoryMessage";

        // original error messages
        const string OriginalMobileWarningMessage = @"Please note that Unity WebGL is not currently supported on mobiles. Press OK if you wish to continue anyway.";
        const string OriginalGenericErrorMessage = @"An error occurred running the Unity content on this page. See your browser JavaScript console for more info. The error was:";
        const string OriginalUnhandledExceptionMessage = @"An exception has occurred, but exception handling has been disabled in this build. If you are the developer of this content, enable exceptions in your project WebGL player settings to be able to catch the exception or see the stack trace.";
        const string OriginalOutOfMemoryMessage = @"Out of memory. If you are the developer of this content, try allocating more memory to your WebGL build in the WebGL player settings.";
        const string OriginalNotEnoughMemoryMessage = @"The browser could not allocate enough memory for the WebGL content. If you are the developer of this content, try allocating less memory to your WebGL build in the WebGL player settings.";

        // folder and extension names
        const string ProgressTitle = "Applying WebGL Fix";
        const string JsExt = ".js";
        const string JsgzExt = ".jsgz";
        const string JsbrExt = ".jsbr";
        const string UnitywebExt = ".unityweb";
#if UNITY_5_6_OR_NEWER
        const string RelFolder = "Build";
        const string DevFolder = "Build";
        static readonly string[] SourceFileTypes = { JsExt, UnitywebExt };
        static readonly string[] ExcludeFileNames = { "asm.memory", ".asm.code", ".data", "wasm.code" };
#else
        const string RelFolder = "Release";
        const string DevFolder = "Development";
        static readonly string[] SourceFileTypes = { JsExt, JsgzExt, JsbrExt };
        static readonly string[] ExcludeFileNames = { "UnityLoader" };
#endif

        // Settings
        public static EditWebGLSettingsWizard.WebGLRetinaToolsSettings Settings { get; set; }

        // Accessors for indvidual settings
        public static bool AutoRunFix { get { return Settings.autoRunAfterBuild; } set { Settings.autoRunAfterBuild = value; } }
        public static float DesktopScale { get { return Settings.desktopScale; } set { Settings.desktopScale = value; } }
        public static float MobileScale { get { return Settings.mobileScale; } set { Settings.mobileScale = value; } }

        public static bool DisableMobileCheck { get { return Settings.disableMobileCheck; } set { Settings.disableMobileCheck = value; } }
        public static bool ShouldCreateCopy { get { return Settings.createCopy; } set { Settings.createCopy = value; } }
        public static string CopyAppendString { get { return Settings.copyAppendString; } set { Settings.copyAppendString = value; } }
        public static bool AutoLaunchBrowser { get { return Settings.autoLaunchBrowser; } set { Settings.autoLaunchBrowser = value; } }
        public static string AutoLaunchBrowserURL { get { return Settings.autoLaunchURL; } set { Settings.autoLaunchURL = value; } }

        // Messages
        public static string MobileWarningMessage { get { return Settings.mobileWarningMessage; } set { Settings.mobileWarningMessage = value; } }
        public static string GenericErrorMessage { get { return Settings.genericErrorMessage; } set { Settings.genericErrorMessage = value; } }
        public static string UnhandledExceptionMessage { get { return Settings.unhandledExceptionMessage; } set { Settings.unhandledExceptionMessage = value; } }
        public static string OutOfMemoryMessage { get { return Settings.outOfMemoryMessage; } set { Settings.outOfMemoryMessage = value; } }
        public static string NotEnoughMemoryMessage { get { return Settings.notEnoughMemoryMessage; } set { Settings.notEnoughMemoryMessage = value; } }

        // Cached values
        public static string LastFixFolder { get { return EditorPrefs.GetString(LastFixFolder_Key, ""); } set { EditorPrefs.SetString(LastFixFolder_Key, value); } }

        public static bool _quiteMode = false;

        public static List<string> _debugMessages = new List<string>();

        enum CompressionType
        {
            None,
            GZip,
            Brotli
        };

        //
        // Post build event if active, try and happen last so as not to mess with others
        [PostProcessBuild(10000)]
        public static void OnPostProcessBuild(BuildTarget target, string pathToBuiltProject)
        {
            if (AutoRunFix && target == BuildTarget.WebGL)
            {
                _quiteMode = true;
                RetinaFixExistingBuild(pathToBuiltProject);
                _quiteMode = false;
            }
        }

        //
        // Show settings window
        [MenuItem("Hbx/WebGL/Settings", false, 20)]
        public static void DisplaySettings()
        {
            EditWebGLSettingsWizard.CreateWizard();
        }

        //
        // Run fix on the last build made by unity
        [MenuItem("Hbx/WebGL/Fix Last Build", false, 0)]
        public static void RetinaFixLastBuild()
        {
            if (EditorUserBuildSettings.development)
            {
                RetinaFixCodeFolder(DevFolder);
            }
            else
            {
                RetinaFixCodeFolder(RelFolder);
            }
        }

        //
        // Run fix on a selected build folder
        [MenuItem("Hbx/WebGL/Fix Existing Build", false, 1)]
        public static void RetinaFixExistingBuild()
        {
            string path = EditorUtility.OpenFolderPanel("Select a WebGL build folder", LastFixFolder, "");
            if (string.IsNullOrEmpty(path))
            {
                UnityEngine.Debug.LogWarning("WebGLRetinaTools: No build folder selected.");
                return;
            }

            RetinaFixExistingBuild(path);
        }

        //
        // Run fix on a specific build folder
        public static void RetinaFixExistingBuild(string aBuildPath)
        {
#if UNITY_5_6_OR_NEWER
            if (Directory.Exists(Path.Combine(aBuildPath, RelFolder)))
            {
                RetinaFixCodeFolder(RelFolder, aBuildPath);
            }
#else
            // look for release and/or development folders
            if (Directory.Exists(Path.Combine(aBuildPath, RelFolder)))
            {
                RetinaFixCodeFolder(RelFolder, aBuildPath);
            }
            if (Directory.Exists(Path.Combine(aBuildPath, DevFolder)))
            {
                RetinaFixCodeFolder(DevFolder, aBuildPath);
            }
#endif
        }


        //
        // Opens the jsgz and/or the js file in the current webgl build folder 
        // and inserts devicePixelRatio accordingly to add support for retina/hdpi 
        //
        public static void RetinaFixCodeFolder(string codeFolder, string buildOverridePath = "")
        {
            _debugMessages.Clear();

            if (!_quiteMode) UnityEngine.Debug.Log("WebGLRetinaTools: Fix build started.");

            // get path of the last webgl build or use override path
            string webglBuildPath = string.IsNullOrEmpty(buildOverridePath) ? EditorUserBuildSettings.GetBuildLocation(BuildTarget.WebGL) : buildOverridePath;

            LastFixFolder = webglBuildPath; // cache the folder

            // do we need to make a copy
            if (ShouldCreateCopy)
            {
                string copyname = webglBuildPath + CopyAppendString;
                // check if the copy already exists
                if (Directory.Exists(copyname))
                {
                    Directory.Delete(copyname, true);
                }
                FileUtil.CopyFileOrDirectory(webglBuildPath, copyname);
                webglBuildPath = copyname;
            }

            string codeFolderPath = Path.Combine(webglBuildPath, codeFolder);

            if (string.IsNullOrEmpty(codeFolderPath))
            {
                UnityEngine.Debug.LogError("WebGLRetinaTools: WebGL build path is empty, have you created a WebGL build yet?");
                return;
            }

            // check there is a release folder
            if (!Directory.Exists(codeFolderPath))
            {
                UnityEngine.Debug.LogError("WebGLRetinaTools: Couldn't find folder for WebGL build at path:\n" + codeFolderPath);
                return;
            }

            // find source files in release folder and fix
            string[] sourceFiles = FindSourceFilesInBuildFolder(codeFolderPath);
            foreach (string sourceFile in sourceFiles)
            {
                FixSourceFile(sourceFile);
            }

            // rehash files if requested
#if UNITY_5_5_OR_NEWER
            if (UnityEditor.PlayerSettings.WebGL.nameFilesAsHashes)
            {
                RenameFilesAsHashes(webglBuildPath);
            }
#endif

            if (!_quiteMode) UnityEngine.Debug.Log("WebGLRetinaTools: Complete, fixed " + sourceFiles.Length + " source files.");

            EditorUtility.ClearProgressBar();

            // Print report
            if (!_quiteMode && _debugMessages.Count > 0)
            {
                string report = "Following fixes applied...\n";
                foreach (string msg in _debugMessages)
                {
                    report += "    " + msg + "\n";
                }
                Debug.Log(report);
            }

            if (AutoLaunchBrowser && !string.IsNullOrEmpty(AutoLaunchBrowserURL))
            {
                var folder = new DirectoryInfo(webglBuildPath).Name;
                Application.OpenURL(Path.Combine(AutoLaunchBrowserURL, folder));
            }
        }

        //
        // Fix a source file based on it's extension type
        //
        static void FixSourceFile(string aSourceFile)
        {
            if (!_quiteMode) UnityEngine.Debug.Log("WebGLRetinaTools: Fixing " + aSourceFile);
            CompressionType ct = GetCompressionType(aSourceFile);
            if (ct == CompressionType.None)
            {
                FixJSFile(aSourceFile);
            }
            else
            {
                FixCompressedJSFile(aSourceFile, ct);
            }
        }

        //
        // Fix a standard .js file
        //
        static void FixJSFile(string jsPath)
        {
            string fileName = Path.GetFileName(jsPath);

            EditorUtility.DisplayProgressBar(ProgressTitle, "Opening " + fileName + "...", 0.0f);

            if (!_quiteMode) UnityEngine.Debug.Log("WebGLRetinaTools: Fixing raw JS file " + jsPath);

            // load the uncompressed js code (this might trip over on large projects)
            string sourcestr = File.ReadAllText(jsPath);
            bool ismin = IsMinified(ref sourcestr);
            StringBuilder source = new StringBuilder(sourcestr);
            sourcestr = "";

            EditorUtility.DisplayProgressBar(ProgressTitle, "Fixing js source in " + fileName + "...", 0.5f);

            if (ismin)
            {
                FixJSFileContentsMinified(ref source);
            }
            else
            {
                FixJSFileContents(fileName.Contains(".wasm."), ref source);
            }

            EditorUtility.DisplayProgressBar(ProgressTitle, "Saving js " + fileName + "...", 1.0f);

            // save the file
            File.WriteAllText(jsPath, source.ToString());
        }

        //
        // Fix a compressed jsgz file, decompresses and recompress accordingly
        //
        static void FixCompressedJSFile(string jsgzPath, CompressionType compressType)
        {
            string fileName = Path.GetFileName(jsgzPath);

            EditorUtility.DisplayProgressBar(ProgressTitle, "Uncompressing file " + fileName + "...", 0.0f);

            if (!_quiteMode) UnityEngine.Debug.Log("WebGLRetinaTools: Fixing Compressed file " + jsgzPath);

            byte[] sourcebytes = null;
            int readcount = 0;

            byte[] headerbytes = null;

            // open jsgz file
            using (FileStream inputFileStream = new FileStream(jsgzPath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                // create demcompress stream from opened jsgz file
                if (compressType == CompressionType.GZip)
                {
                    // read header bytes to copy back later
                    headerbytes = new byte[10];
                    inputFileStream.Read(headerbytes, 0, 10);
                    inputFileStream.Position = 0;
                    // 
                    using (GZipStream decompressionStream = new GZipStream(inputFileStream, CompressionMode.Decompress))
                    {
                        // read decompressed buffer
                        using (MemoryStream reader = new MemoryStream())
                        {
                            decompressionStream.CopyTo(reader);
                            sourcebytes = reader.ToArray();
                            readcount = sourcebytes.Length;
                        }
                    }
                }
                else
                {
#if BROTLISTREAM_AVALIABLE
	            	using (BrotliStream decompressionStream = new BrotliStream(inputFileStream, CompressionMode.Decompress))
					{
						// read decompressed buffer
						readcount = decompressionStream.Read(sourcebytes, 0, size);
					}
#endif
                }
                if (readcount <= 0)
                {
                    UnityEngine.Debug.LogError("WebGLRetinaTools: Failed to read from compressed file " + jsgzPath + " can't continue."); return;
                }
                inputFileStream.Close();
            }

            // create a string builder to edit from the decompressed buffer
            string decompressedSourceStr = Encoding.UTF8.GetString(sourcebytes, 0, readcount);
            sourcebytes = null;
            bool ismin = IsMinified(ref decompressedSourceStr);

            StringBuilder source = new StringBuilder(decompressedSourceStr);
            decompressedSourceStr = "";

            EditorUtility.DisplayProgressBar(ProgressTitle, "Fixing compressed source " + fileName + "...", 0.5f);

            // fix the source
            if (ismin)
            {
                FixJSFileContentsMinified(ref source);
            }
            else
            {
                FixJSFileContents(fileName.Contains(".wasm."), ref source);
            }

            EditorUtility.DisplayProgressBar(ProgressTitle, "Recompressing file " + fileName + "...", 1.0f);

            sourcebytes = Encoding.UTF8.GetBytes(source.ToString());
            source = null;

            // write out a compressed file with custom header
            using (FileStream fileOutputStream = File.Create(jsgzPath))
            {
                if (compressType == CompressionType.GZip)
                {

#if UNITY_5_6_OR_NEWER
                    // write the gzip header
                    fileOutputStream.Write(headerbytes, 0, 10);
                    // write the file name and comment (the comment is important as Unityloader.js looks for it)
                    byte[] fnbytes = Encoding.UTF8.GetBytes(fileName);
                    fileOutputStream.Write(fnbytes, 0, fnbytes.Length);
                    fileOutputStream.WriteByte(0); //zero terminate
                    byte[] cmbytes = Encoding.UTF8.GetBytes("UnityWeb Compressed Content (gzip)");
                    fileOutputStream.Write(cmbytes, 0, cmbytes.Length);
                    fileOutputStream.WriteByte(0); //zero terminate
#endif

                    // compress the sourc bytes and add to the output file
                    using (MemoryStream compressedMemoryStream = new MemoryStream())
                    {
                        using (GZipStream compressStream = new GZipStream(compressedMemoryStream, CompressionMode.Compress))
                        {
                            compressStream.Write(sourcebytes, 0, sourcebytes.Length);
                            compressStream.Close(); compressedMemoryStream.Close();
                            byte[] compressedbytes = compressedMemoryStream.ToArray();
#if UNITY_5_6_OR_NEWER
                            // now copy the compressed bytes excludeing the gzipstream generated header as we're using our own
                            fileOutputStream.Write(compressedbytes, 10, compressedbytes.Length - 10);
#else
                            fileOutputStream.Write(compressedbytes, 0, compressedbytes.Length);
#endif
                        }
                    }
                }
                else
                {
#if BROTLISTREAM_AVALIABLE
	        		using (BrotliStream output = new BrotliStream(fileOutputStream, CompressionMode.Compress))
	        		{
	            		output.Write(sourcebytes, 0, sourcebytes.Length);
	        		}
#endif
                }

                fileOutputStream.Close();
            }

        }

        public static string GetCodeFolder(bool isRelease)
        {
            return isRelease ? RelFolder : DevFolder;
        }

        //
        // Search folder path for all supported SourceFileTypes
        // excluding any with names containing ExcludeFileNames
        // Update:
        // in unity 2017 onward find the .js file as it's the unityloader
        // and use the build json file to find the framework file.
        //
        static string[] FindSourceFilesInBuildFolder(string aBuildPath)
        {

#if UNITY_2017_1_OR_NEWER
            List<string> files = new List<string>();
            string[] jsfiles = Directory.GetFiles(aBuildPath, "*.js");
            if(jsfiles.Length > 0)
            {
                files.Add(jsfiles[0]);
            }

            string[] jsonfiles = Directory.GetFiles(aBuildPath, "*.json");
            if (jsonfiles.Length > 0)
            {
                string buildJsonSrc = File.ReadAllText(jsonfiles[0]);
                WebGLBuildJson buildJsonObject = JsonUtility.FromJson<WebGLBuildJson>(buildJsonSrc);
                files.Add(Path.Combine(aBuildPath, buildJsonObject.wasmFrameworkUrl));
            }
            return files.ToArray();
#else
            string[] files = Directory.GetFiles(aBuildPath);
            List<string> found = new List<string>();
            foreach (string file in files)
            {
                string ext = Path.GetExtension(file);
                if (Array.IndexOf(SourceFileTypes, ext) == -1) continue;
                string name = Path.GetFileNameWithoutExtension(file);
                bool exclude = false;
                foreach (string exname in ExcludeFileNames)
                {
                    if (name.Contains(exname)) { exclude = true; break; }
                }
                if (!exclude) found.Add(file);
            }
            return found.ToArray();
#endif
        }

        //
        // returns true if passed source is minified (bit flaky but should work)
        //
        static bool IsMinified(ref string source)
        {
            return !source.Contains("},\n");
        }

        //
        // determine the compression type of a file
        //
        static CompressionType GetCompressionType(string aSourceFile)
        {
            string ext = Path.GetExtension(aSourceFile);
            if (ext == JsExt) return CompressionType.None;
            if (ext == JsgzExt) return CompressionType.GZip;
            if (ext == JsbrExt) return CompressionType.Brotli;

            // unityweb can be compressed or uncompressed so 
            // open a stream and determine type from header,
            // only supports gzip test for now
            if (ext == UnitywebExt)
            {
                using (FileStream s = File.Open(aSourceFile, FileMode.Open))
                {
                    bool isGZip = IsGzipCompressed(s);
                    s.Seek(0, SeekOrigin.Begin); //reset for when we do next check
                    s.Close();
                    return isGZip ? CompressionType.GZip : CompressionType.None;
                }
            }
            return CompressionType.None;
        }

        //
        // check if a stream contains the gzip header bytes
        static byte[] GZipHeaderBytes = { 31, 139, 0, 0, 0, 0, 0, 0, 0, 3 };
        static bool IsGzipCompressed(Stream stream)
        {
            byte[] headerbuf = new byte[10];
            int res = stream.Read(headerbuf, 0, 10);
            stream.Position = 0;
            if (res != 10) return false;

            return headerbuf[0] == GZipHeaderBytes[0] && headerbuf[1] == GZipHeaderBytes[1];//  System.Linq.Enumerable.SequenceEqual(headerbuf, GZipHeaderBytes);
        }

        //
        // Calculate an MD5 sum of the file contents

        static string CalculateMD5(string fileContents)
        {
            using (var md5 = MD5.Create())
            {
                var hash = md5.ComputeHash(Encoding.UTF8.GetBytes(fileContents));
                return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
            }
        }

        static string CalculateMD5(byte[] fileContents)
        {
            using (var md5 = MD5.Create())
            {
                var hash = md5.ComputeHash(fileContents);
                return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
            }
        }

        //
        // Rename files as hashes if need be, rebuilds the json and file also
        //

        static bool RenameFilesAsHashes(string aBuildPath)
        {
            string codeFolder = Path.Combine(aBuildPath, RelFolder);

            // find the html and json files, these tell us the names of the files we want
            string htmlFilePath = Path.Combine(aBuildPath, "index.html");
            if(!File.Exists(htmlFilePath))
            {
                UnityEngine.Debug.LogError("WebGLRetinaTools: Failed to find " + htmlFilePath + " unable to Name Files As Hashes.");
                return false;
            }

            string[] jsonFiles = Directory.GetFiles(codeFolder, "*.json");
            if (jsonFiles.Length == 0)
            {
                UnityEngine.Debug.LogError("WebGLRetinaTools: Failed to find a json file in " + codeFolder + " unable to Name Files As Hashes.");
                return false;
            }
            string jsonFilePath = jsonFiles[0];

            string[] jsFiles = Directory.GetFiles(codeFolder, "*.js");
            if (jsFiles.Length == 0)
            {
                UnityEngine.Debug.LogError("WebGLRetinaTools: Failed to find a js file in " + codeFolder + " unable to Name Files As Hashes.");
                return false;
            }
            string jsFilePath = jsFiles[0];

            // deserialise the json file
            string buildJsonSrc = File.ReadAllText(jsonFilePath);
            WebGLBuildJson buildJsonObject = JsonUtility.FromJson<WebGLBuildJson>(buildJsonSrc);
            StringBuilder buildJsonSrcBuilder = new StringBuilder(buildJsonSrc); 

            // rehash data url
            string newDataURL = RehashAndRenameFile(Path.Combine(codeFolder, buildJsonObject.dataUrl));
            buildJsonSrcBuilder.Replace(buildJsonObject.dataUrl, newDataURL);

            // rehash wasmCodeUrl
            string newWasmCodeUrl = RehashAndRenameFile(Path.Combine(codeFolder, buildJsonObject.wasmCodeUrl));
            buildJsonSrcBuilder.Replace(buildJsonObject.wasmCodeUrl, newWasmCodeUrl);

            // rehash wasmFrameworkUrl
            string newWasmFrameworkUrl = RehashAndRenameFile(Path.Combine(codeFolder, buildJsonObject.wasmFrameworkUrl));
            buildJsonSrcBuilder.Replace(buildJsonObject.wasmFrameworkUrl, newWasmFrameworkUrl);

            // now re serialise the json so we can hash that too
            File.WriteAllText(jsonFilePath, buildJsonSrcBuilder.ToString());

            string newjsonName = RehashAndRenameFile(jsonFilePath);

            // rehash the js
            string newjsName = RehashAndRenameFile(jsFilePath);

            // now update the index.html to load the correct
            string oldJsonFileName = Path.GetFileName(jsonFilePath);
            string oldJsFileName = Path.GetFileName(jsFilePath);

            StringBuilder htmlBuilder = new StringBuilder(File.ReadAllText(htmlFilePath));
            htmlBuilder.Replace(oldJsonFileName, newjsonName);
            htmlBuilder.Replace(oldJsFileName, newjsName);

            File.WriteAllText(htmlFilePath, htmlBuilder.ToString());

            return true;
        }

        static string RehashAndRenameFile(string aFilePath)
        {
            if (!File.Exists(aFilePath)) return string.Empty;

            // get the file contents
            CompressionType ct = GetCompressionType(aFilePath);
            if (ct == CompressionType.None)
            {
                return RehashAndRenameUncompressedFile(aFilePath);
            }
            else
            {
                return RehashAndRenameCompressedFile(aFilePath, ct);
            }
            return string.Empty;
        }

        static string RehashAndRenameUncompressedFile(string aFilePath)
        {
            string path = Path.GetDirectoryName(aFilePath);
            string ext = Path.GetExtension(aFilePath);

            string sourcestr = File.ReadAllText(aFilePath);
            string hash = CalculateMD5(sourcestr);

            string newname = hash + ext;
            File.Move(aFilePath, Path.Combine(path, newname));
            //File.Copy(aFilePath, Path.Combine(path, newname) + "-converted");

            return newname;
        }

        static string RehashAndRenameCompressedFile(string aFilePath, CompressionType compressType)
        {
            string path = Path.GetDirectoryName(aFilePath);
            string ext = Path.GetExtension(aFilePath);

            byte[] sourcebytes = null;
            int readcount = 0;

            byte[] headerbytes = null;

            // open jsgz file
            using (FileStream inputFileStream = new FileStream(aFilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                // create demcompress stream from opened jsgz file
                if (compressType == CompressionType.GZip)
                {
                    // read header bytes to copy back later
                    headerbytes = new byte[10];
                    inputFileStream.Read(headerbytes, 0, 10);
                    inputFileStream.Position = 0;

                    // 
                    using (GZipStream decompressionStream = new GZipStream(inputFileStream, CompressionMode.Decompress))
                    {
                        // read decompressed buffer
                        using (MemoryStream reader = new MemoryStream())
                        {
                            decompressionStream.CopyTo(reader);
                            sourcebytes = reader.ToArray();
                            readcount = sourcebytes.Length;
                        }
                    }
                }
                else
                {
#if BROTLISTREAM_AVALIABLE
                    using (BrotliStream decompressionStream = new BrotliStream(inputFileStream, CompressionMode.Decompress))
                    {
                        // read decompressed buffer
                        readcount = decompressionStream.Read(sourcebytes, 0, size);
                    }
#endif
                }
                if (readcount <= 0)
                {
                    UnityEngine.Debug.LogError("WebGLRetinaTools: Failed to read from compressed file " + aFilePath + " can't continue."); return string.Empty;
                }
                inputFileStream.Close();
            }

            // calc hash
            string hash = CalculateMD5(sourcebytes);
            string newname = hash + ext;
            string newFilePath = Path.Combine(path, newname);

            // delete the old file
            File.Delete(aFilePath);

            // write out a compressed file with custom header
            using (FileStream fileOutputStream = File.Create(newFilePath))
            {
                if (compressType == CompressionType.GZip)
                {

#if UNITY_5_6_OR_NEWER
                    // write the gzip header
                    fileOutputStream.Write(headerbytes, 0, 10);
                    // write the file name and comment (the comment is important as Unityloader.js looks for it)
                    byte[] fnbytes = Encoding.UTF8.GetBytes(newname);
                    fileOutputStream.Write(fnbytes, 0, fnbytes.Length);
                    fileOutputStream.WriteByte(0); //zero terminate
                    byte[] cmbytes = Encoding.UTF8.GetBytes("UnityWeb Compressed Content (gzip)");
                    fileOutputStream.Write(cmbytes, 0, cmbytes.Length);
                    fileOutputStream.WriteByte(0); //zero terminate
#endif

                    // compress the sourc bytes and add to the output file
                    using (MemoryStream compressedMemoryStream = new MemoryStream())
                    {
                        using (GZipStream compressStream = new GZipStream(compressedMemoryStream, CompressionMode.Compress))
                        {
                            compressStream.Write(sourcebytes, 0, sourcebytes.Length);
                            compressStream.Close(); compressedMemoryStream.Close();
                            byte[] compressedbytes = compressedMemoryStream.ToArray();
#if UNITY_5_6_OR_NEWER
                            // now copy the compressed bytes excludeing the gzipstream generated header as we're using our own
                            fileOutputStream.Write(compressedbytes, 10, compressedbytes.Length - 10);
#else
                            fileOutputStream.Write(compressedbytes, 0, compressedbytes.Length);
#endif
                        }
                    }
                }
                else
                {
#if BROTLISTREAM_AVALIABLE
                    using (BrotliStream output = new BrotliStream(fileOutputStream, CompressionMode.Compress))
                    {
                        output.Write(sourcebytes, 0, sourcebytes.Length);
                    }
#endif
                }

                fileOutputStream.Close();
            }
            return newname;
        }

        //
        // Load/Save Settings
        //

        public static string GetSettingsJsonPath()
        {
            const string filename = "WebGLRetinaToolsSettings";
            string[] shaderGUID = AssetDatabase.FindAssets(filename);
            if (shaderGUID == null || shaderGUID.Length == 0)
            {
                return Path.Combine(Application.dataPath, filename + ".json");
            }

            string datapath = Application.dataPath;
            datapath = datapath.Remove(datapath.Length - ("Assets").Length);
            return Path.Combine(datapath, AssetDatabase.GUIDToAssetPath(shaderGUID[0]));
        }

        public static void LoadSettings()
        {
            string jsonpath = GetSettingsJsonPath();
            if (!File.Exists(jsonpath))
            {
                // no file found, upgrade or create default
                Settings = new EditWebGLSettingsWizard.WebGLRetinaToolsSettings();

                // if we find settings in the editor prefs upgrade 
                if (EditorPrefs.GetFloat(DesktopScale_Key, float.MaxValue) != float.MaxValue)
                {
                    Settings.autoRunAfterBuild = EditorPrefs.GetBool(AutoRunFix_Key, false);
                    Settings.desktopScale = EditorPrefs.GetFloat(DesktopScale_Key, 1.0f);
                    Settings.mobileScale = EditorPrefs.GetFloat(MobileScale_Key, 1.0f);
                    Settings.disableMobileCheck = EditorPrefs.GetBool(Disable_MoblieCheck_Key, false);
                    Settings.createCopy = EditorPrefs.GetBool(CreateCopy_Key, false);
                    Settings.copyAppendString = EditorPrefs.GetString(CopyAppendString_Key, "-fixed");
                    Settings.autoLaunchBrowser = EditorPrefs.GetBool(AutoLaunchBrowser_Key, false);
                    Settings.autoLaunchURL = EditorPrefs.GetString(AutoLaunchBrowserURL_Key, "");
                    Settings.mobileWarningMessage = EditorPrefs.GetString(MobileWarningMessage_Key, "");
                    Settings.genericErrorMessage = EditorPrefs.GetString(GenericErrorMessage_Key, "");
                    Settings.unhandledExceptionMessage = EditorPrefs.GetString(UnhandledExceptionMessage_Key, "");
                    Settings.outOfMemoryMessage = EditorPrefs.GetString(OutOfMemoryMessage_Key, "");
                    Settings.notEnoughMemoryMessage = EditorPrefs.GetString(NotEnoughMemoryMessage_Key, "");
                }
                SaveSettings();

            }
            else
            {
                Settings = JsonUtility.FromJson<EditWebGLSettingsWizard.WebGLRetinaToolsSettings>(File.ReadAllText(jsonpath));
            }
        }

        public static void SaveSettings()
        {
            string jsonpath = GetSettingsJsonPath();

            if (Settings == null) LoadSettings();

            if (!Directory.Exists(Path.GetDirectoryName(jsonpath)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(jsonpath));
            }
            File.WriteAllText(jsonpath, JsonUtility.ToJson(Settings, true));
        }

        //
        // Perform the find and replace hack for a release source
        //

        static void FixJSFileContentsMinified(ref StringBuilder source)
        {
            int slength = source.Length;

            // fix fillMouseEventData
            slength = source.Length;
#if UNITY_2018_2_OR_NEWER
            source.Replace("fillMouseEventData:(function(eventStruct,e,target){HEAPF64[eventStruct>>3]=JSEvents.tick();HEAP32[eventStruct+8>>2]=e.screenX;HEAP32[eventStruct+12>>2]=e.screenY;HEAP32[eventStruct+16>>2]=e.clientX;HEAP32[eventStruct+20>>2]=e.clientY;HEAP32[eventStruct+24>>2]=e.ctrlKey;HEAP32[eventStruct+28>>2]=e.shiftKey;HEAP32[eventStruct+32>>2]=e.altKey;HEAP32[eventStruct+36>>2]=e.metaKey;HEAP16[eventStruct+40>>1]=e.button;HEAP16[eventStruct+42>>1]=e.buttons;HEAP32[eventStruct+44>>2]=e[\"movementX\"]||e[\"mozMovementX\"]||e[\"webkitMovementX\"]||e.screenX-JSEvents.previousScreenX;HEAP32[eventStruct+48>>2]=e[\"movementY\"]||e[\"mozMovementY\"]||e[\"webkitMovementY\"]||e.screenY-JSEvents.previousScreenY;if(Module[\"canvas\"]){var rect=Module[\"canvas\"].getBoundingClientRect();HEAP32[eventStruct+60>>2]=e.clientX-rect.left;HEAP32[eventStruct+64>>2]=e.clientY-rect.top}else{HEAP32[eventStruct+60>>2]=0;HEAP32[eventStruct+64>>2]=0}if(target){var rect=JSEvents.getBoundingClientRectOrZeros(target);HEAP32[eventStruct+52>>2]=e.clientX-rect.left;HEAP32[eventStruct+56>>2]=e.clientY-rect.top}else{HEAP32[eventStruct+52>>2]=0;HEAP32[eventStruct+56>>2]=0}if(e.type!==\"wheel\"&&e.type!==\"mousewheel\"){JSEvents.previousScreenX=e.screenX;JSEvents.previousScreenY=e.screenY}})",
                           "fillMouseEventData:(function(eventStruct,e,target){var dpr=window.hbxDpr;HEAPF64[eventStruct>>3]=JSEvents.tick();HEAP32[eventStruct+8>>2]=e.screenX*dpr;HEAP32[eventStruct+12>>2]=e.screenY*dpr;HEAP32[eventStruct+16>>2]=e.clientX*dpr;HEAP32[eventStruct+20>>2]=e.clientY*dpr;HEAP32[eventStruct+24>>2]=e.ctrlKey;HEAP32[eventStruct+28>>2]=e.shiftKey;HEAP32[eventStruct+32>>2]=e.altKey;HEAP32[eventStruct+36>>2]=e.metaKey;HEAP16[eventStruct+40>>1]=e.button;HEAP16[eventStruct+42>>1]=e.buttons;HEAP32[eventStruct+44>>2]=e[\"movementX\"]||e[\"mozMovementX\"]||e[\"webkitMovementX\"]||(e.screenX*dpr)-JSEvents.previousScreenX;HEAP32[eventStruct+48>>2]=e[\"movementY\"]||e[\"mozMovementY\"]||e[\"webkitMovementY\"]||(e.screenY*dpr)-JSEvents.previousScreenY;if(Module[\"canvas\"]){var rect=Module[\"canvas\"].getBoundingClientRect();HEAP32[eventStruct+60>>2]=(e.clientX-rect.left)*dpr;HEAP32[eventStruct+64>>2]=(e.clientY-rect.top)*dpr}else{HEAP32[eventStruct+60>>2]=0;HEAP32[eventStruct+64>>2]=0}if(target){var rect=JSEvents.getBoundingClientRectOrZeros(target);HEAP32[eventStruct+52>>2]=(e.clientX-rect.left)*dpr;HEAP32[eventStruct+56>>2]=(e.clientY-rect.top)*dpr;}else{HEAP32[eventStruct+52>>2]=0;HEAP32[eventStruct+56>>2]=0}if(e.type!==\"wheel\"&&e.type!==\"mousewheel\"){JSEvents.previousScreenX=e.screenX*dpr;JSEvents.previousScreenY=e.screenY*dpr}})");
#else
            source.Replace("fillMouseEventData:(function(eventStruct,e,target){HEAPF64[eventStruct>>3]=JSEvents.tick();HEAP32[eventStruct+8>>2]=e.screenX;HEAP32[eventStruct+12>>2]=e.screenY;HEAP32[eventStruct+16>>2]=e.clientX;HEAP32[eventStruct+20>>2]=e.clientY;HEAP32[eventStruct+24>>2]=e.ctrlKey;HEAP32[eventStruct+28>>2]=e.shiftKey;HEAP32[eventStruct+32>>2]=e.altKey;HEAP32[eventStruct+36>>2]=e.metaKey;HEAP16[eventStruct+40>>1]=e.button;HEAP16[eventStruct+42>>1]=e.buttons;HEAP32[eventStruct+44>>2]=e[\"movementX\"]||e[\"mozMovementX\"]||e[\"webkitMovementX\"]||e.screenX-JSEvents.previousScreenX;HEAP32[eventStruct+48>>2]=e[\"movementY\"]||e[\"mozMovementY\"]||e[\"webkitMovementY\"]||e.screenY-JSEvents.previousScreenY;if(Module[\"canvas\"]){var rect=Module[\"canvas\"].getBoundingClientRect();HEAP32[eventStruct+60>>2]=e.clientX-rect.left;HEAP32[eventStruct+64>>2]=e.clientY-rect.top}else{HEAP32[eventStruct+60>>2]=0;HEAP32[eventStruct+64>>2]=0}if(target){var rect=JSEvents.getBoundingClientRectOrZeros(target);HEAP32[eventStruct+52>>2]=e.clientX-rect.left;HEAP32[eventStruct+56>>2]=e.clientY-rect.top}else{HEAP32[eventStruct+52>>2]=0;HEAP32[eventStruct+56>>2]=0}JSEvents.previousScreenX=e.screenX;JSEvents.previousScreenY=e.screenY})",
                           "fillMouseEventData:(function(eventStruct,e,target){var dpr=window.hbxDpr;HEAPF64[eventStruct>>3]=JSEvents.tick();HEAP32[eventStruct+8>>2]=e.screenX*dpr;HEAP32[eventStruct+12>>2]=e.screenY*dpr;HEAP32[eventStruct+16>>2]=e.clientX*dpr;HEAP32[eventStruct+20>>2]=e.clientY*dpr;HEAP32[eventStruct+24>>2]=e.ctrlKey;HEAP32[eventStruct+28>>2]=e.shiftKey;HEAP32[eventStruct+32>>2]=e.altKey;HEAP32[eventStruct+36>>2]=e.metaKey;HEAP16[eventStruct+40>>1]=e.button;HEAP16[eventStruct+42>>1]=e.buttons;HEAP32[eventStruct+44>>2]=e[\"movementX\"]||e[\"mozMovementX\"]||e[\"webkitMovementX\"]||(e.screenX*dpr)-JSEvents.previousScreenX;HEAP32[eventStruct+48>>2]=e[\"movementY\"]||e[\"mozMovementY\"]||e[\"webkitMovementY\"]||(e.screenY*dpr)-JSEvents.previousScreenY;if(Module[\"canvas\"]){var rect=Module[\"canvas\"].getBoundingClientRect();HEAP32[eventStruct+60>>2]=(e.clientX-rect.left)*dpr;HEAP32[eventStruct+64>>2]=(e.clientY-rect.top)*dpr}else{HEAP32[eventStruct+60>>2]=0;HEAP32[eventStruct+64>>2]=0}if(target){var rect=JSEvents.getBoundingClientRectOrZeros(target);HEAP32[eventStruct+52>>2]=(e.clientX-rect.left)*dpr;HEAP32[eventStruct+56>>2]=(e.clientY-rect.top)*dpr;}else{HEAP32[eventStruct+52>>2]=0;HEAP32[eventStruct+56>>2]=0}JSEvents.previousScreenX=e.screenX*dpr;JSEvents.previousScreenY=e.screenY*dpr})");
#endif

            if (slength != source.Length) _debugMessages.Add("Applied fix 01");

            // fix SystemInfo screen width height 
            slength = source.Length;
#if UNITY_2018_2_OR_NEWER // There's a stray newline in a release build (asm.js)
            source.Replace("return{width:screen.width?screen.width:0,\nheight:screen.height?screen.height:0,browser:i,",
                           "return{dpr:window.hbxDpr,width:screen.width?screen.width*this.dpr:0,height:screen.height?screen.height*this.dpr:0,browser:i,");
#elif UNITY_5_6_OR_NEWER
            source.Replace("return{width:screen.width?screen.width:0,height:screen.height?screen.height:0,browser:i,",
                            "return{dpr:window.hbxDpr,width:screen.width?screen.width*this.dpr:0,height:screen.height?screen.height*this.dpr:0,browser:i,");
#else
            source.Replace("var systemInfo={get:(function(){if(systemInfo.hasOwnProperty(\"hasWebGL\"))return this;var unknown=\"-\";this.width=screen.width?screen.width:0;this.height=screen.height?screen.height:0;",
                           "var systemInfo={get:(function(){if(systemInfo.hasOwnProperty(\"hasWebGL\"))return this;var unknown=\"-\";var dpr=window.hbxDpr;this.width=screen.width?screen.width*dpr:0;this.height=screen.height?screen.height*dpr:0;");

#endif
            if (slength != source.Length) _debugMessages.Add("Applied fix 02");

            // fix _JS_SystemInfo_GetCurrentCanvasHeight
            slength = source.Length;
            source.Replace("function _JS_SystemInfo_GetCurrentCanvasHeight(){return Module[\"canvas\"].clientHeight}",
                           "function _JS_SystemInfo_GetCurrentCanvasHeight(){return Module[\"canvas\"].clientHeight*window.hbxDpr;}");

            if (slength != source.Length) _debugMessages.Add("Applied fix 03");

            // fix get _JS_SystemInfo_GetCurrentCanvasWidth
            slength = source.Length;
            source.Replace("function _JS_SystemInfo_GetCurrentCanvasWidth(){return Module[\"canvas\"].clientWidth}",
                           "function _JS_SystemInfo_GetCurrentCanvasWidth(){return Module[\"canvas\"].clientWidth*window.hbxDpr;}");

            if (slength != source.Length) _debugMessages.Add("Applied fix 04");

            // fix updateCanvasDimensions (it removes the canvas style width height which prevents the fullscreen toggle via style)
            slength = source.Length;
            source.Replace("if((document[\"fullscreenElement\"]||document[\"mozFullScreenElement\"]||document[\"msFullscreenElement\"]||document[\"webkitFullscreenElement\"]||document[\"webkitCurrentFullScreenElement\"])===canvas.parentNode&&typeof screen!=\"undefined\"){var factor=Math.min(screen.width/w,screen.height/h);w=Math.round(w*factor);h=Math.round(h*factor)}if(Browser.resizeCanvas){if(canvas.width!=w)canvas.width=w;if(canvas.height!=h)canvas.height=h;if(typeof canvas.style!=\"undefined\"){canvas.style.removeProperty(\"width\");canvas.style.removeProperty(\"height\")}}else{if(canvas.width!=wNative)canvas.width=wNative;if(canvas.height!=hNative)canvas.height=hNative;if(typeof canvas.style!=\"undefined\"){if(w!=wNative||h!=hNative){canvas.style.setProperty(\"width\",w+\"px\",\"important\");canvas.style.setProperty(\"height\",h+\"px\",\"important\")}else{canvas.style.removeProperty(\"width\");canvas.style.removeProperty(\"height\")}}",
                           "var dpr=window.hbxDpr;if((document[\"fullscreenElement\"]||document[\"mozFullScreenElement\"]||document[\"msFullscreenElement\"]||document[\"webkitFullscreenElement\"]||document[\"webkitCurrentFullScreenElement\"])===canvas.parentNode&&typeof screen!=\"undefined\"){var factor=Math.min((screen.width*dpr)/w,(screen.height*dpr)/h);w=Math.round(w*factor);h=Math.round(h*factor)}if(Browser.resizeCanvas){if(canvas.width!=w)canvas.width=w;if(canvas.height!=h)canvas.height=h;if(typeof canvas.style!=\"undefined\"){canvas.style.removeProperty(\"width\");canvas.style.removeProperty(\"height\")}}else{if(canvas.width!=wNative)canvas.width=wNative;if(canvas.height!=hNative)canvas.height=hNative;if(typeof canvas.style!=\"undefined\"){if(!canvas.style.getPropertyValue(\"width\").includes(\"%\")){canvas.style.setProperty(\"width\",(w/dpr)+\"px\",\"important\");}if(!canvas.style.getPropertyValue(\"height\").includes(\"%\")){canvas.style.setProperty(\"height\",(h/dpr)+\"px\",\"important\")}}");

            if (slength != source.Length) _debugMessages.Add("Applied fix 05");

            // fix full screen dimensions
            slength = source.Length;
            source.Replace("HEAP32[eventStruct+264>>2]=reportedElement?reportedElement.clientWidth:0;HEAP32[eventStruct+268>>2]=reportedElement?reportedElement.clientHeight:0;HEAP32[eventStruct+272>>2]=screen.width;HEAP32[eventStruct+276>>2]=screen.height;",
                            "HEAP32[eventStruct+264>>2]=reportedElement?reportedElement.clientWidth:0;HEAP32[eventStruct+268>>2]=reportedElement?reportedElement.clientHeight:0;HEAP32[eventStruct+272>>2]=screen.width*window.hbxDpr;HEAP32[eventStruct+276>>2]=screen.height*window.hbxDpr;");

            if (slength != source.Length) _debugMessages.Add("Applied fix 06");

#if UNITY_5_6_OR_NEWER
            // fix touches
            slength = source.Length;
            source.Replace("for(var i in touches){var t=touches[i];HEAP32[ptr>>2]=t.identifier;HEAP32[ptr+4>>2]=t.screenX;HEAP32[ptr+8>>2]=t.screenY;HEAP32[ptr+12>>2]=t.clientX;HEAP32[ptr+16>>2]=t.clientY;HEAP32[ptr+20>>2]=t.pageX;HEAP32[ptr+24>>2]=t.pageY;HEAP32[ptr+28>>2]=t.changed;HEAP32[ptr+32>>2]=t.onTarget;if(canvasRect){HEAP32[ptr+44>>2]=t.clientX-canvasRect.left;HEAP32[ptr+48>>2]=t.clientY-canvasRect.top}else{HEAP32[ptr+44>>2]=0;HEAP32[ptr+48>>2]=0}HEAP32[ptr+36>>2]=t.clientX-targetRect.left;HEAP32[ptr+40>>2]=t.clientY-targetRect.top;ptr+=52;if(++numTouches>=32){break}}",
                           "var dpr=window.hbxDpr; for(var i in touches){var t=touches[i];HEAP32[ptr>>2]=t.identifier;HEAP32[ptr+4>>2]=t.screenX*dpr;HEAP32[ptr+8>>2]=t.screenY*dpr;HEAP32[ptr+12>>2]=t.clientX*dpr;HEAP32[ptr+16>>2]=t.clientY*dpr;HEAP32[ptr+20>>2]=t.pageX*dpr;HEAP32[ptr+24>>2]=t.pageY*dpr;HEAP32[ptr+28>>2]=t.changed;HEAP32[ptr+32>>2]=t.onTarget;if(canvasRect){HEAP32[ptr+44>>2]=(t.clientX-canvasRect.left)*dpr;HEAP32[ptr+48>>2]=(t.clientY-canvasRect.top)*dpr}else{HEAP32[ptr+44>>2]=0;HEAP32[ptr+48>>2]=0}HEAP32[ptr+36>>2]=(t.clientX-targetRect.left)*dpr;HEAP32[ptr+40>>2]=(t.clientY-targetRect.top)*dpr;ptr+=52;if(++numTouches>=32){break}}");

            if (slength != source.Length) _debugMessages.Add("Applied fix 07");
#endif

            // conditional edits

            // this only needs to apply to UnityLoader.js

            // insert dpr calc
            slength = source.Length;
#if UNITY_2018_1_OR_NEWER
            source.Replace("compatibilityCheck:function(e,t,r){",
               "compatibilityCheck:function(e,t,r){var dprs=UnityLoader.SystemInfo.mobile?" + MobileScale + ":" + DesktopScale + ";window.devicePixelRatio=window.devicePixelRatio||1;window.hbxDpr=window.devicePixelRatio*dprs;");
#else
            source.Replace("var UnityLoader=UnityLoader||{compatibilityCheck:function(e,t,r){",
               "var UnityLoader=UnityLoader||{compatibilityCheck:function(e,t,r){var dprs=UnityLoader.SystemInfo.mobile?" + MobileScale + ":" + DesktopScale + ";window.devicePixelRatio=window.devicePixelRatio||1;window.hbxDpr=window.devicePixelRatio*dprs;");

#endif
            if (slength != source.Length) _debugMessages.Add("Applied fix 08");

            if (DisableMobileCheck)
            {
                slength = source.Length;
                source.Replace("UnityLoader.SystemInfo.mobile?e.popup(\"Please note that Unity WebGL is not currently supported on mobiles. Press OK if you wish to continue anyway.\",[{text:\"OK\",callback:t}]):", "");
                if (slength != source.Length) _debugMessages.Add("Applied fix 09");
            }
            ApplyErrorMessageEdits(ref source);
        }

        //
        // Perform the find and replace hack for a development source
        //
        static void FixJSFileContents(bool iswasm, ref StringBuilder source)
        {
            int slength = source.Length;

#if UNITY_2018_2_OR_NEWER
            iswasm = false;
#endif

            // fix fill mouse event
            string findFillMouseString = "", replaceFillMouseString = "";
            if (!iswasm)
            {
#if UNITY_2018_2_OR_NEWER
                findFillMouseString =
@" fillMouseEventData: (function(eventStruct, e, target) {
  HEAPF64[eventStruct >> 3] = JSEvents.tick();
  HEAP32[eventStruct + 8 >> 2] = e.screenX;
  HEAP32[eventStruct + 12 >> 2] = e.screenY;
  HEAP32[eventStruct + 16 >> 2] = e.clientX;
  HEAP32[eventStruct + 20 >> 2] = e.clientY;
  HEAP32[eventStruct + 24 >> 2] = e.ctrlKey;
  HEAP32[eventStruct + 28 >> 2] = e.shiftKey;
  HEAP32[eventStruct + 32 >> 2] = e.altKey;
  HEAP32[eventStruct + 36 >> 2] = e.metaKey;
  HEAP16[eventStruct + 40 >> 1] = e.button;
  HEAP16[eventStruct + 42 >> 1] = e.buttons;
  HEAP32[eventStruct + 44 >> 2] = e[""movementX""] || e[""mozMovementX""] || e[""webkitMovementX""] || e.screenX - JSEvents.previousScreenX;
  HEAP32[eventStruct + 48 >> 2] = e[""movementY""] || e[""mozMovementY""] || e[""webkitMovementY""] || e.screenY - JSEvents.previousScreenY;
  if (Module[""canvas""]) {
   var rect = Module[""canvas""].getBoundingClientRect();
   HEAP32[eventStruct + 60 >> 2] = e.clientX - rect.left;
   HEAP32[eventStruct + 64 >> 2] = e.clientY - rect.top;
  } else {
   HEAP32[eventStruct + 60 >> 2] = 0;
   HEAP32[eventStruct + 64 >> 2] = 0;
  }
  if (target) {
   var rect = JSEvents.getBoundingClientRectOrZeros(target);
   HEAP32[eventStruct + 52 >> 2] = e.clientX - rect.left;
   HEAP32[eventStruct + 56 >> 2] = e.clientY - rect.top;
  } else {
   HEAP32[eventStruct + 52 >> 2] = 0;
   HEAP32[eventStruct + 56 >> 2] = 0;
  }
  if (e.type !== ""wheel"" && e.type !== ""mousewheel"") {
   JSEvents.previousScreenX = e.screenX;
   JSEvents.previousScreenY = e.screenY;
  }
 }),";

                replaceFillMouseString =
    @" fillMouseEventData: (function(eventStruct, e, target) {
  var devicePixelRatio = window.hbxDpr;
  HEAPF64[eventStruct >> 3] = JSEvents.tick();
  HEAP32[eventStruct + 8 >> 2] = e.screenX*devicePixelRatio;
  HEAP32[eventStruct + 12 >> 2] = e.screenY*devicePixelRatio;
  HEAP32[eventStruct + 16 >> 2] = e.clientX*devicePixelRatio;
  HEAP32[eventStruct + 20 >> 2] = e.clientY*devicePixelRatio;
  HEAP32[eventStruct + 24 >> 2] = e.ctrlKey;
  HEAP32[eventStruct + 28 >> 2] = e.shiftKey;
  HEAP32[eventStruct + 32 >> 2] = e.altKey;
  HEAP32[eventStruct + 36 >> 2] = e.metaKey;
  HEAP16[eventStruct + 40 >> 1] = e.button;
  HEAP16[eventStruct + 42 >> 1] = e.buttons;
  HEAP32[eventStruct + 44 >> 2] = e[""movementX""] || e[""mozMovementX""] || e[""webkitMovementX""] || (e.screenX*devicePixelRatio) - JSEvents.previousScreenX;
  HEAP32[eventStruct + 48 >> 2] = e[""movementY""] || e[""mozMovementY""] || e[""webkitMovementY""] || (e.screenY*devicePixelRatio) - JSEvents.previousScreenY;
  if (Module[""canvas""]) {
   var rect = Module[""canvas""].getBoundingClientRect();
   HEAP32[eventStruct + 60 >> 2] = (e.clientX - rect.left) * devicePixelRatio;
   HEAP32[eventStruct + 64 >> 2] = (e.clientY - rect.top) * devicePixelRatio;
  } else {
   HEAP32[eventStruct + 60 >> 2] = 0;
   HEAP32[eventStruct + 64 >> 2] = 0;
  }
  if (target) {
   var rect = JSEvents.getBoundingClientRectOrZeros(target);
   HEAP32[eventStruct + 52 >> 2] = (e.clientX - rect.left) * devicePixelRatio;
   HEAP32[eventStruct + 56 >> 2] = (e.clientY - rect.top) * devicePixelRatio;
  } else {
   HEAP32[eventStruct + 52 >> 2] = 0;
   HEAP32[eventStruct + 56 >> 2] = 0;
  }
  if (e.type !== ""wheel"" && e.type !== ""mousewheel"") {
   JSEvents.previousScreenX = e.screenX*devicePixelRatio;
   JSEvents.previousScreenY = e.screenY*devicePixelRatio;
  }
 }),";

#else
                findFillMouseString =
    @" fillMouseEventData: (function(eventStruct, e, target) {
  HEAPF64[eventStruct >> 3] = JSEvents.tick();
  HEAP32[eventStruct + 8 >> 2] = e.screenX;
  HEAP32[eventStruct + 12 >> 2] = e.screenY;
  HEAP32[eventStruct + 16 >> 2] = e.clientX;
  HEAP32[eventStruct + 20 >> 2] = e.clientY;
  HEAP32[eventStruct + 24 >> 2] = e.ctrlKey;
  HEAP32[eventStruct + 28 >> 2] = e.shiftKey;
  HEAP32[eventStruct + 32 >> 2] = e.altKey;
  HEAP32[eventStruct + 36 >> 2] = e.metaKey;
  HEAP16[eventStruct + 40 >> 1] = e.button;
  HEAP16[eventStruct + 42 >> 1] = e.buttons;
  HEAP32[eventStruct + 44 >> 2] = e[""movementX""] || e[""mozMovementX""] || e[""webkitMovementX""] || e.screenX - JSEvents.previousScreenX;
  HEAP32[eventStruct + 48 >> 2] = e[""movementY""] || e[""mozMovementY""] || e[""webkitMovementY""] || e.screenY - JSEvents.previousScreenY;
  if (Module[""canvas""]) {
   var rect = Module[""canvas""].getBoundingClientRect();
   HEAP32[eventStruct + 60 >> 2] = e.clientX - rect.left;
   HEAP32[eventStruct + 64 >> 2] = e.clientY - rect.top;
  } else {
   HEAP32[eventStruct + 60 >> 2] = 0;
   HEAP32[eventStruct + 64 >> 2] = 0;
  }
  if (target) {
   var rect = JSEvents.getBoundingClientRectOrZeros(target);
   HEAP32[eventStruct + 52 >> 2] = e.clientX - rect.left;
   HEAP32[eventStruct + 56 >> 2] = e.clientY - rect.top;
  } else {
   HEAP32[eventStruct + 52 >> 2] = 0;
   HEAP32[eventStruct + 56 >> 2] = 0;
  }
  JSEvents.previousScreenX = e.screenX;
  JSEvents.previousScreenY = e.screenY;
 }),";

                replaceFillMouseString =
    @" fillMouseEventData: (function(eventStruct, e, target) {
  var devicePixelRatio = window.hbxDpr;
  HEAPF64[eventStruct >> 3] = JSEvents.tick();
  HEAP32[eventStruct + 8 >> 2] = e.screenX*devicePixelRatio;
  HEAP32[eventStruct + 12 >> 2] = e.screenY*devicePixelRatio;
  HEAP32[eventStruct + 16 >> 2] = e.clientX*devicePixelRatio;
  HEAP32[eventStruct + 20 >> 2] = e.clientY*devicePixelRatio;
  HEAP32[eventStruct + 24 >> 2] = e.ctrlKey;
  HEAP32[eventStruct + 28 >> 2] = e.shiftKey;
  HEAP32[eventStruct + 32 >> 2] = e.altKey;
  HEAP32[eventStruct + 36 >> 2] = e.metaKey;
  HEAP16[eventStruct + 40 >> 1] = e.button;
  HEAP16[eventStruct + 42 >> 1] = e.buttons;
  HEAP32[eventStruct + 44 >> 2] = e[""movementX""] || e[""mozMovementX""] || e[""webkitMovementX""] || (e.screenX*devicePixelRatio) - JSEvents.previousScreenX;
  HEAP32[eventStruct + 48 >> 2] = e[""movementY""] || e[""mozMovementY""] || e[""webkitMovementY""] || (e.screenY*devicePixelRatio) - JSEvents.previousScreenY;
  if (Module[""canvas""]) {
   var rect = Module[""canvas""].getBoundingClientRect();
   HEAP32[eventStruct + 60 >> 2] = (e.clientX - rect.left) * devicePixelRatio;
   HEAP32[eventStruct + 64 >> 2] = (e.clientY - rect.top) * devicePixelRatio;
  } else {
   HEAP32[eventStruct + 60 >> 2] = 0;
   HEAP32[eventStruct + 64 >> 2] = 0;
  }
  if (target) {
   var rect = JSEvents.getBoundingClientRectOrZeros(target);
   HEAP32[eventStruct + 52 >> 2] = (e.clientX - rect.left) * devicePixelRatio;
   HEAP32[eventStruct + 56 >> 2] = (e.clientY - rect.top) * devicePixelRatio;
  } else {
   HEAP32[eventStruct + 52 >> 2] = 0;
   HEAP32[eventStruct + 56 >> 2] = 0;
  }
  JSEvents.previousScreenX = e.screenX*devicePixelRatio;
  JSEvents.previousScreenY = e.screenY*devicePixelRatio;
 }),";

#endif
            }
            else
            {

                findFillMouseString =
    @"fillMouseEventData:function (eventStruct, e, target) {
        HEAPF64[((eventStruct)>>3)]=JSEvents.tick();
        HEAP32[(((eventStruct)+(8))>>2)]=e.screenX;
        HEAP32[(((eventStruct)+(12))>>2)]=e.screenY;
        HEAP32[(((eventStruct)+(16))>>2)]=e.clientX;
        HEAP32[(((eventStruct)+(20))>>2)]=e.clientY;
        HEAP32[(((eventStruct)+(24))>>2)]=e.ctrlKey;
        HEAP32[(((eventStruct)+(28))>>2)]=e.shiftKey;
        HEAP32[(((eventStruct)+(32))>>2)]=e.altKey;
        HEAP32[(((eventStruct)+(36))>>2)]=e.metaKey;
        HEAP16[(((eventStruct)+(40))>>1)]=e.button;
        HEAP16[(((eventStruct)+(42))>>1)]=e.buttons;
        HEAP32[(((eventStruct)+(44))>>2)]=e[""movementX""] || e[""mozMovementX""] || e[""webkitMovementX""] || (e.screenX-JSEvents.previousScreenX);
        HEAP32[(((eventStruct)+(48))>>2)]=e[""movementY""] || e[""mozMovementY""] || e[""webkitMovementY""] || (e.screenY-JSEvents.previousScreenY);
  
        if (Module['canvas']) {
          var rect = Module['canvas'].getBoundingClientRect();
          HEAP32[(((eventStruct)+(60))>>2)]=e.clientX - rect.left;
          HEAP32[(((eventStruct)+(64))>>2)]=e.clientY - rect.top;
        } else { // Canvas is not initialized, return 0.
          HEAP32[(((eventStruct)+(60))>>2)]=0;
          HEAP32[(((eventStruct)+(64))>>2)]=0;
        }
        if (target) {
          var rect = JSEvents.getBoundingClientRectOrZeros(target);
          HEAP32[(((eventStruct)+(52))>>2)]=e.clientX - rect.left;
          HEAP32[(((eventStruct)+(56))>>2)]=e.clientY - rect.top;        
        } else { // No specific target passed, return 0.
          HEAP32[(((eventStruct)+(52))>>2)]=0;
          HEAP32[(((eventStruct)+(56))>>2)]=0;
        }
        JSEvents.previousScreenX = e.screenX;
        JSEvents.previousScreenY = e.screenY;
      },";

                replaceFillMouseString =
    @"fillMouseEventData:function (eventStruct, e, target) {
		var devicePixelRatio = window.hbxDpr;
        HEAPF64[((eventStruct)>>3)]=JSEvents.tick();
        HEAP32[(((eventStruct)+(8))>>2)]=e.screenX*devicePixelRatio;
        HEAP32[(((eventStruct)+(12))>>2)]=e.screenY*devicePixelRatio;
        HEAP32[(((eventStruct)+(16))>>2)]=e.clientX*devicePixelRatio;
        HEAP32[(((eventStruct)+(20))>>2)]=e.clientY*devicePixelRatio;
        HEAP32[(((eventStruct)+(24))>>2)]=e.ctrlKey;
        HEAP32[(((eventStruct)+(28))>>2)]=e.shiftKey;
        HEAP32[(((eventStruct)+(32))>>2)]=e.altKey;
        HEAP32[(((eventStruct)+(36))>>2)]=e.metaKey;
        HEAP16[(((eventStruct)+(40))>>1)]=e.button;
        HEAP16[(((eventStruct)+(42))>>1)]=e.buttons;
        HEAP32[(((eventStruct)+(44))>>2)]=e[""movementX""] || e[""mozMovementX""] || e[""webkitMovementX""] || ((e.screenX*devicePixelRatio)-JSEvents.previousScreenX);
        HEAP32[(((eventStruct)+(48))>>2)]=e[""movementY""] || e[""mozMovementY""] || e[""webkitMovementY""] || ((e.screenY*devicePixelRatio)-JSEvents.previousScreenY);
  
        if (Module['canvas']) {
          var rect = Module['canvas'].getBoundingClientRect();
          HEAP32[(((eventStruct)+(60))>>2)]=(e.clientX - rect.left)*devicePixelRatio;
          HEAP32[(((eventStruct)+(64))>>2)]=(e.clientY - rect.top)*devicePixelRatio;
        } else { // Canvas is not initialized, return 0.
          HEAP32[(((eventStruct)+(60))>>2)]=0;
          HEAP32[(((eventStruct)+(64))>>2)]=0;
        }
        if (target) {
          var rect = JSEvents.getBoundingClientRectOrZeros(target);
          HEAP32[(((eventStruct)+(52))>>2)]=(e.clientX - rect.left)*devicePixelRatio;
          HEAP32[(((eventStruct)+(56))>>2)]=(e.clientY - rect.top)*devicePixelRatio;        
        } else { // No specific target passed, return 0.
          HEAP32[(((eventStruct)+(52))>>2)]=0;
          HEAP32[(((eventStruct)+(56))>>2)]=0;
        }
        JSEvents.previousScreenX = e.screenX*devicePixelRatio;
        JSEvents.previousScreenY = e.screenY*devicePixelRatio;
      },";

            }

            source.Replace(findFillMouseString, replaceFillMouseString);
            if (slength != source.Length) _debugMessages.Add("Applied fix 01");

#if UNITY_5_6_OR_NEWER
			// fix SystemInfo screen width height 
			string findSystemInfoString = 
@"    return {
      width: screen.width ? screen.width : 0,
      height: screen.height ? screen.height : 0,
      browser: browser,";

			string replaceSystemInfoString = 
@"    return {
      devicePixelRatio: window.hbxDpr,
      width: screen.width ? screen.width * this.devicePixelRatio : 0,
      height: screen.height ? screen.height * this.devicePixelRatio : 0,
      browser: browser,";
#else
            // fix SystemInfo screen width height 
            string findSystemInfoString =
@"var systemInfo = {
 get: (function() {
  if (systemInfo.hasOwnProperty(""hasWebGL"")) return this;
  var unknown = ""-"";
  this.width = screen.width ? screen.width : 0;
  this.height = screen.height ? screen.height : 0;";

            string replaceSystemInfoString =
@"var systemInfo = {
 get: (function() {
  if (systemInfo.hasOwnProperty(""hasWebGL"")) return this;
  var unknown = ""-"";
  var devicePixelRatio = window.hbxDpr;
  this.width = screen.width ? screen.width*devicePixelRatio : 0;
  this.height = screen.height ? screen.height*devicePixelRatio : 0;";
#endif

            slength = source.Length;
            source.Replace(findSystemInfoString, replaceSystemInfoString);
            if (slength != source.Length) _debugMessages.Add("Applied fix 02");


            // fix _JS_SystemInfo_GetCurrentCanvasHeight

            string findGetCurrentCanvasHeightString = !iswasm ?
@"function _JS_SystemInfo_GetCurrentCanvasHeight() {
 return Module[""canvas""].clientHeight;
}" :
@"function _JS_SystemInfo_GetCurrentCanvasHeight() 
  	{
  		return Module['canvas'].clientHeight;
  	}";

            string replaceGetCurrentCanvasHeightString =
@"function _JS_SystemInfo_GetCurrentCanvasHeight() {
 return Module[""canvas""].clientHeight*window.hbxDpr;
}";

            slength = source.Length;
            source.Replace(findGetCurrentCanvasHeightString, replaceGetCurrentCanvasHeightString);
            if (slength != source.Length) _debugMessages.Add("Applied fix 03");


            // fix get _JS_SystemInfo_GetCurrentCanvasWidth

            string findGetCurrentCanvasWidthString = !iswasm ?
@"function _JS_SystemInfo_GetCurrentCanvasWidth() {
 return Module[""canvas""].clientWidth;
}" :
@"function _JS_SystemInfo_GetCurrentCanvasWidth() 
  	{
  		return Module['canvas'].clientWidth;
  	}";

            string replaceGetCurrentCanvasWidthString =
@"function _JS_SystemInfo_GetCurrentCanvasWidth() {
 return Module[""canvas""].clientWidth*window.hbxDpr;
}";

            slength = source.Length;
            source.Replace(findGetCurrentCanvasWidthString, replaceGetCurrentCanvasWidthString);
            if (slength != source.Length) _debugMessages.Add("Applied fix 04");


            // fix updateCanvasDimensions

            string findUpdateCanvasString = !iswasm ?
@"if ((document[""fullscreenElement""] || document[""mozFullScreenElement""] || document[""msFullscreenElement""] || document[""webkitFullscreenElement""] || document[""webkitCurrentFullScreenElement""]) === canvas.parentNode && typeof screen != ""undefined"") {
   var factor = Math.min(screen.width / w, screen.height / h);
   w = Math.round(w * factor);
   h = Math.round(h * factor);
  }
  if (Browser.resizeCanvas) {
   if (canvas.width != w) canvas.width = w;
   if (canvas.height != h) canvas.height = h;
   if (typeof canvas.style != ""undefined"") {
    canvas.style.removeProperty(""width"");
    canvas.style.removeProperty(""height"");
   }
  } else {
   if (canvas.width != wNative) canvas.width = wNative;
   if (canvas.height != hNative) canvas.height = hNative;
   if (typeof canvas.style != ""undefined"") {
    if (w != wNative || h != hNative) {
     canvas.style.setProperty(""width"", w + ""px"", ""important"");
     canvas.style.setProperty(""height"", h + ""px"", ""important"");
    } else {
     canvas.style.removeProperty(""width"");
     canvas.style.removeProperty(""height"");
    }
   }
  }" :
@"if (((document['fullscreenElement'] || document['mozFullScreenElement'] ||
             document['msFullscreenElement'] || document['webkitFullscreenElement'] ||
             document['webkitCurrentFullScreenElement']) === canvas.parentNode) && (typeof screen != 'undefined')) {
           var factor = Math.min(screen.width / w, screen.height / h);
           w = Math.round(w * factor);
           h = Math.round(h * factor);
        }
        if (Browser.resizeCanvas) {
          if (canvas.width  != w) canvas.width  = w;
          if (canvas.height != h) canvas.height = h;
          if (typeof canvas.style != 'undefined') {
            canvas.style.removeProperty( ""width"");
            canvas.style.removeProperty(""height"");
          }
        } else {
          if (canvas.width  != wNative) canvas.width  = wNative;
          if (canvas.height != hNative) canvas.height = hNative;
          if (typeof canvas.style != 'undefined') {
            if (w != wNative || h != hNative) {
              canvas.style.setProperty( ""width"", w + ""px"", ""important"");
              canvas.style.setProperty(""height"", h + ""px"", ""important"");
            } else {
              canvas.style.removeProperty( ""width"");
              canvas.style.removeProperty(""height"");
            }
          }
        }";

            string replaceUpdateCanvasString =
@"var dpr = window.hbxDpr;
  if ((document[""fullscreenElement""] || document[""mozFullScreenElement""] || document[""msFullscreenElement""] || document[""webkitFullscreenElement""] || document[""webkitCurrentFullScreenElement""]) === canvas.parentNode && typeof screen != ""undefined"") {
   var factor = Math.min((screen.width*dpr) / w, (screen.height*dpr) / h);
   w = Math.round(w * factor);
   h = Math.round(h * factor);
  }
  if (Browser.resizeCanvas) {
   if (canvas.width != w) canvas.width = w;
   if (canvas.height != h) canvas.height = h;
   if (typeof canvas.style != ""undefined"") {
    canvas.style.removeProperty(""width"");
    canvas.style.removeProperty(""height"");
   }
  } else {
   if (canvas.width != wNative) canvas.width = wNative;
   if (canvas.height != hNative) canvas.height = hNative;
   if (typeof canvas.style != ""undefined"") {
     if(!canvas.style.getPropertyValue(""width"").includes(""%""))canvas.style.setProperty(""width"", (w/dpr) + ""px"", ""important"");
     if(!canvas.style.getPropertyValue(""height"").includes(""%""))canvas.style.setProperty(""height"", (h/dpr) + ""px"", ""important"");
   }
  }";

            slength = source.Length;
            source.Replace(findUpdateCanvasString, replaceUpdateCanvasString);
            if (slength != source.Length) _debugMessages.Add("Applied fix 05");


            string findFullscreenEventString = !iswasm ?
@"  HEAP32[eventStruct + 264 >> 2] = reportedElement ? reportedElement.clientWidth : 0;
  HEAP32[eventStruct + 268 >> 2] = reportedElement ? reportedElement.clientHeight : 0;
  HEAP32[eventStruct + 272 >> 2] = screen.width;
  HEAP32[eventStruct + 276 >> 2] = screen.height;" :
@"        HEAP32[(((eventStruct)+(264))>>2)]=reportedElement ? reportedElement.clientWidth : 0;
        HEAP32[(((eventStruct)+(268))>>2)]=reportedElement ? reportedElement.clientHeight : 0;
        HEAP32[(((eventStruct)+(272))>>2)]=screen.width;
        HEAP32[(((eventStruct)+(276))>>2)]=screen.height;";

            string replaceFullscreenEventString =
@"  HEAP32[eventStruct + 264 >> 2] = reportedElement ? reportedElement.clientWidth : 0;
  HEAP32[eventStruct + 268 >> 2] = reportedElement ? reportedElement.clientHeight : 0;
  HEAP32[eventStruct + 272 >> 2] = screen.width * window.hbxDpr;
  HEAP32[eventStruct + 276 >> 2] = screen.height * window.hbxDpr;";

            slength = source.Length;
            source.Replace(findFullscreenEventString, replaceFullscreenEventString);
            if (slength != source.Length) _debugMessages.Add("Applied fix 06");


#if UNITY_5_6_OR_NEWER
		//
		// touches
			string findTouchesString = 
@"for (var i in touches) {
    var t = touches[i];
    HEAP32[ptr >> 2] = t.identifier;
    HEAP32[ptr + 4 >> 2] = t.screenX;
    HEAP32[ptr + 8 >> 2] = t.screenY;
    HEAP32[ptr + 12 >> 2] = t.clientX;
    HEAP32[ptr + 16 >> 2] = t.clientY;
    HEAP32[ptr + 20 >> 2] = t.pageX;
    HEAP32[ptr + 24 >> 2] = t.pageY;
    HEAP32[ptr + 28 >> 2] = t.changed;
    HEAP32[ptr + 32 >> 2] = t.onTarget;
    if (canvasRect) {
     HEAP32[ptr + 44 >> 2] = t.clientX - canvasRect.left;
     HEAP32[ptr + 48 >> 2] = t.clientY - canvasRect.top;
    } else {
     HEAP32[ptr + 44 >> 2] = 0;
     HEAP32[ptr + 48 >> 2] = 0;
    }
    HEAP32[ptr + 36 >> 2] = t.clientX - targetRect.left;
    HEAP32[ptr + 40 >> 2] = t.clientY - targetRect.top;
    ptr += 52;
    if (++numTouches >= 32) {
     break;
    }
   }";

			string replaceTouchesString = 
@" var devicePixelRatio = window.hbxDpr;
   for (var i in touches) {
    var t = touches[i];
    HEAP32[ptr >> 2] = t.identifier;
    HEAP32[ptr + 4 >> 2] = t.screenX*devicePixelRatio;
    HEAP32[ptr + 8 >> 2] = t.screenY*devicePixelRatio;
    HEAP32[ptr + 12 >> 2] = t.clientX*devicePixelRatio;
    HEAP32[ptr + 16 >> 2] = t.clientY*devicePixelRatio;
    HEAP32[ptr + 20 >> 2] = t.pageX*devicePixelRatio;
    HEAP32[ptr + 24 >> 2] = t.pageY*devicePixelRatio;
    HEAP32[ptr + 28 >> 2] = t.changed;
    HEAP32[ptr + 32 >> 2] = t.onTarget;
    if (canvasRect) {
     HEAP32[ptr + 44 >> 2] = (t.clientX - canvasRect.left) * devicePixelRatio;
     HEAP32[ptr + 48 >> 2] = (t.clientY - canvasRect.top) * devicePixelRatio;
    } else {
     HEAP32[ptr + 44 >> 2] = 0;
     HEAP32[ptr + 48 >> 2] = 0;
    }
    HEAP32[ptr + 36 >> 2] = (t.clientX - targetRect.left) * devicePixelRatio;
    HEAP32[ptr + 40 >> 2] = (t.clientY - targetRect.top) * devicePixelRatio;
    ptr += 52;
    if (++numTouches >= 32) {
     break;
    }
   }";
            slength = source.Length;
			source.Replace(findTouchesString, replaceTouchesString);
            if (slength != source.Length) _debugMessages.Add("Applied fix 07");

#endif

            // instert dpr initalise code
#if UNITY_2018_1_OR_NEWER

            // this only needs to apply to UnityLoader.js
#if UNITY_2019_1_OR_NEWER
            string findDPRInsertPoint =
@"compatibilityCheck: function (unityInstance, onsuccess, onerror) {";
#else
            string findDPRInsertPoint =
@"compatibilityCheck: function (gameInstance, onsuccess, onerror) {";
#endif


            string replaceDPRInsertPoint = findDPRInsertPoint +
@"
    var dprs = UnityLoader.SystemInfo.mobile ? " + MobileScale + " : " + DesktopScale + @";
    window.devicePixelRatio = window.devicePixelRatio || 1;
    window.hbxDpr = window.devicePixelRatio * dprs;";

            slength = source.Length;
            source.Replace(findDPRInsertPoint, replaceDPRInsertPoint);
            if (slength != source.Length) _debugMessages.Add("Applied fix 08");

#else
            // this only needs to apply to UnityLoader.js
            string findDPRInsertPoint =
@"var UnityLoader = UnityLoader || {
  compatibilityCheck: function (gameInstance, onsuccess, onerror) {";

            string replaceDPRInsertPoint =
@"var UnityLoader = UnityLoader || {
  compatibilityCheck: function (gameInstance, onsuccess, onerror) {
    var dprs = UnityLoader.SystemInfo.mobile ? " + MobileScale + " : " + DesktopScale + @";
    window.devicePixelRatio = window.devicePixelRatio || 1;
    window.hbxDpr = window.devicePixelRatio * dprs;";

            slength = source.Length;
            source.Replace(findDPRInsertPoint, replaceDPRInsertPoint);
            if (slength != source.Length) _debugMessages.Add("Applied fix 08");
#endif

            // conditional edits

#if UNITY_2019_1_OR_NEWER
            string findMobileCheckString =
@"else if (UnityLoader.SystemInfo.mobile) {
      unityInstance.popup(""Please note that Unity WebGL is not currently supported on mobiles. Press OK if you wish to continue anyway."",
        [{text: ""OK"", callback: onsuccess}]);
    } ";
#else
            string findMobileCheckString = 
@"else if (UnityLoader.SystemInfo.mobile) {
      gameInstance.popup(""Please note that Unity WebGL is not currently supported on mobiles. Press OK if you wish to continue anyway."",
        [{text: ""OK"", callback: onsuccess}]);
    } ";
#endif

            if (DisableMobileCheck)
			{
                slength = source.Length;
				source.Replace(findMobileCheckString, "");
                if (slength != source.Length) _debugMessages.Add("Applied fix 09");
			}

			ApplyErrorMessageEdits(ref source);
		}

		//
		// Edit the UnityLoader.js error messages
		static void ApplyErrorMessageEdits(ref StringBuilder source)
		{
			// mobile check, only edit this if it's not being removed
			if(!DisableMobileCheck)
			{
				if(!string.IsNullOrEmpty(MobileWarningMessage))
				{
					source.Replace(OriginalMobileWarningMessage, MobileWarningMessage);
				}
			}

			if(!string.IsNullOrEmpty(GenericErrorMessage))
			{
				source.Replace(OriginalGenericErrorMessage, GenericErrorMessage);
			}

			if(!string.IsNullOrEmpty(UnhandledExceptionMessage))
			{
				source.Replace(OriginalUnhandledExceptionMessage, UnhandledExceptionMessage);
			}

			if(!string.IsNullOrEmpty(OutOfMemoryMessage))
			{
				source.Replace(OriginalOutOfMemoryMessage, OutOfMemoryMessage);
			}

			if(!string.IsNullOrEmpty(NotEnoughMemoryMessage))
			{
				source.Replace(OriginalNotEnoughMemoryMessage, NotEnoughMemoryMessage);
			}
		}
	}

}
