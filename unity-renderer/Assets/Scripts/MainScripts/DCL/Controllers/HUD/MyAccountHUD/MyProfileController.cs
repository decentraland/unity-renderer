using Cysharp.Threading.Tasks;
using DCL.Browser;
using DCL.Tasks;
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
        private CancellationTokenSource lifeTimeCancellationToken;
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

            LoadAndShowOwnedNamesAsync(lifeTimeCancellationToken.Token)
               .ContinueWith(() => view.SetLoadingActive(false))
               .Forget();

            ShowAboutDescription(ownUserProfile);
            ShowLinks(ownUserProfile);
            ShowAdditionalInfoOptions(ownUserProfile);
            ShowAdditionalInfoValues(ownUserProfile);
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
            ShowAdditionalInfoOptions(userProfile);
            ShowAdditionalInfoValues(userProfile);
        }

        private void ShowLinks(UserProfile userProfile)
        {
            view.SetLinks(userProfile.Links?.Select(link => (link.title, link.url)).ToList()
                          ?? new List<(string title, string url)>());

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

        private void ShowAdditionalInfoOptions(UserProfile userProfile)
        {
            Dictionary<string, AdditionalInfoOptionsModel.Option> options = new ();

            options.Add("Country", new AdditionalInfoOptionsModel.Option
            {
                IsAvailable = string.IsNullOrEmpty(userProfile.Country),
                Name = "Country",
                InputType = AdditionalInfoOptionsModel.InputType.StrictValueList,
                Values = GetCountryList(),
                OnValueSubmitted = country =>
                {
                    SaveAdditionalInfo(country, userProfile.Gender, userProfile.Pronouns, userProfile.RelationshipStatus,
                        userProfile.SexualOrientation, userProfile.Language, userProfile.Profession,
                        userProfile.BirthDate, userProfile.RealName, userProfile.Hobbies, userProfile.EmploymentStatus);

                    myAccountAnalyticsService.SendProfileInfoAdditionalInfoAddAnalytic("Country", country);
                },
                OnRemoved = () =>
                {
                    SaveAdditionalInfo(null, userProfile.Gender, userProfile.Pronouns, userProfile.RelationshipStatus,
                        userProfile.SexualOrientation, userProfile.Language, userProfile.Profession,
                        userProfile.BirthDate, userProfile.RealName, userProfile.Hobbies, userProfile.EmploymentStatus);

                    myAccountAnalyticsService.SendProfileInfoAdditionalInfoRemoveAnalytic("Country");
                },
            });

            options.Add("Gender", new AdditionalInfoOptionsModel.Option
            {
                IsAvailable = string.IsNullOrEmpty(userProfile.Gender),
                Name = "Gender",
                InputType = AdditionalInfoOptionsModel.InputType.StrictValueList,
                Values = GetGenderList(),
                OnValueSubmitted = gender =>
                {
                    SaveAdditionalInfo(userProfile.Country, gender, userProfile.Pronouns, userProfile.RelationshipStatus,
                        userProfile.SexualOrientation, userProfile.Language, userProfile.Profession,
                        userProfile.BirthDate, userProfile.RealName, userProfile.Hobbies, userProfile.EmploymentStatus);

                    myAccountAnalyticsService.SendProfileInfoAdditionalInfoAddAnalytic("Gender", gender);
                },
                OnRemoved = () =>
                {
                    SaveAdditionalInfo(userProfile.Country, null, userProfile.Pronouns, userProfile.RelationshipStatus,
                        userProfile.SexualOrientation, userProfile.Language, userProfile.Profession,
                        userProfile.BirthDate, userProfile.RealName, userProfile.Hobbies, userProfile.EmploymentStatus);

                    myAccountAnalyticsService.SendProfileInfoAdditionalInfoRemoveAnalytic("Gender");
                },
            });

            options.Add("Pronouns", new AdditionalInfoOptionsModel.Option
            {
                IsAvailable = string.IsNullOrEmpty(userProfile.Pronouns),
                Name = "Pronouns",
                InputType = AdditionalInfoOptionsModel.InputType.StrictValueList,
                Values = GetPronounList(),
                OnValueSubmitted = pronouns =>
                {
                    SaveAdditionalInfo(userProfile.Country, userProfile.Gender, pronouns, userProfile.RelationshipStatus,
                        userProfile.SexualOrientation, userProfile.Language, userProfile.Profession,
                        userProfile.BirthDate, userProfile.RealName, userProfile.Hobbies, userProfile.EmploymentStatus);

                    myAccountAnalyticsService.SendProfileInfoAdditionalInfoAddAnalytic("Pronouns", pronouns);
                },
                OnRemoved = () =>
                {
                    SaveAdditionalInfo(userProfile.Country, userProfile.Gender, null, userProfile.RelationshipStatus,
                        userProfile.SexualOrientation, userProfile.Language, userProfile.Profession,
                        userProfile.BirthDate, userProfile.RealName, userProfile.Hobbies, userProfile.EmploymentStatus);

                    myAccountAnalyticsService.SendProfileInfoAdditionalInfoRemoveAnalytic("Pronouns");
                },
            });

            options.Add("Relationship Status", new AdditionalInfoOptionsModel.Option
            {
                IsAvailable = string.IsNullOrEmpty(userProfile.RelationshipStatus),
                Name = "Relationship Status",
                InputType = AdditionalInfoOptionsModel.InputType.StrictValueList,
                Values = GetRelationshipStatusList(),
                OnValueSubmitted = relationshipStatus =>
                {
                    SaveAdditionalInfo(userProfile.Country, userProfile.Gender, userProfile.Pronouns, relationshipStatus,
                        userProfile.SexualOrientation, userProfile.Language, userProfile.Profession,
                        userProfile.BirthDate, userProfile.RealName, userProfile.Hobbies, userProfile.EmploymentStatus);

                    myAccountAnalyticsService.SendProfileInfoAdditionalInfoAddAnalytic("Relationship Status", relationshipStatus);
                },
                OnRemoved = () =>
                {
                    SaveAdditionalInfo(userProfile.Country, userProfile.Gender, userProfile.Pronouns, null,
                        userProfile.SexualOrientation, userProfile.Language, userProfile.Profession,
                        userProfile.BirthDate, userProfile.RealName, userProfile.Hobbies, userProfile.EmploymentStatus);

                    myAccountAnalyticsService.SendProfileInfoAdditionalInfoRemoveAnalytic("Relationship Status");
                },
            });

            options.Add("Sexual Orientation", new AdditionalInfoOptionsModel.Option
            {
                IsAvailable = string.IsNullOrEmpty(userProfile.SexualOrientation),
                Name = "Sexual Orientation",
                InputType = AdditionalInfoOptionsModel.InputType.StrictValueList,
                Values = GetSexualOrientationList(),
                OnValueSubmitted = sexualOrientation =>
                {
                    SaveAdditionalInfo(userProfile.Country, userProfile.Gender, userProfile.Pronouns, userProfile.RelationshipStatus,
                        sexualOrientation, userProfile.Language, userProfile.Profession,
                        userProfile.BirthDate, userProfile.RealName, userProfile.Hobbies, userProfile.EmploymentStatus);

                    myAccountAnalyticsService.SendProfileInfoAdditionalInfoAddAnalytic("Sexual Orientation", sexualOrientation);
                },
                OnRemoved = () =>
                {
                    SaveAdditionalInfo(userProfile.Country, userProfile.Gender, userProfile.Pronouns, userProfile.RelationshipStatus,
                        null, userProfile.Language, userProfile.Profession,
                        userProfile.BirthDate, userProfile.RealName, userProfile.Hobbies, userProfile.EmploymentStatus);

                    myAccountAnalyticsService.SendProfileInfoAdditionalInfoRemoveAnalytic("Sexual Orientation");
                },
            });

            options.Add("Language", new AdditionalInfoOptionsModel.Option
            {
                IsAvailable = string.IsNullOrEmpty(userProfile.Language),
                Name = "Language",
                InputType = AdditionalInfoOptionsModel.InputType.StrictValueList,
                Values = GetLanguageList(),
                OnValueSubmitted = language =>
                {
                    SaveAdditionalInfo(userProfile.Country, userProfile.Gender, userProfile.Pronouns, userProfile.RelationshipStatus,
                        userProfile.SexualOrientation, language, userProfile.Profession,
                        userProfile.BirthDate, userProfile.RealName, userProfile.Hobbies, userProfile.EmploymentStatus);

                    myAccountAnalyticsService.SendProfileInfoAdditionalInfoAddAnalytic("Language", language);
                },
                OnRemoved = () =>
                {
                    SaveAdditionalInfo(userProfile.Country, userProfile.Gender, userProfile.Pronouns, userProfile.RelationshipStatus,
                        userProfile.SexualOrientation, null, userProfile.Profession,
                        userProfile.BirthDate, userProfile.RealName, userProfile.Hobbies, userProfile.EmploymentStatus);

                    myAccountAnalyticsService.SendProfileInfoAdditionalInfoRemoveAnalytic("Language");
                },
            });

            options.Add("Employment Status", new AdditionalInfoOptionsModel.Option
            {
                IsAvailable = string.IsNullOrEmpty(userProfile.EmploymentStatus),
                Name = "Employment Status",
                InputType = AdditionalInfoOptionsModel.InputType.StrictValueList,
                Values = GetEmploymentStatusList(),
                OnValueSubmitted = employmentStatus =>
                {
                    SaveAdditionalInfo(userProfile.Country, userProfile.Gender, userProfile.Pronouns, userProfile.RelationshipStatus,
                        userProfile.SexualOrientation, userProfile.Language, userProfile.Profession,
                        userProfile.BirthDate, userProfile.RealName, userProfile.Hobbies, employmentStatus);

                    myAccountAnalyticsService.SendProfileInfoAdditionalInfoAddAnalytic("Employment Status", employmentStatus);
                },
                OnRemoved = () =>
                {
                    SaveAdditionalInfo(userProfile.Country, userProfile.Gender, userProfile.Pronouns, userProfile.RelationshipStatus,
                        userProfile.SexualOrientation, userProfile.Language, userProfile.Profession,
                        userProfile.BirthDate, userProfile.RealName, userProfile.Hobbies, null);

                    myAccountAnalyticsService.SendProfileInfoAdditionalInfoRemoveAnalytic("Employment Status");
                },
            });

            options.Add("Profession", new AdditionalInfoOptionsModel.Option
            {
                IsAvailable = string.IsNullOrEmpty(userProfile.Profession),
                Name = "Profession",
                InputType = AdditionalInfoOptionsModel.InputType.FreeFormText,
                OnValueSubmitted = profession =>
                {
                    SaveAdditionalInfo(userProfile.Country, userProfile.Gender, userProfile.Pronouns, userProfile.RelationshipStatus,
                        userProfile.SexualOrientation, userProfile.Language, profession,
                        userProfile.BirthDate, userProfile.RealName, userProfile.Hobbies, userProfile.EmploymentStatus);

                    myAccountAnalyticsService.SendProfileInfoAdditionalInfoAddAnalytic("Profession", profession);
                },
                OnRemoved = () =>
                {
                    SaveAdditionalInfo(userProfile.Country, userProfile.Gender, userProfile.Pronouns, userProfile.RelationshipStatus,
                        userProfile.SexualOrientation, userProfile.Language, null,
                        userProfile.BirthDate, userProfile.RealName, userProfile.Hobbies, userProfile.EmploymentStatus);

                    myAccountAnalyticsService.SendProfileInfoAdditionalInfoRemoveAnalytic("Profession");
                },
            });

            options.Add("Birth Date", new AdditionalInfoOptionsModel.Option
            {
                IsAvailable = userProfile.BirthDate == null,
                Name = "Birth Date",
                InputType = AdditionalInfoOptionsModel.InputType.Date,
                DateFormat = "dd/MM/yyyy",
                OnValueSubmitted = birthDate =>
                {
                    SaveAdditionalInfo(userProfile.Country, userProfile.Gender, userProfile.Pronouns, userProfile.RelationshipStatus,
                        userProfile.SexualOrientation, userProfile.Language, userProfile.Profession,
                        DateTime.ParseExact(birthDate, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal),
                        userProfile.RealName, userProfile.Hobbies, userProfile.EmploymentStatus);

                    myAccountAnalyticsService.SendProfileInfoAdditionalInfoAddAnalytic("Birth Date", birthDate);
                },
                OnRemoved = () =>
                {
                    SaveAdditionalInfo(userProfile.Country, userProfile.Gender, userProfile.Pronouns, userProfile.RelationshipStatus,
                        userProfile.SexualOrientation, userProfile.Language, userProfile.Profession,
                        null, userProfile.RealName, userProfile.Hobbies, userProfile.EmploymentStatus);

                    myAccountAnalyticsService.SendProfileInfoAdditionalInfoRemoveAnalytic("Birth Date");
                },
            });

            options.Add("Real Name", new AdditionalInfoOptionsModel.Option
            {
                IsAvailable = string.IsNullOrEmpty(userProfile.RealName),
                Name = "Real Name",
                InputType = AdditionalInfoOptionsModel.InputType.FreeFormText,
                OnValueSubmitted = realName =>
                {
                    SaveAdditionalInfo(userProfile.Country, userProfile.Gender, userProfile.Pronouns, userProfile.RelationshipStatus,
                        userProfile.SexualOrientation, userProfile.Language, userProfile.Profession,
                        userProfile.BirthDate, realName, userProfile.Hobbies, userProfile.EmploymentStatus);

                    myAccountAnalyticsService.SendProfileInfoAdditionalInfoAddAnalytic("Real Name", realName);
                },
                OnRemoved = () =>
                {
                    SaveAdditionalInfo(userProfile.Country, userProfile.Gender, userProfile.Pronouns, userProfile.RelationshipStatus,
                        userProfile.SexualOrientation, userProfile.Language, userProfile.Profession,
                        userProfile.BirthDate, null, userProfile.Hobbies, userProfile.EmploymentStatus);

                    myAccountAnalyticsService.SendProfileInfoAdditionalInfoRemoveAnalytic("Real Name");
                },
            });

            options.Add("Hobbies", new AdditionalInfoOptionsModel.Option
            {
                IsAvailable = string.IsNullOrEmpty(userProfile.Hobbies),
                Name = "Hobbies",
                InputType = AdditionalInfoOptionsModel.InputType.FreeFormText,
                OnValueSubmitted = hobbies =>
                {
                    SaveAdditionalInfo(userProfile.Country, userProfile.Gender, userProfile.Pronouns, userProfile.RelationshipStatus,
                        userProfile.SexualOrientation, userProfile.Language, userProfile.Profession,
                        userProfile.BirthDate, userProfile.RealName, hobbies, userProfile.EmploymentStatus);

                    myAccountAnalyticsService.SendProfileInfoAdditionalInfoAddAnalytic("Hobbies", hobbies);
                },
                OnRemoved = () =>
                {
                    SaveAdditionalInfo(userProfile.Country, userProfile.Gender, userProfile.Pronouns, userProfile.RelationshipStatus,
                        userProfile.SexualOrientation, userProfile.Language, userProfile.Profession,
                        userProfile.BirthDate, userProfile.RealName, null, userProfile.EmploymentStatus);

                    myAccountAnalyticsService.SendProfileInfoAdditionalInfoRemoveAnalytic("Hobbies");
                },
            });

            view.SetAdditionalInfoOptions(new AdditionalInfoOptionsModel
            {
                Options = options,
            });
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

        private void SaveAdditionalInfo(string country, string gender, string pronouns, string relationshipStatus,
            string sexualOrientation, string language, string profession, DateTime? birthDate,
            string realName, string hobbies, string employmentStatus)
        {
            async UniTaskVoid SaveAdditionalInfoAsync(string country, string gender, string pronouns, string relationshipStatus,
                string sexualOrientation, string language, string profession, DateTime? birthDate,
                string realName, string hobbies, string employmentStatus, CancellationToken cancellationToken)
            {
                await userProfileBridge.SaveAdditionalInfo(country, gender, pronouns, relationshipStatus,
                    sexualOrientation, language, profession, birthDate, realName, hobbies, employmentStatus, cancellationToken);

                myAccountSectionHUDController.ShowAccountSettingsUpdatedToast();
            }

            additionalInfoCancellationToken = additionalInfoCancellationToken.SafeRestart();

            SaveAdditionalInfoAsync(country, gender, pronouns, relationshipStatus,
                    sexualOrientation, language, profession, birthDate,
                    realName, hobbies, employmentStatus,
                    additionalInfoCancellationToken.Token)
               .Forget();
        }

        private void ShowAdditionalInfoValues(UserProfile userProfile)
        {
            Dictionary<string, (string title, string value)> values = new ();

            if (!string.IsNullOrEmpty(userProfile.Country))
                values["Country"] = ("Country", userProfile.Country);

            if (!string.IsNullOrEmpty(userProfile.Gender))
                values["Gender"] = ("Gender", userProfile.Gender);

            if (!string.IsNullOrEmpty(userProfile.Pronouns))
                values["Pronouns"] = ("Pronouns", userProfile.Pronouns);

            if (!string.IsNullOrEmpty(userProfile.RelationshipStatus))
                values["Relationship Status"] = ("Relationship Status", userProfile.RelationshipStatus);

            if (!string.IsNullOrEmpty(userProfile.SexualOrientation))
                values["Sexual Orientation"] = ("Sexual Orientation", userProfile.SexualOrientation);

            if (!string.IsNullOrEmpty(userProfile.Language))
                values["Language"] = ("Language", userProfile.Language);

            if (!string.IsNullOrEmpty(userProfile.EmploymentStatus))
                values["Employment Status"] = ("Employment Status", userProfile.EmploymentStatus);

            if (!string.IsNullOrEmpty(userProfile.Profession))
                values["Profession"] = ("Profession", userProfile.Profession);

            if (userProfile.BirthDate != null)
                values["Birth Date"] = ("Birth Date", userProfile.BirthDate?.ToString("dd/MM/yyyy"));

            if (!string.IsNullOrEmpty(userProfile.RealName))
                values["Real Name"] = ("Real Name", userProfile.RealName);

            if (!string.IsNullOrEmpty(userProfile.Hobbies))
                values["Hobbies"] = ("Hobbies", userProfile.Hobbies);

            view.SetAdditionalInfoValues(values);
        }
    }
}
