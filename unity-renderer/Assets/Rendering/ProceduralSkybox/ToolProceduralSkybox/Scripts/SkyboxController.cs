using System;
using System.Linq;
using UnityEngine;

namespace DCL.Skybox
{
    /// <summary>
    /// This class will handle runtime execution of skybox cycle. 
    /// Load and assign material to the Skybox.
    /// This will mostly increment the time cycle and apply values from configuration to the material.
    /// </summary>
    public class SkyboxController : PluginFeature
    {
        public static SkyboxController i { get; private set; }

        public const string DEFAULT_SKYBOX_ID = "Generic Skybox";

        //Time for one complete circle. In Hours. default 24
        public float cycleTime = 24;
        public float minutesPerSecond = 60;

        private float timeOfTheDay;                            // (Nishant.K) Time will be provided from outside, So remove this variable
        private Light directionalLight;
        private SkyboxConfiguration configuration;
        private Material selectedMat;
        private bool overrideDefaultSkybox;
        private string overrideSkyboxID;
        private bool isPaused;
        private float timeNormalizationFactor;

        public override void Initialize()
        {
            base.Initialize();

            i = this;

            // Enable/Disable or Create new Directional Light Object
            directionalLight = GameObject.FindObjectsOfType<Light>().Where(s => s.type == LightType.Directional).FirstOrDefault();

            if (directionalLight == null)
            {
                GameObject temp = new GameObject("The Sun");
                // Add the light component
                directionalLight = temp.AddComponent<Light>();
                directionalLight.type = LightType.Directional;
            }

            timeOfTheDay = 0;

            DataStore.i.skyboxConfig.useProceduralSkybox.Set(true);

            UpdateConfig();
            DataStore.i.skyboxConfig.objectUpdated.OnChange += UpdateConfig;
        }


        public void UpdateConfig(bool current = true, bool previous = false)
        {
            // Apply configuration
            overrideDefaultSkybox = true;
            overrideSkyboxID = DataStore.i.skyboxConfig.configToLoad.Get();

            // Apply time
            minutesPerSecond = DataStore.i.skyboxConfig.minutesPerSecond.Get();

            // if Paused
            if (DataStore.i.skyboxConfig.pauseTime.Get())
            {
                PauseTime();
            }
            else
            {
                ResumeTime();
            }

            // Jump to time
            if (DataStore.i.skyboxConfig.jumpTime)
            {
                float jumpToTime = DataStore.i.skyboxConfig.jumpToTime.Get();
                timeOfTheDay = Mathf.Clamp(jumpToTime, 0, cycleTime);
                DataStore.i.skyboxConfig.jumpTime = false;
            }

            if (DataStore.i.skyboxConfig.useProceduralSkybox.Get())
            {
                Applyconfig();
            }
            else
            {
                RenderProfileManifest.i.currentProfile.Apply();
            }

            // Reset Object Update value without notifying
            DataStore.i.skyboxConfig.objectUpdated.Set(false, false);
        }

        void Applyconfig()
        {
            SelectSkyboxConfiguration();

            if (!configuration.useDirectionalLight)
            {
                directionalLight.gameObject.SetActive(false);
            }
            //DCL.DataStore.i.isProceduralSkyboxInUse.Set(true);

            // Calculate time factor
            if (minutesPerSecond <= 0)
            {
                minutesPerSecond = 0.01f;
            }
            timeNormalizationFactor = 60 / minutesPerSecond;
        }

        private void SelectSkyboxConfiguration()
        {
            string configToLoad = DEFAULT_SKYBOX_ID;

            if (overrideDefaultSkybox)
            {
                configToLoad = overrideSkyboxID;
                overrideDefaultSkybox = false;
            }
            configuration = Resources.Load<SkyboxConfiguration>("Skybox Configurations/" + configToLoad);

            if (configuration == null)
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                Debug.LogError("No configuration found in Resources. (Default path through tool is Assets/Scripts/Resources/Skybox Configurations)");
#endif
                return;
            }

            // Apply material as per number of Slots.
            //TODO: Change shader on same material instead of having multiple material.
            MaterialReferenceContainer.Mat_Layer matLayer = MaterialReferenceContainer.i.GetMat_LayerForLayers(configuration.slots.Count);
            if (matLayer == null)
            {
                matLayer = MaterialReferenceContainer.i.materials[0];
            }

            configuration.ResetMaterial(matLayer.material, matLayer.numberOfSlots);
            selectedMat = matLayer.material;

            if (DataStore.i.skyboxConfig.useProceduralSkybox.Get())
            {
                RenderSettings.skybox = selectedMat;
            }
        }


        // Update is called once per frame
        public override void Update()
        {
            if (configuration == null || isPaused || !DataStore.i.skyboxConfig.useProceduralSkybox.Get())
            {
                return;
            }
            timeOfTheDay += Time.deltaTime / timeNormalizationFactor;
            timeOfTheDay = Mathf.Clamp(timeOfTheDay, 0.01f, cycleTime);

            configuration.ApplyOnMaterial(selectedMat, timeOfTheDay, GetNormalizedDayTime(), directionalLight);

            if (timeOfTheDay >= cycleTime)
            {
                timeOfTheDay = 0.01f;
            }
        }

        public override void Dispose()
        {
            base.Dispose();
            //DCL.DataStore.i.isProceduralSkyboxInUse.Set(false);
            DataStore.i.skyboxConfig.objectUpdated.OnChange -= UpdateConfig;
        }

        public void PauseTime() { isPaused = true; }

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

    }
}
