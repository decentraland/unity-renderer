using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BlurV3RTControl : MonoBehaviour
{
    [Header("Material")]
    [SerializeField] private Material _blurMaterial;
    
    [Space]
    
    [Header("UI Controls")]

    // ui slider 

    [SerializeField] private Slider _DistrortionPowerSlider;
    [SerializeField] private Slider _BlurAmountSlider;
    
    [Space]

    [SerializeField] private Slider _ReflectionPassSlider;
    [SerializeField] private Slider _LightPassSlider;
    [SerializeField] private Slider _FilmOpacitySlider;
    [SerializeField] private Slider _DepthColorAmountSlider;

    [Space]

    // blur power slider
    [SerializeField] private Slider _BlurPowerSlider;
    // Ior hor slider
    [SerializeField] private Slider _IorHorSlider;
    // Ior Ver slider
    [SerializeField] private Slider _IorVerSlider;
    // Ior Opacity Volume slider
    [SerializeField] private Slider _IorOpacityVolumeSlider;
    // Box Blur Opacity slider
    [SerializeField] private Slider _BoxBlurOpacitySlider;

    [Space]

    [Header("Shader Properties")]

    [SerializeField] private string _DistrortionPowerName = "_DistrortionPower";
    [SerializeField] private string _BlurAmountName = "_BlurAmount";
    [SerializeField] private string _ReflectionPassName = "_ReflectionPass";
    [SerializeField] private string _LightPassName = "_LightPass";
    [SerializeField] private string _FilmOpacityName = "_FilmOpacity";
    [SerializeField] private string _DepthColorAmountName = "_DepthColorAmount";

    [Space]

    [SerializeField] private string _BlurPowerName = "_BlurPower";
    [SerializeField] private string _IorHorName = "_IorHor";
    [SerializeField] private string _IorVerName = "_IorVer";
    [SerializeField] private string _IorOpacityVolumeName = "_IorOpacityVolume";
    [SerializeField] private string _BoxBlurOpacityName = "_BoxBlurOpacity";
    



    // awake
    private void Awake()
    {
        // set default values
        SetDefaults();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        GetMaterialPropertiesValues();
    }

    // get material properties values and store them to the shader properties
    public void GetMaterialPropertiesValues()
    {
        // DistrortionPower
        _blurMaterial.SetFloat(_DistrortionPowerName, _DistrortionPowerSlider.value);

        // BlurAmount
        _blurMaterial.SetVector(_BlurAmountName, new Vector4(_BlurAmountSlider.value , 0.4f, 0.4f, 0.4f));
        
        // ReflectionPass
        _blurMaterial.SetFloat(_ReflectionPassName, _ReflectionPassSlider.value);

        // LightPass
        _blurMaterial.SetFloat(_LightPassName, _LightPassSlider.value);

        // FilmOpacity
        _blurMaterial.SetFloat(_FilmOpacityName, _FilmOpacitySlider.value);

        // DepthColorAmount
        _blurMaterial.SetFloat(_DepthColorAmountName, _DepthColorAmountSlider.value);

        // blur power
        _blurMaterial.SetFloat(_BlurPowerName, _BlurPowerSlider.value);

        // ior hor
        _blurMaterial.SetFloat(_IorHorName, _IorHorSlider.value);

        // ior ver
        _blurMaterial.SetFloat(_IorVerName, _IorVerSlider.value);

        // ior opacity volume
        _blurMaterial.SetFloat(_IorOpacityVolumeName, _IorOpacityVolumeSlider.value);

        // box blur opacity
        _blurMaterial.SetFloat(_BoxBlurOpacityName, _BoxBlurOpacitySlider.value);


    }

    public void SetDefaults()
    {

        // set default values to ui 
        _DistrortionPowerSlider.value = 0;
        _BlurAmountSlider.value = 0.4f;
        _ReflectionPassSlider.value = 0.35f;
        _LightPassSlider.value = 1f;
        _FilmOpacitySlider.value = 0.8f;
        _DepthColorAmountSlider.value = 0.85f;
        
        _BlurPowerSlider.value = 0.085f;
        _IorHorSlider.value = 0.004559522f;
        _IorVerSlider.value = 0.01706193f;
        _IorOpacityVolumeSlider.value = 1f;
        _BoxBlurOpacitySlider.value = 1f;

        // set default values to the shader properties

        _blurMaterial.SetFloat(_DistrortionPowerName, 0);
        _blurMaterial.SetVector("_BlurAmount", new Vector4(0.4f, 0.4f, 0.4f, 0.4f));
        _blurMaterial.SetFloat(_ReflectionPassName, 0.35f);
        _blurMaterial.SetFloat(_LightPassName, 1f);
        _blurMaterial.SetFloat(_FilmOpacityName, 0.8f);
        _blurMaterial.SetFloat(_DepthColorAmountName, 0.85f);

        _blurMaterial.SetFloat(_BlurPowerName, 0.085f);
        _blurMaterial.SetFloat(_IorHorName, 0.004559522f);
        _blurMaterial.SetFloat(_IorVerName, 0.01706193f);
        _blurMaterial.SetFloat(_IorOpacityVolumeName, 1f);
        _blurMaterial.SetFloat(_BoxBlurOpacityName, 1f);
        
    }

    // on disable
    private void OnDisable()
    {
        SetDefaults();
    }

    // on destroy
    private void OnDestroy()
    {
        SetDefaults();
    }
}
