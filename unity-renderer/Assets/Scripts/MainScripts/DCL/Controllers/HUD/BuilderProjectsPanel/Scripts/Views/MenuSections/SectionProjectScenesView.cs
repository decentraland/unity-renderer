using System;
using DCL.Helpers;
using UnityEngine;
using UnityEngine.UI;

internal class SectionProjectScenesView : MonoBehaviour
{
    public event Action OnScrollRectValueChanged;

    [SerializeField] public Transform scenesCardContainer;
    [SerializeField] public ScrollRect scrollRect;

    public void SetParent(Transform parent)
    {
        transform.SetParent(parent);
        transform.ResetLocalTRS();
    }

    public void SetActive(bool active)
    {
        gameObject.SetActive(active);
    }

    public void ResetScrollRect()
    {
        scrollRect.verticalNormalizedPosition = 1;
    }

    private void Awake()
    {
        scrollRect.onValueChanged.AddListener(OnScrollValueChanged);
    }

    private void OnDestroy()
    {
        scrollRect.onValueChanged.RemoveListener(OnScrollValueChanged);
    }

    private void OnScrollValueChanged(Vector2 value)
    {
        OnScrollRectValueChanged?.Invoke();
    }
}
