using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;
 
/// <summary>
/// Enables the use of the Universal Render Pipeline's post-processing stack PER render feature
/// </summary>
public class ToggleRendererFeature : MonoBehaviour
{
    [SerializeField] private ScriptableRendererFeature featureOutlined;
    [SerializeField] private ScriptableRendererFeature featureOccluded;
    
    [SerializeField] private bool isOutlined = false;
    [SerializeField] private bool isOccluded = false;
 
    // Start is called before the first frame update
    void Start()
    {
        //StartCoroutine(ToggleOutlined());
        CheckRendererFeaturesOutlined();
        CheckRendererFeaturesOccluded();
    }

    #region Check Renderer Features
    private void CheckRendererFeaturesOutlined()
    {
        // check for is outlined and call the coroutine
        if (isOutlined == true)
        {
            StartCoroutine(EnableOutlined());
        }
        else
        {
            StartCoroutine(DisableOutlined());
        }
        
    }
    
    private void CheckRendererFeaturesOccluded()
    {
        if (isOccluded == true)
        {
            StartCoroutine(EnableOccluded());
        }
        else
        {
            StartCoroutine(DisableOccluded());
        }
    }
    
    #endregion

    #region Enable Disable Render feature Version

    IEnumerator EnableOutlined()
    {
        
        featureOutlined.SetActive(true);
        yield return new WaitForSeconds(1);
    }
    
    IEnumerator DisableOutlined()
    {
        
        featureOutlined.SetActive(false);
        yield return new WaitForSeconds(1);
    }

    IEnumerator EnableOccluded()
    {
        
        featureOccluded.SetActive(true);
        yield return new WaitForSeconds(1);
    }
    
    IEnumerator DisableOccluded()
    {

        
        featureOccluded.SetActive(false);
        yield return new WaitForSeconds(1);

    }

    #endregion
    

    #region Toggled Version

    #region Toggle Outlined
    IEnumerator ToggleOutlined()
    {
        while(true) {
            featureOutlined.SetActive(!featureOutlined.isActive);
            yield return new WaitForSecondsRealtime(1);
        }
    }

    #endregion
    
    
    #region Toggle Occluded
    IEnumerator ToggleOccluded()
    {
        while(true) {
            featureOccluded.SetActive(!featureOccluded.isActive);
            yield return new WaitForSecondsRealtime(1);
        }
    }
    #endregion

    #endregion

}