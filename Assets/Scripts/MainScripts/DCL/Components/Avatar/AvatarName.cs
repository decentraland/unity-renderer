using UnityEngine;

public class AvatarName : MonoBehaviour
{
    const float NAME_VANISHING_POINT_DISTANCE = 40.0f;

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
        uiContainer.alpha = 1.0f + (1.0f - (screenPoint.z / NAME_VANISHING_POINT_DISTANCE));

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
