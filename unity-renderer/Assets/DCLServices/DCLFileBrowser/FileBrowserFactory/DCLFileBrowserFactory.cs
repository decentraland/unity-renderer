namespace DCLServices.DCLFileBrowser.DCLFileBrowserFactory
{
    public static class DCLFileBrowserFactory
    {
        public static IDCLFileBrowserService GetFileBrowserService()
        {

#if UNITY_WEBGL && !UNITY_EDITOR
UnityEngine.Debug.Log($"DCLFileBrowserFactory.GetFileBrowserService UNITY_WEBGL && !UNITY_EDITOR");
            return new DCLFileBrowserServiceWebGL();
#endif
#if FILE_BROWSER_PRESENT
            UnityEngine.Debug.Log($"DCLFileBrowserFactory.GetFileBrowserService FILE_BROWSER_PRESENT is defined");
            return new FileBrowserIntegration.FileBrowserWrapper();
#endif
            UnityEngine.Debug.Log($"DCLFileBrowserFactory.GetFileBrowserService FILE_BROWSER_PRESENT is NOT defined, returning service mock!");
            return new DCLFileBrowserServiceMock();
        }
    }
}
