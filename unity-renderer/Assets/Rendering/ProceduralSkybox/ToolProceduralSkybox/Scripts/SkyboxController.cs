using System.Linq;
using UnityEngine;

/// <summary>
/// This class will handle runtime execution of skybox cycle. 
/// Load and assign material to the Skybox.
/// This will mostly increment the time cycle and apply values from configuration to the material.
/// </summary>
public class SkyboxController : PluginFeature
{
    public static SkyboxController i { get; private set; }

    public const string DEFAULT_SKYBOX_ID = "Generic Skybox";
    public float timeOfTheDay;                                      // (Nishant.K) Time will be provided from outside, So remove this variable

    private Light directionalLight;
    private SkyboxConfiguration configuration;
    private Material selectedMat;
    private bool overrideDefaultSkybox;
    private string overrideSkyboxID;

    public override void Initialize()
    {
        base.Initialize();
        SelectSkyboxConfiguration();

        // Enable/Disable or Create new Directional Light Object
        directionalLight = GameObject.FindObjectsOfType<Light>().Where(s => s.type == LightType.Directional).FirstOrDefault();

        if (directionalLight == null)
        {
            GameObject temp = new GameObject("The Sun");
            // Add the light component
            directionalLight = temp.AddComponent<Light>();
            directionalLight.type = LightType.Directional;
        }
    }

    private void SelectSkyboxConfiguration()
    {
        string configToLoad = DEFAULT_SKYBOX_ID;

        if (overrideDefaultSkybox)
        {
            configToLoad = overrideSkyboxID;
        }
        configuration = Resources.Load<SkyboxConfiguration>("Skybox Configurations/" + configToLoad);

        if (configuration == null)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.LogError("No configuration found in Resources. (Default path through tool is Assets/Scripts/Resources/Skybox Configurations)");
#endif
            return;
        }

        // Apply material as per number of Layers.
        //TODO: Change shader on same materialinstead of having multiple material.
        selectedMat = MaterialReferenceContainer.i.GetMaterialForLayers(configuration.textureLayers.Count);
        RenderSettings.skybox = selectedMat;
    }


    // Update is called once per frame
    public override void Update()
    {
        if (configuration == null)
        {
            return;
        }

        timeOfTheDay += Time.deltaTime;
        timeOfTheDay = Mathf.Clamp(timeOfTheDay, 0.01f, 24);

        configuration.ApplyOnMaterial(selectedMat, timeOfTheDay, GetNormalizedDayTime(), directionalLight);

        if (timeOfTheDay >= 24)
        {
            timeOfTheDay = 0.01f;
        }
    }

    private float GetNormalizedDayTime()
    {
        float tTime = 0;

        tTime = timeOfTheDay / 24;

        tTime = Mathf.Clamp(tTime, 0, 1);

        return tTime;
    }
}
