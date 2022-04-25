using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class BlurController : MonoBehaviour
{
    [SerializeField]private ForwardRendererData rendererData = null;
    private string featureName = "GaussianBlurHandler";
    private ScriptableRendererFeature feature;

    [Range(1, 25)]
    public int passes = 1;
    [Range(1, 10)]
    public int downsampling = 1;

    private void Awake()
    {
        if(rendererData)
            feature = rendererData.rendererFeatures.Where((f) => f.name == featureName).FirstOrDefault();
    }

    void Update()
    {
        if(feature)
        {
            var blurFeature = feature as GaussianBlurHandler;
            blurFeature.settings.blurPasses = passes;
            blurFeature.settings.downsample = downsampling;
            rendererData.SetDirty();
        }
    }
}
