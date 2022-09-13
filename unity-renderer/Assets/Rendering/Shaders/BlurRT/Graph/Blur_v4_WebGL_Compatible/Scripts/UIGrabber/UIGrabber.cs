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

    [Space]

    [Header("Render Texture to Texture2D")]
    [SerializeField] private Texture2D _grabbedRTTexture2D;

    [Space]

    [Header("Debug")]

    [SerializeField] private bool _debug = false;

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

        // Get Dimensions of first active ui element that blur will be applied to
        //GetDimensionsFirstActiveElement();

        // Get dimensions of ui canvas
        ResetCameraToCanvas();
    }

    // check if ui element is active and has a rect transform
    public bool CheckIfActive(RectTransform rectTransform)
    {
        if (rectTransform.gameObject.activeInHierarchy && rectTransform != null)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    // get dimensions of ui canvas
    public void ResetCameraToCanvas()
    {
        float _offset = 0.12f;
        // new cam position - offset
        Vector3 newCamPos = new Vector3(_ui.transform.position.x, _ui.transform.position.y, (_ui.transform.position.z - _offset));
        
        // rest camera position to canvas
        _uiCamera.transform.position = newCamPos;

        // MatchCameraSizeToCanvas
        MatchCameraSizeToCanvas();
    }

    // match camera size to canvas size
    public void MatchCameraSizeToCanvas()
    {
        // get canvas rect transform
        RectTransform canvasRectTransform = _ui.GetComponent<RectTransform>();

        // get canvas width and height
        float canvasWidth = canvasRectTransform.rect.width;
        float canvasHeight = canvasRectTransform.rect.height;
        float canvasScale = canvasRectTransform.localScale.x;

        // set camera size to canvas width and height
        _uiCamera.orthographicSize = (canvasWidth * canvasScale) / 2;

        // Grab Render Texture
        //GrabRenderTexture();

        _grabbedRTTexture2D = toTexture2D(_renderTexture);
    }


    Texture2D toTexture2D(RenderTexture rTex)
    {
        Texture2D tex = new Texture2D(512, 512, TextureFormat.RGB24, false);
        // ReadPixels looks at the active RenderTexture.
        RenderTexture.active = rTex;
        tex.ReadPixels(new Rect(0, 0, rTex.width, rTex.height), 0, 0);
        tex.Apply();
        return tex;
    }
    
    // grab render texture to a texture
    public void GrabRenderTexture()
    {
        // get render texture
        RenderTexture renderTexture = _uiCamera.targetTexture;

        // create new texture2D
        _grabbedRTTexture2D = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.RGB24, true);

        // read pixels from render texture
        //RenderTexture.active = renderTexture;
        //_grabbedRTTexture2D.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
        //_grabbedRTTexture2D.Apply();

        // set render texture to null
        RenderTexture.active = null;

        Graphics.Blit(_grabbedRTTexture2D , renderTexture);
    }




    // take dimensions of first active ui element
    public Vector2 GetDimensionsFirstActiveElement()
    {
        foreach (CanvasRenderer element in _uiElements)
        {
            if (element.gameObject.activeSelf)
            {
                if (_debug == true )
                {
                    Debug.Log($" {element.name} is being checked");
                }
                var rectTransform = element.transform.GetComponent<RectTransform>();
                float width = rectTransform.sizeDelta.x;
                float height = rectTransform.sizeDelta.y;

                if (_debug == true)
                {
                    Debug.Log($"UI {element.name} dimensions are x: {width} y: {height} ");
                }
                
                return new Vector2(width, height);                
            }

            break;
        }
        return Vector2.zero;
    }
}
