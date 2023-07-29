using DG.Tweening;
using UnityEngine;

public class PanelSizeModeAnimator : MonoBehaviour
{
    private const float DURATION = 0.5f;

    [SerializeField] private RectTransform panelRectTransform;

    private float rightOffset;
    private float initOffset;

    private bool isExpanded;

    private void Awake()
    {
        initOffset = panelRectTransform.offsetMax.x;
        rightOffset = -initOffset;
    }

    public void ToggleSizeMode()
    {
        if (isExpanded)
            DOVirtual.Float(0, -initOffset, DURATION, UpdateSizeMode);
        else
            DOVirtual.Float(rightOffset, 0, DURATION, UpdateSizeMode);

        isExpanded = !isExpanded;
    }

    private void UpdateSizeMode(float value)
    {
        rightOffset = value;
        panelRectTransform.offsetMax = new Vector2(-rightOffset, panelRectTransform.offsetMax.y);
    }
}
