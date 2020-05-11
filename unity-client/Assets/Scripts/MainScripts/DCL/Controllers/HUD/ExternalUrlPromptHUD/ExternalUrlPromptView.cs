using System;
using UnityEngine;
using UnityEngine.UI;
using DCL.Helpers;

public class ExternalUrlPromptView : MonoBehaviour
{
    [SerializeField] internal GameObject content;
    [SerializeField] internal Button closeButton;
    [SerializeField] internal Button continueButton;
    [SerializeField] internal Button cancelButton;
    [SerializeField] internal TMPro.TextMeshProUGUI domainText;
    [SerializeField] internal TMPro.TextMeshProUGUI urlText;
    [SerializeField] internal Toggle trustToggle;

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
        Utils.UnlockCursor();
        content.SetActive(true);

        resultCallback = result;
        domainText.text = uri.Host;
        urlText.text = uri.OriginalString;
        trustToggle.isOn = false;
    }

    private void Dismiss()
    {
        resultCallback?.Invoke(ResultType.CANCELED);
        Close();
    }

    private void Approve()
    {
        resultCallback?.Invoke(trustToggle.isOn ? ResultType.APPROVED_TRUSTED : ResultType.APPROVED);
        Close();
    }

    private void Close()
    {
        content.SetActive(false);
    }
}
