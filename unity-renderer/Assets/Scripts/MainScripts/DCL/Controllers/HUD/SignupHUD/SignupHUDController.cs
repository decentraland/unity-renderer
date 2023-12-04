using Cysharp.Threading.Tasks;
using DCL;
using DCL.Browser;
using DCL.Helpers;
using DCL.Interface;
using DCL.Tasks;
using DCLServices.SubscriptionsAPIService;
using System;
using System.Threading;
using UnityEngine;

namespace SignupHUD
{
    public class SignupHUDController : IHUD
    {
        private const string TOS_URL = "https://decentraland.org/terms/";
        private const string PRIVACY_POLICY_URL = "https://decentraland.org/privacy/";
        private const string NEW_TOS_AND_EMAIL_SUBSCRIPTION_FF = "new_terms_of_service_and_email_subscription";
        private const string CURRENT_SUBSCRIPTION_ID_LOCAL_STORAGE_KEY = "currentSubscriptionId";

        private readonly NewUserExperienceAnalytics newUserExperienceAnalytics;
        private readonly DataStore_LoadingScreen loadingScreenDataStore;
        private readonly DataStore_HUDs dataStoreHUDs;
        private readonly DataStore_FeatureFlag dataStoreFeatureFlag;
        private readonly DataStore_BackpackV2 dataStoreBackpack;
        private readonly DataStore_Common dataStoreCommon;
        private readonly IBrowserBridge browserBridge;
        private readonly ISubscriptionsAPIService subscriptionsAPIService;
        internal readonly ISignupHUDView view;

        internal string name;
        internal string email;
        private BaseVariable<bool> signupVisible => dataStoreHUDs.signupVisible;
        private BaseVariable<bool> backpackVisible => dataStoreHUDs.avatarEditorVisible;
        private bool isNewTermsOfServiceAndEmailSubscriptionEnabled => dataStoreFeatureFlag.flags.Get().IsFeatureEnabled(NEW_TOS_AND_EMAIL_SUBSCRIPTION_FF);

        private CancellationTokenSource createSubscriptionCts;

        public SignupHUDController(
            IAnalytics analytics,
            ISignupHUDView view,
            DataStore_LoadingScreen loadingScreenDataStore,
            DataStore_HUDs dataStoreHUDs,
            DataStore_FeatureFlag dataStoreFeatureFlag,
            DataStore_BackpackV2 dataStoreBackpack,
            DataStore_Common dataStoreCommon,
            IBrowserBridge browserBridge,
            ISubscriptionsAPIService subscriptionsAPIService)
        {
            newUserExperienceAnalytics = new NewUserExperienceAnalytics(analytics);
            this.view = view;
            this.loadingScreenDataStore = loadingScreenDataStore;
            this.dataStoreHUDs = dataStoreHUDs;
            this.dataStoreFeatureFlag = dataStoreFeatureFlag;
            this.dataStoreBackpack = dataStoreBackpack;
            this.dataStoreCommon = dataStoreCommon;
            this.browserBridge = browserBridge;
            this.subscriptionsAPIService = subscriptionsAPIService;
            loadingScreenDataStore.decoupledLoadingHUD.visible.OnChange += OnLoadingScreenAppear;
            dataStoreBackpack.isWaitingToBeSavedAfterSignUp.OnChange += OnTermsOfServiceAgreedStepAfterSaveBackpack;
        }

        public void Initialize()
        {
            if (view == null)
                return;

            signupVisible.OnChange += OnSignupVisibleChanged;

            view.OnNameScreenNext += OnNameScreenNext;
            view.OnEditAvatar += OnEditAvatar;
            view.OnTermsOfServiceAgreed += OnTermsOfServiceAgreedStepBeforeSaveBackpack;
            view.OnTermsOfServiceBack += OnTermsOfServiceBack;
            view.OnLinkClicked += OnLinkClicked;

            CommonScriptableObjects.isLoadingHUDOpen.OnChange += OnLoadingScreenAppear;
            signupVisible.Set(signupVisible.Get(), true);
        }

        private void OnLoadingScreenAppear(bool current, bool previous)
        {
            if (signupVisible.Get() && current)
            {
                signupVisible.Set(false);
                backpackVisible.Set(false);
            }
        }

        private void OnSignupVisibleChanged(bool current, bool previous)
        {
            SetVisibility(current);
        }

        internal void StartSignupProcess()
        {
            name = null;
            email = null;
            view?.ShowNameScreen();
        }

        internal void OnNameScreenNext(string newName, string newEmail)
        {
            name = newName;
            email = newEmail;
            view?.ShowTermsOfServiceScreen();
        }

        internal void OnEditAvatar()
        {
            signupVisible.Set(false);
            dataStoreHUDs.avatarEditorVisible.Set(true, true);
        }

        private void OnTermsOfServiceAgreedStepBeforeSaveBackpack()
        {
            WebInterface.SendPassport(name, email);
            dataStoreBackpack.isWaitingToBeSavedAfterSignUp.Set(true);

            newUserExperienceAnalytics?.SendTermsOfServiceAcceptedNux(name, email);

            if (!isNewTermsOfServiceAndEmailSubscriptionEnabled)
                return;

            createSubscriptionCts = createSubscriptionCts.SafeRestart();
            CreateSubscriptionAsync(email, createSubscriptionCts.Token).Forget();
        }

        private void OnTermsOfServiceAgreedStepAfterSaveBackpack(bool isBackpackWaitingToBeSaved, bool _)
        {
            if (isBackpackWaitingToBeSaved)
                return;

            dataStoreCommon.isSignUpFlow.Set(false);
        }

        private async UniTaskVoid CreateSubscriptionAsync(string emailAddress, CancellationToken cancellationToken)
        {
            try
            {
                var newSubscription = await subscriptionsAPIService.CreateSubscription(emailAddress, cancellationToken);
                PlayerPrefsBridge.SetString(CURRENT_SUBSCRIPTION_ID_LOCAL_STORAGE_KEY, newSubscription.id);
                PlayerPrefsBridge.Save();
            }
            catch (Exception ex) { Debug.LogError($"An error occurred while creating the subscription for {emailAddress}: {ex.Message}"); }
        }

        internal void OnTermsOfServiceBack() { StartSignupProcess(); }

        private void OnLinkClicked(string linkId)
        {
            switch (linkId)
            {
                case "tosUrl":
                    browserBridge.OpenUrl(TOS_URL);
                    break;
                case "privacyPolicyUrl":
                    browserBridge.OpenUrl(PRIVACY_POLICY_URL);
                    break;
            }
        }

        public void SetVisibility(bool visible)
        {
            view?.SetVisibility(visible);
            if (visible)
                StartSignupProcess();
        }

        public void Dispose()
        {
            signupVisible.OnChange -= OnSignupVisibleChanged;
            if (view == null)
                return;
            view.OnNameScreenNext -= OnNameScreenNext;
            view.OnEditAvatar -= OnEditAvatar;
            view.OnTermsOfServiceAgreed -= OnTermsOfServiceAgreedStepBeforeSaveBackpack;
            view.OnTermsOfServiceBack -= OnTermsOfServiceBack;
            CommonScriptableObjects.isFullscreenHUDOpen.OnChange -= OnLoadingScreenAppear;
            dataStoreBackpack.isWaitingToBeSavedAfterSignUp.OnChange -= OnTermsOfServiceAgreedStepAfterSaveBackpack;
            loadingScreenDataStore.decoupledLoadingHUD.visible.OnChange -= OnLoadingScreenAppear;
            createSubscriptionCts.SafeCancelAndDispose();
            view.Dispose();
        }
    }
}
