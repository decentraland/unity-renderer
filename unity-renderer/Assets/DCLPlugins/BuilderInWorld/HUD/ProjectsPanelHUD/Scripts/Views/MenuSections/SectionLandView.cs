using System;
using DCL.Helpers;
using UnityEngine;
using UnityEngine.UI;

internal class SectionLandView : MonoBehaviour, IDisposable
{
    public event Action OnOpenMarketplaceRequested;

    [SerializeField] internal LandElementView landElementView;
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] internal GameObject contentContainer;
    [SerializeField] internal GameObject emptyContainer;
    [SerializeField] internal GameObject loadingAnimationContainer;
    [SerializeField] internal GameObject noSearchResultContainer;
    [SerializeField] internal Button buttonNoLandCTA;

    private bool isDestroyed = false;

    private void Awake() { buttonNoLandCTA.onClick.AddListener(() => OnOpenMarketplaceRequested?.Invoke()); }

    private void OnDestroy() { isDestroyed = true; }

    public void SetParent(Transform parent)
    {
        transform.SetParent(parent);
        transform.ResetLocalTRS();
    }

    public void SetActive(bool active) { gameObject.SetActive(active); }

    public void ResetScrollRect() { scrollRect.verticalNormalizedPosition = 1; }

    public Transform GetLandElementsContainer() { return landElementView.GetParent(); }

    public LandElementView GetLandElementeBaseView() { return landElementView; }

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

    public void Dispose()
    {
        if (!isDestroyed)
        {
            Destroy(gameObject);
        }
    }
}