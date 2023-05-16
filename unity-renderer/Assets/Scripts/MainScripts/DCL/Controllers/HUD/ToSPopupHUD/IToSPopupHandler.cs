using DCL;
using DCL.Interface;
using System;

public interface IToSPopupHandler
{
    void Accept();
    void Cancel();
    void ViewToS();
}

public class ToSPopupHandler : IToSPopupHandler
{
    private const string VIEW_TOS_URL = "https://decentraland.org/placeholderlink-shouldbereplaced-hitmeifyouseethis";
    private const string CANCEL_TOS_URL = "https://play.decentraland.org";


    public ToSPopupHandler()
    {
    }

    public void Accept()
    {
    }

    public void Cancel()
    {
        WebInterface.OpenURL(CANCEL_TOS_URL);
    }

    public void ViewToS()
    {
        WebInterface.OpenURL(VIEW_TOS_URL);
    }
}
