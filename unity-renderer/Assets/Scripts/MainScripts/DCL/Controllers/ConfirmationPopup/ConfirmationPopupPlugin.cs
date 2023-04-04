namespace DCL.ConfirmationPopup
{
    public class ConfirmationPopupPlugin : IPlugin
    {
        private readonly ConfirmationPopupHUDController controller;

        public ConfirmationPopupPlugin()
        {
            controller = new ConfirmationPopupHUDController(ConfirmationPopupHUDComponentView.Create(),
                DataStore.i.notifications);
        }

        public void Dispose()
        {
            controller.Dispose();
        }
    }
}
