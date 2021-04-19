using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public interface IBuilderInWorldLoadingView
{
    event System.Action OnCancelLoading;

    void Show(bool showTips = true);
    void Hide(bool forzeHidding = false);
    void StartTipsCarousel();
    void StopTipsCarousel();
    void CancelLoading();
}

public class BuilderInWorldLoadingView : MonoBehaviour, IBuilderInWorldLoadingView
{
    private const string VIEW_PATH = "BuilderInWorldLoadingView";

    [SerializeField] internal List<string> loadingTips;
    [SerializeField] internal float timeBetweenTips = 3f;
    [SerializeField] internal TMP_Text tipsText;
    [SerializeField] internal Button cancelButton;
    [SerializeField] internal float minVisibilityTime = 1.5f;

    public event System.Action OnCancelLoading;

    internal Coroutine tipsCoroutine;
    internal Coroutine hideCoroutine;
    internal float showTime = 0f;

    internal static BuilderInWorldLoadingView Create()
    {
        var view = Instantiate(Resources.Load<GameObject>(VIEW_PATH)).GetComponent<BuilderInWorldLoadingView>();
        view.gameObject.name = "_BuilderInWorldLoadingView";

        return view;
    }

    private void Awake() { cancelButton.onClick.AddListener(CancelLoading); }

    private void OnDestroy() { cancelButton.onClick.RemoveListener(CancelLoading); }

    public void Show(bool showTips = true)
    {
        gameObject.SetActive(true);
        showTime = Time.realtimeSinceStartup;

        if (showTips && loadingTips.Count > 0)
            StartTipsCarousel();
        else
            tipsText.text = string.Empty;
    }

    public void Hide(bool forzeHidding = false)
    {
        if (hideCoroutine != null)
            CoroutineStarter.Stop(hideCoroutine);

        hideCoroutine = CoroutineStarter.Start(TryToHideCoroutine(forzeHidding));
    }

    public void StartTipsCarousel()
    {
        StopTipsCarousel();
        tipsCoroutine = CoroutineStarter.Start(ShowRandomTipsCoroutine());
    }

    public void StopTipsCarousel()
    {
        if (tipsCoroutine == null)
            return;

        CoroutineStarter.Stop(tipsCoroutine);
        tipsCoroutine = null;
    }

    internal IEnumerator TryToHideCoroutine(bool forzeHidding)
    {
        while (!forzeHidding && (Time.realtimeSinceStartup - showTime) < minVisibilityTime)
        {
            yield return null;
        }

        StopTipsCarousel();
        gameObject.SetActive(false);
    }

    internal IEnumerator ShowRandomTipsCoroutine()
    {
        while (true)
        {
            tipsText.text = loadingTips[Random.Range(0, loadingTips.Count - 1)];
            yield return new WaitForSeconds(timeBetweenTips);
        }
    }

    public void CancelLoading()
    {
        Hide();
        OnCancelLoading?.Invoke();
    }
}