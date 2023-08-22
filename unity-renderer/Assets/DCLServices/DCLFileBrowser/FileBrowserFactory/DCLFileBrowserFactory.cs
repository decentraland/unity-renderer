namespace DCLServices.DCLFileBrowser.DCLFileBrowserFactory
{
    public static class DCLFileBrowserFactory
    {
        public static IDCLFileBrowserService GetFileBrowserService()
        {

#if UNITY_WEBGL && !UNITY_EDITOR
            return new DCLFileBrowserServiceWebGL();
#endif

#if FILE_BROWSER_PRESENT
            return new FileBrowserIntegration.FileBrowserWrapper();
#endif
            return new DCLFileBrowserServiceMock();
        }
    }
}
