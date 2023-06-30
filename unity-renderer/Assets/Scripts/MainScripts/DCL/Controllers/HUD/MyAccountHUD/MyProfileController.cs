using Cysharp.Threading.Tasks;
using DCL.Browser;
using DCL.Tasks;
using DCL.UserProfiles;
using DCLServices.Lambdas.NamesService;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using UnityEngine;

namespace DCL.MyAccount
{
    public class MyProfileController
    {
        private const string CLAIM_UNIQUE_NAME_URL = "https://builder.decentraland.org/claim-name?utm_source=dcl_explorer";

        private readonly IMyProfileComponentView view;
        private readonly DataStore dataStore;
        private readonly IUserProfileBridge userProfileBridge;
        private readonly INamesService namesService;
        private readonly IBrowserBridge browserBridge;
        private readonly MyAccountSectionHUDController myAccountSectionHUDController;
        private readonly KernelConfig kernelConfig;
        private readonly IMyAccountAnalyticsService myAccountAnalyticsService;
        private readonly IProfileAdditionalInfoValueListProvider countryListProvider;
        private readonly IProfileAdditionalInfoValueListProvider genderListProvider;
        private readonly IProfileAdditionalInfoValueListProvider sexualOrientationProvider;
        private readonly IProfileAdditionalInfoValueListProvider employmentStatusProvider;
        private readonly IProfileAdditionalInfoValueListProvider relationshipStatusProvider;
        private readonly IProfileAdditionalInfoValueListProvider languageListProvider;
        private readonly IProfileAdditionalInfoValueListProvider pronounListProvider;
        private readonly List<string> loadedNames = new ();

        private CancellationTokenSource saveLinkCancellationToken = new ();
        private CancellationTokenSource saveNameCancellationToken = new ();
        private CancellationTokenSource saveDescriptionCancellationToken = new ();
        private CancellationTokenSource additionalInfoCancellationToken = new ();
        private CancellationTokenSource refreshContentCancellationToken = new ();
        private CancellationTokenSource lifeTimeCancellationToken;
        private Regex nameRegex;

        private UserProfile ownUserProfile => userProfileBridge.GetOwn();
        private string ownUserMainName => SplitUserName(ownUserProfile).mainName;
        private string ownUserNonClaimedHashtag => SplitUserName(ownUserProfile).nonClaimedHashtag;

        public MyProfileController(
            IMyProfileComponentView view,
            DataStore dataStore,
            IUserProfileBridge userProfileBridge,
            INamesService namesService,
            IBrowserBridge browserBridge,
            MyAccountSectionHUDController myAccountSectionHUDController,
            KernelConfig kernelConfig,
            IMyAccountAnalyticsService myAccountAnalyticsService,
            IProfileAdditionalInfoValueListProvider countryListProvider,
            IProfileAdditionalInfoValueListProvider genderListProvider,
            IProfileAdditionalInfoValueListProvider sexualOrientationProvider,
            IProfileAdditionalInfoValueListProvider employmentStatusProvider,
            IProfileAdditionalInfoValueListProvider relationshipStatusProvider,
            IProfileAdditionalInfoValueListProvider languageListProvider,
            IProfileAdditionalInfoValueListProvider pronounListProvider)
        {
            this.view = view;
            this.dataStore = dataStore;
            this.userProfileBridge = userProfileBridge;
            this.namesService = namesService;
            this.browserBridge = browserBridge;
            this.myAccountSectionHUDController = myAccountSectionHUDController;
            this.kernelConfig = kernelConfig;
            this.myAccountAnalyticsService = myAccountAnalyticsService;
            this.countryListProvider = countryListProvider;
            this.genderListProvider = genderListProvider;
            this.sexualOrientationProvider = sexualOrientationProvider;
            this.employmentStatusProvider = employmentStatusProvider;
            this.relationshipStatusProvider = relationshipStatusProvider;
            this.languageListProvider = languageListProvider;
            this.pronounListProvider = pronounListProvider;

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

            saveLinkCancellationToken.SafeCancelAndDispose();
            saveNameCancellationToken.SafeCancelAndDispose();
            saveDescriptionCancellationToken.SafeCancelAndDispose();
            additionalInfoCancellationToken.SafeCancelAndDispose();
            refreshContentCancellationToken.SafeCancelAndDispose();
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
            lifeTimeCancellationToken = lifeTimeCancellationToken.SafeRestart();
            view.SetLoadingActive(true);

            ShowAboutDescription(ownUserProfile);
            ShowLinks(ownUserProfile);
            UserProfile userProfile = ownUserProfile;
            ShowAdditionalInfoOptions(userProfile, userProfile.AdditionalInfo);
            ShowAdditionalInfoValues(userProfile.AdditionalInfo);

            LoadAndShowOwnedNamesAsync(lifeTimeCancellationToken.Token)
               .ContinueWith(() =>
                {
                    view.SetLoadingActive(false);
                    refreshContentCancellationToken = refreshContentCancellationToken.SafeRestart();
                    RefreshContentLayout(refreshContentCancellationToken.Token).Forget();
                })
               .Forget();
        }

        private async UniTask RefreshContentLayout(CancellationToken ct)
        {
            await UniTask.DelayFrame(1, cancellationToken: ct);
            view.RefreshContentLayout();
        }

        private void CloseSection() =>
            lifeTimeCancellationToken.SafeCancelAndDispose();

        private async UniTask LoadAndShowOwnedNamesAsync(CancellationToken ct)
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
            view.SetClaimedNameModeAsInput(!ownUserProfile.hasClaimedName);
            view.SetCurrentName(ownUserMainName, ownUserNonClaimedHashtag);
            view.SetClaimNameBannerActive(loadedNames.Count == 0);
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
                myAccountAnalyticsService.SendPlayerSwapNameAnalytic(isClaimed, loadedNames.Count);
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
                myAccountAnalyticsService.SendProfileInfoEditAnalytic(newDesc.Length);
            }

            saveDescriptionCancellationToken = saveDescriptionCancellationToken.SafeRestart();
            SaveDescriptionAsync(newDesc, saveDescriptionCancellationToken.Token).Forget();
        }

        private void ShowAboutDescription(UserProfile userProfile)
        {
            view.SetAboutDescription(userProfile.description);
            view.SetAboutEnabled(!userProfile.isGuest);
        }

        private bool IsValidUserName(string newName) =>
            nameRegex == null || nameRegex.IsMatch(newName);

        private void OnOwnUserProfileUpdated(UserProfile userProfile)
        {
            if (userProfile == null)
                return;

            RefreshNamesSectionStatus();
            ShowAboutDescription(userProfile);
            ShowLinks(userProfile);
            ShowAdditionalInfoOptions(userProfile, userProfile.AdditionalInfo);
            ShowAdditionalInfoValues(userProfile.AdditionalInfo);
        }

        private void ShowLinks(UserProfile userProfile)
        {
            view.SetLinks(userProfile.Links ?? new List<UserProfileModel.Link>(0));
            view.EnableOrDisableAddLinksOption(userProfile.Links == null || userProfile.Links?.Count < 5);
            view.SetLinksEnabled(!userProfile.isGuest);
        }

        private void GoFromClaimedToNonClaimName() =>
            view.SetClaimedNameModeAsInput(true, ownUserProfile.hasClaimedName);

        private void OnClaimNameRequested()
        {
            browserBridge.OpenUrl(CLAIM_UNIQUE_NAME_URL);
            myAccountAnalyticsService.SendPlayerOpenClaimNameAnalytic();
        }

        private (string mainName, string nonClaimedHashtag) SplitUserName(UserProfile userProfile)
        {
            (string mainName, string nonClaimedHashtag) result = (
                string.Empty,
                userProfile.ethAddress != null ? userProfile.ethAddress.Length >= 4 ? userProfile.ethAddress.Substring(userProfile.ethAddress.Length - 4) : string.Empty : string.Empty);

            string[] splitName = userProfile.userName.Split('#');

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
                List<UserProfileModel.Link> links = new ((IEnumerable<UserProfileModel.Link>)ownUserProfile.Links
                                                         ?? Array.Empty<UserProfileModel.Link>())
                {
                    new UserProfileModel.Link(title, url),
                };

                try
                {
                    await userProfileBridge.SaveLinks(links, cancellationToken);

                    view.ClearLinkInput();
                    myAccountSectionHUDController.ShowAccountSettingsUpdatedToast();
                    myAccountAnalyticsService.SendProfileLinkAddAnalytic(title, url);
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
                    myAccountAnalyticsService.SendProfileLinkRemoveAnalytic(title, url);
                }
                catch (OperationCanceledException) { }
                catch (Exception e) { Debug.LogException(e); }
            }

            saveLinkCancellationToken = saveLinkCancellationToken.SafeRestart();
            RemoveAndSaveLinkAsync(obj.title, obj.url, saveLinkCancellationToken.Token).Forget();
        }

        private void ShowAdditionalInfoOptions(UserProfile userProfile, AdditionalInfo additionalInfo)
        {
            Dictionary<string, AdditionalInfoOptionsModel.Option> options = new ();

            options.Add("Country", new AdditionalInfoOptionsModel.Option
            {
                IsAvailable = string.IsNullOrEmpty(additionalInfo.Country),
                Name = "Country",
                InputType = AdditionalInfoOptionsModel.InputType.StrictValueList,
                Values = GetCountryList(),
                OnValueSubmitted = country =>
                {
                    SaveAdditionalInfo(new AdditionalInfo(additionalInfo)
                    {
                        Country = country,
                    });

                    myAccountAnalyticsService.SendProfileInfoAdditionalInfoAddAnalytic("Country", country);
                },
                OnRemoved = () =>
                {
                    SaveAdditionalInfo(new AdditionalInfo(additionalInfo)
                    {
                        Country = null,
                    });

                    myAccountAnalyticsService.SendProfileInfoAdditionalInfoRemoveAnalytic("Country");
                },
            });

            options.Add("Gender", new AdditionalInfoOptionsModel.Option
            {
                IsAvailable = string.IsNullOrEmpty(additionalInfo.Gender),
                Name = "Gender",
                InputType = AdditionalInfoOptionsModel.InputType.StrictValueList,
                Values = GetGenderList(),
                OnValueSubmitted = gender =>
                {
                    SaveAdditionalInfo(new AdditionalInfo(additionalInfo)
                    {
                        Gender = gender,
                    });

                    myAccountAnalyticsService.SendProfileInfoAdditionalInfoAddAnalytic("Gender", gender);
                },
                OnRemoved = () =>
                {
                    SaveAdditionalInfo(new AdditionalInfo(additionalInfo)
                    {
                        Gender = null,
                    });

                    myAccountAnalyticsService.SendProfileInfoAdditionalInfoRemoveAnalytic("Gender");
                },
            });

            options.Add("Pronouns", new AdditionalInfoOptionsModel.Option
            {
                IsAvailable = string.IsNullOrEmpty(additionalInfo.Pronouns),
                Name = "Pronouns",
                InputType = AdditionalInfoOptionsModel.InputType.StrictValueList,
                Values = GetPronounList(),
                OnValueSubmitted = pronouns =>
                {
                    SaveAdditionalInfo(new AdditionalInfo(additionalInfo)
                    {
                        Pronouns = pronouns,
                    });

                    myAccountAnalyticsService.SendProfileInfoAdditionalInfoAddAnalytic("Pronouns", pronouns);
                },
                OnRemoved = () =>
                {
                    SaveAdditionalInfo(new AdditionalInfo(additionalInfo)
                    {
                        Pronouns = null,
                    });

                    myAccountAnalyticsService.SendProfileInfoAdditionalInfoRemoveAnalytic("Pronouns");
                },
            });

            options.Add("Relationship Status", new AdditionalInfoOptionsModel.Option
            {
                IsAvailable = string.IsNullOrEmpty(additionalInfo.RelationshipStatus),
                Name = "Relationship Status",
                InputType = AdditionalInfoOptionsModel.InputType.StrictValueList,
                Values = GetRelationshipStatusList(),
                OnValueSubmitted = relationshipStatus =>
                {
                    SaveAdditionalInfo(new AdditionalInfo(additionalInfo)
                    {
                        RelationshipStatus = relationshipStatus,
                    });

                    myAccountAnalyticsService.SendProfileInfoAdditionalInfoAddAnalytic("Relationship Status", relationshipStatus);
                },
                OnRemoved = () =>
                {
                    SaveAdditionalInfo(new AdditionalInfo(additionalInfo)
                    {
                        RelationshipStatus = null,
                    });

                    myAccountAnalyticsService.SendProfileInfoAdditionalInfoRemoveAnalytic("Relationship Status");
                },
            });

            options.Add("Sexual Orientation", new AdditionalInfoOptionsModel.Option
            {
                IsAvailable = string.IsNullOrEmpty(additionalInfo.SexualOrientation),
                Name = "Sexual Orientation",
                InputType = AdditionalInfoOptionsModel.InputType.StrictValueList,
                Values = GetSexualOrientationList(),
                OnValueSubmitted = sexualOrientation =>
                {
                    SaveAdditionalInfo(new AdditionalInfo(additionalInfo)
                    {
                        SexualOrientation = sexualOrientation,
                    });

                    myAccountAnalyticsService.SendProfileInfoAdditionalInfoAddAnalytic("Sexual Orientation", sexualOrientation);
                },
                OnRemoved = () =>
                {
                    SaveAdditionalInfo(new AdditionalInfo(additionalInfo)
                    {
                        SexualOrientation = null,
                    });

                    myAccountAnalyticsService.SendProfileInfoAdditionalInfoRemoveAnalytic("Sexual Orientation");
                },
            });

            options.Add("Language", new AdditionalInfoOptionsModel.Option
            {
                IsAvailable = string.IsNullOrEmpty(additionalInfo.Language),
                Name = "Language",
                InputType = AdditionalInfoOptionsModel.InputType.StrictValueList,
                Values = GetLanguageList(),
                OnValueSubmitted = language =>
                {
                    SaveAdditionalInfo(new AdditionalInfo(additionalInfo)
                    {
                        Language = language,
                    });

                    myAccountAnalyticsService.SendProfileInfoAdditionalInfoAddAnalytic("Language", language);
                },
                OnRemoved = () =>
                {
                    SaveAdditionalInfo(new AdditionalInfo(additionalInfo)
                    {
                        Language = null,
                    });

                    myAccountAnalyticsService.SendProfileInfoAdditionalInfoRemoveAnalytic("Language");
                },
            });

            options.Add("Employment Status", new AdditionalInfoOptionsModel.Option
            {
                IsAvailable = string.IsNullOrEmpty(additionalInfo.EmploymentStatus),
                Name = "Employment Status",
                InputType = AdditionalInfoOptionsModel.InputType.StrictValueList,
                Values = GetEmploymentStatusList(),
                OnValueSubmitted = employmentStatus =>
                {
                    SaveAdditionalInfo(new AdditionalInfo(additionalInfo)
                    {
                        EmploymentStatus = employmentStatus,
                    });

                    myAccountAnalyticsService.SendProfileInfoAdditionalInfoAddAnalytic("Employment Status", employmentStatus);
                },
                OnRemoved = () =>
                {
                    SaveAdditionalInfo(new AdditionalInfo(additionalInfo)
                    {
                        EmploymentStatus = null,
                    });

                    myAccountAnalyticsService.SendProfileInfoAdditionalInfoRemoveAnalytic("Employment Status");
                },
            });

            options.Add("Profession", new AdditionalInfoOptionsModel.Option
            {
                IsAvailable = string.IsNullOrEmpty(additionalInfo.Profession),
                Name = "Profession",
                InputType = AdditionalInfoOptionsModel.InputType.FreeFormText,
                OnValueSubmitted = profession =>
                {
                    SaveAdditionalInfo(new AdditionalInfo(additionalInfo)
                    {
                        Profession = profession,
                    });

                    myAccountAnalyticsService.SendProfileInfoAdditionalInfoAddAnalytic("Profession", profession);
                },
                OnRemoved = () =>
                {
                    SaveAdditionalInfo(new AdditionalInfo(additionalInfo)
                    {
                        Profession = null,
                    });

                    myAccountAnalyticsService.SendProfileInfoAdditionalInfoRemoveAnalytic("Profession");
                },
            });

            options.Add("Birth Date", new AdditionalInfoOptionsModel.Option
            {
                IsAvailable = additionalInfo.BirthDate == null,
                Name = "Birth Date",
                InputType = AdditionalInfoOptionsModel.InputType.Date,
                DateFormat = "dd/MM/yyyy",
                OnValueSubmitted = birthDate =>
                {
                    var dateTime = DateTime.ParseExact(birthDate, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal);

                    SaveAdditionalInfo(new AdditionalInfo(additionalInfo)
                    {
                        BirthDate = dateTime,
                    });

                    myAccountAnalyticsService.SendProfileInfoAdditionalInfoAddAnalytic("Birth Date", birthDate);
                },
                OnRemoved = () =>
                {
                    SaveAdditionalInfo(new AdditionalInfo(additionalInfo)
                    {
                        BirthDate = null,
                    });

                    myAccountAnalyticsService.SendProfileInfoAdditionalInfoRemoveAnalytic("Birth Date");
                },
            });

            options.Add("Real Name", new AdditionalInfoOptionsModel.Option
            {
                IsAvailable = string.IsNullOrEmpty(additionalInfo.RealName),
                Name = "Real Name",
                InputType = AdditionalInfoOptionsModel.InputType.FreeFormText,
                OnValueSubmitted = realName =>
                {
                    SaveAdditionalInfo(new AdditionalInfo(additionalInfo)
                    {
                        RealName = realName,
                    });

                    myAccountAnalyticsService.SendProfileInfoAdditionalInfoAddAnalytic("Real Name", realName);
                },
                OnRemoved = () =>
                {
                    SaveAdditionalInfo(new AdditionalInfo(additionalInfo)
                    {
                        RealName = null,
                    });

                    myAccountAnalyticsService.SendProfileInfoAdditionalInfoRemoveAnalytic("Real Name");
                },
            });

            options.Add("Hobbies", new AdditionalInfoOptionsModel.Option
            {
                IsAvailable = string.IsNullOrEmpty(additionalInfo.Hobbies),
                Name = "Hobbies",
                InputType = AdditionalInfoOptionsModel.InputType.FreeFormText,
                OnValueSubmitted = hobbies =>
                {
                    SaveAdditionalInfo(new AdditionalInfo(additionalInfo)
                    {
                        Hobbies = hobbies,
                    });

                    myAccountAnalyticsService.SendProfileInfoAdditionalInfoAddAnalytic("Hobbies", hobbies);
                },
                OnRemoved = () =>
                {
                    SaveAdditionalInfo(new AdditionalInfo(additionalInfo)
                    {
                        Hobbies = null,
                    });

                    myAccountAnalyticsService.SendProfileInfoAdditionalInfoRemoveAnalytic("Hobbies");
                },
            });

            view.SetAdditionalInfoOptions(new AdditionalInfoOptionsModel
            {
                Options = options,
            });

            view.SetAdditionalInfoEnabled(!userProfile.isGuest);
        }

        private string[] GetEmploymentStatusList() =>
            employmentStatusProvider.Provide();

        private string[] GetLanguageList() =>
            languageListProvider.Provide();

        private string[] GetSexualOrientationList() =>
            sexualOrientationProvider.Provide();

        private string[] GetRelationshipStatusList() =>
            relationshipStatusProvider.Provide();

        private string[] GetPronounList() =>
            pronounListProvider.Provide();

        private string[] GetGenderList() =>
            genderListProvider.Provide();

        private string[] GetCountryList() =>
            countryListProvider.Provide();

        private void SaveAdditionalInfo(AdditionalInfo additionalInfo)
        {
            async UniTaskVoid SaveAdditionalInfoAsync(AdditionalInfo additionalInfo, CancellationToken cancellationToken)
            {
                await userProfileBridge.SaveAdditionalInfo(additionalInfo, cancellationToken);

                myAccountSectionHUDController.ShowAccountSettingsUpdatedToast();
            }

            additionalInfoCancellationToken = additionalInfoCancellationToken.SafeRestart();

            SaveAdditionalInfoAsync(additionalInfo,
                    additionalInfoCancellationToken.Token)
               .Forget();
        }

        private void ShowAdditionalInfoValues(AdditionalInfo additionalInfo)
        {
            Dictionary<string, (string title, string value)> values = new ();

            if (!string.IsNullOrEmpty(additionalInfo.Country))
                values["Country"] = ("Country", additionalInfo.Country);

            if (!string.IsNullOrEmpty(additionalInfo.Gender))
                values["Gender"] = ("Gender", additionalInfo.Gender);

            if (!string.IsNullOrEmpty(additionalInfo.Pronouns))
                values["Pronouns"] = ("Pronouns", additionalInfo.Pronouns);

            if (!string.IsNullOrEmpty(additionalInfo.RelationshipStatus))
                values["Relationship Status"] = ("Relationship Status", additionalInfo.RelationshipStatus);

            if (!string.IsNullOrEmpty(additionalInfo.SexualOrientation))
                values["Sexual Orientation"] = ("Sexual Orientation", additionalInfo.SexualOrientation);

            if (!string.IsNullOrEmpty(additionalInfo.Language))
                values["Language"] = ("Language", additionalInfo.Language);

            if (!string.IsNullOrEmpty(additionalInfo.EmploymentStatus))
                values["Employment Status"] = ("Employment Status", additionalInfo.EmploymentStatus);

            if (!string.IsNullOrEmpty(additionalInfo.Profession))
                values["Profession"] = ("Profession", additionalInfo.Profession);

            if (additionalInfo.BirthDate != null)
                values["Birth Date"] = ("Birth Date", additionalInfo.BirthDate?.ToString("dd/MM/yyyy"));

            if (!string.IsNullOrEmpty(additionalInfo.RealName))
                values["Real Name"] = ("Real Name", additionalInfo.RealName);

            if (!string.IsNullOrEmpty(additionalInfo.Hobbies))
                values["Hobbies"] = ("Hobbies", additionalInfo.Hobbies);

            view.SetAdditionalInfoValues(values);
        }
    }
}
