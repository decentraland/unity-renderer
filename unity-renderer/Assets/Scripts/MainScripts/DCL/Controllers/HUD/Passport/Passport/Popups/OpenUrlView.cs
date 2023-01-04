using DCL.Helpers;
using DCL.Interface;
using System;
using UnityEngine;
using UnityEngine.UI;

public class OpenUrlView : BaseComponentView
{
    [SerializeField] private ShowHideAnimator openUrlAnimator;
    [SerializeField] internal Button closeButton;
    [SerializeField] internal Button continueButton;
    [SerializeField] internal Button cancelButton;
    [SerializeField] internal TMPro.TextMeshProUGUI domainText;
    [SerializeField] internal TMPro.TextMeshProUGUI urlText;
    private string currentUrl;

    public void Start()
    {
        closeButton.onClick.AddListener(() => SetVisibility(false));
        cancelButton.onClick.AddListener(() => SetVisibility(false));
        continueButton.onClick.AddListener(OpenLink);
    }

    public override void RefreshControl()
    {
    }

    private void OpenLink()
    {
        Utils.UnlockCursor();
        WebInterface.OpenURL(currentUrl);
        AnalyticsHelper.SendExternalLinkAnalytic(currentUrl);
        SetVisibility(true);
    }

    public void SetUrlInfo(string url, string domain)
    {
        currentUrl = url;
        urlText.text = url;
        domainText.text = domain;
    }

    public void SetVisibility(bool visible)
    {
        if (visible)
        {
            openUrlAnimator.gameObject.SetActive(true);
            openUrlAnimator.Show();
        }
        else
        {
            openUrlAnimator.Hide();
        }
    }
}
