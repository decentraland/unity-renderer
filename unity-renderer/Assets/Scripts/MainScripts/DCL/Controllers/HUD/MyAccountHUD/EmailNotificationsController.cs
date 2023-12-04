using Cysharp.Threading.Tasks;
using DCL.Helpers;
using DCL.Tasks;
using DCLServices.SubscriptionsAPIService;
using SocialFeaturesAnalytics;
using System;
using System.Net.Mail;
using System.Threading;
using UnityEngine;

namespace DCL.MyAccount
{
    public class EmailNotificationsController
    {
        private enum ConfirmationModalStatus
        {
            None,
            Accepted,
            Rejected,
        }

        private const string CURRENT_SUBSCRIPTION_ID_LOCAL_STORAGE_KEY = "currentSubscriptionId";

        private readonly IEmailNotificationsComponentView view;
        private readonly IUpdateEmailConfirmationHUDComponentView updateEmailConfirmationHUDComponentView;
        private readonly MyAccountSectionHUDController myAccountSectionHUDController;
        private readonly DataStore dataStore;
        private readonly ISubscriptionsAPIService subscriptionsAPIService;
        private readonly ISocialAnalytics socialAnalytics;

        private CancellationTokenSource loadEmailCancellationToken;
        private CancellationTokenSource updateEmailCancellationToken;

        private string savedEmail;
        private bool savedIsPending;
        private ConfirmationModalStatus confirmationModalStatus = ConfirmationModalStatus.None;

        public EmailNotificationsController(
            IEmailNotificationsComponentView view,
            IUpdateEmailConfirmationHUDComponentView updateEmailConfirmationHUDComponentView,
            MyAccountSectionHUDController myAccountSectionHUDController,
            DataStore dataStore,
            ISubscriptionsAPIService subscriptionsAPIService,
            ISocialAnalytics socialAnalytics)
        {
            this.view = view;
            this.updateEmailConfirmationHUDComponentView = updateEmailConfirmationHUDComponentView;
            this.myAccountSectionHUDController = myAccountSectionHUDController;
            this.dataStore = dataStore;
            this.subscriptionsAPIService = subscriptionsAPIService;
            this.socialAnalytics = socialAnalytics;

            dataStore.myAccount.isMyAccountSectionVisible.OnChange += OnMyAccountSectionOpen;
            dataStore.myAccount.openSection.OnChange += OnMyAccountSectionTabChanged;
            view.OnEmailEdited += OnEmailEdited;
            view.OnEmailSubmitted += OnEmailSubmitted;
            view.OnReSendConfirmationEmailClicked += OnReSendConfirmationEmailClicked;
            updateEmailConfirmationHUDComponentView.OnConfirmationModalAccepted += isAccepted => confirmationModalStatus = isAccepted ? ConfirmationModalStatus.Accepted : ConfirmationModalStatus.Rejected;
        }

        public void Dispose()
        {
            loadEmailCancellationToken.SafeCancelAndDispose();
            updateEmailCancellationToken.SafeCancelAndDispose();
            dataStore.myAccount.isMyAccountSectionVisible.OnChange -= OnMyAccountSectionOpen;
            dataStore.myAccount.openSection.OnChange -= OnMyAccountSectionTabChanged;
            view.OnEmailEdited -= OnEmailEdited;
            view.OnEmailSubmitted -= OnEmailSubmitted;
            view.OnReSendConfirmationEmailClicked -= OnReSendConfirmationEmailClicked;
        }

        private void OnMyAccountSectionOpen(bool isVisible, bool _)
        {
            if (!isVisible || dataStore.myAccount.openSection.Get() != MyAccountSection.EmailNotifications.ToString())
                return;

            RefreshForm();
        }

        private void OnMyAccountSectionTabChanged(string currentOpenSection, string _)
        {
            if (currentOpenSection != MyAccountSection.EmailNotifications.ToString())
                return;

            RefreshForm();
        }

        private void RefreshForm()
        {
            view.ResetForm();
            loadEmailCancellationToken = loadEmailCancellationToken.SafeRestart();
            LoadEmailAsync(loadEmailCancellationToken.Token).Forget();
            updateEmailConfirmationHUDComponentView.HideConfirmationModal();
        }

        private void OnEmailEdited(string newEmail)
        {
            view.SetEmailFormValid(IsValidEmail(newEmail));
            view.SetStatusAsPending(newEmail == savedEmail && savedIsPending);
        }

        private void OnEmailSubmitted(string newEmail)
        {
            if (!IsValidEmail(newEmail))
            {
                SetSavedEmail();
                return;
            }

            if (newEmail == savedEmail)
                return;

            updateEmailCancellationToken = updateEmailCancellationToken.SafeRestart();
            UpdateEmailAsync(newEmail, true, updateEmailCancellationToken.Token).Forget();
        }

        private void SetSavedEmail()
        {
            view.SetEmail(savedEmail);
            view.SetStatusAsPending(savedIsPending);
            view.SetEmailFormValid(true);
            view.SetEmailInputInteractable(true);
            view.SetEmailUpdateLoadingActive(false);
        }

        private void OnReSendConfirmationEmailClicked()
        {
            updateEmailCancellationToken = updateEmailCancellationToken.SafeRestart();
            UpdateEmailAsync(savedEmail, false, updateEmailCancellationToken.Token).Forget();
        }

        private static bool IsValidEmail(string email)
        {
            if (email.Length == 0)
                return true;

            try
            {
                MailAddress mailAddress = new MailAddress(email);
                return mailAddress.Address == email;
            }
            catch
            {
                return false;
            }
        }

        private async UniTaskVoid LoadEmailAsync(CancellationToken cancellationToken)
        {
            view.SetLoadingActive(true);
            savedEmail = string.Empty;
            savedIsPending = false;

            if (!string.IsNullOrEmpty(PlayerPrefsBridge.GetString(CURRENT_SUBSCRIPTION_ID_LOCAL_STORAGE_KEY)))
            {
                string subscriptionToGet = PlayerPrefsBridge.GetString(CURRENT_SUBSCRIPTION_ID_LOCAL_STORAGE_KEY);

                try
                {
                    var existingSubscription = await subscriptionsAPIService.GetSubscription(subscriptionToGet, cancellationToken);

                    if (existingSubscription != null)
                    {
                        if (existingSubscription.status == "inactive")
                            PlayerPrefsBridge.SetString(CURRENT_SUBSCRIPTION_ID_LOCAL_STORAGE_KEY, string.Empty);
                        else
                        {
                            savedEmail = existingSubscription.email;
                            savedIsPending = existingSubscription.status is "pending";
                        }
                    }
                }
                catch (Exception ex)
                {
                    if (ex.Message.Contains("404"))
                        PlayerPrefsBridge.SetString(CURRENT_SUBSCRIPTION_ID_LOCAL_STORAGE_KEY, string.Empty);
                    else
                        Debug.LogError($"An error occurred while getting the email subscription '{subscriptionToGet}': {ex.Message}");
                }
            }

            SetSavedEmail();
            view.SetLoadingActive(false);
        }

        private async UniTaskVoid UpdateEmailAsync(string newEmail, bool needConfirmation, CancellationToken cancellationToken)
        {
            var wasSuccessfullyUpdated = false;
            view.SetEmailInputInteractable(false);
            view.SetEmailUpdateLoadingActive(true);

            if (needConfirmation && !string.IsNullOrEmpty(savedEmail))
            {
                confirmationModalStatus = ConfirmationModalStatus.None;
                updateEmailConfirmationHUDComponentView.ShowConfirmationModal(
                    string.IsNullOrEmpty(newEmail) ? view.deleteEmailLogo : view.updateEmailLogo,
                    string.IsNullOrEmpty(newEmail) ? "Are you sure you want to unsubscribe from Decentraland's newsletter?" : "Are you sure you want to update your Email Address?");

                await UniTask.WaitUntil(() => confirmationModalStatus != ConfirmationModalStatus.None, cancellationToken: cancellationToken);
            }

            if (confirmationModalStatus == ConfirmationModalStatus.Rejected)
            {
                SetSavedEmail();
                return;
            }

            if (!string.IsNullOrEmpty(PlayerPrefsBridge.GetString(CURRENT_SUBSCRIPTION_ID_LOCAL_STORAGE_KEY)))
            {
                string subscriptionToDelete = PlayerPrefsBridge.GetString(CURRENT_SUBSCRIPTION_ID_LOCAL_STORAGE_KEY);

                try
                {
                    await subscriptionsAPIService.DeleteSubscription(subscriptionToDelete, cancellationToken);
                    PlayerPrefsBridge.SetString(CURRENT_SUBSCRIPTION_ID_LOCAL_STORAGE_KEY, string.Empty);
                    PlayerPrefsBridge.Save();
                    savedEmail = string.Empty;
                    savedIsPending = false;
                    wasSuccessfullyUpdated = true;
                }
                catch (Exception ex)
                {
                    wasSuccessfullyUpdated = false;
                    Debug.LogError($"An error occurred while deleting the email subscription '{subscriptionToDelete}': {ex.Message}");
                }
            }

            if (!string.IsNullOrEmpty(newEmail))
            {
                try
                {
                    var newSubscription = await subscriptionsAPIService.CreateSubscription(newEmail, cancellationToken);
                    PlayerPrefsBridge.SetString(CURRENT_SUBSCRIPTION_ID_LOCAL_STORAGE_KEY, newSubscription.id);
                    PlayerPrefsBridge.Save();
                    savedEmail = newEmail;
                    savedIsPending = newSubscription.status is "pending" or "validating";
                    wasSuccessfullyUpdated = true;
                }
                catch (Exception ex)
                {
                    wasSuccessfullyUpdated = false;
                    Debug.LogError($"An error occurred while creating the subscription for {newEmail}: {ex.Message}");
                }
            }

            view.SetEmailInputInteractable(true);
            view.SetStatusAsPending(savedIsPending);
            view.SetEmailUpdateLoadingActive(false);

            if (wasSuccessfullyUpdated)
            {
                myAccountSectionHUDController.ShowAccountSettingsUpdatedToast();
                socialAnalytics.SendProfileEdit(0, false, PlayerActionSource.EmailNotifications, ProfileField.Email);
            }
        }
    }
}
