using System;
using System.Collections.Generic;

namespace DCL
{
    public class PluginGroup
    {
        public Dictionary<Type, PluginInfo> plugins = new Dictionary<Type, PluginInfo>();

        public bool Add(Type type, PluginInfo pluginInfo)
        {
            if ( plugins.ContainsKey(type) )
                return false;

            plugins.Add(type, pluginInfo);
            return true;
        }

        public bool Remove(Type type)
        {
            if ( !plugins.ContainsKey(type))
                return false;

            plugins.Remove(type);
            return true;
        }

        public bool ContainsKey(Type type)
        {
            return plugins.ContainsKey(type);
        }
    }
}