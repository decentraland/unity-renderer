using System;
using UnityEngine.EventSystems;

public interface IFirstPersonModeController
{
    event Action OnClick;

    void Initialize(IFirstPersonModeView firstPersonModeView, ITooltipController tooltipController);
    void Dispose();
    void Click();
    void ShowTooltip(BaseEventData eventData, string tooltipText);
    void HideTooltip();
}

public class FirstPersonModeController : IFirstPersonModeController
{
    public event Action OnClick;

    internal IFirstPersonModeView firstPersonModeView;
    internal ITooltipController tooltipController;

    public void Initialize(IFirstPersonModeView firstPersonModeView, ITooltipController tooltipController)
    {
        this.firstPersonModeView = firstPersonModeView;
        this.tooltipController = tooltipController;

        firstPersonModeView.OnFirstPersonModeClick += Click;
        firstPersonModeView.OnShowTooltip += ShowTooltip;
        firstPersonModeView.OnHideTooltip += HideTooltip;
    }

    public void Dispose()
    {
        firstPersonModeView.OnFirstPersonModeClick -= Click;
        firstPersonModeView.OnShowTooltip -= ShowTooltip;
        firstPersonModeView.OnHideTooltip -= HideTooltip;
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
}
