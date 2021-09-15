#if UNITY_WEBGL && !UNITY_EDITOR
using System.Runtime.InteropServices;
#endif

namespace MainScripts.DCL.WebPlugin
{
    public static class WebGLPlugin
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")]
        public static extern string GetUserAgent();
#else
        public static string GetUserAgent() { return ""; }
#endif
    }
}