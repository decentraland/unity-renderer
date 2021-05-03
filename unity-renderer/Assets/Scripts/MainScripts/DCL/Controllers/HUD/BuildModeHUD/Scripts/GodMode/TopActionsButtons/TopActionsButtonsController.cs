using System;
using UnityEngine.EventSystems;

public interface ITopActionsButtonsController
{
    event Action OnChangeModeClick;
    event Action OnExtraClick;
    event Action OnTranslateClick;
    event Action OnRotateClick;
    event Action OnScaleClick;
    event Action OnResetClick;
    event Action OnDuplicateClick;
    event Action OnDeleteClick;
    event Action OnLogOutClick;
    event Action OnSnapModeClick;

    IExtraActionsController extraActionsController { get; }

    void Initialize(ITopActionsButtonsView topActionsButtonsView, ITooltipController tooltipController);
    void Dispose();
    void ChangeModeClicked();
    void ExtraClicked();
    void TranslateClicked();
    void RotateClicked();
    void ScaleClicked();
    void ResetClicked();
    void DuplicateClicked();
    void DeleteClicked();
    void LogoutClicked();
    void ConfirmLogout(BuildModeModalType modalType);
    void TooltipPointerEntered(BaseEventData eventData, string tooltipText);
    void TooltipPointerExited();
    void SetExtraActionsActive(bool isActive);
    void SetGizmosActive(string gizmos);
    void SetActionsInteractable(bool isActive);
    void SetSnapActive(bool isActive);
}

public class TopActionsButtonsController : ITopActionsButtonsController
{
    public event Action OnChangeModeClick;
    public event Action OnExtraClick;
    public event Action OnTranslateClick;
    public event Action OnRotateClick;
    public event Action OnScaleClick;
    public event Action OnResetClick;
    public event Action OnDuplicateClick;
    public event Action OnDeleteClick;
    public event Action OnLogOutClick;
    public event Action OnSnapModeClick;

    public IExtraActionsController extraActionsController { get; private set; }

    internal ITopActionsButtonsView topActionsButtonsView;
    internal ITooltipController tooltipController;

    public void Initialize(ITopActionsButtonsView topActionsButtonsView, ITooltipController tooltipController)
    {
        this.topActionsButtonsView = topActionsButtonsView;
        this.tooltipController = tooltipController;

        topActionsButtonsView.OnChangeModeClicked += ChangeModeClicked;
        topActionsButtonsView.OnExtraClicked += ExtraClicked;
        topActionsButtonsView.OnTranslateClicked += TranslateClicked;
        topActionsButtonsView.OnRotateClicked += RotateClicked;
        topActionsButtonsView.OnScaleClicked += ScaleClicked;
        topActionsButtonsView.OnResetClicked += ResetClicked;
        topActionsButtonsView.OnDuplicateClicked += DuplicateClicked;
        topActionsButtonsView.OnDeleteClicked += DeleteClicked;
        topActionsButtonsView.OnLogOutClicked += LogoutClicked;
        topActionsButtonsView.OnPointerExit += TooltipPointerExited;
        topActionsButtonsView.OnChangeCameraModePointerEnter += TooltipPointerEntered;
        topActionsButtonsView.OnTranslatePointerEnter += TooltipPointerEntered;
        topActionsButtonsView.OnRotatePointerEnter += TooltipPointerEntered;
        topActionsButtonsView.OnScalePointerEnter += TooltipPointerEntered;
        topActionsButtonsView.OnResetPointerEnter += TooltipPointerEntered;
        topActionsButtonsView.OnDuplicatePointerEnter += TooltipPointerEntered;
        topActionsButtonsView.OnDeletePointerEnter += TooltipPointerEntered;
        topActionsButtonsView.OnMoreActionsPointerEnter += TooltipPointerEntered;
        topActionsButtonsView.OnLogoutPointerEnter += TooltipPointerEntered;
        topActionsButtonsView.OnSnapModeClicked += SnapModeClicked;

        extraActionsController = new ExtraActionsController();
        topActionsButtonsView.ConfigureExtraActions(extraActionsController);
        extraActionsController.SetActive(false);
    }

    public void Dispose()
    {
        topActionsButtonsView.OnChangeModeClicked -= ChangeModeClicked;
        topActionsButtonsView.OnExtraClicked -= ExtraClicked;
        topActionsButtonsView.OnTranslateClicked -= TranslateClicked;
        topActionsButtonsView.OnRotateClicked -= RotateClicked;
        topActionsButtonsView.OnScaleClicked -= ScaleClicked;
        topActionsButtonsView.OnResetClicked -= ResetClicked;
        topActionsButtonsView.OnDuplicateClicked -= DuplicateClicked;
        topActionsButtonsView.OnDeleteClicked -= DeleteClicked;
        topActionsButtonsView.OnLogOutClicked -= LogoutClicked;
        topActionsButtonsView.OnPointerExit -= TooltipPointerExited;
        topActionsButtonsView.OnChangeCameraModePointerEnter -= TooltipPointerEntered;
        topActionsButtonsView.OnTranslatePointerEnter -= TooltipPointerEntered;
        topActionsButtonsView.OnRotatePointerEnter -= TooltipPointerEntered;
        topActionsButtonsView.OnScalePointerEnter -= TooltipPointerEntered;
        topActionsButtonsView.OnResetPointerEnter -= TooltipPointerEntered;
        topActionsButtonsView.OnDuplicatePointerEnter -= TooltipPointerEntered;
        topActionsButtonsView.OnDeletePointerEnter -= TooltipPointerEntered;
        topActionsButtonsView.OnMoreActionsPointerEnter -= TooltipPointerEntered;
        topActionsButtonsView.OnLogoutPointerEnter -= TooltipPointerEntered;
    }

    public void ChangeModeClicked() { OnChangeModeClick?.Invoke(); }

    public void ExtraClicked() { OnExtraClick?.Invoke(); }

    public void TranslateClicked() { OnTranslateClick?.Invoke(); }

    public void RotateClicked() { OnRotateClick?.Invoke(); }

    public void ScaleClicked() { OnScaleClick?.Invoke(); }

    public void ResetClicked() { OnResetClick?.Invoke(); }

    public void DuplicateClicked() { OnDuplicateClick?.Invoke(); }

    public void DeleteClicked() { OnDeleteClick?.Invoke(); }

    public void LogoutClicked() { OnLogOutClick?.Invoke(); }
    public void SnapModeClicked() { OnSnapModeClick?.Invoke(); }

    public void ConfirmLogout(BuildModeModalType modalType)
    {
        if (modalType != BuildModeModalType.EXIT)
            return;

        OnLogOutClick?.Invoke();
    }

    public void TooltipPointerEntered(BaseEventData eventData, string tooltipText)
    {
        tooltipController.ShowTooltip(eventData);
        tooltipController.SetTooltipText(tooltipText);
    }

    public void TooltipPointerExited() { tooltipController.HideTooltip(); }

    public void SetExtraActionsActive(bool isActive) { extraActionsController.SetActive(isActive); }
    public void SetGizmosActive(string gizmos) { topActionsButtonsView.SetGizmosActive(gizmos); }
    public void SetActionsInteractable(bool isActive) { topActionsButtonsView.SetActionsInteractable(isActive); }
    public void SetSnapActive(bool isActive) { topActionsButtonsView.SetSnapActive(isActive); }
}