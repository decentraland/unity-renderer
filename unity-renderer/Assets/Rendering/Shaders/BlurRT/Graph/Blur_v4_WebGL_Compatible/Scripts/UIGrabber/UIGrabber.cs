using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIGrabber : MonoBehaviour
{
    [Header("UI - Cam - RenderTexture Setup")]
    [SerializeField] private GameObject _ui;
    [SerializeField] private Camera _uiCamera;

    [Space]
    
    [Header("Main Camera")]
    
    [SerializeField] private GameObject _mainCamera;
    [SerializeField] private Camera _mainCamComp;

    [Header("Render Texture")]
    [SerializeField] private RenderTexture _renderTexture;

    [Space]

    [Header("Render Texture to Texture2D")]
    [SerializeField] private Texture2D _grabbedRTTexture2D;

    [Space]

    [Header("Debug")]

    [SerializeField] private bool _isDebug = false;

    // list for ui elements
    [SerializeField] private List<CanvasRenderer> _uiElements = new List<CanvasRenderer>();

    

    private void Awake()
    {
        // 1. First step assign the render texture
        SetRenderTexture(_renderTexture);

        // main camera camera component
        _mainCamComp = _mainCamera.GetComponent<Camera>();

    }
    // Start is called before the first frame update
    private void Start()
    {
        // 2. Second step grab the ui elements
        SetCanvasChildren();
    }
    
    

    // Update is called once per frame
    void Update()
    {
        // Check if first ui element is closed
        // then change the main camera layers to everything
        
        CheckForActiveUI();
        // 2. Second step grab the ui elements
        SetCanvasChildren();
    }

    // check for active UI
    private void CheckForActiveUI()
    {
        /*
         * examples 
         * 
         * render everything except transparent layer
         * ~(1 << LayerMask.NameToLayer("Transparent"))
         * 
         * render only a spesific layer
         * ~1 << LayerMask.NameToLayer("Transparent")
         * 
         */
        
        if (_uiElements[0].transform.gameObject.activeSelf == false)
        {
            // render everything except the UIBlurred
            // TODO: bug here MUST CHECK as its not working as expected
            _mainCamComp.cullingMask = ~(1 << LayerMask.NameToLayer("UI"));
            //_mainCamComp.cullingMask = ~1 << LayerMask.NameToLayer("UI");
        }

        if (_isDebug)
        {
            Debug.Log("Main camera layers is set up to normal");
        }
        
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

        int _counter = -1;
        
        foreach (Transform child in _ui.transform)
        {
            _uiElements.Add(child.GetComponent<CanvasRenderer>());

            _counter++;

            if (_counter == 0 && child.GetComponent<CanvasRenderer>().gameObject.activeSelf == true)
            {
                           
            
                Debug.Log("First element is active");
            
            
                // set proper layers to ui elements
                // first active ui element becomes UIMainTab

                _uiElements[_counter].gameObject.layer = 7;

            }

        }


        // second active ui element is UI Layer , no changes here
        // projector ui element is UIBlurred , no changes here

        // 3. Third step pass canvas zise and settings
        // Get dimensions of ui canvas
        ResetCameraToCanvas();
    }

    // get dimensions of ui canvas
    public void ResetCameraToCanvas()
    {
        float _offset = 0.12f;
        // new cam position - offset
        UnityEngine.Vector3 newCamPos = new UnityEngine.Vector3(_ui.transform.position.x, _ui.transform.position.y, (_ui.transform.position.z - _offset));
        
        // rest camera position to canvas
        _uiCamera.transform.position = newCamPos;

        // 4. Forth step match camera size to canvas size
        
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

        _grabbedRTTexture2D = toTexture2D(_renderTexture);

        // change layers of the cameras 

        // Main Camera --> Culling Mask UI OFF so we dont see the ui elements that have this layer
        OccludeUILayer();
        // dont forget to set it back to see UI Layer at the end of the process

    }

    // function that occludes UI Layer from the main camera
    public void OccludeUILayer()
    {
        // occlude UI Layer from the main camera

        _mainCamComp.cullingMask = ~(1 << 5);

        if (_isDebug == true)
        {
            Debug.Log("Occluded UI Layer from the main camera");
        }

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



    #region Secondary Functions

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

        Graphics.Blit(_grabbedRTTexture2D, renderTexture);
    }




    // take dimensions of first active ui element
    public Vector2 GetDimensionsFirstActiveElement()
    {
        foreach (CanvasRenderer element in _uiElements)
        {
            if (element.gameObject.activeSelf)
            {
                if (_isDebug == true)
                {
                    Debug.Log($" {element.name} is being checked");
                }
                var rectTransform = element.transform.GetComponent<RectTransform>();
                float width = rectTransform.sizeDelta.x;
                float height = rectTransform.sizeDelta.y;

                if (_isDebug == true)
                {
                    Debug.Log($"UI {element.name} dimensions are x: {width} y: {height} ");
                }

                return new Vector2(width, height);
            }

            break;
        }
        return Vector2.zero;
    }
    #endregion
}
