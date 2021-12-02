using System;
using System.Linq;
using UnityEngine;
using DCL.ServerTime;

namespace DCL.Skybox
{
    /// <summary>
    /// This class will handle runtime execution of skybox cycle. 
    /// Load and assign material to the Skybox.
    /// This will mostly increment the time cycle and apply values from configuration to the material.
    /// </summary>
    public class SkyboxController : IPlugin
    {
        public event SkyboxConfiguration.TimelineEvents OnTimelineEvent;

        public static SkyboxController i { get; private set; }

        public const string DEFAULT_SKYBOX_ID = "Generic Skybox";

        public string loadedConfig;
        //Time for one complete circle. In Hours. default 24
        public float cycleTime = 24;
        public float lifecycleDuration = 1;

        private float timeOfTheDay;                            // (Nishant.K) Time will be provided from outside, So remove this variable
        private Light directionalLight;
        private SkyboxConfiguration configuration;
        private Material selectedMat;
        private bool overrideDefaultSkybox;
        private string overrideSkyboxID;
        private bool isPaused;
        private float timeNormalizationFactor;
        private int slotCount;
        private bool overrideByEditor = false;

        public SkyboxController()
        {
            i = this;

            // Find and delete test directional light obj if any
            Light[] testDirectionalLight = GameObject.FindObjectsOfType<Light>().Where(s => s.name == "The Sun_Temp").ToArray();
            for (int i = 0; i < testDirectionalLight.Length; i++)
            {
                GameObject.DestroyImmediate(testDirectionalLight[i].gameObject);
            }

            // Get or Create new Directional Light Object
            directionalLight = GameObject.FindObjectsOfType<Light>().Where(s => s.type == LightType.Directional).FirstOrDefault();

            if (directionalLight == null)
            {
                GameObject temp = new GameObject("The Sun");
                // Add the light component
                directionalLight = temp.AddComponent<Light>();
                directionalLight.type = LightType.Directional;
            }

            // Get current time from the server
            GetTimeFromTheServer(WorldTimer.i.GetCurrentTime());
            WorldTimer.i.OnTimeChanged += GetTimeFromTheServer;

            // Update config whenever skybox config changed in data store. Can be used for both testing and runtime
            DataStore.i.skyboxConfig.objectUpdated.OnChange += UpdateConfig;

            // Change as Kernel config is initialized or updated
            KernelConfig.i.EnsureConfigInitialized()
                        .Then(config =>
                        {
                            KernelConfig_OnChange(config, null);
                        });

            KernelConfig.i.OnChange += KernelConfig_OnChange;

            DCL.Environment.i.platform.updateEventHandler.AddListener(IUpdateEventHandler.EventType.Update, Update);
        }

        private void KernelConfig_OnChange(KernelConfigModel current, KernelConfigModel previous)
        {
            if (overrideByEditor)
            {
                return;
            }
            // set skyboxConfig to true
            DataStore.i.skyboxConfig.useProceduralSkybox.Set(true);
            DataStore.i.skyboxConfig.configToLoad.Set(current.proceduralSkyboxConfig.configToLoad);
            DataStore.i.skyboxConfig.lifecycleDuration.Set(current.proceduralSkyboxConfig.lifecycleDuration);
            DataStore.i.skyboxConfig.jumpToTime.Set(current.proceduralSkyboxConfig.fixedTime);

            // Call update on skybox config which will call Update config in this class.
            DataStore.i.skyboxConfig.objectUpdated.Set(true, true);
        }

        /// <summary>
        /// Called whenever any change in skyboxConfig is observed
        /// </summary>
        /// <param name="current"></param>
        /// <param name="previous"></param>
        public void UpdateConfig(bool current = true, bool previous = false)
        {
            if (overrideByEditor)
            {
                return;
            }

            if (loadedConfig != DataStore.i.skyboxConfig.configToLoad.Get())
            {
                // Apply configuration
                overrideDefaultSkybox = true;
                overrideSkyboxID = DataStore.i.skyboxConfig.configToLoad.Get();
            }

            // Apply time
            lifecycleDuration = DataStore.i.skyboxConfig.lifecycleDuration.Get();

            if (DataStore.i.skyboxConfig.useProceduralSkybox.Get())
            {
                if (!ApplyConfig())
                {
                    RenderProfileManifest.i.currentProfile.Apply();
                }
            }
            else
            {
                RenderProfileManifest.i.currentProfile.Apply();
            }

            // Reset Object Update value without notifying
            DataStore.i.skyboxConfig.objectUpdated.Set(false, false);

            // if Paused
            if (DataStore.i.skyboxConfig.jumpToTime.Get() > 0)
            {
                PauseTime(true, DataStore.i.skyboxConfig.jumpToTime.Get());
            }
            else
            {
                ResumeTime();
            }
        }

        /// <summary>
        /// Apply changed configuration
        /// </summary>
        bool ApplyConfig()
        {
            if (overrideByEditor)
            {
                return false;
            }

            if (!SelectSkyboxConfiguration())
            {
                return false;
            }

            if (!configuration.useDirectionalLight)
            {
                directionalLight.gameObject.SetActive(false);
            }

            // Calculate time factor
            if (lifecycleDuration <= 0)
            {
                lifecycleDuration = 0.01f;
            }

            // Convert minutes in seconds and then normalize with cycle time
            timeNormalizationFactor = lifecycleDuration * 60 / cycleTime;
            return true;
        }

        void GetTimeFromTheServer(DateTime serverTime)
        {
            // Convert miliseconds to seconds
            float seconds = serverTime.Second + ((float)serverTime.Millisecond / 1000);
            // Convert seconds to minutes
            float minutes = serverTime.Minute + (seconds / 60);
            // Convert minutes to hour (in float format)
            float hours = serverTime.Hour + (minutes / 60);
            // divide by lifecycleDuration.... + 1 as hour is from 0 to 23
            float timeInCycle = (hours / (lifecycleDuration / 60)) + 1;
            // get percentage part for converting to skybox time
            float percentageSkyboxtime = timeInCycle - (int)timeInCycle;

            timeOfTheDay = percentageSkyboxtime * cycleTime;
        }

        /// <summary>
        /// Select Configuration to load.
        /// </summary>
        private bool SelectSkyboxConfiguration()
        {
            bool tempConfigLoaded = true;

            string configToLoad = loadedConfig;
            if (string.IsNullOrEmpty(loadedConfig))
            {
                configToLoad = DEFAULT_SKYBOX_ID;
            }


            if (overrideDefaultSkybox)
            {
                configToLoad = overrideSkyboxID;
                overrideDefaultSkybox = false;
            }

            // config already loaded, return
            if (configToLoad.Equals(loadedConfig))
            {
                return tempConfigLoaded;
            }

            SkyboxConfiguration newConfiguration = Resources.Load<SkyboxConfiguration>("Skybox Configurations/" + configToLoad);

            if (newConfiguration == null)
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                Debug.LogError(configToLoad + " configuration not found in Resources. Trying to load Default config: " + DEFAULT_SKYBOX_ID + "(Default path through tool is Assets/Scripts/Resources/Skybox Configurations)");
#endif
                // Try to load default config
                configToLoad = DEFAULT_SKYBOX_ID;
                newConfiguration = Resources.Load<SkyboxConfiguration>("Skybox Configurations/" + configToLoad);

                if (newConfiguration == null)
                {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                    Debug.LogError("Default configuration not found in Resources. Shifting to old skybox. (Default path through tool is Assets/Scripts/Resources/Skybox Configurations)");
#endif
                    tempConfigLoaded = false;
                    return tempConfigLoaded;
                }
            }

            // Register to timelineEvents
            if (configuration != null)
            {
                configuration.OnTimelineEvent -= Configuration_OnTimelineEvent;
            }
            newConfiguration.OnTimelineEvent += Configuration_OnTimelineEvent;
            configuration = newConfiguration;

            // Apply material as per number of Slots.
            MaterialReferenceContainer.Mat_Layer matLayer = MaterialReferenceContainer.i.GetMat_LayerForLayers(configuration.slots.Count);
            if (matLayer == null)
            {
                matLayer = MaterialReferenceContainer.i.materials[0];
            }

            configuration.ResetMaterial(matLayer.material, matLayer.numberOfSlots);
            selectedMat = matLayer.material;
            slotCount = matLayer.numberOfSlots;

            if (DataStore.i.skyboxConfig.useProceduralSkybox.Get())
            {
                RenderSettings.skybox = selectedMat;
            }

            // Update loaded config
            loadedConfig = configToLoad;

            return tempConfigLoaded;
        }

        private void Configuration_OnTimelineEvent(string tag, bool enable, bool trigger) { OnTimelineEvent?.Invoke(tag, enable, trigger); }

        // Update is called once per frame
        public void Update()
        {
            if (configuration == null || isPaused || !DataStore.i.skyboxConfig.useProceduralSkybox.Get())
            {
                return;
            }

            // Control is in editor tool
            if (overrideByEditor)
            {
                return;
            }

            timeOfTheDay += Time.deltaTime / timeNormalizationFactor;
            timeOfTheDay = Mathf.Clamp(timeOfTheDay, 0.01f, cycleTime);

            configuration.ApplyOnMaterial(selectedMat, timeOfTheDay, GetNormalizedDayTime(), slotCount, directionalLight, cycleTime);

            // Cycle resets
            if (timeOfTheDay >= cycleTime)
            {
                timeOfTheDay = 0.01f;
                configuration.CycleResets();
            }
        }

        public void Dispose()
        {
            // set skyboxConfig to false
            DataStore.i.skyboxConfig.useProceduralSkybox.Set(false);
            DataStore.i.skyboxConfig.objectUpdated.OnChange -= UpdateConfig;

            WorldTimer.i.OnTimeChanged -= GetTimeFromTheServer;
            configuration.OnTimelineEvent -= Configuration_OnTimelineEvent;
            KernelConfig.i.OnChange -= KernelConfig_OnChange;
            DCL.Environment.i.platform.updateEventHandler.RemoveListener(IUpdateEventHandler.EventType.Update, Update);
        }

        public void PauseTime(bool overrideTime = false, float newTime = 0)
        {
            isPaused = true;
            if (overrideTime)
            {
                timeOfTheDay = Mathf.Clamp(newTime, 0, 24);
                configuration.ApplyOnMaterial(selectedMat, timeOfTheDay, GetNormalizedDayTime(), slotCount, directionalLight, cycleTime);
            }
        }

        public void ResumeTime(bool overrideTime = false, float newTime = 0)
        {
            isPaused = false;
            if (overrideTime)
            {
                timeOfTheDay = newTime;
            }
        }

        public bool IsPaused() { return isPaused; }

        private float GetNormalizedDayTime()
        {
            float tTime = 0;

            tTime = timeOfTheDay / cycleTime;

            tTime = Mathf.Clamp(tTime, 0, 1);

            return tTime;
        }

        public SkyboxConfiguration GetCurrentConfiguration() { return configuration; }

        public float GetCurrentTimeOfTheDay() { return timeOfTheDay; }

        public bool SetOverrideController(bool editorOveride)
        {
            overrideByEditor = editorOveride;
            return overrideByEditor;
        }

        // Whenever Skybox editor closed at runtime control returns back to controller with the values in the editor
        public bool GetControlBackFromEditor(string currentConfig, float timeOfTheday, float lifecycleDuration, bool isPaused)
        {
            overrideByEditor = false;

            DataStore.i.skyboxConfig.configToLoad.Set(currentConfig);
            DataStore.i.skyboxConfig.lifecycleDuration.Set(lifecycleDuration);

            if (isPaused)
            {
                PauseTime(true, timeOfTheday);
            }
            else
            {
                ResumeTime(true, timeOfTheday);
            }

            // Call update on skybox config which will call Update config in this class.
            DataStore.i.skyboxConfig.objectUpdated.Set(true, true);

            return overrideByEditor;
        }

        public void UpdateConfigurationTimelineEvent(SkyboxConfiguration newConfig)
        {
            configuration.OnTimelineEvent -= Configuration_OnTimelineEvent;
            newConfig.OnTimelineEvent += Configuration_OnTimelineEvent;
        }

    }
}
