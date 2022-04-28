using System;
using UnityEngine;

public class FriendsInitializationRetryTooltipComponentView : BaseComponentView
{
    [SerializeField] internal ButtonComponentView closeButton;
    [SerializeField] internal ButtonComponentView retryButton;

    public event Action OnClose;
    public event Action OnRetry;

    public override void Awake()
    {
        base.Awake();

        closeButton.onClick.AddListener(() => OnClose?.Invoke());
        retryButton.onClick.AddListener(() => OnRetry?.Invoke());
    }

    public override void Dispose()
    {
        base.Dispose();

        closeButton.onClick.RemoveAllListeners();
        retryButton.onClick.RemoveAllListeners();
    }

    public override void RefreshControl() { }
}
