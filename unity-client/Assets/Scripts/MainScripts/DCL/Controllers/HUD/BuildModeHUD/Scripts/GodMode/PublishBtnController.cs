using System;
using UnityEngine.EventSystems;

public interface IPublishBtnController
{
    event Action OnClick;

    void Initialize(IPublishBtnView publishBtnView, ITooltipController tooltipController);
    void Dispose();
    void Click();
    void ShowTooltip(BaseEventData eventData, string tooltipText);
    void HideTooltip();
    void SetInteractable(bool isInteractable);
}

public class PublishBtnController : IPublishBtnController
{
    public event Action OnClick;

    internal IPublishBtnView publishBtnView;
    internal ITooltipController tooltipController;

    public void Initialize(IPublishBtnView publishBtnView, ITooltipController tooltipController)
    {
        this.publishBtnView = publishBtnView;
        this.tooltipController = tooltipController;

        publishBtnView.OnPublishButtonClick += Click;
        publishBtnView.OnShowTooltip += ShowTooltip;
        publishBtnView.OnHideTooltip += HideTooltip;
    }

    public void Dispose()
    {
        publishBtnView.OnPublishButtonClick -= Click;
        publishBtnView.OnShowTooltip -= ShowTooltip;
        publishBtnView.OnHideTooltip -= HideTooltip;
    }

    public void Click()
    {
        OnClick?.Invoke();
    }

    public void ShowTooltip(BaseEventData eventData, string tooltipText)
    {
        tooltipController.ShowTooltip(eventData);
        tooltipController.SetTooltipText(tooltipText);
    }

    public void HideTooltip()
    {
        tooltipController.HideTooltip();
    }

    public void SetInteractable(bool isInteractable)
    {
        publishBtnView.SetInteractable(isInteractable);
    }
}
