using System;

public interface IExtraActionsController
{
    event Action OnControlsClick,
                 OnHideUIClick,
                 OnTutorialClick;

    void Initialize(IExtraActionsView extraActionsView);
    void Dispose();
    void SetActive(bool isActive);
    void ControlsClicked();
    void HideUIClicked();
    void TutorialClicked();
}

public class ExtraActionsController : IExtraActionsController
{
    public event Action OnControlsClick,
                        OnHideUIClick, 
                        OnTutorialClick;

    internal IExtraActionsView extraActionsView;

    public void Initialize(IExtraActionsView extraActionsView)
    {
        this.extraActionsView = extraActionsView;

        extraActionsView.OnControlsClicked += ControlsClicked;
        extraActionsView.OnHideUIClicked += HideUIClicked;
        extraActionsView.OnTutorialClicked += TutorialClicked;
    }

    public void Dispose()
    {
        extraActionsView.OnControlsClicked -= ControlsClicked;
        extraActionsView.OnHideUIClicked -= HideUIClicked;
        extraActionsView.OnTutorialClicked -= TutorialClicked;
    }

    public void SetActive(bool isActive)
    {
        if (extraActionsView != null)
            extraActionsView.SetActive(isActive);
    }

    public void ControlsClicked()
    {
        OnControlsClick?.Invoke();
    }

    public void HideUIClicked()
    {
        OnHideUIClick?.Invoke();
    }

    public void TutorialClicked()
    {
        OnTutorialClick?.Invoke();
    }
}
