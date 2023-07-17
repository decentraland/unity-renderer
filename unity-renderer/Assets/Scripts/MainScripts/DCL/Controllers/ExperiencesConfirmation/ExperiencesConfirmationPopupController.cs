using Cysharp.Threading.Tasks;
using DCL.Tasks;
using DCL.World.PortableExperiences;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace DCL.PortableExperiences.Confirmation
{
    public class ExperiencesConfirmationPopupController
    {
        private enum InputType
        {
            Accept,
            Reject,
            Cancel,
        }

        private readonly IExperiencesConfirmationPopupView view;
        private readonly DataStore dataStore;
        private readonly IConfirmedExperiencesRepository confirmedExperiencesRepository;
        private readonly IUserProfileBridge userProfileBridge;
        private readonly List<string> descriptionBuffer = new ();

        private string experienceId;
        private bool dontShowAnymore;
        private CancellationTokenSource openProcessCancellationToken = new ();
        private UniTaskCompletionSource<InputType> userInputTask = new ();

        public ExperiencesConfirmationPopupController(IExperiencesConfirmationPopupView view,
            DataStore dataStore,
            IConfirmedExperiencesRepository confirmedExperiencesRepository,
            IUserProfileBridge userProfileBridge)
        {
            this.view = view;
            this.dataStore = dataStore;
            this.confirmedExperiencesRepository = confirmedExperiencesRepository;
            this.userProfileBridge = userProfileBridge;

            view.Hide(true);

            view.OnAccepted += () => userInputTask.TrySetResult(InputType.Accept);
            view.OnRejected += () => userInputTask.TrySetResult(InputType.Reject);
            view.OnCancelled += () => userInputTask.TrySetResult(InputType.Cancel);
            view.OnDontShowAnymore += () => dontShowAnymore = true;
            view.OnKeepShowing += () => dontShowAnymore = false;

            dataStore.world.portableExperiencePendingToConfirm.OnChange += OnConfirmRequested;
        }

        public void Dispose()
        {
            view.Dispose();
            openProcessCancellationToken.SafeCancelAndDispose();

            dataStore.world.portableExperiencePendingToConfirm.OnChange -= OnConfirmRequested;
        }

        private void OnConfirmRequested(ExperiencesConfirmationData current, ExperiencesConfirmationData previous)
        {
            async UniTaskVoid ShowAndWaitForUserInputThenHide(
                ExperiencesConfirmationData confirmationData,
                CancellationToken cancellationToken)
            {
                string pxId = confirmationData.Experience.ExperienceId;

                ExperiencesConfirmationData.ExperienceMetadata metadata = confirmationData.Experience;

                experienceId = pxId;

                descriptionBuffer.Clear();

                if (metadata.Permissions != null)
                {
                    foreach (string permission in metadata.Permissions)
                        descriptionBuffer.Add(ConvertPermissionIdToDescription(permission));
                }

                view.Show();

                view.SetModel(new ExperiencesConfirmationViewModel
                {
                    Name = metadata.ExperienceName,
                    IconUrl = metadata.IconUrl,
                    Permissions = descriptionBuffer,
                    Description = metadata.Description,
                    IsSmartWearable = userProfileBridge.GetOwn().avatar.wearables.Contains(pxId),
                });

                try
                {
                    InputType inputType = await userInputTask.Task.AttachExternalCancellation(cancellationToken);

                    switch (inputType)
                    {
                        case InputType.Accept:
                            if (dontShowAnymore)
                                confirmedExperiencesRepository.Set(experienceId, true);

                            confirmationData.OnAcceptCallback?.Invoke();
                            break;
                        case InputType.Reject:
                            if (dontShowAnymore)
                                confirmedExperiencesRepository.Set(experienceId, false);

                            confirmationData.OnRejectCallback?.Invoke();
                            break;
                        case InputType.Cancel:
                            confirmationData.OnRejectCallback?.Invoke();
                            break;
                    }

                    view.Hide();
                }
                catch (OperationCanceledException) { }
                catch (Exception e) { Debug.LogException(e); }
            }

            openProcessCancellationToken = openProcessCancellationToken.SafeRestart();
            openProcessCancellationToken = new CancellationTokenSource();
            userInputTask.TrySetCanceled();
            userInputTask = new UniTaskCompletionSource<InputType>();
            ShowAndWaitForUserInputThenHide(current, openProcessCancellationToken.Token).Forget();
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
