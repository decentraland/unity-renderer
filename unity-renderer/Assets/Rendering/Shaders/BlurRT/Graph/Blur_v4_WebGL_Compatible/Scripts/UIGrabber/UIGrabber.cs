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

    [Header("Debug")]

    [SerializeField] private bool _debug = false;

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
