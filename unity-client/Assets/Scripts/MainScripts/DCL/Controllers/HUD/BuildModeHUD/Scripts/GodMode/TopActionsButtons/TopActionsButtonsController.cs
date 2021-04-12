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

    IExtraActionsController extraActionsController { get; }

    void Initialize(ITopActionsButtonsView topActionsButtonsView, ITooltipController tooltipController, IBuildModeConfirmationModalController buildModeConfirmationModalController);
    void Dispose();
    void ChangeModeClicked();
    void ExtraClicked();
    void TranslateClicked();
    void RotateClicked();
    void ScaleClicked();
    void ResetClicked();
    void DuplicateClicked();
    void DeleteClicked();
    void ShowLogoutConfirmation();
    void HideLogoutConfirmation();
    void ConfirmLogout();
    void TooltipPointerEntered(BaseEventData eventData, string tooltipText);
    void TooltipPointerExited();
    void SetExtraActionsActive(bool isActive);
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

    public IExtraActionsController extraActionsController { get; private set; }

    private const string EXIT_MODAL_TITLE = "Exiting Builder mode";
    private const string EXIT_MODAL_SUBTITLE = "Are you sure you want to exit Builder mode?";
    private const string EXIT_MODAL_CONFIRM_BUTTON = "EXIT";
    private const string EXIT_MODAL_CANCEL_BUTTON = "CANCEL";

    internal ITopActionsButtonsView topActionsButtonsView;
    internal ITooltipController tooltipController;
    internal IBuildModeConfirmationModalController buildModeConfirmationModalController;

    public void Initialize(ITopActionsButtonsView topActionsButtonsView, ITooltipController tooltipController, IBuildModeConfirmationModalController buildModeConfirmationModalController)
    {
        this.topActionsButtonsView = topActionsButtonsView;
        this.tooltipController = tooltipController;
        this.buildModeConfirmationModalController = buildModeConfirmationModalController;

        topActionsButtonsView.OnChangeModeClicked += ChangeModeClicked;
        topActionsButtonsView.OnExtraClicked += ExtraClicked;
        topActionsButtonsView.OnTranslateClicked += TranslateClicked;
        topActionsButtonsView.OnRotateClicked += RotateClicked;
        topActionsButtonsView.OnScaleClicked += ScaleClicked;
        topActionsButtonsView.OnResetClicked += ResetClicked;
        topActionsButtonsView.OnDuplicateClicked += DuplicateClicked;
        topActionsButtonsView.OnDeleteClicked += DeleteClicked;
        topActionsButtonsView.OnLogOutClicked += ShowLogoutConfirmation;
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
        buildModeConfirmationModalController.OnCancelExit += HideLogoutConfirmation;
        buildModeConfirmationModalController.OnConfirmExit += ConfirmLogout;

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
        topActionsButtonsView.OnLogOutClicked -= ShowLogoutConfirmation;
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
        buildModeConfirmationModalController.OnCancelExit -= HideLogoutConfirmation;
        buildModeConfirmationModalController.OnConfirmExit -= ConfirmLogout;
    }

    public void ChangeModeClicked() { OnChangeModeClick?.Invoke(); }

    public void ExtraClicked() { OnExtraClick?.Invoke(); }

    public void TranslateClicked() { OnTranslateClick?.Invoke(); }

    public void RotateClicked() { OnRotateClick?.Invoke(); }

    public void ScaleClicked() { OnScaleClick?.Invoke(); }

    public void ResetClicked() { OnResetClick?.Invoke(); }

    public void DuplicateClicked() { OnDuplicateClick?.Invoke(); }

    public void DeleteClicked() { OnDeleteClick?.Invoke(); }

    public void HideLogoutConfirmation() { buildModeConfirmationModalController.SetActive(false); }

    public void ShowLogoutConfirmation()
    {
        buildModeConfirmationModalController.SetActive(true);
        buildModeConfirmationModalController.SetTitle(EXIT_MODAL_TITLE);
        buildModeConfirmationModalController.SetSubTitle(EXIT_MODAL_SUBTITLE);
        buildModeConfirmationModalController.SetCancelButtonText(EXIT_MODAL_CANCEL_BUTTON);
        buildModeConfirmationModalController.SetConfirmButtonText(EXIT_MODAL_CONFIRM_BUTTON);
    }

    public void ConfirmLogout() { OnLogOutClick?.Invoke(); }

    public void TooltipPointerEntered(BaseEventData eventData, string tooltipText)
    {
        tooltipController.ShowTooltip(eventData);
        tooltipController.SetTooltipText(tooltipText);
    }

    public void TooltipPointerExited() { tooltipController.HideTooltip(); }

    public void SetExtraActionsActive(bool isActive) { extraActionsController.SetActive(isActive); }
}