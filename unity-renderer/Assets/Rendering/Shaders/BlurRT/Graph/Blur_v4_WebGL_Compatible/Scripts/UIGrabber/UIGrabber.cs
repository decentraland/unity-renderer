using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIGrabber : MonoBehaviour
{
    [Header("UI - Cam - RenderTexture Setup")]
    [SerializeField] private GameObject _ui;
    [SerializeField] private Camera _uiCamera;

    [Header("Render Texture")]
    [SerializeField] private RenderTexture _renderTexture;

    // list for ui elements
    [SerializeField] private List<CanvasRenderer> _uiElements = new List<CanvasRenderer>();
    


    private void Awake()
    {
        SetRenderTexture(_renderTexture);
    }
    // Start is called before the first frame update
    private void Start()
    {
        SetCanvasChildren();
    }
    
    

    // Update is called once per frame
    void Update()
    {
        
    }

    // set render texture to ui camera
    public void SetRenderTexture(RenderTexture renderTexture)
    {
        _uiCamera.targetTexture = renderTexture;
    }

    // set canvas children to list
    public void SetCanvasChildren()
    {
        _uiElements.Clear();
        foreach (Transform child in _ui.transform)
        {
            _uiElements.Add(child.GetComponent<CanvasRenderer>());
        }
    }
}
