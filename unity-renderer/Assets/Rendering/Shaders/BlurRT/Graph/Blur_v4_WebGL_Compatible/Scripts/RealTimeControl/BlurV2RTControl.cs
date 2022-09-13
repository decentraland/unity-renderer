using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BlurV2RTControl : MonoBehaviour
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

    [Header("Shader Properties")]

    [SerializeField] private string _DistrortionPowerName = "_DistrortionPower";
    [SerializeField] private string _BlurAmountName = "_BlurAmount";
    [SerializeField] private string _ReflectionPassName = "_ReflectionPass";
    [SerializeField] private string _LightPassName = "_LightPass";
    [SerializeField] private string _FilmOpacityName = "_FilmOpacity";
    [SerializeField] private string _DepthColorAmountName = "_DepthColorAmount";



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


    }

    public void SetDefaults()
    {

        // set default values to ui 
        _DistrortionPowerSlider.value = 0;
        _BlurAmountSlider.value = 0.4f;
        _ReflectionPassSlider.value = 0.35f;
        _LightPassSlider.value = 0.65f;
        _FilmOpacitySlider.value = 0.8f;
        _DepthColorAmountSlider.value = 0.5f;

        // set default values to the shader properties

        _blurMaterial.SetFloat(_DistrortionPowerName, 0);
        _blurMaterial.SetVector("_BlurAmount", new Vector4(0.4f, 0.4f, 0.4f, 0.4f));
        _blurMaterial.SetFloat(_ReflectionPassName, 0.35f);
        _blurMaterial.SetFloat(_LightPassName, 0.65f);
        _blurMaterial.SetFloat(_FilmOpacityName, 0.8f);
        _blurMaterial.SetFloat(_DepthColorAmountName, 0.5f);
    }

    // on disable
    private void OnDisable()
    {
        SetDefaults();
    }
}
