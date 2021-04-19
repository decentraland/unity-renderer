using System;
using DCL.Helpers;
using UnityEngine;
using UnityEngine.UI;

internal class SectionDeployedScenesView : MonoBehaviour, IDisposable
{
    public event Action OnScrollRectValueChanged;
    
    [SerializeField] public Transform scenesCardContainer;
    [SerializeField] public ScrollRect scrollRect;

    private bool isDestroyed = false;
    
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

    public void Dispose()
    {
        if (!isDestroyed)
        {
            Destroy(gameObject);
        }
    }

    private void Awake()
    {
        scrollRect.onValueChanged.AddListener(OnScrollValueChanged);
    }

    private void OnDestroy()
    {
        isDestroyed = true;
        scrollRect.onValueChanged.RemoveListener(OnScrollValueChanged);
    }

    private void OnScrollValueChanged(Vector2 value)
    {
        OnScrollRectValueChanged?.Invoke();
    }
}
