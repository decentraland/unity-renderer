using DCL.Interface;

namespace MainScripts.DCL.Controllers.HUD.ToSPopupHUD
{
    public class ToSPopupHandler : IToSPopupHandler
    {
        private readonly BaseVariable<bool> toSPopupVisibleVariable;

        public ToSPopupHandler(BaseVariable<bool> toSPopupVisibleVariable)
        {
            this.toSPopupVisibleVariable = toSPopupVisibleVariable;
        }

        public void Accept()
        {
            toSPopupVisibleVariable.Set(false);
            WebInterface.SendToSPopupAccepted();
        }

        public void Cancel()
        {
            toSPopupVisibleVariable.Set(false);
            WebInterface.SendToSPopupRejected();
        }

        public void ViewToS()
        {
            WebInterface.SendGoToTos();
        }
    }
}
