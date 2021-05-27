using System;
using UnityEngine.EventSystems;

public interface IPublishBtnController
{
    event Action OnClick;

    bool isFeedbackMessageActive { get; }

    void Initialize(IPublishBtnView publishBtnView, ITooltipController tooltipController, ITooltipController feedbackTooltipController);
    void Dispose();
    void Click();
    void ShowTooltip(BaseEventData eventData, string tooltipText);
    void HideTooltip();
    void ShowTooltipFeedback(string newText);
    void HideTooltipFeedback();
    void SetInteractable(bool isInteractable);
}

public class PublishBtnController : IPublishBtnController
{
    public event Action OnClick;

    public bool isFeedbackMessageActive { get; private set; }

    internal IPublishBtnView publishBtnView;
    internal ITooltipController tooltipController;
    internal ITooltipController feedbackTooltipController;

    public void Initialize(IPublishBtnView publishBtnView, ITooltipController tooltipController, ITooltipController feedbackTooltipController)
    {
        this.publishBtnView = publishBtnView;
        this.tooltipController = tooltipController;
        this.feedbackTooltipController = feedbackTooltipController;

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

    public void Click() { OnClick?.Invoke(); }

    public void ShowTooltip(BaseEventData eventData, string tooltipText)
    {
        if (isFeedbackMessageActive)
            return;

        tooltipController.SetTooltipText(tooltipText);
        tooltipController.ShowTooltip(eventData);
    }

    public void HideTooltip() { tooltipController.HideTooltip(); }

    public void ShowTooltipFeedback(string newText)
    {
        feedbackTooltipController.SetTooltipText(newText);
        feedbackTooltipController.ShowTooltip(publishBtnView.feedbackTooltipPos);
        isFeedbackMessageActive = true;
    }

    public void HideTooltipFeedback()
    {
        feedbackTooltipController.HideTooltip();
        isFeedbackMessageActive = false;
    }

    public void SetInteractable(bool isInteractable) { publishBtnView.SetInteractable(isInteractable); }
}