using Cysharp.Threading.Tasks;
using DCL.Tasks;
using DCL.World.PortableExperiences;
using DCLServices.PortableExperiences.Analytics;
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
        private readonly IPortableExperiencesAnalyticsService analytics;
        private readonly List<string> descriptionBuffer = new ();

        private string experienceId;
        private bool dontShowAnymore;
        private CancellationTokenSource openProcessCancellationToken = new ();
        private UniTaskCompletionSource<InputType> userInputTask = new ();

        public ExperiencesConfirmationPopupController(IExperiencesConfirmationPopupView view,
            DataStore dataStore,
            IConfirmedExperiencesRepository confirmedExperiencesRepository,
            IUserProfileBridge userProfileBridge,
            IPortableExperiencesAnalyticsService analytics)
        {
            this.view = view;
            this.dataStore = dataStore;
            this.confirmedExperiencesRepository = confirmedExperiencesRepository;
            this.userProfileBridge = userProfileBridge;
            this.analytics = analytics;

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
                        descriptionBuffer.Add(permission);
                }

                view.Show();

                bool isSmartWearable = userProfileBridge.GetOwn().avatar.wearables.Contains(pxId);

                view.SetModel(new ExperiencesConfirmationViewModel
                {
                    Name = metadata.ExperienceName,
                    IconUrl = metadata.IconUrl,
                    Permissions = descriptionBuffer,
                    Description = metadata.Description,
                    IsSmartWearable = isSmartWearable,
                });

                try
                {
                    InputType inputType = await userInputTask.Task.AttachExternalCancellation(cancellationToken);

                    switch (inputType)
                    {
                        case InputType.Accept:
                            if (dontShowAnymore)
                                confirmedExperiencesRepository.Set(experienceId, true);

                            analytics.Accept(experienceId, dontShowAnymore, isSmartWearable ? "smart_wearable" : "scene");

                            confirmationData.OnAcceptCallback?.Invoke();
                            break;
                        case InputType.Reject:
                            if (dontShowAnymore)
                                confirmedExperiencesRepository.Set(experienceId, false);

                            analytics.Reject(experienceId, dontShowAnymore, isSmartWearable ? "smart_wearable" : "scene");

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
    }
}
