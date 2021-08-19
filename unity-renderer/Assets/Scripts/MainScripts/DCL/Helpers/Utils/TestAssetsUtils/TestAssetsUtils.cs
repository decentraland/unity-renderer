using UnityEngine;

namespace DCL.Helpers
{
    public static class TestAssetsUtils
    {
        public static string GetPathRaw() { return Application.dataPath + "/../TestResources"; }

        public static string GetPath(bool useWebServerPath = false)
        {
            if (useWebServerPath)
            {
                return "http://127.0.0.1:9991";
            }
            else
            {
                var uri = new System.Uri(GetPathRaw());
                var converted = uri.AbsoluteUri;
                return converted;
            }
        }

        public static string GetAbsolutePath(bool useWebServerPath = false)
        {
            if (useWebServerPath)
            {
                return "http://127.0.0.1:9991";
            }
            else
            {
                var uri = new System.Uri(GetPathRaw());
                var converted = uri.AbsolutePath;
                return converted;
            }
        }
    }
}