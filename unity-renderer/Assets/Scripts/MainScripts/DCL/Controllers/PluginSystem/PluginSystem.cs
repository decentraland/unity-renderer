using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace DCL
{
    public delegate IPlugin PluginBuilder();

    public class PluginInfo
    {
        public bool enabled;
        public string flag;
        public PluginBuilder builder;
        public IPlugin instance;
    }

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

    public class PluginSystem : IDisposable
    {
        private static ILogger logger = new Logger(Debug.unityLogger);
        private PluginGroup allPlugins = new PluginGroup();
        private Dictionary<string, PluginGroup> pluginGroupByFlag = new Dictionary<string, PluginGroup>();
        private BaseVariable<FeatureFlag> featureFlagsDataSource;

        public bool IsEnabled(PluginBuilder pluginBuilder)
        {
            if (!allPlugins.plugins.ContainsKey(pluginBuilder))
                return false;

            return allPlugins.plugins[pluginBuilder].enabled;
        }

        public void RegisterWithFlag(PluginBuilder pluginBuilder, string featureFlag)
        {
            Register(pluginBuilder, false);
            ConfigureFlag(pluginBuilder, featureFlag);
        }

        public void Register(PluginBuilder pluginBuilder, bool enable = true)
        {
            Assert.IsNotNull(pluginBuilder);

            PluginInfo pluginInfo = new PluginInfo() { builder = pluginBuilder, enabled = false };
            allPlugins.Add(pluginBuilder, pluginInfo);

            if (enable)
            {
                pluginInfo.enabled = true;
                pluginInfo.instance = pluginInfo.builder.Invoke();
                pluginInfo.instance.Initialize();
            }
        }

        public void Unregister(PluginBuilder plugin)
        {
            if ( !allPlugins.plugins.ContainsKey(plugin))
                return;

            string flag = allPlugins.plugins[plugin].flag;

            allPlugins.Remove(plugin);
            pluginGroupByFlag[flag].Remove(plugin);
        }

        public void ConfigureFlag(PluginBuilder plugin, string featureFlag)
        {
            Assert.IsNotNull(plugin);
            //Assert.IsTrue(allPlugins.plugins.Contains(plugin), $"Plugin for {featureFlag} should be registered first!");
            //Assert.IsFalse(flagByPlugin.ContainsKey(plugin), $"Plugin flag was already configured! ({featureFlag})");

            if ( !pluginGroupByFlag.ContainsKey(featureFlag) )
                pluginGroupByFlag.Add(featureFlag, new PluginGroup());

            //flagByPlugin.Add(plugin, featureFlag);
            pluginGroupByFlag[featureFlag].Add(plugin, allPlugins.plugins[plugin]);
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
                PluginInfo info = feature.Value;

                if ( info.enabled )
                    continue;

                info.enabled = true;
                info.instance = info.builder.Invoke();
                info.instance.Initialize();
            }
        }

        public void DisableFlag(string featureFlag)
        {
            PluginGroup pluginGroup = pluginGroupByFlag[featureFlag];

            foreach ( var feature in pluginGroup.plugins )
            {
                PluginInfo info = feature.Value;

                if ( !info.enabled )
                    continue;

                info.enabled = false;
                info.instance.Dispose();
            }
        }

        public void SetFeatureFlagsData(BaseVariable<FeatureFlag> flags)
        {
            if ( featureFlagsDataSource != null )
                featureFlagsDataSource.OnChange -= OnFeatureFlagsChange;

            featureFlagsDataSource = flags;
            featureFlagsDataSource.OnChange += OnFeatureFlagsChange;
            OnFeatureFlagsChange(flags.Get(), flags.Get());
        }

        private void OnFeatureFlagsChange(FeatureFlag current, FeatureFlag previous)
        {
            Assert.IsNotNull(current, "Current feature flags object should never be null!");

            foreach ( var flag in current.flags )
            {
                SetFlag(flag.Key, flag.Value);
            }
        }

        public void Dispose()
        {
            foreach ( var kvp in allPlugins.plugins )
            {
                PluginInfo info = kvp.Value;

                if ( !info.enabled )
                    continue;

                info.instance.Dispose();
            }

            if ( featureFlagsDataSource != null )
                featureFlagsDataSource.OnChange -= OnFeatureFlagsChange;
        }

        public void Update()
        {
            foreach ( var kvp in allPlugins.plugins )
            {
                if (kvp.Value.enabled)
                    kvp.Value.instance.Update();
            }
        }

        public void LateUpdate()
        {
            foreach ( var kvp in allPlugins.plugins )
            {
                if (kvp.Value.enabled)
                    kvp.Value.instance.LateUpdate();
            }
        }

        public void OnGUI()
        {
            foreach ( var kvp in allPlugins.plugins )
            {
                if (kvp.Value.enabled)
                    kvp.Value.instance.OnGUI();
            }
        }
    }
}