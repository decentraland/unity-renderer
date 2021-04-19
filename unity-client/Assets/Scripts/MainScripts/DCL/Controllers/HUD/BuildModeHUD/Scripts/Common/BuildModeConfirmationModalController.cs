using System;

public enum BuildModeModalType
{
    EXIT,
    PUBLISH
}

public interface IBuildModeConfirmationModalController
{
    event Action<BuildModeModalType> OnCancelExit;
    event Action<BuildModeModalType> OnConfirmExit;

    void Initialize(IBuildModeConfirmationModalView exitFromBiWModalView);
    void Dispose();
    void Configure(string titleText, string subTitleText, string cancelBtnText, string confirmBtnText);
    void SetActive(bool isActive, BuildModeModalType modalType);
    void CancelExit();
    void ConfirmExit();
}

public class BuildModeConfirmationModalController : IBuildModeConfirmationModalController
{
    public event Action<BuildModeModalType> OnCancelExit;
    public event Action<BuildModeModalType> OnConfirmExit;

    internal IBuildModeConfirmationModalView exitFromBiWModalView;
    internal BuildModeModalType modalType;

    public void Initialize(IBuildModeConfirmationModalView exitFromBiWModalView)
    {
        this.exitFromBiWModalView = exitFromBiWModalView;

        exitFromBiWModalView.OnCancelExit += CancelExit;
        exitFromBiWModalView.OnConfirmExit += ConfirmExit;
    }

    public void Dispose()
    {
        exitFromBiWModalView.OnCancelExit -= CancelExit;
        exitFromBiWModalView.OnConfirmExit -= ConfirmExit;
    }

    public void Configure(string titleText, string subTitleText, string cancelBtnText, string confirmBtnText)
    {
        exitFromBiWModalView.SetTitle(titleText);
        exitFromBiWModalView.SetSubTitle(subTitleText);
        exitFromBiWModalView.SetCancelButtonText(cancelBtnText);
        exitFromBiWModalView.SetConfirmButtonText(confirmBtnText);
    }

    public void SetActive(bool isActive, BuildModeModalType modalType)
    {
        this.modalType = modalType;
        exitFromBiWModalView.SetActive(isActive);
    }

    public void CancelExit()
    {
        SetActive(false, modalType);
        OnCancelExit?.Invoke(modalType);
    }

    public void ConfirmExit()
    {
        SetActive(false, modalType);
        OnConfirmExit?.Invoke(modalType);
    }
}