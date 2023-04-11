namespace DCL.Backpack
{
    public class BackpackEditorV2Plugin : IPlugin
    {
        private readonly BackpackEditorHUDController hudController;

        public BackpackEditorV2Plugin()
        {
            var view = BackpackEditorHUDV2ComponentView.Create();
            hudController = new BackpackEditorHUDController(view, DataStore.i);
            view.Initialize();
        }

        public void Dispose()
        {
            hudController.Dispose();
        }
    }
}
