namespace DCL.Backpack
{
    public class BackpackEditorV2Plugin : IPlugin
    {
        private readonly BackpackEditorHUDController hudController;

        public BackpackEditorV2Plugin()
        {
            hudController = new BackpackEditorHUDController(BackpackEditorHUDV2ComponentView.Create());
        }

        public void Dispose()
        {
            hudController.Dispose();
        }
    }
}
