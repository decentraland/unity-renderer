using System;
using UnityEngine.EventSystems;

public interface IPublishBtnController
{
    event Action OnClick;

    void Initialize(IPublishBtnView publishBtnView, ITooltipController tooltipController, ITooltipController feedbackTooltipController);
    void Dispose();
    void Click();
    void ShowTooltip(BaseEventData eventData, string tooltipText);
    void HideTooltip();
    void SetFeedbackMessage(string newText);
    void SetInteractable(bool isInteractable);
}

public class PublishBtnController : IPublishBtnController
{
    public event Action OnClick;

    internal IPublishBtnView publishBtnView;
    internal ITooltipController tooltipController;
    internal ITooltipController feedbackTooltipController;
    internal string currentFeedbackmessage = string.Empty;

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
        if (publishBtnView == null)
            return;
        
        publishBtnView.OnPublishButtonClick -= Click;
        publishBtnView.OnShowTooltip -= ShowTooltip;
        publishBtnView.OnHideTooltip -= HideTooltip;
    }

    public void Click() { OnClick?.Invoke(); }

    public void ShowTooltip(BaseEventData eventData, string tooltipText)
    {
        if (string.IsNullOrEmpty(currentFeedbackmessage))
        {
            tooltipController.SetTooltipText(tooltipText);
            tooltipController.ShowTooltip(eventData);
        }
        else
        {
            feedbackTooltipController.SetTooltipText(currentFeedbackmessage);
            feedbackTooltipController.ShowTooltip(eventData);
        }
    }

    public void HideTooltip()
    {
        tooltipController.HideTooltip();
        feedbackTooltipController.HideTooltip();
    }

    public void SetFeedbackMessage(string newText) { currentFeedbackmessage = newText; }

    public void SetInteractable(bool isInteractable) { publishBtnView.SetInteractable(isInteractable); }
}