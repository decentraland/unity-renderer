using System.Collections.Generic;

namespace DCL
{
    public class PluginGroup
    {
        public Dictionary<PluginBuilder, PluginInfo> plugins = new Dictionary<PluginBuilder, PluginInfo>();

        public bool Add(PluginBuilder plugin, PluginInfo pluginInfo)
        {
            if ( plugins.ContainsKey(plugin) )
                return false;

            plugins.Add(plugin, pluginInfo);
            return true;
        }

        public bool Remove(PluginBuilder plugin)
        {
            if ( !plugins.ContainsKey(plugin))
                return false;

            plugins.Remove(plugin);
            return true;
        }
    }
}