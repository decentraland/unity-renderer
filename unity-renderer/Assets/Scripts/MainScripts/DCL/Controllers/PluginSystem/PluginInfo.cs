using System;
using UnityEngine;

namespace DCL
{
    public class PluginInfo
    {
        public bool isEnabled => instance != null;
        public string flag;
        public PluginBuilder builder;
        public IPlugin instance;
        public bool enableOnInit = false;

        public void Enable()
        {
            if ( isEnabled )
                return;

            try
            {
                instance = builder.Invoke();
                enableOnInit = false;
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
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
