using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InfFloorBlenderControl : MonoBehaviour
{
    [Header("Skybox Data")]
    [SerializeField] private Material skyboxMaterial;
    
    [Header("Infinite Floor Blender Data")]
    [SerializeField] private Material infFloorBlenderMaterial;
    
    [Space]
    [Header("Colors Procedural Skybox ")] 
    [SerializeField] private Color _proceduralSkyboxGroundColor;
    [SerializeField] private Color _proceduralSkyboxHorizonColor;
    
    [Space]
    [Header("Time Interval")]
    [Tooltip("Time interval dictates the amount of time script will wait before changing the colors")]
    
    [SerializeField] private bool _isTimeIntervalEnabled;
    [SerializeField] private float _timeInterval = 5f;
    
    [Header("Debug")]
    [SerializeField] private bool _isDebug;
    
    // Start is called before the first frame update

    private void Awake()
    {
        
    }
    void Start()
    {
        GetProceduralSkyboxColors();
        SetProceduralInfBlenderColors();
    }

    // Update is called once per frame
    void Update()
    {
        // example
        if (_isTimeIntervalEnabled == true)
        {
            // every 5 seconds change the colors
            Invoke("GetProceduralSkyboxColors" , _timeInterval);
            Invoke("SetProceduralInfBlenderColors" , _timeInterval);
        }
    }
    
    // get _proceduralSkyboxGroundColor and _proceduralSkyboxHorizonColor from skybox material
    private void GetProceduralSkyboxColors()
    {
        _proceduralSkyboxGroundColor = skyboxMaterial.GetColor("_groundColor");
        _proceduralSkyboxHorizonColor = skyboxMaterial.GetColor("_horizonColor");

        if (_isDebug == true)
        {
            Debug.Log("Getting Procedural Colors from Skybox");
        }
    }
    
    // set _proceduralSkyboxGroundColor and _proceduralSkyboxHorizonColor to infFloorBlenderMaterial
    private void SetProceduralInfBlenderColors()
    {
        infFloorBlenderMaterial.SetColor("_ColorSky", _proceduralSkyboxHorizonColor);
        infFloorBlenderMaterial.SetColor("_ColorFloor", _proceduralSkyboxGroundColor);
        
        if (_isDebug == true)
        {
            Debug.Log("Setting Procedural Colors to InfFloorBlender");
        }
    }
    
}
