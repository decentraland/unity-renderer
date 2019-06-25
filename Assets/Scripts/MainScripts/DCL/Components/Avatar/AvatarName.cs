using UnityEngine;

public class AvatarName : MonoBehaviour
{
    public CanvasGroup uiContainer;
    public Transform sourceTransform;
    public Vector3 offset;
    Canvas canvas;
    Camera mainCamera;

    private void Start()
    {
        mainCamera = Camera.main;
        canvas = GetComponentInParent<Canvas>();
    }
    void LateUpdate()
    {
        Transform t = sourceTransform;
        Vector3 screenPoint = mainCamera.WorldToViewportPoint(t.position + offset);
        float near = (mainCamera.farClipPlane - mainCamera.nearClipPlane) * 0.75f;
        uiContainer.alpha = 1.0f + (1.0f - (screenPoint.z / 40.0f));

        if (screenPoint.z > 0)
        {
            if (!uiContainer.gameObject.activeSelf)
            {
                uiContainer.gameObject.SetActive(true);
            }

            RectTransform canvasRect = (RectTransform)canvas.transform;
            float width = canvasRect.rect.width;
            float height = canvasRect.rect.height;
            screenPoint.Scale(new Vector3(width, height, 0));
            ((RectTransform)transform).anchoredPosition = screenPoint;
        }
        else
        {
            if (uiContainer.gameObject.activeSelf)
            {
                uiContainer.gameObject.SetActive(false);
            }
        }
    }
}
