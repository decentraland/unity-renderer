using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

internal interface IUnpublishPopupView : IDisposable
{
    event Action OnConfirmPressed;
    event Action OnCancelPressed;
    void Show(string title, string info);
    void Hide();
    void SetProgress(string title, float progress);
    void SetError(string title, string error);
    void SetSuccess(string title, string info);
}

internal class UnpublishPopupView : MonoBehaviour, IUnpublishPopupView
{
    [SerializeField] internal TextMeshProUGUI titleText;
    [SerializeField] internal TextMeshProUGUI infoText;
    [SerializeField] internal TextMeshProUGUI errorText;
    [SerializeField] internal Button cancelButton;
    [SerializeField] internal Button unpublishButton;
    [SerializeField] internal Button doneButton;
    [SerializeField] internal GameObject loadingBar;
    [SerializeField] internal RectTransform loadingImageRT;
    [SerializeField] internal TextMeshProUGUI loadingText;

    public event Action OnConfirmPressed;
    public event Action OnCancelPressed;

    private enum State
    {
        NONE,
        CONFIRM_UNPUBLISH,
        UNPUBLISHING,
        DONE_UNPUBLISH,
        ERROR_UNPUBLISH
    }

    private bool isDestroyed = false;
    private State state = State.NONE;
    private float loadingImageFullWidth;

    private void Awake()
    {
        unpublishButton.onClick.AddListener(() => { OnConfirmPressed?.Invoke(); });
        cancelButton.onClick.AddListener(() => { OnCancelPressed?.Invoke(); });
        doneButton.onClick.AddListener(() => { OnCancelPressed?.Invoke(); });
        loadingImageFullWidth = loadingImageRT.rect.size.x;
    }

    private void OnDestroy() { isDestroyed = true; }

    public void Dispose()
    {
        if (!isDestroyed)
        {
            Destroy(gameObject);
        }
    }

    void IUnpublishPopupView.Show(string title, string info)
    {
        titleText.text = title;
        infoText.text = info;
        SetState(State.CONFIRM_UNPUBLISH);
        gameObject.SetActive(true);
    }

    void IUnpublishPopupView.Hide() { gameObject.SetActive(false); }

    void IUnpublishPopupView.SetProgress(string title, float progress)
    {
        titleText.text = title;
        loadingText.text = $"{Mathf.FloorToInt(100 * progress)}%";
        loadingImageRT.sizeDelta = new Vector2(loadingImageFullWidth * progress, loadingImageRT.sizeDelta.y);
        SetState(State.UNPUBLISHING);
    }

    void IUnpublishPopupView.SetError(string title, string error)
    {
        titleText.text = title;
        errorText.text = error;
        SetState(State.ERROR_UNPUBLISH);
    }

    void IUnpublishPopupView.SetSuccess(string title, string info)
    {
        titleText.text = title;
        infoText.text = info;
        SetState(State.DONE_UNPUBLISH);
    }

    private void SetState(State newState)
    {
        if (state == newState)
            return;

        state = newState;
        switch (state)
        {
            case State.CONFIRM_UNPUBLISH:
                cancelButton.gameObject.SetActive(true);
                unpublishButton.gameObject.SetActive(true);
                doneButton.gameObject.SetActive(false);
                infoText.gameObject.SetActive(true);
                errorText.gameObject.SetActive(false);
                loadingBar.SetActive(false);
                break;
            case State.UNPUBLISHING:
                cancelButton.gameObject.SetActive(false);
                unpublishButton.gameObject.SetActive(false);
                doneButton.gameObject.SetActive(false);
                infoText.gameObject.SetActive(false);
                errorText.gameObject.SetActive(false);
                loadingBar.SetActive(true);
                break;
            case State.DONE_UNPUBLISH:
                cancelButton.gameObject.SetActive(false);
                unpublishButton.gameObject.SetActive(false);
                doneButton.gameObject.SetActive(true);
                infoText.gameObject.SetActive(true);
                errorText.gameObject.SetActive(false);
                loadingBar.SetActive(false);
                break;
            case State.ERROR_UNPUBLISH:
                cancelButton.gameObject.SetActive(false);
                unpublishButton.gameObject.SetActive(false);
                doneButton.gameObject.SetActive(true);
                infoText.gameObject.SetActive(false);
                errorText.gameObject.SetActive(true);
                loadingBar.SetActive(false);
                break;
        }
    }
}