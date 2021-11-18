using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace DCL
{
    public class PluginGroup
    {
        public HashSet<IPlugin> plugins;

        public bool Add(IPlugin plugin)
        {
            if ( plugins.Contains(plugin) )
                return false;

            plugins.Add(plugin);
            return true;
        }

        public bool Remove(IPlugin plugin)
        {
            if ( !plugins.Contains(plugin))
                return false;

            plugins.Remove(plugin);
            return true;
        }
    }

    public class PluginSystemV2 : IDisposable
    {
        private static ILogger logger = new Logger(Debug.unityLogger);
        private PluginGroup allPlugins = new PluginGroup();
        private Dictionary<string, PluginGroup> pluginGroupByFlag = new Dictionary<string, PluginGroup>();
        private Dictionary<IPlugin, string> flagByPlugin = new Dictionary<IPlugin, string>();
        private BaseVariable<FeatureFlag> featureFlagsDataSource;

        public void RegisterWithFlag( IPlugin plugin, string featureFlag )
        {
            Register(plugin, false);
            ConfigureFlag(plugin, featureFlag);
        }

        public void Register(IPlugin plugin, bool enable = true)
        {
            Assert.IsNotNull(plugin);

            if ( allPlugins.plugins.Contains(plugin))
                return;

            allPlugins.Add(plugin);

            if (enable)
                plugin.Enable();
        }

        public void Unregister(IPlugin plugin)
        {
            if ( !allPlugins.plugins.Contains(plugin))
                return;

            string flag = flagByPlugin[plugin];

            allPlugins.Remove(plugin);
            pluginGroupByFlag[flag].Remove(plugin);
            flagByPlugin.Remove(plugin);
        }

        public void ConfigureFlag(IPlugin plugin, string featureFlag)
        {
            Assert.IsNotNull(plugin);
            Assert.IsTrue(allPlugins.plugins.Contains(plugin), $"Plugin for {featureFlag} should be registered first!");
            Assert.IsFalse(flagByPlugin.ContainsKey(plugin), $"Plugin flag was already configured! ({featureFlag})");

            if ( !pluginGroupByFlag.ContainsKey(featureFlag) )
                pluginGroupByFlag.Add(featureFlag, new PluginGroup());

            flagByPlugin.Add(plugin, featureFlag);
            pluginGroupByFlag[featureFlag].Add(plugin);
        }

        public void SetFlag(string featureFlag, bool value)
        {
            if (value)
                EnableFlag(featureFlag);
            else
                DisableFlag(featureFlag);
        }

        public void EnableFlag(string featureFlag)
        {
            PluginGroup pluginGroup = pluginGroupByFlag[featureFlag];

            foreach ( var feature in pluginGroup.plugins )
            {
                if ( feature.enabled )
                    continue;

                feature.Enable();
            }
        }

        public void DisableFlag(string featureFlag)
        {
            PluginGroup pluginGroup = pluginGroupByFlag[featureFlag];

            foreach ( var feature in pluginGroup.plugins )
            {
                if ( !feature.enabled )
                    continue;

                feature.Disable();
            }
        }

        public void SetFeatureFlagsData(BaseVariable<FeatureFlag> flags)
        {
            if ( featureFlagsDataSource != null )
                featureFlagsDataSource.OnChange -= OnFeatureFlagsChange;

            featureFlagsDataSource = flags;
            featureFlagsDataSource.OnChange += OnFeatureFlagsChange;
        }

        private void OnFeatureFlagsChange(FeatureFlag current, FeatureFlag previous)
        {
            foreach ( var flag in current.flags )
            {
                SetFlag(flag.Key, flag.Value);
            }
        }

        public void Dispose()
        {
            foreach ( var feature in allPlugins.plugins )
            {
                feature.Dispose();
            }

            if ( featureFlagsDataSource != null )
                featureFlagsDataSource.OnChange -= OnFeatureFlagsChange;
        }

        public void Update()
        {
            foreach ( var feature in allPlugins.plugins )
            {
                feature.Update();
            }
        }

        public void LateUpdate()
        {
            foreach ( var feature in allPlugins.plugins )
            {
                feature.LateUpdate();
            }
        }

        public void OnGUI()
        {
            foreach ( var feature in allPlugins.plugins )
            {
                feature.OnGUI();
            }
        }
    }
}