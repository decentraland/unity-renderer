using DCL.Browser;
using DCL.Controllers;
using DCL.Models;
using DCLServices.PlacesAPIService;
using NSubstitute;
using NUnit.Framework;
using System;
using System.Collections;
using Tests;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace DCL.ContentModeration.Tests
{
    public class ContentModerationHUDControllerShould : IntegrationTestSuite
    {
        private const int TEST_TEEN_SCENE_NUMBER = 1;
        private const int TEST_ADULT_SCENE_NUMBER = 2;
        private const int TEST_RESTRICTED_SCENE_NUMBER = 3;

        private ContentModerationHUDController contentModerationHUDController;
        private IAdultContentSceneWarningComponentView adultContentSceneWarningComponentView;
        private IAdultContentAgeConfirmationComponentView adultContentAgeConfirmationComponentView;
        private IAdultContentEnabledNotificationComponentView adultContentEnabledNotificationComponentView;
        private IContentModerationReportingComponentView contentModerationReportingComponentView;
        private IContentModerationReportingButtonComponentView contentModerationReportingButtonForWorldsComponentView;
        private DataStore_Common commonDataStore;
        private DataStore_Settings settingsDataStore;
        private DataStore_ContentModeration contentModerationDataStore;
        private IBrowserBridge browserBridge;
        private IPlacesAPIService placesAPIService;
        private IUserProfileBridge userProfileBridge;
        private IContentModerationAnalytics contentModerationAnalytics;
        private NotificationsController notificationsController;

        private ParcelScene testTeenScene;
        private ParcelScene testAdultScene;
        private ParcelScene testRestrictedScene;

        protected override void InitializeServices(ServiceLocator serviceLocator) =>
            serviceLocator.Register<IWorldState>(() => new WorldState());

        [UnitySetUp]
        protected override IEnumerator SetUp()
        {
            yield return base.SetUp();

            adultContentSceneWarningComponentView = Substitute.For<IAdultContentSceneWarningComponentView>();
            adultContentAgeConfirmationComponentView = Substitute.For<IAdultContentAgeConfirmationComponentView>();
            adultContentEnabledNotificationComponentView = Substitute.For<IAdultContentEnabledNotificationComponentView>();
            contentModerationReportingComponentView = Substitute.For<IContentModerationReportingComponentView>();
            contentModerationReportingButtonForWorldsComponentView = Substitute.For<IContentModerationReportingButtonComponentView>();
            commonDataStore = new DataStore_Common();
            settingsDataStore = new DataStore_Settings();
            contentModerationDataStore = new DataStore_ContentModeration();
            browserBridge = Substitute.For<IBrowserBridge>();
            placesAPIService = Substitute.For<IPlacesAPIService>();
            userProfileBridge = Substitute.For<IUserProfileBridge>();
            contentModerationAnalytics = Substitute.For<IContentModerationAnalytics>();
            notificationsController = Object.Instantiate(new GameObject()).AddComponent<NotificationsController>();
            notificationsController.allowNotifications = false;

            contentModerationHUDController = new ContentModerationHUDController(
                adultContentSceneWarningComponentView,
                adultContentAgeConfirmationComponentView,
                adultContentEnabledNotificationComponentView,
                contentModerationReportingComponentView,
                contentModerationReportingButtonForWorldsComponentView,
                Environment.i.world.state,
                commonDataStore,
                settingsDataStore,
                contentModerationDataStore,
                browserBridge,
                placesAPIService,
                userProfileBridge,
                contentModerationAnalytics,
                notificationsController);

            CreateTestScene(new Vector2Int { x = 50, y = 50 }, SceneContentCategory.TEEN, "testTeenPlace", TEST_TEEN_SCENE_NUMBER);
            CreateTestScene(new Vector2Int { x = 100, y = 100 }, SceneContentCategory.ADULT, "testAdultPlace", TEST_ADULT_SCENE_NUMBER);
            CreateTestScene(new Vector2Int { x = 101, y = 100 }, SceneContentCategory.RESTRICTED, "testRestrictedPlace", TEST_RESTRICTED_SCENE_NUMBER);
        }

        protected override IEnumerator TearDown()
        {
            contentModerationHUDController.Dispose();
            Object.Destroy(testTeenScene.gameObject);
            Object.Destroy(testAdultScene.gameObject);
            Object.Destroy(testRestrictedScene.gameObject);
            Object.Destroy(notificationsController.gameObject);

            return base.TearDown();
        }

        [Test]
        [TestCase(TEST_TEEN_SCENE_NUMBER)]
        [TestCase(TEST_ADULT_SCENE_NUMBER)]
        [TestCase(TEST_RESTRICTED_SCENE_NUMBER)]
        public void RaiseOnSceneNumberChangedCorrectly(int sceneNumber)
        {
            // Act
            CommonScriptableObjects.sceneNumber.Set(sceneNumber);

            // Assert
            switch (sceneNumber)
            {
                case TEST_ADULT_SCENE_NUMBER:
                    adultContentSceneWarningComponentView.Received(1).ShowModal();
                    adultContentSceneWarningComponentView.Received(1).SetRestrictedMode(false);
                    break;
                case TEST_RESTRICTED_SCENE_NUMBER:
                    adultContentSceneWarningComponentView.Received(1).ShowModal();
                    adultContentSceneWarningComponentView.Received(1).SetRestrictedMode(true);
                    break;
                case TEST_TEEN_SCENE_NUMBER:
                    adultContentSceneWarningComponentView.Received(1).HideModal();
                    break;
            }
            contentModerationReportingComponentView.Received(1).HidePanel(true);
        }

        [Test]
        public void RaiseOnGoToSettingsPanelClickedCorrectly()
        {
            // Act
            adultContentSceneWarningComponentView.OnGoToSettingsClicked += Raise.Event<Action>();

            // Assert
            Assert.IsTrue(settingsDataStore.settingsPanelVisible.Get());
            contentModerationAnalytics.Received(1).OpenSettingsFromContentWarning();
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void RaiseOnAdultContentAgeConfirmationVisibleCorrectly(bool isVisible)
        {
            // Act
            contentModerationDataStore.adultContentAgeConfirmationVisible.Set(isVisible, true);

            // Assert
            if (isVisible)
                adultContentAgeConfirmationComponentView.Received(1).ShowModal();
            else
                adultContentAgeConfirmationComponentView.DidNotReceive().ShowModal();
        }

        [Test]
        public void RaiseOnAgeConfirmationAcceptedCorrectly()
        {
            // Act
            adultContentAgeConfirmationComponentView.OnConfirmClicked += Raise.Event<Action>();

            // Assert
            Assert.AreEqual(DataStore_ContentModeration.AdultContentAgeConfirmationResult.Accepted, contentModerationDataStore.adultContentAgeConfirmationResult.Get());
            Assert.IsFalse(settingsDataStore.settingsPanelVisible.Get());
            adultContentEnabledNotificationComponentView.Received(1).ShowNotification();
        }

        [Test]
        public void RaiseOnAgeConfirmationRejectedCorrectly()
        {
            // Act
            adultContentAgeConfirmationComponentView.OnCancelClicked += Raise.Event<Action>();

            // Assert
            Assert.AreEqual(DataStore_ContentModeration.AdultContentAgeConfirmationResult.Rejected, contentModerationDataStore.adultContentAgeConfirmationResult.Get());
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void RaiseOnAdultContentSettingChangedCorrectly(bool isEnabled)
        {
            // Arrange
            CommonScriptableObjects.sceneNumber.Set(TEST_ADULT_SCENE_NUMBER);
            contentModerationDataStore.adultContentSettingEnabled.Set(!isEnabled, false);

            // Act
            contentModerationDataStore.adultContentSettingEnabled.Set(isEnabled);

            // Assert
            if (!isEnabled)
                adultContentEnabledNotificationComponentView.Received(1).HideNotification();
            adultContentSceneWarningComponentView.Received(1).ShowModal();
            adultContentSceneWarningComponentView.Received(1).SetRestrictedMode(false);
            contentModerationReportingComponentView.Received().HidePanel(true);
            contentModerationAnalytics.Received(1).SwitchAdultContentSetting(isEnabled);
        }

        [Test]
        [TestCase(true, SceneContentCategory.TEEN, TEST_TEEN_SCENE_NUMBER)]
        [TestCase(false, SceneContentCategory.TEEN, TEST_TEEN_SCENE_NUMBER)]
        [TestCase(true, SceneContentCategory.ADULT, TEST_ADULT_SCENE_NUMBER)]
        [TestCase(false, SceneContentCategory.ADULT, TEST_ADULT_SCENE_NUMBER)]
        [TestCase(true, SceneContentCategory.RESTRICTED, TEST_RESTRICTED_SCENE_NUMBER)]
        [TestCase(false, SceneContentCategory.RESTRICTED, TEST_RESTRICTED_SCENE_NUMBER)]
        public void RaiseOnReportingScenePanelVisibleCorrectly(bool isVisible, SceneContentCategory rating, int sceneNumber)
        {
            // Arrange
            CommonScriptableObjects.sceneNumber.Set(sceneNumber);

            // Act
            contentModerationDataStore.reportingScenePanelVisible.Set((isVisible, rating), true);

            // Assert
            if (isVisible)
            {
                contentModerationReportingComponentView.Received(1).ShowPanel();
                contentModerationReportingComponentView.Received(1).SetRatingAsMarked(rating);
                contentModerationReportingComponentView.Received(1).SetRating(rating);

                switch (rating)
                {
                    default:
                    case SceneContentCategory.TEEN:
                        contentModerationAnalytics.Received(1).OpenReportForm(testTeenScene.associatedPlaceId);
                        break;
                    case SceneContentCategory.ADULT:
                        contentModerationAnalytics.Received(1).OpenReportForm(testAdultScene.associatedPlaceId);
                        break;
                    case SceneContentCategory.RESTRICTED:
                        contentModerationAnalytics.Received(1).OpenReportForm(testRestrictedScene.associatedPlaceId);
                        break;
                }
            }
            else
                contentModerationReportingComponentView.Received(1).HidePanel(false);
        }

        [Test]
        [TestCase(true, SceneContentCategory.TEEN, TEST_TEEN_SCENE_NUMBER)]
        [TestCase(false, SceneContentCategory.TEEN, TEST_TEEN_SCENE_NUMBER)]
        [TestCase(true, SceneContentCategory.ADULT, TEST_ADULT_SCENE_NUMBER)]
        [TestCase(false, SceneContentCategory.ADULT, TEST_ADULT_SCENE_NUMBER)]
        [TestCase(true, SceneContentCategory.RESTRICTED, TEST_RESTRICTED_SCENE_NUMBER)]
        [TestCase(false, SceneContentCategory.RESTRICTED, TEST_RESTRICTED_SCENE_NUMBER)]
        public void RaiseOnContentModerationReportingClosedCorrectly(bool isCancelled, SceneContentCategory category, int sceneNumber)
        {
            // Arrange
            CommonScriptableObjects.sceneNumber.Set(0);
            CommonScriptableObjects.sceneNumber.Set(sceneNumber);
            contentModerationDataStore.reportingScenePanelVisible.Set((true, category), false);

            // Act
            contentModerationReportingComponentView.OnPanelClosed += Raise.Event<Action<bool>>(isCancelled);

            // Assert
            Assert.AreEqual((false, category), contentModerationDataStore.reportingScenePanelVisible.Get());

            switch (category)
            {
                default:
                case SceneContentCategory.TEEN:
                    contentModerationAnalytics.Received(1).CloseReportForm(testTeenScene.associatedPlaceId, isCancelled);
                    break;
                case SceneContentCategory.ADULT:
                    contentModerationAnalytics.Received(1).CloseReportForm(testAdultScene.associatedPlaceId, isCancelled);
                    break;
                case SceneContentCategory.RESTRICTED:
                    contentModerationAnalytics.Received(1).CloseReportForm(testRestrictedScene.associatedPlaceId, isCancelled);
                    break;
            }
        }

        [Test]
        public void RaiseOnLearnMoreClickedCorrectly()
        {
            // Act
            contentModerationReportingComponentView.OnLearnMoreClicked += Raise.Event<Action>();

            // Assert
            browserBridge.Received(1).OpenUrl(Arg.Any<string>());
            contentModerationAnalytics.Received(1).ClickLearnMoreContentModeration(Arg.Any<string>());
        }

        private void CreateTestScene(
            Vector2Int coords,
            SceneContentCategory category,
            string associatedPlaceId,
            int sceneNumber)
        {
            var testScene = new GameObject().AddComponent<ParcelScene>();
            testScene.isTestScene = true;
            testScene.isPersistent = false;
            testScene.SetContentCategory(category);
            testScene.SetAssociatedPlace(associatedPlaceId);
            testScene.SetData(new LoadParcelScenesMessage.UnityParcelScene
            {
                parcels = new[] { coords },
                sceneNumber = sceneNumber,
            });

            if (Environment.i.world.state.ContainsScene(sceneNumber))
                Environment.i.world.state.RemoveScene(sceneNumber);

            Environment.i.world.state.AddScene(testScene);

            switch (category)
            {
                default:
                case SceneContentCategory.TEEN:
                    testTeenScene = testScene;
                    break;
                case SceneContentCategory.ADULT:
                    testAdultScene = testScene;
                    break;
                case SceneContentCategory.RESTRICTED:
                    testRestrictedScene = testScene;
                    break;
            }
        }
    }
}
