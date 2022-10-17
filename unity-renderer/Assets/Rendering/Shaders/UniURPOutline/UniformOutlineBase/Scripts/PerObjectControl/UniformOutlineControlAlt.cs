using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UniOutline.Outline;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UniOutline.Outline;
using UnityEngine.Rendering;

/// <summary>
/// Sets the layer of the object based on the outline render flag PER object
/// </summary>
public class UniformOutlineControlAlt : MonoBehaviour
{
    #region Variables

    [SerializeField] private ForwardRendererData rendererData;
    
    // make an OutlineRenderFlags enum dropdown with all the outline modes
    [Tooltip("The outline render mode to use")]
    [SerializeField] private OutlineRenderFlags outlineRenderFlags;
    
    [Tooltip("The layer to set the object to default after mouse exit")]
    [SerializeField] private OutlineLayerDefault outlineLayerDefault;

    [Header("Layer Control")]
    [Space]
    [Tooltip("The layer to set the object to when the mouse is over it")]
    [SerializeField] private bool isLayerChangedOnMouseOver;
    
    [Header("Depth Normals Control")]
    [Space]
    [SerializeField] private bool isExcludedFromDepthNormals = true;

    [SerializeField] private Shader outlineShader;

    [Header("Debug")]
    [Space]
    [SerializeField] private bool isDebug;
    
    #endregion
    
    
    // awake is called before start
    private void Awake()
    {
        //Init();
        ExcludeFromDepthNormals();
        if (isDebug)
        {
            Debug.Log("Entered Exclude from Depth Normals");
        }
    }
    
    // Start is called before the first frame update
    void Start()
    {
        
        rendererData.SetDirty();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void Init()
    {
        // // control the Render Feature
        var uniOutlineFeature = rendererData.rendererFeatures.OfType<OutlineRenderFeature>().FirstOrDefault();
        
        if (uniOutlineFeature == null) return;
        
        
        Debug.Log(uniOutlineFeature._shaderPassNames);
    }

    #region Mouse Control

    // change the layer of the object
    void OnMouseEnter()
    {
        //TODO // check the object tag

        //IncludeToDepthNormals();
        //DisableZwriteOnShader();

    }
    
    // on mouse exit change layer of an object to default
    void OnMouseExit()
    {
        //gameObject.layer = (int) outlineLayerDefault;
        //ExcludeFromDepthNormals();
    }

    #endregion

    #region Colliders check

    // check if object has a box collider on it 
    private bool HasBoxCollider()
    {
        return gameObject.GetComponent<BoxCollider>() != null;
    }
    
    #endregion

    #region Shader Control

    // disable Zwrite from shader
    private void DisableZwriteOnShader()
    {
        if (isExcludedFromDepthNormals)
        {
            // get the shader
            var shader = outlineShader;
            // get the material
            var material = GetComponent<Renderer>().material;
            
            

        }
    }

    #endregion

    #region DepthNormals Operations

    // set ZWrite to 0 if the object has a box collider
    private void ExcludeFromDepthNormals()
    {
        if (HasBoxCollider())
        {
            // exclude from depth normals calculation
            //rendererData.rendererFeatures.OfType<OutlineRenderFeature>().FirstOrDefault()._shaderPassNames[0] = "DepthNormals";
            
            // include in depth normals calculation
            // rendererData.rendererFeatures.OfType<OutlineRenderFeature>().FirstOrDefault()._shaderPassNames[0] = "DepthNormals";
    
        }
    }
    
    private void IncludeToDepthNormals()
    {
        if (HasBoxCollider())
        {
            // exclude from depth normals calculation
            // rendererData.rendererFeatures.OfType<OutlineRenderFeature>().FirstOrDefault()._shaderPassNames[0] = "DepthNormals";
            
            // include in depth normals calculation
            //rendererData.rendererFeatures.OfType<OutlineRenderFeature>().FirstOrDefault()._shaderPassNames[0] = "DepthNormals";
    
        }
    }

    #endregion
    
    

    
    
    
    
    
}
