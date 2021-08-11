using System;

public interface IExtraActionsController
{
    event Action OnControlsClick,
                 OnHideUIClick,
                 OnTutorialClick,
                 OnResetClick,
                 OnResetCameraClick;

    void Initialize(IExtraActionsView extraActionsView);
    void Dispose();
    void SetActive(bool isActive);
    void ControlsClicked();
    void HideUIClicked();
    void TutorialClicked();
    void ResetCameraClicked();
    void SetResetButtonInteractable(bool isEnabled);
}

public class ExtraActionsController : IExtraActionsController
{
    public event Action OnControlsClick,
                        OnHideUIClick,
                        OnTutorialClick,
                        OnResetClick,
                        OnResetCameraClick;

    internal IExtraActionsView extraActionsView;

    public void Initialize(IExtraActionsView extraActionsView)
    {
        this.extraActionsView = extraActionsView;

        extraActionsView.OnControlsClicked += ControlsClicked;
        extraActionsView.OnHideUIClicked += HideUIClicked;
        extraActionsView.OnTutorialClicked += TutorialClicked;
        extraActionsView.OnResetClicked += ResetClicked;
        extraActionsView.OnResetCameraClicked += ResetCameraClicked;
    }

    public void Dispose()
    {
        extraActionsView.OnControlsClicked -= ControlsClicked;
        extraActionsView.OnHideUIClicked -= HideUIClicked;
        extraActionsView.OnTutorialClicked -= TutorialClicked;
        extraActionsView.OnResetClicked -= ResetClicked;
        extraActionsView.OnResetCameraClicked -= ResetCameraClicked;
    }

    public void SetActive(bool isActive)
    {
        if (extraActionsView != null)
            extraActionsView.SetActive(isActive);
    }

    public void ResetClicked()
    {
        OnResetClick?.Invoke();
        SetActive(false);
    }

    public void ControlsClicked()
    {
        OnControlsClick?.Invoke();
        SetActive(false);
    }

    public void HideUIClicked()
    {
        OnHideUIClick?.Invoke();
        SetActive(false);
    }

    public void TutorialClicked()
    {
        OnTutorialClick?.Invoke();
        SetActive(false);
    }

    public void ResetCameraClicked()
    {
        OnResetCameraClick?.Invoke();
        SetActive(false);
    }

    public void SetResetButtonInteractable(bool isInteractable) { extraActionsView.SetResetButtonInteractable(isInteractable); }
}