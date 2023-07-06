using System;

namespace DCL.PortableExperiences.Confirmation
{
    public class ExperiencesConfirmationPopupController
    {
        private readonly IExperiencesConfirmationPopupView view;
        private readonly DataStore dataStore;
        private Action acceptCallback;
        private Action rejectCallback;

        public ExperiencesConfirmationPopupController(IExperiencesConfirmationPopupView view,
            DataStore dataStore)
        {
            this.view = view;
            this.dataStore = dataStore;

            view.Hide(true);

            view.OnAccepted += () =>
            {
                view.Hide();
                acceptCallback?.Invoke();
            };
            view.OnRejected += () =>
            {
                view.Hide();
                rejectCallback?.Invoke();
            };

            dataStore.PendingPortableExperienceToBeConfirmed.Confirm.OnChange += OnConfirmRequested;
        }

        public void Dispose()
        {
            view.Dispose();

            dataStore.PendingPortableExperienceToBeConfirmed.Confirm.OnChange -= OnConfirmRequested;
        }

        private void OnConfirmRequested(ExperiencesConfirmationData current, ExperiencesConfirmationData previous)
        {
            ExperiencesConfirmationData.ExperienceMetadata metadata = current.Experience;

            acceptCallback = current.OnAcceptCallback;
            rejectCallback = current.OnRejectCallback;

            view.Show();
            view.SetModel(new ExperiencesConfirmationViewModel
            {
                Name = metadata.ExperienceName,
                IconUrl = metadata.IconUrl,
                Permissions = metadata.Permissions,
            });
        }
    }
}
