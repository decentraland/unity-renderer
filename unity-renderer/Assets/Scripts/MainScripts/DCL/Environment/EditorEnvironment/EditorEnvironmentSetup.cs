using System;

namespace DCL.EditorEnvironment
{
    public class EditorEnvironmentSetup
    {
        public static IDisposable Execute()
        {
            var serviceLocator = ServiceLocatorEditorFactory.Create();
            Environment.Setup(serviceLocator);
            return Environment.i;
        }
    }
}
