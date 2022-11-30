using System.Collections.Generic;
using Altom.AltTester;
using UnityEngine;

namespace Altom.AltTesterEditor
{
    public class AltEditorConfiguration : ScriptableObject
    {
        public bool appendToName;
        public string AdbPath = "/usr/local/bin/adb";
        public string IProxyPath = "/usr/local/bin/iproxy";
        public string XcrunPath = "/usr/bin/xcrun";
        public List<AltMyTest> MyTests = new List<AltMyTest>();
        public List<AltMyScenes> Scenes = new List<AltMyScenes>();
        public AltPlatform platform = AltPlatform.Editor;
        public UnityEditor.BuildTarget StandaloneTarget = UnityEditor.BuildTarget.StandaloneWindows;
        public bool RanInEditor = false;
        public bool ScenePathDisplayed;
        public bool InputVisualizer;
        public bool ShowPopUp = true;
        public string BuildLocationPath = "";
        public string LatestDesktopVersion = "";
        public bool ShowDesktopPopUpInEditor = false;
        public bool createXMLReport = false;
        public string xMLFilePath = "";

        public int ProxyPort = 13000;
        public string ProxyHost = "127.0.0.1";

        public string GameName = "__default__";

        public AltInstrumentationSettings GetInstrumentationSettings()
        {
            return new AltInstrumentationSettings()
            {
                ShowPopUp = ShowPopUp,
                InputVisualizer = InputVisualizer,
                ProxyPort = ProxyPort,
                ProxyHost = ProxyHost,
                GameName = GameName
            };
        }
        public bool KeepAUTSymbolDefined = false;
    }
}
