using DCL.Interface;

public class ToSPopupHandler : IToSPopupHandler
{
    private const string VIEW_TOS_URL = "https://decentraland.org/terms";
    private const string CANCEL_TOS_URL = "https://play.decentraland.org";

    private readonly BaseVariable<bool> toSPopupVisibleVariable;

    public ToSPopupHandler(BaseVariable<bool> toSPopupVisibleVariable)
    {
        this.toSPopupVisibleVariable = toSPopupVisibleVariable;
    }

    public void Accept()
    {
        toSPopupVisibleVariable.Set(false);
    }

    public void Cancel()
    {
        toSPopupVisibleVariable.Set(false);
        WebInterface.OpenURL(CANCEL_TOS_URL);
    }

    public void ViewToS()
    {
        WebInterface.OpenURL(VIEW_TOS_URL);
    }
}
