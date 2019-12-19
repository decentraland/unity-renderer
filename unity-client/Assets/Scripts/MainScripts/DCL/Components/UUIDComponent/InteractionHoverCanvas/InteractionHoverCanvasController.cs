using UnityEngine;
using DCL.Interface;
using TMPro;

public class InteractionHoverCanvasController : MonoBehaviour
{
    public Vector3 offset = new Vector3(0f, 0f, 0f);
    public Canvas canvas;
    public RectTransform backgroundTransform;
    public TextMeshProUGUI text;
    public GameObject pointerActionIconPrefab;
    public GameObject primaryActionIconPrefab;
    public GameObject secondaryActionIconPrefab;
    public GameObject unknownActionIconPrefab;

    Camera mainCamera;
    GameObject hoverIcon;

    void Awake()
    {
        mainCamera = Camera.main;
    }

    public void Setup(WebInterface.ACTION_BUTTON button, string feedbackText)
    {
        text.text = feedbackText;

        ConfigureIcon(button);

        Hide();
    }

    void ConfigureIcon(WebInterface.ACTION_BUTTON button)
    {
        // When we allow for custom input key bindings this implementation will change

        if (hoverIcon != null)
            Destroy(hoverIcon);

        switch (button)
        {
            case WebInterface.ACTION_BUTTON.POINTER:
                hoverIcon = Object.Instantiate(pointerActionIconPrefab, backgroundTransform);
                break;
            case WebInterface.ACTION_BUTTON.PRIMARY:
                hoverIcon = Object.Instantiate(primaryActionIconPrefab, backgroundTransform);
                break;
            case WebInterface.ACTION_BUTTON.SECONDARY:
                hoverIcon = Object.Instantiate(secondaryActionIconPrefab, backgroundTransform);
                break;
            default: // WebInterface.ACTION_BUTTON.UNKNOWN
                hoverIcon = Object.Instantiate(unknownActionIconPrefab, backgroundTransform);
                break;
        }
    }

    public void Show()
    {
        canvas.enabled = true;
    }

    public void Hide()
    {
        canvas.enabled = false;
    }

    void LateUpdate()
    {
        if (!canvas.enabled) return;

        UpdateSizeAndPos();
    }

    public void UpdateSizeAndPos()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;

        Vector3 screenPoint = mainCamera.WorldToViewportPoint(transform.parent.position + offset);

        if (screenPoint.z > 0)
        {
            RectTransform canvasRect = (RectTransform)canvas.transform;
            float width = canvasRect.rect.width;
            float height = canvasRect.rect.height;
            screenPoint.Scale(new Vector3(width, height, 0));

            ((RectTransform)backgroundTransform).anchoredPosition = screenPoint;
        }
    }
}
