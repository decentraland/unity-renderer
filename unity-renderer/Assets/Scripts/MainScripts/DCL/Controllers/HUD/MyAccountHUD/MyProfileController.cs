using Cysharp.Threading.Tasks;
using DCL.Browser;
using DCL.Tasks;
using DCLServices.Lambdas.NamesService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

namespace DCL.MyAccount
{
    public class MyProfileController
    {
        private const string NON_CLAIMED_NAME_OPTION = "- Non-claimed NAME -";
        private const string CLAIM_UNIQUE_NAME_URL = "https://builder.decentraland.org/claim-name";

        private readonly IMyProfileComponentView view;
        private readonly DataStore dataStore;
        private readonly IUserProfileBridge userProfileBridge;
        private readonly INamesService namesService;
        private readonly IBrowserBridge browserBridge;
        private readonly MyAccountSectionHUDController myAccountSectionHUDController;
        private readonly List<string> loadedNames = new ();

        private CancellationTokenSource cts;

        private UserProfile ownUserProfile => userProfileBridge.GetOwn();

        public MyProfileController(
            IMyProfileComponentView view,
            DataStore dataStore,
            IUserProfileBridge userProfileBridge,
            INamesService namesService,
            IBrowserBridge browserBridge,
            MyAccountSectionHUDController myAccountSectionHUDController)
        {
            this.view = view;
            this.dataStore = dataStore;
            this.userProfileBridge = userProfileBridge;
            this.namesService = namesService;
            this.browserBridge = browserBridge;
            this.myAccountSectionHUDController = myAccountSectionHUDController;

            dataStore.myAccount.isMyAccountSectionVisible.OnChange += OnMyAccountSectionVisibleChanged;
            view.OnCurrentNameEdited += OnNameEdited;
            view.OnCurrentNameSubmitted += OnNameSubmitted;
            view.OnClaimNameClicked += OnClaimNameRequested;
            ownUserProfile.OnUpdate += OnOwnUserProfileUpdated;
        }

        public void Dispose()
        {
            dataStore.myAccount.isMyAccountSectionVisible.OnChange -= OnMyAccountSectionVisibleChanged;
            view.OnCurrentNameEdited -= OnNameEdited;
            view.OnCurrentNameSubmitted -= OnNameSubmitted;
            view.OnClaimNameClicked -= OnClaimNameRequested;
            ownUserProfile.OnUpdate -= OnOwnUserProfileUpdated;
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
            cts = cts.SafeRestart();
            LoadOwnedNamesAsync(cts.Token).Forget();
        }

        private void CloseSection() =>
            cts.SafeCancelAndDispose();

        private async UniTask LoadOwnedNamesAsync(CancellationToken ct)
        {
            try
            {
                loadedNames.Clear();
                view.SetLoadingActive(true);
                var names = await namesService.RequestOwnedNamesAsync(
                    ownUserProfile.userId,
                    1,
                    int.MaxValue,
                    true,
                    ct);

                if (names.names is { Count: > 0 })
                {
                    var optionsToLoad = new List<string>();
                    optionsToLoad.AddRange(names.names.Select(x => x.Name));
                    optionsToLoad.Add(NON_CLAIMED_NAME_OPTION);
                    view.SetClaimedNameDropdownOptions(optionsToLoad);
                    loadedNames.AddRange(optionsToLoad);
                }
            }
            catch (OperationCanceledException) { }
            catch (Exception e) { Debug.LogError(e.Message); }
            finally { RefreshNamesSectionStatus(); }
        }

        private void RefreshNamesSectionStatus()
        {
            view.SetClaimedNameMode(loadedNames.Count > 0);
            view.SetClaimedModeAsInput(!ownUserProfile.hasClaimedName);
            view.SetCurrentName(ownUserProfile.userName);
            view.SetClaimBannerActive(loadedNames.Count == 0);
            view.SetLoadingActive(false);
        }

        private void OnNameEdited(string newName)
        {
            // TODO: Name validations...
        }

        private void OnNameSubmitted(string newName, bool isClaimed)
        {
            if (newName == ownUserProfile.userName)
                return;

            if (newName == NON_CLAIMED_NAME_OPTION)
            {
                view.SetClaimedModeAsInput(true);
                return;
            }

            if (isClaimed)
                userProfileBridge.SaveVerifiedName(newName);
            else
                userProfileBridge.SaveUnverifiedName(newName);

            myAccountSectionHUDController.ShowAccountSettingsUpdatedToast();
        }

        private void OnClaimNameRequested() =>
            browserBridge.OpenUrl(CLAIM_UNIQUE_NAME_URL);

        private void OnOwnUserProfileUpdated(UserProfile userProfile)
        {
            if (userProfile == null)
                return;

            RefreshNamesSectionStatus();
        }
    }
}
