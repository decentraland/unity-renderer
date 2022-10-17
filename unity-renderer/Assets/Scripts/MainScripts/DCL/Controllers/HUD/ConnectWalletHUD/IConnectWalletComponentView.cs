using System;

public interface IConnectWalletComponentView
{
    event Action OnCancel;
    event Action OnConnect;
    event Action OnHelp;

    void Show(bool instant = false);
    void Hide(bool instant = false);
}
