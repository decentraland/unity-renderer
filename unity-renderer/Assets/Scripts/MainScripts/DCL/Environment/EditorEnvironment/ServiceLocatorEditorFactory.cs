namespace DCL.EditorEnvironment
{
    /// <summary>
    /// Create Service Locator for Editor scripts
    /// </summary>
    public class ServiceLocatorEditorFactory
    {
        public static ServiceLocator Create()
        {
            var result = new ServiceLocator();

            // Platform
            result.Register<IWebRequestController>(WebRequestController.Create);

            return result;
        }
    }
}
