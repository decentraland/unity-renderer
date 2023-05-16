using DCL;
using System;

public class ToSPopupController : IDisposable
{
    private readonly IToSPopupView view;
    private readonly IToSPopupHandler handler;

    public  ToSPopupController(IToSPopupView view, IToSPopupHandler handler)
    {
        this.view = view;
        this.view.OnAccept += HandleAccept;
        this.view.OnCancel += HandleCancel;
        this.view.OnTermsOfServiceLinkPressed += HandleViewToS;
        this.handler = handler;
        view.Show();
    }

    internal void HandleCancel()
    {
        handler.Cancel();
        Dispose(); //We dont need the popup anymore
    }

    internal void HandleAccept()
    {
        handler.Accept();
        Dispose(); //We dont need the popup anymore
    }

    private void HandleViewToS()
    {
        handler.ViewToS();
    }

    public void Dispose()
    {
        view.OnAccept -= HandleAccept;
        view.Dispose();
    }
}
