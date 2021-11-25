using System;
using System.Collections;
using DCL.Helpers;
using UnityEngine;
using UnityEngine.UI;

internal class SectionProjectView : MonoBehaviour, IDisposable
{
    public event Action OnScrollRectValueChanged;
    public event Action OnCreateProjectRequest;

    [SerializeField] public GameObject contentContainer;
    [SerializeField] public GameObject contentGameObject;
    [SerializeField] public ScrollRect scrollRect;
    [SerializeField] internal GameObject emptyContainer;
    [SerializeField] internal GameObject loadingAnimationContainer;
    [SerializeField] internal GameObject noSearchResultContainer;
    [SerializeField] internal Button buttonNoProjectsCTA;

    private bool isDestroyed = false;

    public void SetParent(Transform parent)
    {
        transform.SetParent(parent);
        transform.ResetLocalTRS();
    }

    public void SetActive(bool active) { gameObject.SetActive(active); }

    public void ResetScrollRect() { scrollRect.verticalNormalizedPosition = 1; }

    public void Dispose()
    {
        if (!isDestroyed)
        {
            Destroy(gameObject);
        }
    }

    private void Awake()
    {
        buttonNoProjectsCTA.onClick.AddListener(() => OnCreateProjectRequest?.Invoke());
        scrollRect.onValueChanged.AddListener(OnScrollValueChanged);
    }

    private void OnDestroy()
    {
        isDestroyed = true;
        buttonNoProjectsCTA.onClick.RemoveAllListeners();
        scrollRect.onValueChanged.RemoveListener(OnScrollValueChanged);
    }

    private void OnScrollValueChanged(Vector2 value) { OnScrollRectValueChanged?.Invoke(); }

    public void SetEmpty()
    {
        contentGameObject.SetActive(false);
        emptyContainer.SetActive(true);
        noSearchResultContainer.SetActive(false);
        loadingAnimationContainer.SetActive(false);
    }

    public void SetLoading()
    {
        contentGameObject.SetActive(false);
        emptyContainer.SetActive(false);
        noSearchResultContainer.SetActive(false);
        loadingAnimationContainer.SetActive(true);
    }

    public void SetNoSearchResult()
    {
        contentGameObject.SetActive(false);
        emptyContainer.SetActive(false);
        noSearchResultContainer.SetActive(true);
        loadingAnimationContainer.SetActive(false);
    }

    public void SetFilled()
    {
        contentGameObject.SetActive(true);
        emptyContainer.SetActive(false);
        noSearchResultContainer.SetActive(false);
        loadingAnimationContainer.SetActive(false);
        ResetScrollRect();
    }
}