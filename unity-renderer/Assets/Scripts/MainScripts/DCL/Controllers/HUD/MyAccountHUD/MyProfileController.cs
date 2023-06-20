using Cysharp.Threading.Tasks;
using DCL.Browser;
using DCL.Tasks;
using DCLServices.Lambdas.NamesService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using UnityEngine;

namespace DCL.MyAccount
{
    public class MyProfileController
    {
        private const string CLAIM_UNIQUE_NAME_URL = "https://builder.decentraland.org/claim-name";

        private readonly IMyProfileComponentView view;
        private readonly DataStore dataStore;
        private readonly IUserProfileBridge userProfileBridge;
        private readonly INamesService namesService;
        private readonly IBrowserBridge browserBridge;
        private readonly MyAccountSectionHUDController myAccountSectionHUDController;
        private readonly KernelConfig kernelConfig;
        private readonly List<string> loadedNames = new ();

        private CancellationTokenSource cts;
        private Regex nameRegex;

        private UserProfile ownUserProfile => userProfileBridge.GetOwn();
        private string ownUserMainName => SplitUserName(ownUserProfile.userName).mainName;
        private string ownUserNonClaimedHashtag => SplitUserName(ownUserProfile.userName).nonClaimedHashtag;

        public MyProfileController(
            IMyProfileComponentView view,
            DataStore dataStore,
            IUserProfileBridge userProfileBridge,
            INamesService namesService,
            IBrowserBridge browserBridge,
            MyAccountSectionHUDController myAccountSectionHUDController,
            KernelConfig kernelConfig)
        {
            this.view = view;
            this.dataStore = dataStore;
            this.userProfileBridge = userProfileBridge;
            this.namesService = namesService;
            this.browserBridge = browserBridge;
            this.myAccountSectionHUDController = myAccountSectionHUDController;
            this.kernelConfig = kernelConfig;

            dataStore.myAccount.isMyAccountSectionVisible.OnChange += OnMyAccountSectionVisibleChanged;
            view.OnCurrentNameEdited += OnNameEdited;
            view.OnCurrentNameSubmitted += OnNameSubmitted;
            view.OnGoFromClaimedToNonClaimNameClicked += GoFromClaimedToNonClaimName;
            view.OnClaimNameClicked += OnClaimNameRequested;
            view.OnLinkAdded += OnAddLinkRequested;
            view.OnLinkRemoved += OnRemoveLinkRequested;
            ownUserProfile.OnUpdate += OnOwnUserProfileUpdated;

            if (!Configuration.EnvironmentSettings.RUNNING_TESTS)
            {
                kernelConfig.EnsureConfigInitialized().Then(config => OnKernelConfigChanged(config, null));
                kernelConfig.OnChange += OnKernelConfigChanged;
            }
        }

        public void Dispose()
        {
            dataStore.myAccount.isMyAccountSectionVisible.OnChange -= OnMyAccountSectionVisibleChanged;
            view.OnCurrentNameEdited -= OnNameEdited;
            view.OnCurrentNameSubmitted -= OnNameSubmitted;
            view.OnGoFromClaimedToNonClaimNameClicked -= GoFromClaimedToNonClaimName;
            view.OnClaimNameClicked -= OnClaimNameRequested;
            view.OnLinkAdded -= OnAddLinkRequested;
            view.OnLinkRemoved -= OnRemoveLinkRequested;
            ownUserProfile.OnUpdate -= OnOwnUserProfileUpdated;

            if (!Configuration.EnvironmentSettings.RUNNING_TESTS)
                kernelConfig.OnChange -= OnKernelConfigChanged;
        }

        private void OnKernelConfigChanged(KernelConfigModel current, KernelConfigModel _) =>
            nameRegex = new Regex(current.profiles.nameValidRegex);

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
            view.SetCurrentName(ownUserMainName, ownUserNonClaimedHashtag);
            view.SetClaimBannerActive(loadedNames.Count == 0);
            view.SetLoadingActive(false);
        }

        private void OnNameEdited(string newName)
        {
            if (string.IsNullOrEmpty(newName) || newName == ownUserMainName)
            {
                view.SetNonValidNameWarningActive(false);
                return;
            }

            view.SetNonValidNameWarningActive(!IsValidUserName(newName));
        }

        private void OnNameSubmitted(string newName, bool isClaimed)
        {
            if (string.IsNullOrEmpty(newName) || newName == ownUserMainName)
            {
                view.SetNonValidNameWarningActive(false);
                return;
            }

            view.SetNonValidNameWarningActive(!IsValidUserName(newName));
            if (!IsValidUserName(newName))
                return;

            if (isClaimed)
                userProfileBridge.SaveVerifiedName(newName);
            else
                userProfileBridge.SaveUnverifiedName(newName);

            myAccountSectionHUDController.ShowAccountSettingsUpdatedToast();
        }

        private bool IsValidUserName(string newName) =>
            nameRegex == null || nameRegex.IsMatch(newName);

        private void OnOwnUserProfileUpdated(UserProfile userProfile)
        {
            if (userProfile == null)
                return;

            RefreshNamesSectionStatus();
            ShowLinks(userProfile);
        }

        private void ShowLinks(UserProfile userProfile) =>
            view.SetLinks(userProfile.Links.Select(link => (link.title, link.url)).ToList());

        private void GoFromClaimedToNonClaimName() =>
            view.SetClaimedModeAsInput(true, ownUserProfile.hasClaimedName);

        private void OnClaimNameRequested() =>
            browserBridge.OpenUrl(CLAIM_UNIQUE_NAME_URL);

        private (string mainName, string nonClaimedHashtag) SplitUserName(string userName)
        {
            (string mainName, string nonClaimedHashtag) result = (string.Empty, string.Empty);

            string[] splitName = userName.Split('#');

            if (splitName.Length > 0)
                result.mainName = splitName[0];

            if (splitName.Length > 1)
                result.nonClaimedHashtag = splitName[1];

            return result;
        }

        private void OnAddLinkRequested((string title, string url) obj)
        {
            if (ownUserProfile.Links.Count >= 5) return;
            ownUserProfile.AddLink(new UserProfileModel.Link(obj.title, obj.url));
            ShowLinks(ownUserProfile);
        }

        private void OnRemoveLinkRequested((string title, string url) obj)
        {
            ownUserProfile.RemoveLink(obj.title, obj.url);
            ShowLinks(ownUserProfile);
        }
    }
}
