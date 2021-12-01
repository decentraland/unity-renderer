namespace DCL
{
    public class PluginInfo
    {
        public bool isEnabled => instance != null;
        public string flag;
        public PluginBuilder builder;
        public IPlugin instance;

        public void Enable()
        {
            if ( isEnabled )
                return;

            instance = builder.Invoke();
        }

        public void Disable()
        {
            if ( !isEnabled )
                return;

            instance.Dispose();
            instance = null;
        }
    }
}