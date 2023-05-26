using MainScripts.DCL.Controllers.HUD.CharacterPreview;
using NSubstitute;
using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

namespace DCL.Backpack
{
    public class AvatarSlotsHUDControllerShould
    {
        private AvatarSlotsHUDController avatarSlotsHUDController;
        private AvatarSlotsDefinitionSO avatarSlotsDefinition;
        internal IAvatarSlotsView avatarSlotsView;

        [SetUp]
        public void SetUp()
        {
            avatarSlotsView = Substitute.For<IAvatarSlotsView>();
            avatarSlotsHUDController = new AvatarSlotsHUDController(avatarSlotsView);
            avatarSlotsDefinition = ScriptableObject.CreateInstance<AvatarSlotsDefinitionSO>();

            avatarSlotsDefinition.slotsDefinition = new SerializableKeyValuePair<string, List<string>>[2];
            avatarSlotsDefinition.slotsDefinition[0] = new SerializableKeyValuePair<string, List<string>>()
            {
                key = "section1",
                value = new List<string>(){ "body_shape", "head", }
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
    }
}
