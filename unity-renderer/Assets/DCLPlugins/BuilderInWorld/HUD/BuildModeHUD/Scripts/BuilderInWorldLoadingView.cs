using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public interface IBuilderInWorldLoadingView
{
    bool isActive { get; }
    GameObject gameObject { get; }
    void Show();
    void Hide(bool forceHide = false, Action onHideAction = null);
    void StartTipsCarousel();
    void StopTipsCarousel();
    void SetPercentage(float newValue);
    void Dispose();
}

public class BuilderInWorldLoadingView : MonoBehaviour, IBuilderInWorldLoadingView
{
    private const string VIEW_PATH = "BuilderInWorldLoadingView";

    [Header("Loading Tips Config")]
    [SerializeField] internal BuilderInWorldLoadingTip loadingTipPrefab;

    [SerializeField] internal RectTransform loadingTipsContainer;
    [SerializeField] internal ScrollRect loadingTipsScroll;
    [SerializeField] internal List<BuilderInWorldLoadingTipModel> loadingTips;
    [SerializeField] internal float timeBetweenTips = 3f;
    [SerializeField] internal AnimationCurve animationTipsCurve;
    [SerializeField] internal float animationTipsTransitionTime = 1f;

    [Header("Other Config")]
    [SerializeField] internal float minVisibilityTime = 1.5f;

    [SerializeField] internal LoadingBar loadingBar;

    public bool isActive => gameObject.activeSelf;

    internal Coroutine tipsCoroutine;
    internal Coroutine hideCoroutine;
    internal float showTime = 0f;
    internal int currentTipIndex = 0;
    internal float currentFinalNormalizedPos;

    internal static BuilderInWorldLoadingView Create()
    {
        var view = Instantiate(Resources.Load<GameObject>(VIEW_PATH)).GetComponent<BuilderInWorldLoadingView>();
        view.gameObject.name = "_BuilderInWorldLoadingView";

        return view;
    }

    private void Awake() { CreateLoadingTips(); }

    private void CreateLoadingTips()
    {
        RectTransform tipsContainerRectTransform = loadingTipsContainer;
        float tipsContainerHorizontalOffset = 0;

        for (int i = 0; i < loadingTips.Count; i++)
        {
            BuilderInWorldLoadingTip newTip = Instantiate(loadingTipPrefab, loadingTipsContainer);
            newTip.Configure(loadingTips[i]);

            if (i > 0)
            {
                tipsContainerHorizontalOffset += ((RectTransform)newTip.transform).rect.size.x;
            }
        }

        tipsContainerRectTransform.offsetMax = new Vector2(tipsContainerHorizontalOffset, tipsContainerRectTransform.offsetMax.y);
    }

    public void Show()
    {
        if (gameObject != null)
            gameObject.SetActive(true);

        showTime = Time.realtimeSinceStartup;

        if (loadingTips.Count > 0)
        {
            StartTipsCarousel();
        }

        AudioScriptableObjects.builderEnter.Play();
    }

    public void Hide(bool forceHide = false, Action onHideAction = null)
    {
        if (hideCoroutine != null)
            CoroutineStarter.Stop(hideCoroutine);

        hideCoroutine = CoroutineStarter.Start(TryToHideCoroutine(forceHide, onHideAction));
    }

    public void Dispose()
    {
        StopTipsCarousel();

        if (hideCoroutine != null)
            CoroutineStarter.Stop(hideCoroutine);

        if (tipsCoroutine != null)
            CoroutineStarter.Stop(tipsCoroutine);
    }

    public void StartTipsCarousel()
    {
        StopTipsCarousel();
        tipsCoroutine = CoroutineStarter.Start(RunTipsCarouselCoroutine());
    }

    public void StopTipsCarousel()
    {
        if (tipsCoroutine == null)
            return;

        CoroutineStarter.Stop(tipsCoroutine);
        tipsCoroutine = null;

        ForceToEndCurrentTipsAnimation();
    }

    internal IEnumerator TryToHideCoroutine(bool forceHide, Action onHideAction)
    {
        while (!forceHide && (Time.realtimeSinceStartup - showTime) < minVisibilityTime)
        {
            yield return null;
        }

        StopTipsCarousel();

        if (gameObject != null)
            gameObject.SetActive(false);

        onHideAction?.Invoke();

        AudioScriptableObjects.builderReady.Play();
    }

    internal IEnumerator RunTipsCarouselCoroutine()
    {
        currentTipIndex = 0;

        while (true)
        {
            yield return new WaitForSeconds(timeBetweenTips);
            float initialNormalizedPos = loadingTipsScroll.horizontalNormalizedPosition;
            currentFinalNormalizedPos = initialNormalizedPos + (1f / (loadingTips.Count - 1));
            loadingTipsScroll.horizontalNormalizedPosition =   currentFinalNormalizedPos;
            IncrementTipIndex();
        }
    }

    private void IncrementTipIndex()
    {
        currentTipIndex++;
        if (currentTipIndex >= loadingTips.Count - 1)
        {
            currentTipIndex = 0;
            loadingTipsScroll.horizontalNormalizedPosition = 0f;

            // Moving the last tip game object to the first position in the hierarchy, we make the carousel cyclical.
            loadingTipsContainer.GetChild(loadingTipsContainer.childCount - 1).SetAsFirstSibling();
        }
    }

    internal IEnumerator RunTipsAnimationCoroutine()
    {
        float currentAnimationTime = 0f;
        float initialNormalizedPos = loadingTipsScroll.horizontalNormalizedPosition;
        currentFinalNormalizedPos = initialNormalizedPos + (1f / (loadingTips.Count - 1));
        loadingTipsScroll.horizontalNormalizedPosition =   currentFinalNormalizedPos;
        while (currentAnimationTime <= animationTipsTransitionTime)
        {
            loadingTipsScroll.horizontalNormalizedPosition = Mathf.Lerp(
                initialNormalizedPos,
                currentFinalNormalizedPos,
                animationTipsCurve.Evaluate(currentAnimationTime / animationTipsTransitionTime));

            currentAnimationTime += Time.deltaTime;

            yield return null;
        }
    }

    internal void ForceToEndCurrentTipsAnimation()
    {
        loadingTipsScroll.horizontalNormalizedPosition = currentFinalNormalizedPos;
        IncrementTipIndex();
    }

    public void SetPercentage(float newValue) { loadingBar.SetPercentage(newValue); }
}