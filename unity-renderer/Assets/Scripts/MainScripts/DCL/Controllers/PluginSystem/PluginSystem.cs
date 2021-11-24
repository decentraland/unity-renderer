using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace DCL
{
    public delegate IPlugin PluginBuilder();

    /// <summary>
    /// This class implements a plugin system pattern.
    /// 
    /// - Many plugins can share the same feature flag
    /// - Plugins are registered by using a PluginBuilder delegate that must create and return the plugin instance.
    /// - Any active plugin is an instantiated plugin. A disabled plugin is a never created or disposed plugin.
    /// - No Initialize() pattern. Plugins abide to RAII idiom.
    /// - Feature flags can be set automatically by using SetFeatureFlagsData.
    /// </summary>
    public class PluginSystem : IDisposable
    {
        private static ILogger logger = new Logger(Debug.unityLogger);
        private PluginGroup allPlugins = new PluginGroup();
        private Dictionary<string, PluginGroup> pluginGroupByFlag = new Dictionary<string, PluginGroup>();
        private BaseVariable<FeatureFlag> featureFlagsDataSource;

        /// <summary>
        /// Returns true if the plugin defined by the PluginBuilder delegate is currently enabled.
        ///
        /// An enabled plugin is an instantiated and currently active plugin.
        /// </summary>
        /// <param name="pluginBuilder">The PluginBuilder delegate instance used to instance this plugin.</param>
        /// <returns>True if the plugin defined by the PluginBuilder delegate is currently enabled</returns>
        public bool IsEnabled(PluginBuilder pluginBuilder)
        {
            if (!allPlugins.plugins.ContainsKey(pluginBuilder))
                return false;

            return allPlugins.plugins[pluginBuilder].isEnabled;
        }

        /// <summary>
        /// Registers a plugin builder and binds it to a feature flag.
        /// </summary>
        /// <param name="pluginBuilder">The builder used to construct the plugin.</param>
        /// <param name="featureFlag">The flag to be bounded. When this flag is true, the plugin will be created.</param>
        public void RegisterWithFlag(PluginBuilder pluginBuilder, string featureFlag)
        {
            Register(pluginBuilder, false);
            BindFlag(pluginBuilder, featureFlag);
        }

        /// <summary>
        /// Registers a new plugin to be built by the PluginBuilder delegate.
        /// </summary>
        /// <param name="pluginBuilder">The pluginBuilder instance. This instance will create the plugin when enabled.</param>
        /// <param name="enable">If true, the plugin will be constructed as soon as this method is called.</param>
        public void Register(PluginBuilder pluginBuilder, bool enable = true)
        {
            Assert.IsNotNull(pluginBuilder);

            PluginInfo pluginInfo = new PluginInfo() { builder = pluginBuilder };
            allPlugins.Add(pluginBuilder, pluginInfo);

            if (enable)
                pluginInfo.Enable();
        }

        /// <summary>
        /// Unregisters a registered plugin. If the plugin was active, it will be disposed.
        /// </summary>
        /// <param name="plugin">The plugin builder instance used to register the plugin.</param>
        public void Unregister(PluginBuilder plugin)
        {
            if ( !allPlugins.plugins.ContainsKey(plugin))
                return;

            PluginInfo info = allPlugins.plugins[plugin];
            info.Disable();

            string flag = info.flag;

            allPlugins.Remove(plugin);
            pluginGroupByFlag[flag].Remove(plugin);
        }

        /// <summary>
        /// Bind a feature flag to the given plugin builder.
        /// When the given feature flag is set to true, this class will construct the plugin, initializing it.
        /// </summary>
        /// <param name="plugin">The given plugin builder.</param>
        /// <param name="featureFlag">The given feature flag. If this feature flag is set to true the plugin will be created.</param>
        public void BindFlag(PluginBuilder plugin, string featureFlag)
        {
            Assert.IsNotNull(plugin);

            if ( !pluginGroupByFlag.ContainsKey(featureFlag) )
                pluginGroupByFlag.Add(featureFlag, new PluginGroup());

            pluginGroupByFlag[featureFlag].Add(plugin, allPlugins.plugins[plugin]);
        }

        /// <summary>
        /// Sets a feature flag. Disabling or enabling any plugin that was bounded to it.
        /// </summary>
        /// <param name="featureFlag">The given feature flag to set.</param>
        /// <param name="enabled">The feature flag is enabled?</param>
        public void SetFlag(string featureFlag, bool enabled)
        {
            if (enabled)
                EnableFlag(featureFlag);
            else
                DisableFlag(featureFlag);
        }

        /// <summary>
        /// Enables a given feature flag. This enables any plugin that was bounded to it.
        /// Enabling a plugin means that the plugin instance will be created.
        /// </summary>
        /// <param name="featureFlag">The feature flag to enable</param>
        public void EnableFlag(string featureFlag)
        {
            if ( !pluginGroupByFlag.ContainsKey(featureFlag) )
                return;

            PluginGroup pluginGroup = pluginGroupByFlag[featureFlag];

            foreach ( var feature in pluginGroup.plugins )
            {
                PluginInfo info = feature.Value;
                info.Enable();
            }
        }

        /// <summary>
        /// Disables a given feature flag. This disables any plugin that was bounded to it.
        /// Disabling a plugin means that the plugin instance will be disposed.
        /// </summary>
        /// <param name="featureFlag">The feature flag to enable</param>
        public void DisableFlag(string featureFlag)
        {
            if ( !pluginGroupByFlag.ContainsKey(featureFlag) )
                return;

            PluginGroup pluginGroup = pluginGroupByFlag[featureFlag];

            foreach ( var feature in pluginGroup.plugins )
            {
                PluginInfo info = feature.Value;
                info.Disable();
            }
        }

        /// <summary>
        /// Sets the FeatureFlag instance used to listen for feature flag changes.
        ///
        /// After setting the feature flags data of the given instance, the PluginSystem
        /// will react to all the feature flag changes.
        /// </summary>
        /// <param name="featureFlagsBaseVariable">The data object to listen to.</param>
        public void SetFeatureFlagsData(BaseVariable<FeatureFlag> featureFlagsBaseVariable)
        {
            if ( featureFlagsDataSource != null )
                featureFlagsDataSource.OnChange -= OnFeatureFlagsChange;

            featureFlagsDataSource = featureFlagsBaseVariable;
            featureFlagsDataSource.OnChange += OnFeatureFlagsChange;
            OnFeatureFlagsChange(featureFlagsBaseVariable.Get(), featureFlagsBaseVariable.Get());
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
                info.Disable();
            }

            if ( featureFlagsDataSource != null )
                featureFlagsDataSource.OnChange -= OnFeatureFlagsChange;
        }
    }
}