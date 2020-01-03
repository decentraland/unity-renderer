using DCL.Models;
using DCL.Components;
using UnityEngine;
using TMPro;

public class InteractionHoverCanvasController : MonoBehaviour
{
    public Canvas canvas;
    public RectTransform backgroundTransform;
    public TextMeshProUGUI text;
    public GameObject pointerActionIconPrefab;
    public GameObject primaryActionIconPrefab;
    public GameObject secondaryActionIconPrefab;
    public GameObject anyActionIconPrefab;

    bool isHovered = false;
    Camera mainCamera;
    GameObject hoverIcon;
    Vector3 meshCenteredPos;
    DecentralandEntity entity;

    const string ACTION_BUTTON_POINTER = "POINTER";
    const string ACTION_BUTTON_PRIMARY = "PRIMARY";
    const string ACTION_BUTTON_SECONDARY = "SECONDARY";

    void Awake()
    {
        mainCamera = Camera.main;
    }

    public void Setup(string button, string feedbackText, DecentralandEntity entity)
    {
        text.text = feedbackText;
        this.entity = entity;

        ConfigureIcon(button);

        canvas.enabled = enabled && isHovered;
    }

    void ConfigureIcon(string button)
    {
        // When we allow for custom input key bindings this implementation will change

        if (hoverIcon != null)
            Destroy(hoverIcon);

        switch (button)
        {
            case ACTION_BUTTON_POINTER:
                hoverIcon = Object.Instantiate(pointerActionIconPrefab, backgroundTransform);
                break;
            case ACTION_BUTTON_PRIMARY:
                hoverIcon = Object.Instantiate(primaryActionIconPrefab, backgroundTransform);
                break;
            case ACTION_BUTTON_SECONDARY:
                hoverIcon = Object.Instantiate(secondaryActionIconPrefab, backgroundTransform);
                break;
            default: // WebInterface.ACTION_BUTTON.ANY
                hoverIcon = Object.Instantiate(anyActionIconPrefab, backgroundTransform);
                break;
        }
    }

    public void SetHoverState(bool hoverState)
    {
        if (!enabled || hoverState == isHovered) return;

        isHovered = hoverState;

        canvas.enabled = isHovered;
    }

    // This method will be used when we apply a "loose aim" for the 3rd person camera
    void CalculateMeshCenteredPos(DCLTransform.Model transformModel = null)
    {
        if (!canvas.enabled) return;

        if (entity.meshesInfo.renderers == null || entity.meshesInfo.renderers.Length == 0)
        {
            meshCenteredPos = transform.parent.position;
        }
        else
        {
            Vector3 sum = Vector3.zero;
            for (int i = 0; i < entity.meshesInfo.renderers.Length; i++)
            {
                sum += entity.meshesInfo.renderers[i].bounds.center;
            }

            meshCenteredPos = sum / entity.meshesInfo.renderers.Length;
        }
    }

    // This method will be used when we apply a "loose aim" for the 3rd person camera
    public void UpdateSizeAndPos()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;

        Vector3 screenPoint = mainCamera.WorldToViewportPoint(meshCenteredPos);

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
