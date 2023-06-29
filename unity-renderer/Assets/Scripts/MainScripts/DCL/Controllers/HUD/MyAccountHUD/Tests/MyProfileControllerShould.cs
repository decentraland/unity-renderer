using Cysharp.Threading.Tasks;
using DCL.Browser;
using DCL.UserProfiles;
using DCLServices.Lambdas.NamesService;
using KernelConfigurationTypes;
using NSubstitute;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using UnityEngine.TestTools;

namespace DCL.MyAccount
{
    public class MyProfileControllerShould
    {
        private const string OWN_USER_ID = "ownUserId";
        private const string NOT_OWNED_FULL_NAME = "myname#af1b";
        private const string NOT_OWNED_NAME_PREFIX = "myname";
        private const string NOT_OWNED_NAME_SUFFIX = "af1b";
        private const string MY_DESCRIPTION = "my description";
        private const string VALID_NAME = "validName";
        private const string OWN_ETH_ADDRESS = "0xdf9ba5f96abdfbf4bf97bfd9b9a764";

        private MyProfileController controller;
        private MyAccountSectionHUDController myAccountSectionController;
        private DataStore dataStore;
        private IMyProfileComponentView view;
        private IUserProfileBridge userProfileBridge;
        private INamesService namesService;
        private IBrowserBridge browserBridge;
        private KernelConfig kernelConfig;
        private IMyAccountAnalyticsService myAccountAnalyticsService;
        private IProfileAdditionalInfoValueListProvider countryListProvider;
        private IProfileAdditionalInfoValueListProvider genderListProvider;
        private IProfileAdditionalInfoValueListProvider sexualOrientationProvider;
        private IProfileAdditionalInfoValueListProvider employmentStatusProvider;
        private IProfileAdditionalInfoValueListProvider relationshipStatusProvider;
        private IProfileAdditionalInfoValueListProvider languageListProvider;
        private IProfileAdditionalInfoValueListProvider pronounListProvider;
        private UserProfile ownUserProfile;

        [SetUp]
        public void SetUp()
        {
            dataStore = new DataStore();

            myAccountSectionController = new MyAccountSectionHUDController(
                Substitute.For<IMyAccountSectionHUDComponentView>(),
                dataStore);

            view = Substitute.For<IMyProfileComponentView>();

            userProfileBridge = Substitute.For<IUserProfileBridge>();
            ownUserProfile = ScriptableObject.CreateInstance<UserProfile>();

            ownUserProfile.UpdateData(new UserProfileModel
            {
                userId = OWN_USER_ID,
                name = NOT_OWNED_FULL_NAME,
                hasClaimedName = false,
                description = MY_DESCRIPTION,
                ethAddress = OWN_ETH_ADDRESS,
            });

            userProfileBridge.GetOwn().Returns(ownUserProfile);

            namesService = Substitute.For<INamesService>();

            namesService.RequestOwnedNamesAsync(default, default, default, default, default)
                        .ReturnsForAnyArgs(UniTask.FromResult<(IReadOnlyList<NamesResponse.NameEntry>, int)>((new List<NamesResponse.NameEntry>(), 0)));

            browserBridge = Substitute.For<IBrowserBridge>();
            kernelConfig = KernelConfig.i;

            kernelConfig.Set(new KernelConfigModel
            {
                profiles = new Profiles
                {
                    nameValidRegex = VALID_NAME,
                }
            });

            myAccountAnalyticsService = Substitute.For<IMyAccountAnalyticsService>();
            countryListProvider = Substitute.For<IProfileAdditionalInfoValueListProvider>();
            genderListProvider = Substitute.For<IProfileAdditionalInfoValueListProvider>();
            sexualOrientationProvider = Substitute.For<IProfileAdditionalInfoValueListProvider>();
            employmentStatusProvider = Substitute.For<IProfileAdditionalInfoValueListProvider>();
            relationshipStatusProvider = Substitute.For<IProfileAdditionalInfoValueListProvider>();
            languageListProvider = Substitute.For<IProfileAdditionalInfoValueListProvider>();
            pronounListProvider = Substitute.For<IProfileAdditionalInfoValueListProvider>();

            controller = new MyProfileController(view,
                dataStore,
                userProfileBridge,
                namesService,
                browserBridge,
                myAccountSectionController,
                kernelConfig,
                myAccountAnalyticsService,
                countryListProvider,
                genderListProvider,
                sexualOrientationProvider,
                employmentStatusProvider,
                relationshipStatusProvider,
                languageListProvider,
                pronounListProvider);
        }

        [TearDown]
        public void TearDown()
        {
            myAccountSectionController.Dispose();
            controller.Dispose();
        }

        [UnityTest]
        public IEnumerator ShowLoadingThenHideAfterNamesProcessFinishes()
        {
            namesService.RequestOwnedNamesAsync(default, default, default, default, default)
                        .ReturnsForAnyArgs(UniTask.Create<(IReadOnlyList<NamesResponse.NameEntry>, int)>(async () =>
                         {
                             await UniTask.Delay(500);
                             return (new List<NamesResponse.NameEntry>(), 0);
                         }));

            dataStore.myAccount.isMyAccountSectionVisible.Set(true, true);

            view.Received(1).SetLoadingActive(true);
            view.Received(0).SetLoadingActive(false);

            yield return new WaitForSeconds(1f);

            view.Received(1).SetLoadingActive(true);
        }

        [Test]
        public void ShowNamesWhenTheUserAlreadyHasOwnedNames()
        {
            const string OWNED_NAME_1 = "ownedName1";
            const string OWNED_NAME_2 = "ownedName2";
            const string OWNED_NAME_3 = "ownedName3";

            var givenOwnedNames = new List<NamesResponse.NameEntry>
            {
                new (OWNED_NAME_1, "contractaddr1", "100"),
                new (OWNED_NAME_2, "contractaddr2", "50"),
                new (OWNED_NAME_3, "contractaddr3", "200"),
            };

            namesService.RequestOwnedNamesAsync(default, default, default, default, default)
                        .ReturnsForAnyArgs(UniTask.FromResult<(IReadOnlyList<NamesResponse.NameEntry>, int)>((givenOwnedNames, givenOwnedNames.Count)));

            UserProfile userProfile = ScriptableObject.CreateInstance<UserProfile>();

            userProfile.UpdateData(new UserProfileModel
            {
                userId = OWN_USER_ID,
                name = OWNED_NAME_1,
                hasClaimedName = true,
                ethAddress = OWN_ETH_ADDRESS,
            });

            userProfileBridge.GetOwn().Returns(userProfile);

            dataStore.myAccount.isMyAccountSectionVisible.Set(true, true);

            view.Received(1)
                .SetClaimedNameDropdownOptions(Arg.Is<List<string>>(l => l[0] == OWNED_NAME_1
                                                                         && l[1] == OWNED_NAME_2
                                                                         && l[2] == OWNED_NAME_3
                                                                         && l.Count == givenOwnedNames.Count));

            view.Received(1).SetClaimedNameMode(true);
            view.Received(1).SetClaimedNameModeAsInput(false);
            view.Received(1).SetCurrentName(OWNED_NAME_1, "a764");
            view.Received(1).SetClaimNameBannerActive(false);
        }

        [Test]
        public void ShowNamesWhenTheUserDoesntHaveAnyOwnedName()
        {
            dataStore.myAccount.isMyAccountSectionVisible.Set(true, true);

            view.Received(1).SetClaimedNameDropdownOptions(Arg.Is<List<string>>(l => l.Count == 0));

            view.Received(1).SetClaimedNameMode(false);
            view.Received(1).SetClaimedNameModeAsInput(true);
            view.Received(1).SetCurrentName(NOT_OWNED_NAME_PREFIX, NOT_OWNED_NAME_SUFFIX);
            view.Received(1).SetClaimNameBannerActive(true);
        }

        [Test]
        public void ShowAboutDescriptionWhenOpens()
        {
            dataStore.myAccount.isMyAccountSectionVisible.Set(true, true);

            view.Received(1).SetAboutDescription(MY_DESCRIPTION);
        }

        [Test]
        public void ShowLinksWhenIsEmpty()
        {
            dataStore.myAccount.isMyAccountSectionVisible.Set(true, true);

            view.Received(1).SetLinks(Arg.Is<List<UserProfileModel.Link>>(l => l.Count == 0));
            view.Received(1).EnableOrDisableAddLinksOption(true);
        }

        [Test]
        public void ShowManyLinks()
        {
            UserProfile userProfile = ScriptableObject.CreateInstance<UserProfile>();

            userProfile.UpdateData(new UserProfileModel
            {
                userId = OWN_USER_ID,
                name = NOT_OWNED_FULL_NAME,
                hasClaimedName = false,
                description = MY_DESCRIPTION,
                links = new List<UserProfileModel.Link>
                {
                    new ("l1", "url1"),
                    new ("l2", "url2"),
                    new ("l3", "url3"),
                },
                ethAddress = OWN_ETH_ADDRESS,
            });

            userProfileBridge.GetOwn().Returns(userProfile);

            dataStore.myAccount.isMyAccountSectionVisible.Set(true, true);

            view.Received(1)
                .SetLinks(Arg.Is<List<UserProfileModel.Link>>(l => l.Count == 3
                                                                   && l[0].title == "l1" && l[0].url == "url1"
                                                                   && l[1].title == "l2" && l[1].url == "url2"
                                                                   && l[2].title == "l3" && l[2].url == "url3"));

            view.Received(1).EnableOrDisableAddLinksOption(true);
        }

        [Test]
        public void DisableLinkAdditionWhenMaxedOut()
        {
            UserProfile userProfile = ScriptableObject.CreateInstance<UserProfile>();

            userProfile.UpdateData(new UserProfileModel
            {
                userId = OWN_USER_ID,
                name = NOT_OWNED_FULL_NAME,
                hasClaimedName = false,
                description = MY_DESCRIPTION,
                links = new List<UserProfileModel.Link>
                {
                    new ("l1", "url1"),
                    new ("l2", "url2"),
                    new ("l3", "url3"),
                    new ("l4", "url4"),
                    new ("l5", "url5"),
                },
                ethAddress = OWN_ETH_ADDRESS,
            });

            userProfileBridge.GetOwn().Returns(userProfile);

            dataStore.myAccount.isMyAccountSectionVisible.Set(true, true);

            view.Received(1).SetLinks(Arg.Is<List<UserProfileModel.Link>>(l => l.Count == 5));
            view.Received(1).EnableOrDisableAddLinksOption(false);
        }

        [Test]
        public void HideInvalidNameWarningWhenModifyingTheName()
        {
            view.OnCurrentNameEdited += Raise.Event<Action<string>>(VALID_NAME);

            view.Received(1).SetNonValidNameWarningActive(false);
        }

        [Test]
        public void ShowInvalidNameWarningWhenModifyingTheName()
        {
            view.OnCurrentNameEdited += Raise.Event<Action<string>>("invalid name");

            view.Received(1).SetNonValidNameWarningActive(true);
        }

        [Test]
        public void SaveUnverifiedNameWhenSubmittingTheName()
        {
            view.OnCurrentNameSubmitted += Raise.Event<Action<string, bool>>(VALID_NAME, false);

            view.Received(1).SetNonValidNameWarningActive(false);

            view.Received(1).SetNonValidNameWarningActive(false);
            userProfileBridge.Received(1).SaveUnverifiedName(VALID_NAME, Arg.Any<CancellationToken>());
            myAccountAnalyticsService.Received(1).SendPlayerSwapNameAnalytic(false, Arg.Any<int>());
        }

        [Test]
        public void SaveVerifiedNameWhenSubmittingTheName()
        {
            view.OnCurrentNameSubmitted += Raise.Event<Action<string, bool>>(VALID_NAME, true);

            view.Received(1).SetNonValidNameWarningActive(false);

            view.Received(1).SetNonValidNameWarningActive(false);
            userProfileBridge.Received(1).SaveVerifiedName(VALID_NAME, Arg.Any<CancellationToken>());
            myAccountAnalyticsService.Received(1).SendPlayerSwapNameAnalytic(true, Arg.Any<int>());
        }

        [Test]
        public void OpenBrowserUrlWhenClickingClaimName()
        {
            view.OnClaimNameClicked += Raise.Event<Action>();

            browserBridge.Received(1).OpenUrl("https://builder.decentraland.org/claim-name?utm_source=dcl_explorer");
            myAccountAnalyticsService.Received(1).SendPlayerOpenClaimNameAnalytic();
        }

        [Test]
        public void SaveAboutDescription()
        {
            const string ANOTHER_DESCRIPTION = "another description";

            view.OnAboutDescriptionSubmitted += Raise.Event<Action<string>>(ANOTHER_DESCRIPTION);

            userProfileBridge.SaveDescription(ANOTHER_DESCRIPTION, Arg.Any<CancellationToken>());
            myAccountAnalyticsService.Received(1).SendProfileInfoEditAnalytic(ANOTHER_DESCRIPTION.Length);
        }

        [TestCase("l1", "http://whatever.com", "l0")]
        [TestCase("l4", "http://whatever.com", "l0", "l1", "l2", "l3")]
        public void AddNewLink(string title, string url, params string[] preExistentLinksParam)
        {
            List<UserProfileModel.Link> preExistentLinks = preExistentLinksParam
                                                          .Select(l => new UserProfileModel.Link(l, l))
                                                          .ToList();

            List<UserProfileModel.Link> expectedLinks = new List<UserProfileModel.Link>(preExistentLinks)
                { new (title, url) };

            UserProfile profileWithNewLink = ScriptableObject.CreateInstance<UserProfile>();

            profileWithNewLink.UpdateData(new UserProfileModel
            {
                userId = OWN_USER_ID,
                name = NOT_OWNED_FULL_NAME,
                hasClaimedName = false,
                description = MY_DESCRIPTION,
                links = expectedLinks,
            });

            UserProfile profile = ScriptableObject.CreateInstance<UserProfile>();

            profile.UpdateData(new UserProfileModel
            {
                userId = OWN_USER_ID,
                name = NOT_OWNED_FULL_NAME,
                hasClaimedName = false,
                description = MY_DESCRIPTION,
                links = preExistentLinks,
            });

            userProfileBridge.GetOwn().Returns(profile);

            userProfileBridge.SaveLinks(Arg.Any<List<UserProfileModel.Link>>(), Arg.Any<CancellationToken>())
                             .Returns(UniTask.FromResult(profileWithNewLink));

            view.OnLinkAdded += Raise.Event<Action<(string title, string url)>>((title, url));

            view.Received(1).ClearLinkInput();

            userProfileBridge.Received(1)
                             .SaveLinks(Arg.Is<List<UserProfileModel.Link>>(l => l.Count == expectedLinks.Count
                                                                                 && l.All(link => expectedLinks.Exists(expectedLink => expectedLink.title == link.title && expectedLink.url == link.url))),
                                  Arg.Any<CancellationToken>());

            myAccountAnalyticsService.Received(1).SendProfileLinkAddAnalytic(title, url);
        }

        [Test]
        public void DontAddNewLinkIfAlreadyMaxedOut()
        {
            UserProfile profile = ScriptableObject.CreateInstance<UserProfile>();

            profile.UpdateData(new UserProfileModel
            {
                userId = OWN_USER_ID,
                name = NOT_OWNED_FULL_NAME,
                hasClaimedName = false,
                description = MY_DESCRIPTION,
                links = new List<UserProfileModel.Link>
                {
                    new ("l0", "l0"),
                    new ("l1", "l1"),
                    new ("l2", "l2"),
                    new ("l3", "l3"),
                    new ("l4", "l4"),
                },
            });

            userProfileBridge.GetOwn().Returns(profile);

            view.OnLinkAdded += Raise.Event<Action<(string title, string url)>>(("l5", "l5"));

            view.Received(0).ClearLinkInput();
            userProfileBridge.DidNotReceiveWithAnyArgs().SaveLinks(default, default);
        }

        [Test]
        public void RemoveLink()
        {
            List<UserProfileModel.Link> preExistentLinks = new List<UserProfileModel.Link>
            {
                new ("l0", "l0"),
                new ("l1", "l1"),
                new ("l2", "l2"),
            };

            List<UserProfileModel.Link> expectedLinks = new List<UserProfileModel.Link>
            {
                new ("l0", "l0"),
                new ("l2", "l2"),
            };

            UserProfile profileWithLinkRemoved = ScriptableObject.CreateInstance<UserProfile>();

            profileWithLinkRemoved.UpdateData(new UserProfileModel
            {
                userId = OWN_USER_ID,
                name = NOT_OWNED_FULL_NAME,
                hasClaimedName = false,
                description = MY_DESCRIPTION,
                links = expectedLinks,
            });

            UserProfile profile = ScriptableObject.CreateInstance<UserProfile>();

            profile.UpdateData(new UserProfileModel
            {
                userId = OWN_USER_ID,
                name = NOT_OWNED_FULL_NAME,
                hasClaimedName = false,
                description = MY_DESCRIPTION,
                links = preExistentLinks,
            });

            userProfileBridge.GetOwn().Returns(profile);

            userProfileBridge.SaveLinks(Arg.Any<List<UserProfileModel.Link>>(), Arg.Any<CancellationToken>())
                             .Returns(UniTask.FromResult(profileWithLinkRemoved));

            view.OnLinkRemoved += Raise.Event<Action<(string title, string url)>>(("l1", "l1"));

            view.Received(1)
                .SetLinks(Arg.Is<List<UserProfileModel.Link>>(l => l.Count == expectedLinks.Count
                                                                   && l.All(viewLinks => expectedLinks.Exists(expectedLink => expectedLink.title == viewLinks.title && expectedLink.url == viewLinks.url))));

            myAccountAnalyticsService.Received(1).SendProfileLinkRemoveAnalytic("l1", "l1");
        }

        [Test]
        public void ShowAllAdditionalInfoValues()
        {
            ownUserProfile.UpdateData(new UserProfileModel
            {
                userId = OWN_USER_ID,
                name = NOT_OWNED_FULL_NAME,
                hasClaimedName = false,
                description = MY_DESCRIPTION,
                AdditionalInfo = new AdditionalInfo
                {
                    BirthDate = new DateTime(1990, 12, 25),
                    Country = "country",
                    Language = "english",
                    Gender = "male",
                    Pronouns = "he/him",
                    Profession = "software dev",
                    EmploymentStatus = "working",
                    RealName = "conito pepote",
                    SexualOrientation = "heterosexual",
                    RelationshipStatus = "single",
                    Hobbies = "sports, games & art",
                },
            });

            view.Received(1)
                .SetAdditionalInfoOptions(Arg.Is<AdditionalInfoOptionsModel>(a =>
                     a.Options.Count == 11
                     && a.Options.All(pair => !pair.Value.IsAvailable)));

            view.Received(1)
                .SetAdditionalInfoValues(Arg.Is<Dictionary<string, (string title, string value)>>(d =>
                     d.Count == 11
                     && d["Country"].title == "Country" && d["Country"].value == "country"
                     && d["Language"].title == "Language" && d["Language"].value == "english"
                     && d["Gender"].title == "Gender" && d["Gender"].value == "male"
                     && d["Pronouns"].title == "Pronouns" && d["Pronouns"].value == "he/him"
                     && d["Profession"].title == "Profession" && d["Profession"].value == "software dev"
                     && d["Employment Status"].title == "Employment Status" && d["Employment Status"].value == "working"
                     && d["Real Name"].title == "Real Name" && d["Real Name"].value == "conito pepote"
                     && d["Sexual Orientation"].title == "Sexual Orientation" && d["Sexual Orientation"].value == "heterosexual"
                     && d["Relationship Status"].title == "Relationship Status" && d["Relationship Status"].value == "single"
                     && d["Hobbies"].title == "Hobbies" && d["Hobbies"].value == "sports, games & art"
                     && d["Birth Date"].title == "Birth Date" && d["Birth Date"].value == "25/12/1990"));
        }

        [Test]
        public void ShowAdditionalInfoOptions()
        {
            pronounListProvider.Provide().Returns(new[] { "" });
            countryListProvider.Provide().Returns(new[] { "" });
            languageListProvider.Provide().Returns(new[] { "" });
            genderListProvider.Provide().Returns(new[] { "" });
            employmentStatusProvider.Provide().Returns(new[] { "" });
            sexualOrientationProvider.Provide().Returns(new[] { "" });
            relationshipStatusProvider.Provide().Returns(new[] { "" });

            ownUserProfile.UpdateData(new UserProfileModel
            {
                userId = OWN_USER_ID,
                name = NOT_OWNED_FULL_NAME,
                hasClaimedName = false,
                description = MY_DESCRIPTION,
            });

            view.Received(1)
                .SetAdditionalInfoOptions(Arg.Is<AdditionalInfoOptionsModel>(a =>
                     a.Options.Count == 11
                     && a.Options.All(pair => pair.Value.IsAvailable)
                     && a.Options["Country"].Name == "Country" && a.Options["Country"].Values.Length > 0 && a.Options["Country"].InputType == AdditionalInfoOptionsModel.InputType.StrictValueList
                     && a.Options["Language"].Name == "Language" && a.Options["Language"].Values.Length > 0 && a.Options["Language"].InputType == AdditionalInfoOptionsModel.InputType.StrictValueList
                     && a.Options["Gender"].Name == "Gender" && a.Options["Gender"].Values.Length > 0 && a.Options["Gender"].InputType == AdditionalInfoOptionsModel.InputType.StrictValueList
                     && a.Options["Pronouns"].Name == "Pronouns" && a.Options["Pronouns"].Values.Length > 0 && a.Options["Pronouns"].InputType == AdditionalInfoOptionsModel.InputType.StrictValueList
                     && a.Options["Profession"].Name == "Profession" && a.Options["Profession"].InputType == AdditionalInfoOptionsModel.InputType.FreeFormText
                     && a.Options["Employment Status"].Name == "Employment Status" && a.Options["Employment Status"].Values.Length > 0 && a.Options["Employment Status"].InputType == AdditionalInfoOptionsModel.InputType.StrictValueList
                     && a.Options["Real Name"].Name == "Real Name" && a.Options["Real Name"].InputType == AdditionalInfoOptionsModel.InputType.FreeFormText
                     && a.Options["Sexual Orientation"].Name == "Sexual Orientation" && a.Options["Sexual Orientation"].Values.Length > 0 && a.Options["Sexual Orientation"].InputType == AdditionalInfoOptionsModel.InputType.StrictValueList
                     && a.Options["Relationship Status"].Name == "Relationship Status" && a.Options["Relationship Status"].Values.Length > 0 && a.Options["Relationship Status"].InputType == AdditionalInfoOptionsModel.InputType.StrictValueList
                     && a.Options["Hobbies"].Name == "Hobbies" && a.Options["Hobbies"].InputType == AdditionalInfoOptionsModel.InputType.FreeFormText
                     && a.Options["Birth Date"].Name == "Birth Date" && a.Options["Birth Date"].InputType == AdditionalInfoOptionsModel.InputType.Date && a.Options["Birth Date"].DateFormat == "dd/MM/yyyy"));

            view.Received(1)
                .SetAdditionalInfoValues(Arg.Is<Dictionary<string, (string title, string value)>>(d =>
                     d.Count == 0));
        }

        [Test]
        public void SaveCountry()
        {
            view.WhenForAnyArgs(v => v.SetAdditionalInfoOptions(default))
                .Do(info => info.Arg<AdditionalInfoOptionsModel>().Options["Country"].OnValueSubmitted.Invoke("country"));

            dataStore.myAccount.isMyAccountSectionVisible.Set(true, true);

            userProfileBridge.Received(1)
                             .SaveAdditionalInfo(new AdditionalInfo
                              {
                                  Country = "country",
                              }, Arg.Any<CancellationToken>());
        }

        [Test]
        public void RemoveCountry()
        {
            view.WhenForAnyArgs(v => v.SetAdditionalInfoOptions(default))
                .Do(info => info.Arg<AdditionalInfoOptionsModel>().Options["Country"].OnRemoved.Invoke());

            ownUserProfile.UpdateData(new UserProfileModel
            {
                userId = OWN_USER_ID,
                name = NOT_OWNED_FULL_NAME,
                hasClaimedName = false,
                description = MY_DESCRIPTION,
                AdditionalInfo = new AdditionalInfo
                {
                    Country = "country",
                },
            });

            userProfileBridge.Received(1)
                             .SaveAdditionalInfo(new AdditionalInfo(), Arg.Any<CancellationToken>());
        }

        [Test]
        public void SaveLanguage()
        {
            view.WhenForAnyArgs(v => v.SetAdditionalInfoOptions(default))
                .Do(info => info.Arg<AdditionalInfoOptionsModel>().Options["Language"].OnValueSubmitted.Invoke("ES"));

            dataStore.myAccount.isMyAccountSectionVisible.Set(true, true);

            userProfileBridge.Received(1)
                             .SaveAdditionalInfo(new AdditionalInfo
                                  {
                                      Language = "ES",
                                  }, Arg.Any<CancellationToken>());
        }

        [Test]
        public void RemoveLanguage()
        {
            view.WhenForAnyArgs(v => v.SetAdditionalInfoOptions(default))
                .Do(info => info.Arg<AdditionalInfoOptionsModel>().Options["Language"].OnRemoved.Invoke());

            ownUserProfile.UpdateData(new UserProfileModel
            {
                userId = OWN_USER_ID,
                name = NOT_OWNED_FULL_NAME,
                hasClaimedName = false,
                description = MY_DESCRIPTION,
                AdditionalInfo = new AdditionalInfo
                {
                    Language = "EN",
                },
            });

            userProfileBridge.Received(1)
                             .SaveAdditionalInfo(new AdditionalInfo(), Arg.Any<CancellationToken>());
        }

        [Test]
        public void SaveGender()
        {
            view.WhenForAnyArgs(v => v.SetAdditionalInfoOptions(default))
                .Do(info => info.Arg<AdditionalInfoOptionsModel>().Options["Gender"].OnValueSubmitted.Invoke("female"));

            dataStore.myAccount.isMyAccountSectionVisible.Set(true, true);

            userProfileBridge.Received(1)
                             .SaveAdditionalInfo(new AdditionalInfo
                                  {
                                      Gender = "female",
                                  }, Arg.Any<CancellationToken>());
        }

        [Test]
        public void RemoveGender()
        {
            view.WhenForAnyArgs(v => v.SetAdditionalInfoOptions(default))
                .Do(info => info.Arg<AdditionalInfoOptionsModel>().Options["Gender"].OnRemoved.Invoke());

            ownUserProfile.UpdateData(new UserProfileModel
            {
                userId = OWN_USER_ID,
                name = NOT_OWNED_FULL_NAME,
                hasClaimedName = false,
                description = MY_DESCRIPTION,
                AdditionalInfo = new AdditionalInfo
                {
                    Gender = "female",
                },
            });

            userProfileBridge.Received(1)
                             .SaveAdditionalInfo(new AdditionalInfo(), Arg.Any<CancellationToken>());
        }

        [Test]
        public void SavePronouns()
        {
            view.WhenForAnyArgs(v => v.SetAdditionalInfoOptions(default))
                .Do(info => info.Arg<AdditionalInfoOptionsModel>().Options["Pronouns"].OnValueSubmitted.Invoke("she/her"));

            dataStore.myAccount.isMyAccountSectionVisible.Set(true, true);

            userProfileBridge.Received(1)
                             .SaveAdditionalInfo(new AdditionalInfo
                                  {
                                      Pronouns = "she/her",
                                  }, Arg.Any<CancellationToken>());
        }

        [Test]
        public void RemovePronouns()
        {
            view.WhenForAnyArgs(v => v.SetAdditionalInfoOptions(default))
                .Do(info => info.Arg<AdditionalInfoOptionsModel>().Options["Pronouns"].OnRemoved.Invoke());

            ownUserProfile.UpdateData(new UserProfileModel
            {
                userId = OWN_USER_ID,
                name = NOT_OWNED_FULL_NAME,
                hasClaimedName = false,
                description = MY_DESCRIPTION,
                AdditionalInfo = new AdditionalInfo
                {
                    Pronouns = "she/her",
                },
            });

            userProfileBridge.Received(1)
                             .SaveAdditionalInfo(new AdditionalInfo(), Arg.Any<CancellationToken>());
        }

        [Test]
        public void SaveProfession()
        {
            view.WhenForAnyArgs(v => v.SetAdditionalInfoOptions(default))
                .Do(info => info.Arg<AdditionalInfoOptionsModel>().Options["Profession"].OnValueSubmitted.Invoke("qa"));

            dataStore.myAccount.isMyAccountSectionVisible.Set(true, true);

            userProfileBridge.Received(1)
                             .SaveAdditionalInfo(new AdditionalInfo
                              {
                                  Profession = "qa",
                              }, Arg.Any<CancellationToken>());
        }

        [Test]
        public void RemoveProfession()
        {
            view.WhenForAnyArgs(v => v.SetAdditionalInfoOptions(default))
                .Do(info => info.Arg<AdditionalInfoOptionsModel>().Options["Profession"].OnRemoved.Invoke());

            ownUserProfile.UpdateData(new UserProfileModel
            {
                userId = OWN_USER_ID,
                name = NOT_OWNED_FULL_NAME,
                hasClaimedName = false,
                description = MY_DESCRIPTION,
                AdditionalInfo = new AdditionalInfo
                {
                    Profession = "qa",
                },
            });

            userProfileBridge.Received(1)
                             .SaveAdditionalInfo(new AdditionalInfo(), Arg.Any<CancellationToken>());
        }

        [Test]
        public void SaveEmploymentStatus()
        {
            view.WhenForAnyArgs(v => v.SetAdditionalInfoOptions(default))
                .Do(info => info.Arg<AdditionalInfoOptionsModel>().Options["Employment Status"].OnValueSubmitted.Invoke("lazy"));

            dataStore.myAccount.isMyAccountSectionVisible.Set(true, true);

            userProfileBridge.Received(1)
                             .SaveAdditionalInfo(new AdditionalInfo
                              {
                                  EmploymentStatus = "lazy",
                              }, Arg.Any<CancellationToken>());
        }

        [Test]
        public void RemoveEmploymentStatus()
        {
            view.WhenForAnyArgs(v => v.SetAdditionalInfoOptions(default))
                .Do(info => info.Arg<AdditionalInfoOptionsModel>().Options["Employment Status"].OnRemoved.Invoke());

            ownUserProfile.UpdateData(new UserProfileModel
            {
                userId = OWN_USER_ID,
                name = NOT_OWNED_FULL_NAME,
                hasClaimedName = false,
                description = MY_DESCRIPTION,
                AdditionalInfo = new AdditionalInfo
                {
                    EmploymentStatus = "lazy",
                },
            });

            userProfileBridge.Received(1)
                             .SaveAdditionalInfo(new AdditionalInfo(), Arg.Any<CancellationToken>());
        }

        [Test]
        public void SaveRealName()
        {
            view.WhenForAnyArgs(v => v.SetAdditionalInfoOptions(default))
                .Do(info => info.Arg<AdditionalInfoOptionsModel>().Options["Real Name"].OnValueSubmitted.Invoke("peperote"));

            dataStore.myAccount.isMyAccountSectionVisible.Set(true, true);

            userProfileBridge.Received(1)
                             .SaveAdditionalInfo(new AdditionalInfo
                              {
                                  RealName = "peperote",
                              }, Arg.Any<CancellationToken>());
        }

        [Test]
        public void RemoveRealName()
        {
            view.WhenForAnyArgs(v => v.SetAdditionalInfoOptions(default))
                .Do(info => info.Arg<AdditionalInfoOptionsModel>().Options["Real Name"].OnRemoved.Invoke());

            ownUserProfile.UpdateData(new UserProfileModel
            {
                userId = OWN_USER_ID,
                name = NOT_OWNED_FULL_NAME,
                hasClaimedName = false,
                description = MY_DESCRIPTION,
                AdditionalInfo = new AdditionalInfo
                {
                    RealName = "peperote",
                },
            });

            userProfileBridge.Received(1)
                             .SaveAdditionalInfo(new AdditionalInfo(), Arg.Any<CancellationToken>());
        }

        [Test]
        public void SaveSexualOrientation()
        {
            view.WhenForAnyArgs(v => v.SetAdditionalInfoOptions(default))
                .Do(info => info.Arg<AdditionalInfoOptionsModel>().Options["Sexual Orientation"].OnValueSubmitted.Invoke("alot"));

            dataStore.myAccount.isMyAccountSectionVisible.Set(true, true);

            userProfileBridge.Received(1)
                             .SaveAdditionalInfo(new AdditionalInfo
                              {
                                  SexualOrientation = "alot",
                              }, Arg.Any<CancellationToken>());
        }

        [Test]
        public void RemoveSexualOrientation()
        {
            view.WhenForAnyArgs(v => v.SetAdditionalInfoOptions(default))
                .Do(info => info.Arg<AdditionalInfoOptionsModel>().Options["Sexual Orientation"].OnRemoved.Invoke());

            ownUserProfile.UpdateData(new UserProfileModel
            {
                userId = OWN_USER_ID,
                name = NOT_OWNED_FULL_NAME,
                hasClaimedName = false,
                description = MY_DESCRIPTION,
                AdditionalInfo = new AdditionalInfo
                {
                    SexualOrientation = "alot",
                },
            });

            userProfileBridge.Received(1)
                             .SaveAdditionalInfo(new AdditionalInfo(), Arg.Any<CancellationToken>());
        }

        [Test]
        public void SaveRelationshipStatus()
        {
            view.WhenForAnyArgs(v => v.SetAdditionalInfoOptions(default))
                .Do(info => info.Arg<AdditionalInfoOptionsModel>().Options["Relationship Status"].OnValueSubmitted.Invoke("free"));

            dataStore.myAccount.isMyAccountSectionVisible.Set(true, true);

            userProfileBridge.Received(1)
                             .SaveAdditionalInfo(new AdditionalInfo
                              {
                                  RelationshipStatus = "free",
                              }, Arg.Any<CancellationToken>());
        }

        [Test]
        public void RemoveRelationshipStatus()
        {
            view.WhenForAnyArgs(v => v.SetAdditionalInfoOptions(default))
                .Do(info => info.Arg<AdditionalInfoOptionsModel>().Options["Relationship Status"].OnRemoved.Invoke());

            ownUserProfile.UpdateData(new UserProfileModel
            {
                userId = OWN_USER_ID,
                name = NOT_OWNED_FULL_NAME,
                hasClaimedName = false,
                description = MY_DESCRIPTION,
                AdditionalInfo = new AdditionalInfo
                {
                    RelationshipStatus = "free",
                },
            });

            userProfileBridge.Received(1)
                             .SaveAdditionalInfo(new AdditionalInfo(), Arg.Any<CancellationToken>());
        }

        [Test]
        public void SaveHobbies()
        {
            view.WhenForAnyArgs(v => v.SetAdditionalInfoOptions(default))
                .Do(info => info.Arg<AdditionalInfoOptionsModel>().Options["Hobbies"].OnValueSubmitted.Invoke("eat"));

            dataStore.myAccount.isMyAccountSectionVisible.Set(true, true);

            userProfileBridge.Received(1)
                             .SaveAdditionalInfo(new AdditionalInfo
                              {
                                  Hobbies = "eat",
                              }, Arg.Any<CancellationToken>());
        }

        [Test]
        public void RemoveHobbies()
        {
            view.WhenForAnyArgs(v => v.SetAdditionalInfoOptions(default))
                .Do(info => info.Arg<AdditionalInfoOptionsModel>().Options["Hobbies"].OnRemoved.Invoke());

            ownUserProfile.UpdateData(new UserProfileModel
            {
                userId = OWN_USER_ID,
                name = NOT_OWNED_FULL_NAME,
                hasClaimedName = false,
                description = MY_DESCRIPTION,
                AdditionalInfo = new AdditionalInfo
                {
                    Hobbies = "eat",
                },
            });

            userProfileBridge.Received(1)
                             .SaveAdditionalInfo(new AdditionalInfo(), Arg.Any<CancellationToken>());
        }

        [Test]
        public void SaveBirthDate()
        {
            view.WhenForAnyArgs(v => v.SetAdditionalInfoOptions(default))
                .Do(info => info.Arg<AdditionalInfoOptionsModel>().Options["Birth Date"].OnValueSubmitted.Invoke("15/12/1985"));

            dataStore.myAccount.isMyAccountSectionVisible.Set(true, true);

            userProfileBridge.Received(1)
                             .SaveAdditionalInfo(Arg.Is<AdditionalInfo>(a => a.BirthDate.Value.Day == 15
                              && a.BirthDate.Value.Month == 12
                              && a.BirthDate.Value.Year == 1985), Arg.Any<CancellationToken>());
        }

        [Test]
        public void RemoveBirthDate()
        {
            view.WhenForAnyArgs(v => v.SetAdditionalInfoOptions(default))
                .Do(info => info.Arg<AdditionalInfoOptionsModel>().Options["Birth Date"].OnRemoved.Invoke());

            ownUserProfile.UpdateData(new UserProfileModel
            {
                userId = OWN_USER_ID,
                name = NOT_OWNED_FULL_NAME,
                hasClaimedName = false,
                description = MY_DESCRIPTION,
                AdditionalInfo = new AdditionalInfo
                {
                    BirthDate = new DateTime(1985, 12, 15),
                },
            });

            userProfileBridge.Received(1)
                             .SaveAdditionalInfo(new AdditionalInfo(), Arg.Any<CancellationToken>());
        }
    }
}
