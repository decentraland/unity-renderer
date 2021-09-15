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