using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UniOutline.Outline;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UniOutline.Outline;

/// <summary>
/// Sets the layer of the object based on the outline render flag PER object
/// </summary>
public class UniformOutlineControl : MonoBehaviour
{
    #region Variables

    [SerializeField] private ForwardRendererData rendererData;
    
    // make an OutlineRenderFlags enum dropdown with all the outline modes
    [Tooltip("The outline render mode to use")]
    [SerializeField] private OutlineRenderFlags outlineRenderFlags;
    
    [Tooltip("The layer to set the object to default after mouse exit")]
    [SerializeField] private OutlineLayerDefault outlineLayerDefault;
    
    #endregion
    
    
    // awake is called before start
    private void Awake()
    {
        //Init();
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
        switch (outlineRenderFlags)
        {
            case OutlineRenderFlags.Standard :
                gameObject.layer = 26;
                
                break;
            
            case OutlineRenderFlags.Occluded :
                gameObject.layer = 27;
                
                break;
            case OutlineRenderFlags.Blurred :
                gameObject.layer = 28;
                
                break;
        }
        
    }
    
    // on mouse exit change layer of an object to default
    void OnMouseExit()
    {
        gameObject.layer = (int) outlineLayerDefault;
    }

    #endregion
    
    
    
    
}
