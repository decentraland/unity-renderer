using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class NavmapHandler : MonoBehaviour, IDragHandler
{
    [SerializeField] Material mapMaterial;
    [SerializeField] internal InputAction_Hold zoomIn;
    [SerializeField] internal InputAction_Hold zoomOut;
    [SerializeField] internal AnimationCurve zoomCurve;
    [SerializeField] internal InputAction_Measurable mouseWheelAction;

    private bool isScaling = false;
    private const float MOUSE_WHEEL_THRESHOLD = 0.04f;
    private const float MAP_ZOOM_LEVELS = 4;
    private int currentZoomLevel;
    private float scale = 1f;
    Vector3 previousScaleSize;
    private float scaleDuration = 0.2f;

    void Start()
    {
        mapMaterial.SetVector("Vector2_75b8f85ff9324f379c2c6ddf2acb8f4f", new Vector4(965,540,0,0));
        mouseWheelAction.OnValueChanged += OnMouseWheelChangeValue;
        zoomIn.OnStarted += OnZoomPlusMinus;
        zoomOut.OnStarted += OnZoomPlusMinus;
        ResetCameraZoom();
    }

    private void ResetCameraZoom()
    {
        currentZoomLevel = Mathf.FloorToInt(MAP_ZOOM_LEVELS / 2);
        scale = zoomCurve.Evaluate(currentZoomLevel);
        mapMaterial.SetFloat("Vector1_d6b5d20802a0431ea86843707c4780d1", scale);
    }

    private void OnMouseWheelChangeValue(DCLAction_Measurable action, float value)
    {
        if (value > -MOUSE_WHEEL_THRESHOLD && value < MOUSE_WHEEL_THRESHOLD) return;
        CalculateZoomLevelAndDirection(value);
    }

    private void OnZoomPlusMinus(DCLAction_Hold action)
    {
        if (action.Equals(DCLAction_Hold.ZoomIn))
        {
            CalculateZoomLevelAndDirection(1);
        }
        else if (action.Equals(DCLAction_Hold.ZoomOut))
        {
            CalculateZoomLevelAndDirection(-1);
        }
    }

    private void CalculateZoomLevelAndDirection(float value)
    {
        if (isScaling) return;
        previousScaleSize = new Vector3(scale, scale, scale);
        if (value > 0 && currentZoomLevel < MAP_ZOOM_LEVELS)
        {
            currentZoomLevel++;
            StartCoroutine(ScaleOverTime(previousScaleSize));
        }
        if (value < 0 && currentZoomLevel >= 1)
        {
            currentZoomLevel--;
            StartCoroutine(ScaleOverTime(previousScaleSize));
        }
    }

    private IEnumerator ScaleOverTime(Vector3 startScaleSize)
    {
        isScaling = true;
        scale = zoomCurve.Evaluate(currentZoomLevel);
        Vector3 targetScale = new Vector3(scale, scale, scale);

        float counter = 0;

        while (counter < scaleDuration)
        {
            counter += Time.deltaTime;
            mapMaterial.SetFloat("Vector1_d6b5d20802a0431ea86843707c4780d1", scale);
            yield return null;
        }

        isScaling = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        mapMaterial.SetVector("Vector2_75b8f85ff9324f379c2c6ddf2acb8f4f", mapMaterial.GetVector("Vector2_75b8f85ff9324f379c2c6ddf2acb8f4f") - (Vector4)eventData.delta);
    }

}
