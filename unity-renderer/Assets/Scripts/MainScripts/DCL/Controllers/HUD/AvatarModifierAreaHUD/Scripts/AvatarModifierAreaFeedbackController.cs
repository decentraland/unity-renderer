namespace DCL.AvatarModifierAreaFeedback
{
    public class AvatarModifierAreaFeedbackController
    {
        internal IAvatarModifierAreaFeedbackView view;
        private BaseRefCounter<AvatarModifierAreaID> avatarModifiersWarnings;
        private readonly DataStore_Common commonDataStore;

        public AvatarModifierAreaFeedbackController(
            BaseRefCounter<AvatarModifierAreaID> avatarAreaWarnings,
            IAvatarModifierAreaFeedbackView view,
            DataStore_Common commonDataStore)
        {
            this.view = view;
            view.SetUp(avatarAreaWarnings);

            this.commonDataStore = commonDataStore;
            commonDataStore.isWorld.OnChange += OnWorldModeChange;
            OnWorldModeChange(commonDataStore.isWorld.Get(), false);
        }

        private void OnWorldModeChange(bool isWorld, bool _) =>
            view.SetWorldMode(isWorld);

        public void Dispose()
        {
            commonDataStore.isWorld.OnChange -= OnWorldModeChange;
            view.Dispose();
        }

    }
}

