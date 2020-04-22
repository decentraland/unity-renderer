namespace DCL.SettingsHUD
{
    public class SettingsHUDController : IHUD
    {
        public SettingsHUDView view { private set; get; }

        public SettingsHUDController()
        {
            view = SettingsHUDView.Create();
        }

        public void SetVisibility(bool visible)
        {
            view.SetVisibility(visible);
        }

        public void Dispose()
        {
        }
    }
}
