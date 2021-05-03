using System;
using DCL.Helpers;
using UnityEngine;
using UnityEngine.UI;

internal class SectionDeployedScenesView : MonoBehaviour, IDisposable
{
    public event Action OnScrollRectValueChanged;
    
    [SerializeField] public Transform scenesCardContainer;
    [SerializeField] public ScrollRect scrollRect;
    [SerializeField] internal GameObject contentContainer;
    [SerializeField] internal GameObject emptyContainer;
    [SerializeField] internal GameObject noSearchResultContainer;
    [SerializeField] internal GameObject loadingAnimationContainer;

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

    public void SetEmpty()
    {
        contentContainer.SetActive(false);
        emptyContainer.SetActive(true);
        noSearchResultContainer.SetActive(false);
        loadingAnimationContainer.SetActive(false);
    }
    
    public void SetLoading()
    {
        contentContainer.SetActive(false);
        emptyContainer.SetActive(false);
        noSearchResultContainer.SetActive(false);
        loadingAnimationContainer.SetActive(true);
    }
    
    public void SetNoSearchResult()
    {
        contentContainer.SetActive(false);
        emptyContainer.SetActive(false);
        noSearchResultContainer.SetActive(true);
        loadingAnimationContainer.SetActive(false);
    }
    
    public void SetFilled()
    {
        contentContainer.SetActive(true);
        emptyContainer.SetActive(false);
        noSearchResultContainer.SetActive(false);
        loadingAnimationContainer.SetActive(false);
        ResetScrollRect();
    }

    public Transform GetCardsContainer()
    {
        return scenesCardContainer;
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
