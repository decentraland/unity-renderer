using DCL.Models;
using DCL.Components;
using UnityEngine;
using DCL.Interface;
using TMPro;

public class InteractionHoverCanvasController : MonoBehaviour
{
    public Vector3 offset = new Vector2(0f, 50f);
    public Canvas canvas;
    public RectTransform backgroundTransform;
    public TextMeshProUGUI text;
    public GameObject pointerActionIconPrefab;
    public GameObject primaryActionIconPrefab;
    public GameObject secondaryActionIconPrefab;
    public GameObject unknownActionIconPrefab;

    Camera mainCamera;
    GameObject hoverIcon;
    Vector3 meshCenteredPos;
    DecentralandEntity entity;

    void Awake()
    {
        mainCamera = Camera.main;
    }

    public void Setup(WebInterface.ACTION_BUTTON button, string feedbackText, DecentralandEntity entity)
    {
        text.text = feedbackText;
        this.entity = entity;

        ConfigureIcon(button);

        entity.OnTransformChange -= CalculateMeshCenteredPos;
        entity.OnTransformChange += CalculateMeshCenteredPos;
        CalculateMeshCenteredPos();

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

        CalculateMeshCenteredPos();
        UpdateSizeAndPos();
    }

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

            ((RectTransform)backgroundTransform).anchoredPosition = screenPoint + offset;
        }
    }
}
