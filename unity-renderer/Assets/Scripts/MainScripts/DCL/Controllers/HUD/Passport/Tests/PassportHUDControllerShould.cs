using AvatarSystem;
using DCL.ProfanityFiltering;
using DCL.Social.Friends;
using DCLServices.Lambdas.LandsService;
using DCLServices.Lambdas.NamesService;
using DCLServices.WearablesCatalogService;
using NSubstitute;
using NUnit.Framework;
using SocialFeaturesAnalytics;

namespace DCL.Social.Passports
{
    public class PassportHUDControllerShould
    {
        private PlayerPassportHUDController controller;
        private IPlayerPassportHUDView view;
        private PassportPlayerInfoComponentController playerInfoController;
        private PassportPlayerPreviewComponentController playerPreviewController;
        private PassportNavigationComponentController passportNavigationController;
        private IUserProfileBridge userProfileBridge;
        private ISocialAnalytics socialAnalytics;
        private IWearableItemResolver wearableItemResolver;
        private DataStore dataStore;
        private IProfanityFilter profanityFilter;
        private IPassportApiBridge passportApiBridge;
        private IFriendsController friendsController;

        [SetUp]
        public void SetUp()
        {
            Environment.Setup(ServiceLocatorTestFactory.CreateMocked());

            view = Substitute.For<IPlayerPassportHUDView>();

            userProfileBridge = Substitute.For<IUserProfileBridge>();
            socialAnalytics = Substitute.For<ISocialAnalytics>();
            wearableItemResolver = Substitute.For<IWearableItemResolver>();
            dataStore = Substitute.For<DataStore>();
            profanityFilter = Substitute.For<IProfanityFilter>();
            passportApiBridge = Substitute.For<IPassportApiBridge>();
            friendsController = Substitute.For<IFriendsController>();
            playerInfoController = new PassportPlayerInfoComponentController(
                                Substitute.For<IPassportPlayerInfoComponentView>(),
                                dataStore,
                                profanityFilter,
                                friendsController,
                                userProfileBridge,
                                socialAnalytics,
                                Substitute.For<IClipboard>(),
                                passportApiBridge);

            var playerPreviewView = Substitute.For<IPassportPlayerPreviewComponentView>();

            playerPreviewController = new PassportPlayerPreviewComponentController(
                playerPreviewView,
                socialAnalytics);
            passportNavigationController = new PassportNavigationComponentController(
                                Substitute.For<IPassportNavigationComponentView>(),
                                profanityFilter,
                                wearableItemResolver,
                                Substitute.For<IWearablesCatalogService>(),
                                Substitute.For<IEmotesCatalogService>(),
                                Substitute.For<INamesService>(),
                                Substitute.For<ILandsService>(),
                                Substitute.For<IUserProfileBridge>(),
                                dataStore,
                                new ViewAllComponentController(
                                    Substitute.For<IViewAllComponentView>(),
                                    Substitute.For<DataStore_HUDs>(),
                                    Substitute.For<IWearablesCatalogService>(),
                                    Substitute.For<ILandsService>(),
                                    Substitute.For<INamesService>(),
                                    NotificationsController.i));

            controller = new PlayerPassportHUDController(
                view,
                playerInfoController,
                playerPreviewController,
                passportNavigationController,
                userProfileBridge,
                passportApiBridge,
                socialAnalytics,
                dataStore,
                Substitute.For<MouseCatcher>(),
                Substitute.For<BooleanVariable>()
            );
        }

        [TearDown]
        public void TearDown()
        {
            controller.Dispose();
        }

        [Test]
        public void SetVisibilityTrueCorrectly()
        {
            controller.SetVisibility(true);

            view.Received(1).SetVisibility(true);
        }

        [Test]
        public void SetVisibilityFalseCorrectly()
        {
            controller.SetVisibility(false);

            view.Received(1).SetVisibility(false);
        }
    }
}
