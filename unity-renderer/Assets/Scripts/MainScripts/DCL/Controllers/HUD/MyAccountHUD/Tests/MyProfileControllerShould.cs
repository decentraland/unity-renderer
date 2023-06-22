using Cysharp.Threading.Tasks;
using DCL.Browser;
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

        private MyProfileController controller;
        private MyAccountSectionHUDController myAccountSectionController;
        private DataStore dataStore;
        private IMyProfileComponentView view;
        private IUserProfileBridge userProfileBridge;
        private INamesService namesService;
        private IBrowserBridge browserBridge;
        private KernelConfig kernelConfig;

        [SetUp]
        public void SetUp()
        {
            dataStore = new DataStore();

            myAccountSectionController = new MyAccountSectionHUDController(
                Substitute.For<IMyAccountSectionHUDComponentView>(),
                dataStore);

            view = Substitute.For<IMyProfileComponentView>();

            userProfileBridge = Substitute.For<IUserProfileBridge>();
            UserProfile userProfile = ScriptableObject.CreateInstance<UserProfile>();

            userProfile.UpdateData(new UserProfileModel
            {
                userId = OWN_USER_ID,
                name = NOT_OWNED_FULL_NAME,
                hasClaimedName = false,
                description = MY_DESCRIPTION,
            });

            userProfileBridge.GetOwn().Returns(userProfile);

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

            controller = new MyProfileController(view,
                dataStore,
                userProfileBridge,
                namesService,
                browserBridge,
                myAccountSectionController,
                kernelConfig);
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
            });

            userProfileBridge.GetOwn().Returns(userProfile);

            dataStore.myAccount.isMyAccountSectionVisible.Set(true, true);

            view.Received(1)
                .SetClaimedNameDropdownOptions(Arg.Is<List<string>>(l => l[0] == OWNED_NAME_1
                                                                         && l[1] == OWNED_NAME_2
                                                                         && l[2] == OWNED_NAME_3
                                                                         && l.Count == givenOwnedNames.Count));

            view.Received(1).SetClaimedNameMode(true);
            view.Received(1).SetClaimedModeAsInput(false);
            view.Received(1).SetCurrentName(OWNED_NAME_1, "");
            view.Received(1).SetClaimBannerActive(false);
        }

        [Test]
        public void ShowNamesWhenTheUserDoesntHaveAnyOwnedName()
        {
            dataStore.myAccount.isMyAccountSectionVisible.Set(true, true);

            view.Received(1).SetClaimedNameDropdownOptions(Arg.Is<List<string>>(l => l.Count == 0));

            view.Received(1).SetClaimedNameMode(false);
            view.Received(1).SetClaimedModeAsInput(true);
            view.Received(1).SetCurrentName(NOT_OWNED_NAME_PREFIX, NOT_OWNED_NAME_SUFFIX);
            view.Received(1).SetClaimBannerActive(true);
        }

        [Test]
        public void ShowAboutDescription()
        {
            dataStore.myAccount.isMyAccountSectionVisible.Set(true, true);

            view.Received(1).SetAboutDescription(MY_DESCRIPTION);
        }

        [Test]
        public void ShowLinksWhenIsEmpty()
        {
            dataStore.myAccount.isMyAccountSectionVisible.Set(true, true);

            view.Received(1).SetLinks(Arg.Is<List<(string title, string url)>>(l => l.Count == 0));
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
                }
            });

            userProfileBridge.GetOwn().Returns(userProfile);

            dataStore.myAccount.isMyAccountSectionVisible.Set(true, true);

            view.Received(1)
                .SetLinks(Arg.Is<List<(string title, string url)>>(l => l.Count == 3
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
                }
            });

            userProfileBridge.GetOwn().Returns(userProfile);

            dataStore.myAccount.isMyAccountSectionVisible.Set(true, true);

            view.Received(1).SetLinks(Arg.Is<List<(string title, string url)>>(l => l.Count == 5));
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
            userProfileBridge.Received(1).SaveUnverifiedName(VALID_NAME);
        }

        [Test]
        public void SaveVerifiedNameWhenSubmittingTheName()
        {
            view.OnCurrentNameSubmitted += Raise.Event<Action<string, bool>>(VALID_NAME, true);

            view.Received(1).SetNonValidNameWarningActive(false);

            view.Received(1).SetNonValidNameWarningActive(false);
            userProfileBridge.Received(1).SaveVerifiedName(VALID_NAME);
        }

        [Test]
        public void OpenBrowserUrlWhenClickingClaimName()
        {
            view.OnClaimNameClicked += Raise.Event<Action>();

            browserBridge.Received(1).OpenUrl("https://builder.decentraland.org/claim-name");
        }

        [Test]
        public void SaveAboutDescription()
        {
            const string ANOTHER_DESCRIPTION = "another description";

            view.OnAboutDescriptionSubmitted += Raise.Event<Action<string>>(ANOTHER_DESCRIPTION);

            userProfileBridge.SaveDescription(ANOTHER_DESCRIPTION);
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

            view.Received(1)
                .SetLinks(Arg.Is<List<(string title, string url)>>(l => l.Count == expectedLinks.Count
                                                                        && l.All(viewLinks => expectedLinks.Exists(expectedLink => expectedLink.title == viewLinks.title && expectedLink.url == viewLinks.url))));

            view.Received(1).EnableOrDisableAddLinksOption(expectedLinks.Count < 5);
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
            view.DidNotReceiveWithAnyArgs().SetLinks(default);
            view.Received(0).EnableOrDisableAddLinksOption(true);
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
                .SetLinks(Arg.Is<List<(string title, string url)>>(l => l.Count == expectedLinks.Count
                                                                        && l.All(viewLinks => expectedLinks.Exists(expectedLink => expectedLink.title == viewLinks.title && expectedLink.url == viewLinks.url))));
        }
    }
}
