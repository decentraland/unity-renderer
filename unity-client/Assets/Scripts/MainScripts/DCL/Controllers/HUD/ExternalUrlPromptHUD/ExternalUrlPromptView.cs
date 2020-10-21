using System;
using UnityEngine;
using UnityEngine.UI;

public class ExternalUrlPromptView : MonoBehaviour
{
    [SerializeField] internal GameObject content;
    [SerializeField] internal Button closeButton;
    [SerializeField] internal Button continueButton;
    [SerializeField] internal Button cancelButton;
    [SerializeField] internal TMPro.TextMeshProUGUI domainText;
    [SerializeField] internal TMPro.TextMeshProUGUI urlText;
    [SerializeField] internal Toggle trustToggle;
    [SerializeField] internal ShowHideAnimator showHideAnimator;

    internal enum ResultType { CANCELED, APPROVED, APPROVED_TRUSTED }

    Action<ResultType> resultCallback = null;

    void Awake()
    {
        closeButton.onClick.AddListener(Dismiss);
        cancelButton.onClick.AddListener(Dismiss);
        continueButton.onClick.AddListener(Approve);
    }

    internal void RequestOpenUrl(Uri uri, Action<ResultType> result)
    {
        resultCallback = result;
        domainText.text = uri.Host;
        urlText.text = uri.OriginalString;
        trustToggle.isOn = false;
    }

    private void Dismiss()
    {
        resultCallback?.Invoke(ResultType.CANCELED);
    }

    private void Approve()
    {
        resultCallback?.Invoke(trustToggle.isOn ? ResultType.APPROVED_TRUSTED : ResultType.APPROVED);
    }
}
