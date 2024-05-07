using Cysharp.Threading.Tasks;
using DCL.Providers;
using System;
using System.Linq;
using UnityEngine;
using System.Collections.Generic;
using System.Threading;

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

        public const string DEFAULT_SKYBOX_ID = "Generic_Skybox";

        public string loadedConfig;
        public float lifecycleDuration = 2;

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
        private SkyboxElements skyboxElements;

        // Reflection probe//
        private ReflectionProbe skyboxProbe;
        private bool probeParented = false;
        private float reflectionUpdateTime = 1;                                 // In Mins
        private ReflectionProbeRuntime runtimeReflectionObj;

        // Timer sync
        private int syncCounter = 0;
        private int syncAfterCount = 10;

        // Report to kernel
        private ITimeReporter timeReporter { get; set; } = new TimeReporter();

        private readonly DataStore dataStore;
        private Service<IAddressableResourceProvider> addresableResolver;
        private Dictionary<string, SkyboxConfiguration> skyboxConfigurationsDictionary;
        private MaterialReferenceContainer materialReferenceContainer;
        private CancellationTokenSource addressableCTS;

        public SkyboxCamera SkyboxCamera { get; private set; }

        public SkyboxController(DataStore dataStore)
        {
            i = this;

            this.dataStore = dataStore;

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

            CommonScriptableObjects.isFullscreenHUDOpen.OnChange += OnFullscreenUIVisibilityChange;
            CommonScriptableObjects.isLoadingHUDOpen.OnChange += OnFullscreenUIVisibilityChange;
            dataStore.skyboxConfig.avatarMatProfile.OnChange += OnAvatarMatProfileOnChange;

            DoAsyncInitializations();
        }

        private async UniTaskVoid DoAsyncInitializations()
        {
            try
            {
                addressableCTS = new CancellationTokenSource();
                materialReferenceContainer = await addresableResolver.Ref.GetAddressable<MaterialReferenceContainer>("SkyboxMaterialData.asset", addressableCTS.Token);
                await GetOrCreateEnvironmentProbe(addressableCTS.Token);
                await LoadConfigurations(addressableCTS.Token);
                skyboxElements = new SkyboxElements();
                await skyboxElements.Initialize(addresableResolver.Ref, materialReferenceContainer, addressableCTS.Token);
            }
            catch (Exception e)
            {
                Debug.LogWarning("Retrying skybox addressables async request...");
                DisposeCT();
                DoAsyncInitializations().Forget();
                return;
            }

            // Create skybox Camera
            SkyboxCamera = new SkyboxCamera();

            // Get current time from the server
            GetTimeFromTheServer(dataStore.worldTimer.GetCurrentTime());
            dataStore.worldTimer.OnTimeChanged += GetTimeFromTheServer;

            // Update config whenever skybox config changed in data store. Can be used for both testing and runtime
            dataStore.skyboxConfig.objectUpdated.OnChange += UpdateConfig;

            // Change as Kernel config is initialized or updated
            KernelConfig.i.EnsureConfigInitialized()
                        .Then(config =>
                         {
                             KernelConfig_OnChange(config, null);
                         });

            KernelConfig.i.OnChange += KernelConfig_OnChange;

            Environment.i.platform.updateEventHandler.AddListener(IUpdateEventHandler.EventType.Update, Update);

            dataStore.skyboxConfig.reflectionResolution.OnChange += ReflectionResolution_OnChange;

            // Register for camera references
            dataStore.camera.transform.OnChange += AssignCameraReferences;
            AssignCameraReferences(dataStore.camera.transform.Get(), null);

            // Register UI related events
            dataStore.skyboxConfig.mode.OnChange += UseDynamicSkybox_OnChange;
            dataStore.skyboxConfig.fixedTime.OnChange += FixedTime_OnChange;
            //Ensure current settings
            UseDynamicSkybox_OnChange(dataStore.skyboxConfig.mode.Get());
            FixedTime_OnChange(dataStore.skyboxConfig.fixedTime.Get());
        }

        private async UniTask LoadConfigurations(CancellationToken ct)
        {
            IList<SkyboxConfiguration> skyboxConfigurations = await addresableResolver.Ref.GetAddressablesList<SkyboxConfiguration>("SkyboxConfiguration", ct);
            skyboxConfigurationsDictionary = new Dictionary<string, SkyboxConfiguration>();
            foreach (SkyboxConfiguration skyboxConfiguration in skyboxConfigurations)
            {
                skyboxConfigurationsDictionary.Add(skyboxConfiguration.name, skyboxConfiguration);
            }
        }

        public void AssignMainOverlayCamera(Transform currentTransform)
        {
            SkyboxCamera.AssignTargetCamera(currentTransform);
        }

        private void AssignCameraReferences(Transform currentTransform, Transform prevTransform)
        {
            SkyboxCamera.AssignTargetCamera(currentTransform);
            skyboxElements.AssignCameraInstance(currentTransform);
        }

        private void OnFullscreenUIVisibilityChange(bool visibleState, bool prevVisibleState)
        {
            if (visibleState == prevVisibleState)
                return;

            if(SkyboxCamera == null) return;

            SkyboxCamera.SetCameraEnabledState(!visibleState && CommonScriptableObjects.rendererState.Get());
        }

        private void FixedTime_OnChange(float current, float _ = 0)
        {
            if (dataStore.skyboxConfig.mode.Get() != SkyboxMode.Dynamic)
            {
                PauseTime(true, current);
            }

            if (runtimeReflectionObj != null)
            {
                runtimeReflectionObj.FixedSkyboxTimeChanged();
            }
        }

        private void UseDynamicSkybox_OnChange(SkyboxMode current, SkyboxMode _ = SkyboxMode.Dynamic)
        {
            if (current == SkyboxMode.Dynamic)
            {
                // Get latest time from server
                UpdateConfig();
            }
            else
            {
                PauseTime(true, dataStore.skyboxConfig.fixedTime.Get());
            }

            if (runtimeReflectionObj != null)
            {
                runtimeReflectionObj.SkyboxModeChanged(current == SkyboxMode.Dynamic);
            }
        }

        private async UniTask GetOrCreateEnvironmentProbe(CancellationToken cts)
        {
            // Get Reflection Probe Object
            skyboxProbe = GameObject.FindObjectsOfType<ReflectionProbe>().Where(s => s.name == "SkyboxProbe").FirstOrDefault();

            if (dataStore.skyboxConfig.disableReflection.Get())
            {
                if (skyboxProbe != null)
                {
                    skyboxProbe.gameObject.SetActive(false);
                }

                RenderSettings.defaultReflectionMode = UnityEngine.Rendering.DefaultReflectionMode.Skybox;
                RenderSettings.customReflection = null;
                return;
            }

            if (skyboxProbe == null)
            {
                // Instantiate new probe from the resources
                GameObject temp = await addresableResolver.Ref.GetAddressable<GameObject>("SkyboxProbe.prefab", cts);
                GameObject probe = GameObject.Instantiate<GameObject>(temp);
                probe.name = "SkyboxProbe";
                skyboxProbe = probe.GetComponent<ReflectionProbe>();

                // make probe a child of main camera
                AssignCameraInstancetoProbe();
            }

            // Update time in Reflection Probe
            runtimeReflectionObj = skyboxProbe.GetComponent<ReflectionProbeRuntime>();
            if (runtimeReflectionObj == null)
            {
                runtimeReflectionObj = skyboxProbe.gameObject.AddComponent<ReflectionProbeRuntime>();
            }

            // Update resolution
            runtimeReflectionObj.UpdateResolution(dataStore.skyboxConfig.reflectionResolution.Get());

            // Assign as seconds
            runtimeReflectionObj.updateAfter = reflectionUpdateTime * 60;

            RenderSettings.defaultReflectionMode = UnityEngine.Rendering.DefaultReflectionMode.Custom;
            RenderSettings.customReflection = null;
        }

        private void ReflectionResolution_OnChange(int current, int previous) { runtimeReflectionObj.UpdateResolution(current); }

        private void AssignCameraInstancetoProbe()
        {
            if (skyboxProbe == null)
            {
#if UNITY_EDITOR
                Debug.LogError("Cannot parent the probe as probe is not instantiated");
#endif
                return;
            }

            // make probe a child of main camera
            if (Camera.main != null)
            {
                GameObject mainCam = Camera.main.gameObject;
                runtimeReflectionObj.followTransform = mainCam.transform;
                probeParented = true;
            }
        }

        private void KernelConfig_OnChange(KernelConfigModel current, KernelConfigModel previous)
        {
            if (overrideByEditor)
            {
                return;
            }
            // set skyboxConfig to true
            dataStore.skyboxConfig.configToLoad.Set(current.proceduralSkyboxConfig.configToLoad);
            dataStore.skyboxConfig.lifecycleDuration.Set(current.proceduralSkyboxConfig.lifecycleDuration);
            dataStore.skyboxConfig.jumpToTime.Set(current.proceduralSkyboxConfig.fixedTime);
            dataStore.skyboxConfig.updateReflectionTime.Set(current.proceduralSkyboxConfig.updateReflectionTime);
            dataStore.skyboxConfig.disableReflection.Set(current.proceduralSkyboxConfig.disableReflection);

            // Call update on skybox config which will call Update config in this class.
            dataStore.skyboxConfig.objectUpdated.Set(true, true);
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

            // Reset Object Update value without notifying
            dataStore.skyboxConfig.objectUpdated.Set(false, false);

            //Ensure default configuration
            SelectSkyboxConfiguration();

            if (dataStore.skyboxConfig.mode.Get() != SkyboxMode.Dynamic)
            {
                return;
            }

            if (loadedConfig != dataStore.skyboxConfig.configToLoad.Get())
            {
                // Apply configuration
                overrideDefaultSkybox = true;
                overrideSkyboxID = dataStore.skyboxConfig.configToLoad.Get();
            }

            // Apply time
            lifecycleDuration = dataStore.skyboxConfig.lifecycleDuration.Get();

            ApplyConfig();

            // if Paused
            if (dataStore.skyboxConfig.jumpToTime.Get() >= 0)
            {
                PauseTime(true, dataStore.skyboxConfig.jumpToTime.Get());
            }
            else
            {
                ResumeTime();
            }

            // Update reflection time
            if (dataStore.skyboxConfig.disableReflection.Get())
            {
                if (skyboxProbe != null)
                {
                    skyboxProbe.gameObject.SetActive(false);
                }

                RenderSettings.defaultReflectionMode = UnityEngine.Rendering.DefaultReflectionMode.Skybox;
                RenderSettings.customReflection = null;
            }
            else if (runtimeReflectionObj != null)
            {
                // If reflection update time is -1 then calculate time based on the cycle time, else assign same
                if (dataStore.skyboxConfig.updateReflectionTime.Get() >= 0)
                {
                    reflectionUpdateTime = dataStore.skyboxConfig.updateReflectionTime.Get();
                }
                else
                {
                    // Evaluate with the cycle time
                    reflectionUpdateTime = 1;               // Default for an hour is 1 min
                    // get cycle time in hours
                    reflectionUpdateTime = (dataStore.skyboxConfig.lifecycleDuration.Get() / 60);
                }
                runtimeReflectionObj.updateAfter = Mathf.Clamp(reflectionUpdateTime * 60, 5, 86400);
            }
        }

        /// <summary>
        /// Apply changed configuration
        /// </summary>
        private bool ApplyConfig()
        {
            if (overrideByEditor)
            {
                return false;
            }

            bool gotConfiguration = SelectSkyboxConfiguration();
            if (!gotConfiguration)
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
            timeNormalizationFactor = lifecycleDuration * 60 / SkyboxUtils.CYCLE_TIME;
            timeReporter.Configure(timeNormalizationFactor, SkyboxUtils.CYCLE_TIME);

            GetTimeFromTheServer(dataStore.worldTimer.GetCurrentTime());

            return true;
        }

        void GetTimeFromTheServer(DateTime serverTime)
        {
            DateTime serverTimeNoTicks = new DateTime(serverTime.Year, serverTime.Month, serverTime.Day, serverTime.Hour, serverTime.Minute, serverTime.Second, serverTime.Millisecond);
            long elapsedTicks = serverTime.Ticks - serverTimeNoTicks.Ticks;

            float miliseconds = serverTime.Millisecond + (elapsedTicks / 10000);

            // Convert miliseconds to seconds
            float seconds = serverTime.Second + (miliseconds / 1000);

            // Convert seconds to minutes
            float minutes = serverTime.Minute + (seconds / 60);

            // Convert minutes to hour (in float format)
            float totalTimeInMins = serverTime.Hour * 60 + minutes;

            float timeInCycle = (totalTimeInMins / lifecycleDuration) + 1;
            float percentageSkyboxtime = timeInCycle - (int)timeInCycle;

            timeOfTheDay = percentageSkyboxtime * SkyboxUtils.CYCLE_TIME;
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

            SkyboxConfiguration newConfiguration = skyboxConfigurationsDictionary[configToLoad];

            if (newConfiguration == null)
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                Debug.LogError(configToLoad + " configuration not found in Addressables. Trying to load Default config: " + DEFAULT_SKYBOX_ID);
#endif
                // Try to load default config
                configToLoad = DEFAULT_SKYBOX_ID;
                newConfiguration = skyboxConfigurationsDictionary[configToLoad];

                if (newConfiguration == null)
                {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                    Debug.LogError("Default configuration not found in Addressables. Shifting to old skybox.");
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

            selectedMat = materialReferenceContainer.skyboxMat;
            slotCount = materialReferenceContainer.skyboxMatSlots;
            configuration.ResetMaterial(selectedMat, slotCount);

            RenderSettings.skybox = selectedMat;

            // Update loaded config
            loadedConfig = configToLoad;

            return tempConfigLoaded;
        }

        private void Configuration_OnTimelineEvent(string tag, bool enable, bool trigger) { OnTimelineEvent?.Invoke(tag, enable, trigger); }

        // Update is called once per frame
        public void Update()
        {
            if (!dataStore.skyboxConfig.disableReflection.Get() && skyboxProbe != null && !probeParented)
            {
                AssignCameraInstancetoProbe();
            }

            if (isPaused || configuration == null)
            {
                return;
            }

            // Control is in editor tool
            if (overrideByEditor)
            {
                return;
            }

            timeOfTheDay += Time.deltaTime / timeNormalizationFactor;

            syncCounter++;

            if (syncCounter >= syncAfterCount)
            {
                GetTimeFromTheServer(dataStore.worldTimer.GetCurrentTime());
                syncCounter = 0;
            }

            timeOfTheDay = Mathf.Clamp(timeOfTheDay, 0.01f, SkyboxUtils.CYCLE_TIME);
            dataStore.skyboxConfig.currentVirtualTime.Set(timeOfTheDay);
            timeReporter.ReportTime(timeOfTheDay);

            float normalizedDayTime = SkyboxUtils.GetNormalizedDayTime(timeOfTheDay);
            configuration.ApplyOnMaterial(selectedMat, timeOfTheDay, normalizedDayTime, slotCount, directionalLight, SkyboxUtils.CYCLE_TIME);
            ApplyAvatarColor(normalizedDayTime);
            skyboxElements.ApplyConfigTo3DElements(configuration, timeOfTheDay, normalizedDayTime, directionalLight, SkyboxUtils.CYCLE_TIME, false);

            // Cycle resets
            if (timeOfTheDay >= SkyboxUtils.CYCLE_TIME)
            {
                timeOfTheDay = 0.01f;
                configuration.CycleResets();
            }

        }

        public void Dispose()
        {
            // set skyboxConfig to false
            dataStore.skyboxConfig.objectUpdated.OnChange -= UpdateConfig;

            dataStore.worldTimer.OnTimeChanged -= GetTimeFromTheServer;
            if(configuration != null) configuration.OnTimelineEvent -= Configuration_OnTimelineEvent;
            KernelConfig.i.OnChange -= KernelConfig_OnChange;
            Environment.i.platform.updateEventHandler.RemoveListener(IUpdateEventHandler.EventType.Update, Update);
            dataStore.skyboxConfig.mode.OnChange -= UseDynamicSkybox_OnChange;
            dataStore.skyboxConfig.fixedTime.OnChange -= FixedTime_OnChange;
            dataStore.skyboxConfig.reflectionResolution.OnChange -= ReflectionResolution_OnChange;
            dataStore.camera.transform.OnChange -= AssignCameraReferences;

            CommonScriptableObjects.isLoadingHUDOpen.OnChange -= OnFullscreenUIVisibilityChange;
            CommonScriptableObjects.isFullscreenHUDOpen.OnChange -= OnFullscreenUIVisibilityChange;
            dataStore.skyboxConfig.avatarMatProfile.OnChange -= OnAvatarMatProfileOnChange;

            timeReporter.Dispose();
            DisposeCT();
        }

        public void PauseTime(bool overrideTime = false, float newTime = 0)
        {
            isPaused = true;
            if (overrideTime)
            {
                timeOfTheDay = Mathf.Clamp(newTime, 0, SkyboxUtils.CYCLE_TIME);
                float normalizedDayTime = SkyboxUtils.GetNormalizedDayTime(timeOfTheDay);
                configuration.ApplyOnMaterial(selectedMat, (float)timeOfTheDay, normalizedDayTime, slotCount, directionalLight, SkyboxUtils.CYCLE_TIME);
                ApplyAvatarColor(normalizedDayTime);
                skyboxElements.ApplyConfigTo3DElements(configuration, timeOfTheDay, normalizedDayTime, directionalLight, SkyboxUtils.CYCLE_TIME, false);
            }
            timeReporter.ReportTime(timeOfTheDay);
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

        public SkyboxConfiguration GetCurrentConfiguration() { return configuration; }

        public float GetCurrentTimeOfTheDay() { return (float)timeOfTheDay; }

        public bool SetOverrideController(bool editorOveride)
        {
            overrideByEditor = editorOveride;
            return overrideByEditor;
        }

        // Whenever Skybox editor closed at runtime control returns back to controller with the values in the editor
        public bool GetControlBackFromEditor(string currentConfig, float timeOfTheday, float lifecycleDuration, bool isPaused)
        {
            overrideByEditor = false;

            dataStore.skyboxConfig.configToLoad.Set(currentConfig);
            dataStore.skyboxConfig.lifecycleDuration.Set(lifecycleDuration);

            if (isPaused)
            {
                PauseTime(true, timeOfTheday);
            }
            else
            {
                ResumeTime(true, timeOfTheday);
            }

            // Call update on skybox config which will call Update config in this class.
            dataStore.skyboxConfig.objectUpdated.Set(true, true);

            return overrideByEditor;
        }

        public void UpdateConfigurationTimelineEvent(SkyboxConfiguration newConfig)
        {
            configuration.OnTimelineEvent -= Configuration_OnTimelineEvent;
            newConfig.OnTimelineEvent += Configuration_OnTimelineEvent;
        }

        public void ApplyAvatarColor(float normalizedDayTime)
        {
            if (configuration == null)
                return;

            if (dataStore.skyboxConfig.avatarMatProfile.Get() == AvatarMaterialProfile.InWorld)
                configuration.ApplyInWorldAvatarColor(normalizedDayTime, directionalLight.gameObject);
            else
                configuration.ApplyEditorAvatarColor();
        }

        private void OnAvatarMatProfileOnChange(AvatarMaterialProfile current, AvatarMaterialProfile previous) =>
            ApplyAvatarColor(SkyboxUtils.GetNormalizedDayTime(timeOfTheDay));

        public SkyboxElements GetSkyboxElements() { return skyboxElements; }

        private void DisposeCT()
        {
            if (addressableCTS != null)
            {
                addressableCTS.Cancel();
                addressableCTS.Dispose();
                addressableCTS = null;
            }
        }
    }
}
