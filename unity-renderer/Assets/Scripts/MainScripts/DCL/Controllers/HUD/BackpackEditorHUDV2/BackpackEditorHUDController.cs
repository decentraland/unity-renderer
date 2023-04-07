namespace DCL.Backpack
{
    public class BackpackEditorHUDController : IHUD
    {
        private readonly IBackpackEditorHUDView view;

        public BackpackEditorHUDController(IBackpackEditorHUDView view,
            DataStore dataStore)
        {
            this.view = view;
            SetVisibility(dataStore.HUDs.avatarEditorVisible.Get());
            dataStore.HUDs.avatarEditorVisible.OnChange += SetVisibility;
            dataStore.HUDs.isAvatarEditorInitialized.Set(true);
        }

        public void Dispose()
        {
            view.Dispose();
        }

        private void SetVisibility(bool current, bool _) =>
            SetVisibility(current);

        public void SetVisibility(bool visible)
        {
            if (visible)
                view.Show();
            else
                view.Hide();
        }
    }
}
