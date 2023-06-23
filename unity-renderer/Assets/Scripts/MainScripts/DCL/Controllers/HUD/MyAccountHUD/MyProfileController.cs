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

        private CancellationTokenSource saveLinkCancellationToken = new ();
        private CancellationTokenSource saveNameCancellationToken = new ();
        private CancellationTokenSource saveDescriptionCancellationToken = new ();
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
            view.OnAboutDescriptionSubmitted += OnAboutDescriptionSubmitted;
            view.OnLinkAdded += OnAddLinkRequested;
            view.OnLinkRemoved += OnRemoveLinkRequested;
            ownUserProfile.OnUpdate += OnOwnUserProfileUpdated;

            kernelConfig.EnsureConfigInitialized()
                        .Then(config => OnKernelConfigChanged(config, null));
            kernelConfig.OnChange += OnKernelConfigChanged;

            userProfileBridge.GetOwn().OnUpdate += OnOwnUserProfileUpdate;
            if (!string.IsNullOrEmpty(userProfileBridge.GetOwn().userId))
                OnOwnUserProfileUpdate(userProfileBridge.GetOwn());
        }

        public void Dispose()
        {
            dataStore.myAccount.isMyAccountSectionVisible.OnChange -= OnMyAccountSectionVisibleChanged;
            view.OnCurrentNameEdited -= OnNameEdited;
            view.OnCurrentNameSubmitted -= OnNameSubmitted;
            view.OnGoFromClaimedToNonClaimNameClicked -= GoFromClaimedToNonClaimName;
            view.OnClaimNameClicked -= OnClaimNameRequested;
            view.OnAboutDescriptionSubmitted -= OnAboutDescriptionSubmitted;
            view.OnLinkAdded -= OnAddLinkRequested;
            view.OnLinkRemoved -= OnRemoveLinkRequested;
            ownUserProfile.OnUpdate -= OnOwnUserProfileUpdated;
            kernelConfig.OnChange -= OnKernelConfigChanged;
            userProfileBridge.GetOwn().OnUpdate -= OnOwnUserProfileUpdate;

            saveLinkCancellationToken.SafeCancelAndDispose();
            saveNameCancellationToken.SafeCancelAndDispose();
            saveDescriptionCancellationToken.SafeCancelAndDispose();
        }

        private void OnKernelConfigChanged(KernelConfigModel current, KernelConfigModel _) =>
            nameRegex = new Regex(current.profiles.nameValidRegex);

        private void OnOwnUserProfileUpdate(UserProfile userProfile)
        {
            if (userProfile == null)
                return;

            view.SetAboutEnabled(!userProfile.isGuest);
            view.SetLinksEnabled(!userProfile.isGuest);
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
            view.SetLoadingActive(true);

            LoadOwnedNamesAsync(cts.Token)
               .ContinueWith(() => view.SetLoadingActive(false))
               .Forget();

            LoadAboutDescription();
            ShowLinks(ownUserProfile);
        }

        private void CloseSection() =>
            cts.SafeCancelAndDispose();

        private async UniTask LoadOwnedNamesAsync(CancellationToken ct)
        {
            try
            {
                loadedNames.Clear();

                var names = await namesService.RequestOwnedNamesAsync(
                    ownUserProfile.userId,
                    1,
                    int.MaxValue,
                    true,
                    ct);

                var optionsToLoad = new List<string>();

                if (names.names is { Count: > 0 })
                {
                    optionsToLoad.AddRange(names.names
                                                .OrderBy(x => x.Name)
                                                .Select(x => x.Name));
                    view.SetClaimedNameDropdownOptions(optionsToLoad);
                    loadedNames.AddRange(optionsToLoad);
                }
                else
                    view.SetClaimedNameDropdownOptions(optionsToLoad);
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

            bool isValidUserName = IsValidUserName(newName);
            view.SetNonValidNameWarningActive(!isValidUserName);

            if (!isValidUserName) return;

            async UniTaskVoid SaveNameAsync(string newName, bool isClaimed, CancellationToken cancellationToken)
            {
                if (isClaimed)
                    await userProfileBridge.SaveVerifiedName(newName, cancellationToken);
                else
                    await userProfileBridge.SaveUnverifiedName(newName, cancellationToken);

                myAccountSectionHUDController.ShowAccountSettingsUpdatedToast();
            }

            saveNameCancellationToken = saveNameCancellationToken.SafeRestart();
            SaveNameAsync(newName, isClaimed, saveNameCancellationToken.Token).Forget();
        }

        private void OnAboutDescriptionSubmitted(string newDesc)
        {
            if (newDesc == ownUserProfile.description)
                return;

            async UniTaskVoid SaveDescriptionAsync(string newDesc, CancellationToken cancellationToken)
            {
                await userProfileBridge.SaveDescription(newDesc, cancellationToken);

                myAccountSectionHUDController.ShowAccountSettingsUpdatedToast();
            }

            saveDescriptionCancellationToken = saveDescriptionCancellationToken.SafeRestart();
            SaveDescriptionAsync(newDesc, saveDescriptionCancellationToken.Token).Forget();
        }

        private void LoadAboutDescription() =>
            view.SetAboutDescription(ownUserProfile.description);

        private bool IsValidUserName(string newName) =>
            nameRegex == null || nameRegex.IsMatch(newName);

        private void OnOwnUserProfileUpdated(UserProfile userProfile)
        {
            if (userProfile == null)
                return;

            RefreshNamesSectionStatus();
            LoadAboutDescription();
            ShowLinks(userProfile);
        }

        private void ShowLinks(UserProfile userProfile)
        {
            view.SetLinks(userProfile.Links?.Select(link => (link.title, link.url)).ToList()
                          ?? new List<(string title, string url)>());

            view.EnableOrDisableAddLinksOption(userProfile.Links == null || userProfile.Links?.Count < 5);
        }

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
            if (ownUserProfile.Links?.Count >= 5) return;

            async UniTaskVoid AddAndSaveLinkAsync(string title, string url, CancellationToken cancellationToken)
            {
                List<UserProfileModel.Link> links = new ((IEnumerable<UserProfileModel.Link>) ownUserProfile.Links
                                                         ?? Array.Empty<UserProfileModel.Link>())
                {
                    new UserProfileModel.Link(title, url),
                };

                try
                {
                    UserProfile profile = await userProfileBridge.SaveLinks(links, cancellationToken);

                    ShowLinks(profile);
                    view.ClearLinkInput();
                    myAccountSectionHUDController.ShowAccountSettingsUpdatedToast();
                }
                catch (OperationCanceledException) { }
                catch (Exception e) { Debug.LogException(e); }
            }

            saveLinkCancellationToken = saveLinkCancellationToken.SafeRestart();
            AddAndSaveLinkAsync(obj.title, obj.url, saveLinkCancellationToken.Token).Forget();
        }

        private void OnRemoveLinkRequested((string title, string url) obj)
        {
            async UniTaskVoid RemoveAndSaveLinkAsync(string title, string url, CancellationToken cancellationToken)
            {
                List<UserProfileModel.Link> links = new (ownUserProfile.Links);
                links.RemoveAll(link => link.title == title && link.url == url);

                try
                {
                    UserProfile profile = await userProfileBridge.SaveLinks(links, cancellationToken);

                    ShowLinks(profile);
                    myAccountSectionHUDController.ShowAccountSettingsUpdatedToast();
                }
                catch (OperationCanceledException) { }
                catch (Exception e) { Debug.LogException(e); }
            }

            saveLinkCancellationToken = saveLinkCancellationToken.SafeRestart();
            RemoveAndSaveLinkAsync(obj.title, obj.url, saveLinkCancellationToken.Token).Forget();
        }
    }
}
