using Cysharp.Threading.Tasks;
using DCL.Helpers;
using DCL.Tasks;
using DCLServices.SubscriptionsAPIService;
using System;
using System.Net.Mail;
using System.Threading;
using UnityEngine;

namespace DCL.MyAccount
{
    public class EmailNotificationsController
    {
        private const string CURRENT_SUBSCRIPTION_ID_LOCAL_STORAGE_KEY = "currentSubscriptionId";

        private readonly IEmailNotificationsComponentView view;
        private readonly MyAccountSectionHUDController myAccountSectionHUDController;
        private readonly DataStore dataStore;
        private readonly ISubscriptionsAPIService subscriptionsAPIService;

        private CancellationTokenSource loadEmailCancellationToken;
        private CancellationTokenSource updateEmailCancellationToken;

        private string savedEmail;
        private bool savedIsPending;

        public EmailNotificationsController(
            IEmailNotificationsComponentView view,
            MyAccountSectionHUDController myAccountSectionHUDController,
            DataStore dataStore,
            ISubscriptionsAPIService subscriptionsAPIService)
        {
            this.view = view;
            this.myAccountSectionHUDController = myAccountSectionHUDController;
            this.dataStore = dataStore;
            this.subscriptionsAPIService = subscriptionsAPIService;

            dataStore.myAccount.isMyAccountSectionVisible.OnChange += OnMyAccountSectionOpen;
            dataStore.myAccount.openSection.OnChange += OnMyAccountSectionTabChanged;
            view.OnEmailEdited += OnEmailEdited;
            view.OnEmailSubmitted += OnEmailSubmitted;
            view.OnReSendConfirmationEmailClicked += OnReSendConfirmationEmailClicked;
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
                view.SetEmail(savedEmail);
                view.SetStatusAsPending(savedIsPending);
                view.SetEmailFormValid(true);
                return;
            }

            if (newEmail == savedEmail)
                return;

            updateEmailCancellationToken = updateEmailCancellationToken.SafeRestart();
            UpdateEmailAsync(newEmail, updateEmailCancellationToken.Token).Forget();
        }

        private void OnReSendConfirmationEmailClicked()
        {
            updateEmailCancellationToken = updateEmailCancellationToken.SafeRestart();
            UpdateEmailAsync(savedEmail, updateEmailCancellationToken.Token).Forget();
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
                        savedEmail = existingSubscription.email;
                        savedIsPending = existingSubscription.status == "pending";
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

            view.SetEmail(savedEmail);
            view.SetStatusAsPending(savedIsPending);
            view.SetLoadingActive(false);
        }

        private async UniTaskVoid UpdateEmailAsync(string newEmail, CancellationToken cancellationToken)
        {
            var wasSuccessfullyUpdated = false;
            view.SetEmailInputInteractable(false);

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
                    savedIsPending = newSubscription.status == "pending";
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

            if (wasSuccessfullyUpdated)
                myAccountSectionHUDController.ShowAccountSettingsUpdatedToast();
        }
    }
}
