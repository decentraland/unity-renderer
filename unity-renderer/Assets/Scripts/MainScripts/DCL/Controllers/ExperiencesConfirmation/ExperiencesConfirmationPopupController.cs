using System;
using System.Collections.Generic;

namespace DCL.PortableExperiences.Confirmation
{
    public class ExperiencesConfirmationPopupController
    {
        private readonly IExperiencesConfirmationPopupView view;
        private readonly DataStore dataStore;
        private readonly IConfirmedExperiencesRepository confirmedExperiencesRepository;
        private readonly List<string> descriptionBuffer = new ();
        private Action acceptCallback;
        private Action rejectCallback;
        private string experienceId;
        private bool dontShowAnymore;

        public ExperiencesConfirmationPopupController(IExperiencesConfirmationPopupView view,
            DataStore dataStore,
            IConfirmedExperiencesRepository confirmedExperiencesRepository)
        {
            this.view = view;
            this.dataStore = dataStore;
            this.confirmedExperiencesRepository = confirmedExperiencesRepository;

            view.Hide(true);

            view.OnAccepted += () =>
            {
                view.Hide();

                if (dontShowAnymore)
                    confirmedExperiencesRepository.Set(experienceId, true);

                acceptCallback?.Invoke();
            };
            view.OnRejected += () =>
            {
                view.Hide();

                if (dontShowAnymore)
                    confirmedExperiencesRepository.Set(experienceId, false);

                rejectCallback?.Invoke();
            };
            view.OnDontShowAnymore += () => dontShowAnymore = true;
            view.OnKeepShowing += () => dontShowAnymore = false;

            dataStore.world.portableExperiencePendingToConfirm.OnChange += OnConfirmRequested;
        }

        public void Dispose()
        {
            view.Dispose();

            dataStore.world.portableExperiencePendingToConfirm.OnChange -= OnConfirmRequested;
        }

        private void OnConfirmRequested(ExperiencesConfirmationData current, ExperiencesConfirmationData previous)
        {
            if (dataStore.world.ignorePortableExperienceConfirmation.Equals(current.Experience.ExperienceId))
            {
                current.OnAcceptCallback?.Invoke();
                return;
            }

            if (confirmedExperiencesRepository.Contains(current.Experience.ExperienceId))
            {
                if (confirmedExperiencesRepository.Get(current.Experience.ExperienceId))
                    current.OnAcceptCallback?.Invoke();
                else
                    current.OnRejectCallback?.Invoke();

                return;
            }

            ExperiencesConfirmationData.ExperienceMetadata metadata = current.Experience;

            experienceId = current.Experience.ExperienceId;
            acceptCallback = current.OnAcceptCallback;
            rejectCallback = current.OnRejectCallback;

            descriptionBuffer.Clear();

            foreach (string permission in metadata.Permissions)
                descriptionBuffer.Add(ConvertPermissionIdToDescription(permission));

            view.Show();
            view.SetModel(new ExperiencesConfirmationViewModel
            {
                Name = metadata.ExperienceName,
                IconUrl = metadata.IconUrl,
                Permissions = descriptionBuffer,
                Description = metadata.Description,
            });
        }

        private string ConvertPermissionIdToDescription(string permissionId)
        {
            switch (permissionId)
            {
                case "USE_FETCH":
                    return "Let the scene perform external HTTP requests.";
                case "USE_WEBSOCKET":
                    return "Let the scene use the Websocket API to establish external connections.";
                case "OPEN_EXTERNAL_LINK":
                    return "Let the scene open a URL (in a browser tab or web view).";
                case "USE_WEB3_API":
                    return "Let the scene communicate with a wallet.";
                case "ALLOW_TO_TRIGGER_AVATAR_EMOTE":
                    return "Let the scene to animate the player’s avatar with an emote.";
                case "ALLOW_TO_MOVE_PLAYER_INSIDE_SCENE":
                    return "Let the scene to change the player’s position.";
            }

            return permissionId;
        }
    }
}
