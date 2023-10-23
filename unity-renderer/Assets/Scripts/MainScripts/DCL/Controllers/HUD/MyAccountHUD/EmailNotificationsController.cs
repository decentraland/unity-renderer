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

        private static string savedSubscription => PlayerPrefsBridge.GetString(CURRENT_SUBSCRIPTION_ID_LOCAL_STORAGE_KEY);
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

            dataStore.myAccount.isMyAccountSectionVisible.OnChange += OnMyAccountSectionVisibleChanged;
            view.OnEmailEdited += OnEmailEdited;
            view.OnEmailSubmitted += OnEmailSubmitted;
        }

        public void Dispose()
        {
            loadEmailCancellationToken.SafeCancelAndDispose();
            dataStore.myAccount.isMyAccountSectionVisible.OnChange -= OnMyAccountSectionVisibleChanged;
            view.OnEmailEdited -= OnEmailEdited;
            view.OnEmailSubmitted -= OnEmailSubmitted;
        }

        private void OnMyAccountSectionVisibleChanged(bool isVisible, bool _)
        {
            if (isVisible)
                OpenSection();
            else
                CloseSection();
        }

        private void OpenSection()
        {
            view.ResetForm();
            loadEmailCancellationToken = loadEmailCancellationToken.SafeRestart();
            LoadEmailAsync(loadEmailCancellationToken.Token).Forget();
        }

        private void CloseSection()
        {
            loadEmailCancellationToken.SafeCancelAndDispose();
        }

        private async UniTaskVoid LoadEmailAsync(CancellationToken cancellationToken)
        {
            view.SetLoadingActive(true);
            savedEmail = string.Empty;
            savedIsPending = false;

            if (PlayerPrefsBridge.HasKey(CURRENT_SUBSCRIPTION_ID_LOCAL_STORAGE_KEY))
            {
                try
                {
                    var existingSubscription = await subscriptionsAPIService.GetSubscription(savedSubscription, cancellationToken);

                    if (existingSubscription != null)
                    {
                        savedEmail = existingSubscription.email;
                        savedIsPending = existingSubscription.status == "pending";
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError($"An error occurred while getting the email subscription '{savedSubscription}': {ex.Message}");
                }
            }

            view.SetEmail(savedEmail);
            view.SetStatusAsPending(savedIsPending);
            view.SetLoadingActive(false);
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

            if (string.IsNullOrEmpty(newEmail))
                Debug.Log($"SANTI ---> UNSUBSCRIBE [{savedEmail}]");
            else
            {
                Debug.Log($"SANTI ---> UNSUBSCRIBE [{savedEmail}]");
                Debug.Log($"SANTI ---> SUBSCRIBE [{newEmail}]");
            }
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
    }
}
