using MainScripts.DCL.Controllers.HUD.CharacterPreview;
using NSubstitute;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace DCL.Backpack
{
    public class AvatarSlotsHUDControllerShould
    {
        private AvatarSlotsHUDController avatarSlotsHUDController;
        private AvatarSlotsDefinitionSO avatarSlotsDefinition;
        private IAvatarSlotsView avatarSlotsView;
        private IBackpackAnalyticsService backpackAnalyticsService;
        private IBaseVariable<FeatureFlag> featureFlag;

        [SetUp]
        public void SetUp()
        {
            avatarSlotsView = Substitute.For<IAvatarSlotsView>();
            backpackAnalyticsService = Substitute.For<IBackpackAnalyticsService>();
            featureFlag = Substitute.For<IBaseVariable<FeatureFlag>>();
            avatarSlotsHUDController = new AvatarSlotsHUDController(avatarSlotsView, backpackAnalyticsService, featureFlag);
            avatarSlotsDefinition = ScriptableObject.CreateInstance<AvatarSlotsDefinitionSO>();

            avatarSlotsDefinition.slotsDefinition = new SerializableKeyValuePair<string, List<string>>[2];
            avatarSlotsDefinition.slotsDefinition[0] = new SerializableKeyValuePair<string, List<string>>()
            {
                key = "section1",
                value = new List<string>(){ "body_shape", "head" }
            };
            avatarSlotsDefinition.slotsDefinition[1] = new SerializableKeyValuePair<string, List<string>>()
            {
                key = "section2",
                value = new List<string>(){ "tiara", "mask", "helmet" }
            };

            avatarSlotsHUDController.avatarSlotsDefinition = avatarSlotsDefinition;
        }

        [TearDown]
        public void TearDown()
        {
            avatarSlotsHUDController.Dispose();
            Object.Destroy(avatarSlotsDefinition);
        }

        [Test]
        public void GenerateSlotsCorrectly()
        {
            avatarSlotsHUDController.GenerateSlots();

            avatarSlotsView.Received().CreateAvatarSlotSection("section1", true);
            avatarSlotsView.Received().AddSlotToSection("section1", "body_shape", false);
            avatarSlotsView.Received().AddSlotToSection("section1", "head", true);

            avatarSlotsView.Received().CreateAvatarSlotSection("section2", false);
            avatarSlotsView.Received().AddSlotToSection("section2", "tiara", true);
            avatarSlotsView.Received().AddSlotToSection("section2", "mask", true);
            avatarSlotsView.Received().AddSlotToSection("section2", "helmet", true);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void ShowHandsCategoryIfFeatureIsEnabled(bool enabled)
        {
            // setup feature flag and additional hands category
            var featureFlags = new FeatureFlag();
            featureFlags.flags.Add(AvatarSlotsHUDController.HANDS_FEATURE, enabled);
            featureFlag.Get().Returns(featureFlags);
            avatarSlotsDefinition.slotsDefinition[0].value.Add("hands");
            avatarSlotsHUDController = new AvatarSlotsHUDController(avatarSlotsView, backpackAnalyticsService, featureFlag);
            avatarSlotsHUDController.avatarSlotsDefinition = avatarSlotsDefinition;

            // when
            avatarSlotsHUDController.GenerateSlots();

            // then
            if (enabled)
            {
                avatarSlotsView.Received().AddSlotToSection("section1", "hands", true);
            }
            else
            {
                avatarSlotsView.DidNotReceive().AddSlotToSection("section1", "hands", true);
            }
        }

        [Test]
        public void ToggleSlotCorrectly()
        {
            avatarSlotsView.OnToggleAvatarSlot += Raise.Event<IAvatarSlotsView.ToggleAvatarSlotDelegate>(
                "tiara", false, PreviewCameraFocus.DefaultEditing, true);
            avatarSlotsView.DidNotReceive().DisablePreviousSlot(Arg.Any<string>());

            avatarSlotsView.OnToggleAvatarSlot += Raise.Event<IAvatarSlotsView.ToggleAvatarSlotDelegate>(
                "mask", false, PreviewCameraFocus.DefaultEditing, true);
            avatarSlotsView.Received().DisablePreviousSlot("tiara");
        }

        [Test]
        public void TrackForceShowAnalytic()
        {
            avatarSlotsView.OnHideUnhidePressed += Raise.Event<Action<string, bool>>("upper_body", true);

            backpackAnalyticsService.Received(1).SendForceShowWearable("upper_body");
        }

        [Test]
        public void TrackForceHideAnalytic()
        {
            avatarSlotsView.OnHideUnhidePressed += Raise.Event<Action<string, bool>>("upper_body", false);

            backpackAnalyticsService.Received(1).SendForceHideWearable("upper_body");
        }
    }
}
