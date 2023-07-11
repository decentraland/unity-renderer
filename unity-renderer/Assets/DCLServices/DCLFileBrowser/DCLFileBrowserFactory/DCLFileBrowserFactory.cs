namespace DCLServices.DCLFileBrowser.DCLFileBrowserFactory
{
    public static class DCLFileBrowserFactory
    {
        public static IDCLFileBrowserService GetFileBrowserService()
        {
#if FILE_BROWSER_PRESENT
            return new FileBrowserIntegration.FileBrowserWrapper();
#endif
            return new DCLFileBrowserServiceMock();
        }
    }
}
