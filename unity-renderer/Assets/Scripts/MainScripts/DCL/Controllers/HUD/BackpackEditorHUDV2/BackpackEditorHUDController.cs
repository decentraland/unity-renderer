using UnityEngine;

namespace DCL.Backpack
{
    public class BackpackEditorHUDController : IHUD
    {
        private readonly IBackpackEditorHUDView view;
        private readonly DataStore dataStore;

        public BackpackEditorHUDController(IBackpackEditorHUDView view, DataStore dataStore)
        {
            this.view = view;
            this.dataStore = dataStore;
            dataStore.HUDs.avatarEditorVisible.OnChange += SetVisibility;
            dataStore.HUDs.isAvatarEditorInitialized.Set(true);
            dataStore.exploreV2.configureBackpackInFullscreenMenu.OnChange += ConfigureBackpackInFullscreenMenuChanged;
            ConfigureBackpackInFullscreenMenuChanged(dataStore.exploreV2.configureBackpackInFullscreenMenu.Get(), null);
            SetVisibility(dataStore.HUDs.avatarEditorVisible.Get());
        }

        public void Dispose()
        {
            dataStore.HUDs.avatarEditorVisible.OnChange -= SetVisibility;
            dataStore.exploreV2.configureBackpackInFullscreenMenu.OnChange -= ConfigureBackpackInFullscreenMenuChanged;
            view.Dispose();
        }

        public void SetVisibility(bool visible)
        {
            if (visible)
                view.Show();
            else
                view.Hide();
        }

        private void SetVisibility(bool current, bool _) =>
            SetVisibility(current);

        private void ConfigureBackpackInFullscreenMenuChanged(Transform currentParentTransform, Transform previousParentTransform) =>
            view.SetAsFullScreenMenuMode(currentParentTransform);
    }
}
